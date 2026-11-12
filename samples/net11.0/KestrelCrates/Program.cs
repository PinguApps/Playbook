// snippet:complete
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace KestrelCrates;

internal static class Program
{
    private static async Task Main()
    {
await using var db = new Crates();
await db.Database.OpenConnectionAsync();
await db.Database.MigrateAsync();
var model = db.GetService<IDesignTimeModel>().Model;
var operations = db.GetService<IMigrationsModelDiffer>().GetDifferences(null, model.GetRelationalModel());
var index = operations.OfType<CreateIndexOperation>().Single(x => x.Name.Contains("Harbour", StringComparison.Ordinal));
var json = (RelationalJsonIndex)index[RelationalAnnotationNames.JsonIndex]!;
if (index.Columns.Length != 1 || index.IsDescending != null || json.Elements.Count != 2
    || json.IsElementDescending(0) || !json.IsElementDescending(1)) throw new Exception("Per-element migration metadata mismatch");
foreach (var order in new IReadOnlyList<bool>?[] { null, [], [false, true] })
{
    var metadata = new RelationalJsonIndex(json.Elements, json.CollectionIndices, order);
    if (metadata.IsElementDescending(0) != (order is { Count: 0 }) || metadata.IsElementDescending(1) != (order != null))
        throw new Exception("Sort shorthand mismatch");
}
try { _ = new RelationalJsonIndex(json.Elements, json.CollectionIndices, [true]); throw new Exception("Mismatch accepted"); }
catch (ArgumentException) { Console.WriteLine("PASS: sort count rejected"); }
using (var unsupported = new AllSlots())
{
    try { _ = unsupported.Model; throw new Exception("All-elements path accepted"); }
    catch (InvalidOperationException error) when (error.Message.Contains("all elements", StringComparison.OrdinalIgnoreCase))
    { Console.WriteLine("PASS: SQLite rejects all-elements index"); }
}
db.AddRange(new Crate { Id = 1, Label = new() { Harbour = "Cove", Priority = 7, Slots = [new() { Code = "Amber" }] } },
    new Crate { Id = 2, Label = new() { Harbour = "Cove", Priority = 3, Slots = [new() { Code = "Blue" }] } });
await db.SaveChangesAsync();
var ids = await db.CratesSet.Where(x => x.Label.Harbour == "Cove")
    .OrderByDescending(x => x.Label.Priority).Select(x => x.Id).ToArrayAsync();
if (!ids.SequenceEqual(new[] { 1, 2 })) throw new Exception("Result mismatch");
if (await db.CratesSet.Where(x => x.Label.Slots[0].Code == "Blue").Select(x => x.Id).SingleAsync() != 2) throw new Exception("Position lookup mismatch");
await using var cmd = db.Database.GetDbConnection().CreateCommand();
cmd.CommandText = "SELECT sql FROM sqlite_schema WHERE type='index' AND name LIKE 'IX_%' ORDER BY name";
await using var reader = await cmd.ExecuteReaderAsync();
var schema = new List<string>();
while (await reader.ReadAsync()) { schema.Add(reader.GetString(0)); Console.WriteLine(reader.GetString(0)); }
if (schema.Count != 2 || !schema.Any(x => x.Contains("Priority", StringComparison.Ordinal) && x.Contains("DESC", StringComparison.Ordinal)) || !schema.Any(x => x.Contains("$.Slots[0].Code", StringComparison.Ordinal))) throw new Exception("Physical schema mismatch");
Console.WriteLine("PASS: migrated SQLite JSON lookups; no performance claim.");

    }
}

public class Crates : DbContext
{
    public DbSet<Crate> CratesSet => Set<Crate>();
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("Data Source=:memory:");
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Crate>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<Crate>().ComplexProperty(x => x.Label, p =>
        {
            p.ToJson();
            p.ComplexCollection(x => x.Slots);
        });
        modelBuilder.Entity<Crate>().HasIndex(x => new { x.Label.Harbour, x.Label.Priority }).IsDescending(false, true);
        modelBuilder.Entity<Crate>().HasIndex(x => x.Label.Slots[0].Code);
    }
}
public sealed class AllSlots : Crates
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Crate>().HasIndex("Label.Slots[].Code");
    }
}
public sealed class Factory : IDesignTimeDbContextFactory<Crates>
{
    public Crates CreateDbContext(string[] args) => new();
}
public sealed class Crate { public int Id { get; set; } public Label Label { get; set; } = new(); }
public sealed class Label { public string Harbour { get; set; } = ""; public int Priority { get; set; } public List<Slot> Slots { get; set; } = []; }
public sealed class Slot { public string Code { get; set; } = ""; }
// endsnippet:complete
