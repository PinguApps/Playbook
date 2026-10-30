// snippet:complete
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace TrailCards;

internal static class Program
{
    public static void Main(string[] args)
    {
        string? supplied = Environment.GetEnvironmentVariable("PLAYBOOK_TRAIL_DATABASE");
        string path = supplied ?? Path.Combine(Path.GetTempPath(), $"trail-{Guid.NewGuid():N}.db");
        try
        {
            using SqliteConnection connection = new($"Data Source={path};Pooling=False");
            connection.Open();
            DbContextOptions<CardContext> options = new DbContextOptionsBuilder<CardContext>().UseSqlite(connection).Options;
            using CardContext context = new(options);
            var waypoint = context.Model.FindEntityType(typeof(Waypoint))!;
            Require(waypoint.IsOwned() && waypoint.FindPrimaryKey()!.Properties.Any(p => p.IsShadowProperty()), "owned model has synthetic identity");
            if (args is ["verify-migrated"])
            {
                Card card = context.Cards.Single(c => c.Id == 1);
                Require(card.Waypoints.Select(p => p.Caption).SequenceEqual(["Lookout", "Gate", "Bridge"]), "new values readable by old mapping");
                Require(card.Waypoints.Select(p => p.Minutes).SequenceEqual([12, 8, 6]), "new numbers readable by old mapping");
                Require(context.Cards.Single(c => c.Id == 2).Waypoints.Count == 0, "empty sequence retained");
                Console.WriteLine("PASS: EF10 fresh context reads EF11 saved values and empty sequence.");
                return;
            }
            Require(args.Length == 0, "use no arguments to seed, or verify-migrated to read the result");
            context.Database.EnsureCreated();
            context.Cards.AddRange(
                new Card { Id = 1, Waypoints = [new() { Caption = "Gate", Minutes = 4 }, new() { Caption = "Lookout", Minutes = 12 }, new() { Caption = "Gate", Minutes = 4 }] },
                new Card { Id = 2 });
            context.SaveChanges();
            context.ChangeTracker.Clear();
            Card original = context.Cards.Single(c => c.Id == 1);
            Require(original.Waypoints.Select(p => p.Caption).SequenceEqual(["Gate", "Lookout", "Gate"]), "original order and duplicates");
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT Waypoints FROM Cards WHERE Id = 1";
            string json = (string)command.ExecuteScalar()!;
            using JsonDocument document = JsonDocument.Parse(json);
            Require(document.RootElement.GetArrayLength() == 3 && document.RootElement[0].EnumerateObject().Count() == 2, "JSON array with no stored synthetic key");
            Console.WriteLine($"ORIGINAL JSON: {json}");
            Console.WriteLine($"PASS: EF10 owned shadow-key model, real SQLite insert/read; runtime {Environment.Version}; EF {typeof(DbContext).Assembly.GetName().Version}.");
        }
        finally
        {
            if (supplied is null) { File.Delete(path); }
        }
    }

    private static void Require(bool condition, string claim)
    {
        if (!condition) { throw new InvalidOperationException(claim); }
    }
}

internal sealed class CardContext(DbContextOptions<CardContext> options) : DbContext(options)
{
    public DbSet<Card> Cards => Set<Card>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Card>().ToTable("Cards");
        modelBuilder.Entity<Card>().OwnsMany(card => card.Waypoints, waypoints => waypoints.ToJson("Waypoints"));
    }
}

internal sealed class Card
{
    public int Id { get; set; }
    public List<Waypoint> Waypoints { get; set; } = [];
}

internal sealed class Waypoint
{
    public required string Caption { get; set; }
    public int Minutes { get; set; }
}
// endsnippet:complete
