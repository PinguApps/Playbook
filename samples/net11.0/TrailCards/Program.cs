// snippet:complete
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace TrailCards;

internal static class Program
{
    public static void Main()
    {
        string path = Environment.GetEnvironmentVariable("PLAYBOOK_TRAIL_DATABASE")
            ?? throw new InvalidOperationException("First seed with the EF10 sample; set PLAYBOOK_TRAIL_DATABASE to that disposable database path.");
        using SqliteConnection connection = new($"Data Source={path};Mode=ReadWrite;Pooling=False");
        connection.Open();
        string schema = ReadText(connection, "SELECT sql FROM sqlite_master WHERE name = 'Cards'");
        string originalJson = ReadText(connection, "SELECT Waypoints FROM Cards WHERE Id = 1");
        string version = ReadText(connection, "SELECT sqlite_version()");
        Require(schema.Contains("\"Waypoints\" TEXT NULL", StringComparison.Ordinal), "old model allows a null JSON column");

        // Regression control: the old mapping still works, but emits the new diagnostic.
        bool diagnosed = false;
        try
        {
            using OwnedControlContext old = new(new DbContextOptionsBuilder<OwnedControlContext>()
                .UseSqlite(connection)
                .ConfigureWarnings(w => w.Throw(RelationalEventId.OwnedEntityMappedToJsonCollectionWarning)).Options);
            _ = old.Model;
        }
        catch (InvalidOperationException exception) when (exception.Message.Contains(nameof(RelationalEventId.OwnedEntityMappedToJsonCollectionWarning), StringComparison.Ordinal))
        {
            diagnosed = true;
        }
        Require(diagnosed, "EF11 reports ordinal-owned JSON warning on the old model");

        DbContextOptions<CardContext> options = new DbContextOptionsBuilder<CardContext>()
            .UseSqlite(connection)
            .ConfigureWarnings(w => w.Throw(RelationalEventId.OwnedEntityMappedToJsonCollectionWarning)).Options;
        using (CardContext context = new(options))
        {
            var entity = context.Model.FindEntityType(typeof(Card))!;
            string newDdl = context.Database.GenerateCreateScript();
            Require(newDdl.Contains("\"Waypoints\" TEXT NOT NULL", StringComparison.Ordinal), "required complex collection changes generated nullability");
            Console.WriteLine($"NEW MODEL DDL: {newDdl}");
            Require(entity.FindNavigation(nameof(Card.Waypoints)) is null
                && entity.FindComplexProperty(nameof(Card.Waypoints)) is not null
                && context.Model.FindEntityType(typeof(Waypoint)) is null, "complex collection has no owned entity or synthetic entity key");
            Card card = context.Cards.Single(c => c.Id == 1);
            Require(card.Waypoints.Select(p => p.Caption).SequenceEqual(["Gate", "Lookout", "Gate"])
                && card.Waypoints.Select(p => p.Minutes).SequenceEqual([4, 12, 4]), "actual EF10 data retains order, duplicates and numbers");
            Require(!ReferenceEquals(card.Waypoints[0], card.Waypoints[2]), "duplicate values remain separate elements");
            Require(context.Cards.Single(c => c.Id == 2).Waypoints.Count == 0, "empty array materializes empty");
            Require(context.SaveChanges() == 0, "reading the new model performs no data rewrite");
            Require(ReadText(connection, "SELECT Waypoints FROM Cards WHERE Id = 1") == originalJson, "original stored JSON unchanged before edit");
            card.Waypoints.RemoveAt(0);
            card.Waypoints[1].Minutes = 8;
            card.Waypoints.Add(new Waypoint { Caption = "Bridge", Minutes = 6 });
            context.SaveChanges();
        }
        using (CardContext context = new(options))
        {
            Card card = context.Cards.AsNoTracking().Single(c => c.Id == 1);
            Require(card.Waypoints.Select(p => p.Caption).SequenceEqual(["Lookout", "Gate", "Bridge"])
                && card.Waypoints.Select(p => p.Minutes).SequenceEqual([12, 8, 6]), "remove, edit and append persist in order");
            Require(context.Cards.AsNoTracking().Single(c => c.Id == 2).Waypoints.Count == 0, "empty array remains empty after save");
        }
        Require(ReadText(connection, "SELECT sql FROM sqlite_master WHERE name = 'Cards'") == schema, "existing table schema unchanged");
        string changedJson = ReadText(connection, "SELECT Waypoints FROM Cards WHERE Id = 1");
        using JsonDocument changed = JsonDocument.Parse(changedJson);
        Require(changed.RootElement.GetArrayLength() == 3 && changed.RootElement.EnumerateArray().All(p => p.EnumerateObject().Count() == 2), "no identity field added to JSON");
        Console.WriteLine($"ORIGINAL JSON: {originalJson}");
        Console.WriteLine($"UPDATED JSON: {changedJson}");
        Console.WriteLine($"SCHEMA: {schema}");
        Console.WriteLine($"PASS: actual EF10 data migrated to EF11 complex collection; old diagnostic, keyless complex metadata, no pre-edit rewrite, order/duplicates/empty, fresh-context edits, unchanged schema; SQLite {version}; runtime {Environment.Version}; EF {typeof(DbContext).Assembly.GetName().Version}.");
    }

    [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Private helper called only with fixed SQL literals; no external query input.")]
    private static string ReadText(SqliteConnection connection, string sql)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        return (string)command.ExecuteScalar()!;
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
        modelBuilder.Entity<Card>().ComplexCollection(card => card.Waypoints, waypoints => waypoints.ToJson("Waypoints"));
    }
}

internal sealed class OwnedControlContext(DbContextOptions<OwnedControlContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Card>().OwnsMany(card => card.Waypoints, waypoints => waypoints.ToJson("Waypoints"));
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
