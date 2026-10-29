// snippet:complete
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace KilnReadings;

internal static class Program
{
    public static void Main()
    {
        using SqliteConnection connection = new("Data Source=:memory:");
        connection.Open();
        DbContextOptions<ReadingContext> options = new DbContextOptionsBuilder<ReadingContext>()
            .UseSqlite(connection).Options;
        
        using (ReadingContext context = new(options))
        {
            IEntityType entity = context.Model.FindEntityType(typeof(Firing))!;
            Require(entity.FindProperty(nameof(Firing.Steps))!.GetElementType()?.ClrType == typeof(int), "integer collection metadata");
            Require(entity.FindProperty(nameof(Firing.SensorCodes))!.GetElementType()?.ClrType == typeof(byte), "byte collection metadata");
            Require(entity.FindProperty(nameof(Firing.Seal))!.GetElementType() is null, "binary scalar metadata");
            context.Database.EnsureCreated();
            context.Firings.Add(new Firing { Steps = [640, 820, 640], SensorCodes = [7, 0, 255], Seal = [7, 0, 255] });
            context.SaveChanges();
            using SqliteCommand stored = connection.CreateCommand();
            stored.CommandText = "SELECT typeof(Steps), typeof(SensorCodes), typeof(Seal), json_array_length(SensorCodes) FROM Firings";
            using SqliteDataReader reader = stored.ExecuteReader();
            Require(reader.Read() && reader.GetString(0) == "text" && reader.GetString(1) == "text"
                && reader.GetString(2) == "blob" && reader.GetInt32(3) == 3, "JSON sequences versus binary scalar storage");
        }
        using (ReadingContext context = new(options))
        {
            Firing firing = context.Firings.Single();
            Require(firing.Steps.SequenceEqual([640, 820, 640]) && firing.SensorCodes.SequenceEqual(new byte[] { 7, 0, 255 })
                && firing.Seal.SequenceEqual(new byte[] { 7, 0, 255 }), "fresh-context values, order and duplicates");
            firing.Steps.Add(900);
            firing.SensorCodes[1] = 19;
            firing.Seal = [4, 5];
            context.SaveChanges();
        }
        using (ReadingContext context = new(options))
        {
            Firing firing = context.Firings.AsNoTracking().Single();
            Require(firing.Steps.SequenceEqual([640, 820, 640, 900]) && firing.SensorCodes.SequenceEqual(new byte[] { 7, 19, 255 })
                && firing.Seal.SequenceEqual(new byte[] { 4, 5 }), "collection mutations and scalar replacement round-trip");
        }
        using PropertyControlContext control = new(new DbContextOptionsBuilder<PropertyControlContext>().UseSqlite(connection).Options);
        Require(control.Model.FindEntityType(typeof(Firing))!.FindProperty(nameof(Firing.Steps))!.GetElementType() is null,
            "EF11 Property collection classification control");
        using SqliteCommand version = connection.CreateCommand();
        version.CommandText = "SELECT sqlite_version()";
        Console.WriteLine($"PASS: collection/scalar metadata, JSON array/TEXT versus BLOB, insert/read/update; SQLite {version.ExecuteScalar()}; EF {typeof(DbContext).Assembly.GetName().Version}.");
    }

    private static void Require(bool condition, string claim)
    {
        if (!condition) { throw new InvalidOperationException(claim); }
    }
}

internal sealed class ReadingContext(DbContextOptions<ReadingContext> options) : DbContext(options)
{
    public DbSet<Firing> Firings => Set<Firing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Firing>().PrimitiveCollection(firing => firing.Steps);
        modelBuilder.Entity<Firing>().PrimitiveCollection(firing => firing.SensorCodes);
        modelBuilder.Entity<Firing>().Property(firing => firing.Seal);
    }
}

internal sealed class PropertyControlContext(DbContextOptions<PropertyControlContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Firing>().Property(firing => firing.Steps);
        modelBuilder.Entity<Firing>().Property(firing => firing.SensorCodes);
        modelBuilder.Entity<Firing>().Property(firing => firing.Seal);
    }
}
internal sealed class Firing
{
    public int Id { get; set; }
    public List<int> Steps { get; set; } = [];
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Deliberate byte-array collection mapping.")]
    public byte[] SensorCodes { get; set; } = [];
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Deliberate binary scalar mapping.")]
    public byte[] Seal { get; set; } = [];
}
// endsnippet:complete
