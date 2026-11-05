// snippet:complete
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

Marsh.InvalidComplexIndex = args.Contains("--invalid-complex-index");
using var connection = new SqliteConnection("Data Source=:memory:");
connection.Open();
var options = new DbContextOptionsBuilder<Marsh>().UseSqlite(connection).Options;
using (var db = new Marsh(options))
{
    if (Marsh.InvalidComplexIndex) {
        try { db.Database.EnsureCreated(); throw new Exception("Expected complex-index rejection"); }
        catch (InvalidOperationException error) when (error.Message.Contains("Reference each scalar property", StringComparison.Ordinal)) { Console.WriteLine("PASS SQLite whole-complex index rejected; use scalar paths"); return; }
    }
    db.Database.EnsureCreated();
    var entity = db.Model.FindEntityType(typeof(Sighting))!;
    var table = StoreObjectIdentifier.Table("Sightings", null);
    var code = entity.FindComplexProperty(nameof(Sighting.Location))!.ComplexType.FindProperty(nameof(Location.Code))!;
    if (code.DeclaringType is not IComplexType || code.GetColumnName(table) != "site_code") throw new Exception("Wrong nested column.");
    if (entity.FindPrimaryKey()!.Properties.Single().Name != "Id") throw new Exception("Wrong key.");
    if (string.Join(",", entity.GetIndexes().Single().Properties.Select(p => p.Name)) != "Code,Sequence") throw new Exception("Wrong index order.");
    if (string.Join(",", entity.GetKeys().Single(k => !k.IsPrimaryKey()).Properties.Select(p => p.Name)) != "Code,Id") throw new Exception("Wrong alternate key order");
    var mapping = code.GetTableColumnMappings().Single();
    if (mapping.Column.Name != "site_code" || mapping.Property != code) throw new Exception("Wrong store mapping");
    foreach (var property in entity.GetProperties().Concat(entity.GetComplexProperties().SelectMany(p => p.ComplexType.GetProperties())))
        Console.WriteLine($"{property.DeclaringType.Name}.{property.Name} => {property.GetColumnName(table)}");
    db.Add(new Sighting { Id = 7, Sequence = 2, Location = new() { Code = "CUR-4" } });
    db.SaveChanges();
    db.Database.ExecuteSqlRaw("CREATE VIEW SeenSites AS SELECT site_code FROM Sightings");
}
using (var db = new Marsh(options))
{
    if (db.Set<Sighting>().Single(s => s.Location.Code == "CUR-4").Id != 7) throw new Exception("Lookup failed.");
    var view = db.Model.FindEntityType(typeof(SeenSite))!;
    if (view.FindProperty("Code")!.GetColumnName(StoreObjectIdentifier.View("SeenSites", null)) != "site_code") throw new Exception("Wrong view mapping.");
    if (view.FindProperty("Code")!.GetViewColumnMappings().Single().Column.Name != "site_code") throw new Exception("Wrong view column mapping");
    if (db.Set<SeenSite>().Single().Code != "CUR-4") throw new Exception("View lookup failed.");
}
Console.WriteLine("PASS entity/complex/table/view mappings, key/index order and executed lookups");
sealed class Marsh(DbContextOptions<Marsh> options) : DbContext(options)
{
    public static bool InvalidComplexIndex { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Sighting>();
        entity.ToTable("Sightings");
        entity.ComplexProperty(x => x.Location).Property(x => x.Code).HasColumnName("site_code");
        if (InvalidComplexIndex) entity.HasIndex(x => new { x.Location, x.Sequence });
        else entity.HasIndex(x => new { x.Location.Code, x.Sequence });
        entity.HasAlternateKey(x => new { x.Location.Code, x.Id });

        modelBuilder.Entity<SeenSite>().HasNoKey().ToView("SeenSites").Property(x => x.Code).HasColumnName("site_code");
    }
}
sealed class Sighting { public int Id { get; set; } public int Sequence { get; set; } public Location Location { get; set; } = new(); }
[ComplexType]
sealed class Location { public string Code { get; set; } = ""; }
sealed class SeenSite { public string Code { get; set; } = ""; }
// endsnippet:complete






