// snippet:complete
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

var cs = Environment.GetEnvironmentVariable("PLAYBOOK_SQL_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Use the isolated SQL Server route.");
await using var connection = new SqlConnection(cs);
await connection.OpenAsync();
if (!connection.Database.StartsWith("playbook_", StringComparison.Ordinal))
    throw new InvalidOperationException("Use a disposable playbook_ database.");
Console.WriteLine($"Runtime={RuntimeInformation.FrameworkDescription}; EF={typeof(DbContext).Assembly.GetName().Version}; SQL={connection.ServerVersion}");
var options = new DbContextOptionsBuilder<FenContext>().UseSqlServer(cs, s => s.UseCompatibilityLevel(170)).Options;
await using (var db = new FenContext(options))
{
    await db.Database.EnsureCreatedAsync();
    var ddl = db.Database.GenerateCreateScript();
    Console.WriteLine(ddl);
    db.Add(new PlankLoan { Id = 11, Location = new() { Bay = "West", Shelf = 4 }, Days = 7 });
    db.Add(new ReedLoan { Id = 21, Location = new() { Bay = "East", Shelf = 5 }, Days = 8 });
    db.Add(new MossLoan { Id = 31, Location = new() { Bay = "North", Shelf = 6 }, Days = 9 });
    db.Add(new SedgeLoan { Id = 41, Location = new() { Bay = "South", Shelf = 7 }, Days = 10 });
    db.Add(new Label { CodeValue = "FN-42", Zone = new() { Bay = "Marsh", Shelf = 8 }, SlotBay = "Quay" });
    await db.SaveChangesAsync();
}
await using (var schema = connection.CreateCommand())
{
    schema.CommandText = "SELECT COUNT(*) FROM sys.indexes WHERE object_id = OBJECT_ID(N'Label') AND name IN (N'PK_Label', N'IX_Label_SlotBay', N'IX_Label_ZoneBay')";
    if (Convert.ToInt32(await schema.ExecuteScalarAsync()) != 3) throw new Exception("Missing physical key or lookup index");
}
Console.WriteLine("PASS: physical scalar primary key, scalar index and computed JSON lookup index");
await using (var db = new FenContext(options))
{
    await CheckPlank(db, 11, "West");
    await CheckReed(db, 21, "East");
    await CheckMoss(db, 31, "North");
    await CheckSedge(db, 41, "South");
    var label = await db.Set<Label>().SingleOrDefaultAsync(x => x.CodeValue == "FN-42") ?? throw new Exception("Nested identity failed");
    if ((await db.Set<Label>().SingleAsync(x => x.Zone.Bay == "Marsh" )).CodeValue != label.CodeValue) throw new Exception("Lookup failed");
    if (!ReferenceEquals(label, await db.Set<Label>().SingleAsync(x => x.CodeValue == "FN-42"))) throw new Exception("Identity resolution failed");
    if ((await db.Set<Label>().SingleAsync(x => x.SlotBay == "Quay" )).CodeValue != "FN-42") throw new Exception("Scalar lookup failed");
    label.Zone.Shelf = 9;
    await db.SaveChangesAsync();
}
await using (var db = new FenContext(options))
{
    if ((await db.Set<Label>().SingleAsync()).Zone.Shelf != 9) throw new Exception("JSON update failed");
    var duplicate = new Label { CodeValue = "FN-42", Zone = new() { Bay = "Other", Shelf = 1 }, SlotBay = "Quay" };
    db.ChangeTracker.Clear();
    db.Add(duplicate);
    try { await db.SaveChangesAsync(); throw new Exception("Duplicate accepted"); }
    catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2627 or 2601 }) { }
}
Console.WriteLine("PASS: TPT/TPC converted JSON insert-read-update; scalar identity, complex JSON lookup and duplicate rejection");

static async Task CheckPlank(FenContext db, int id, string bay)
{
    var item = await db.Set<PlankLoan>().SingleAsync(x => x.Id == id);
    if (item.Location.Bay != bay) throw new Exception("Inheritance round-trip failed");
    item.Location.Shelf += 10;
    await db.SaveChangesAsync();
    db.ChangeTracker.Clear();
    if ((await db.Set<PlankLoan>().SingleAsync(x => x.Id == id)).Location.Shelf < 10) throw new Exception("Update lost");
}

static async Task CheckReed(FenContext db, int id, string bay)
{
    var item = await db.Set<ReedLoan>().SingleAsync(x => x.Id == id);
    if (item.Location.Bay != bay) throw new Exception("Inheritance round-trip failed");
    item.Location.Shelf += 10;
    await db.SaveChangesAsync();
    db.ChangeTracker.Clear();
    if ((await db.Set<ReedLoan>().SingleAsync(x => x.Id == id)).Location.Shelf < 10) throw new Exception("Update lost");
}

static async Task CheckMoss(FenContext db, int id, string bay)
{
    var item = await db.Set<MossLoan>().SingleAsync(x => x.Id == id);
    if (item.Location.Bay != bay) throw new Exception("Inheritance round-trip failed");
    item.Location.Shelf += 10;
    await db.SaveChangesAsync();
    db.ChangeTracker.Clear();
    if ((await db.Set<MossLoan>().SingleAsync(x => x.Id == id)).Location.Shelf < 10) throw new Exception("Update lost");
}

static async Task CheckSedge(FenContext db, int id, string bay)
{
    var item = await db.Set<SedgeLoan>().SingleAsync(x => x.Id == id);
    if (item.Location.Bay != bay) throw new Exception("Inheritance round-trip failed");
    item.Location.Shelf += 10;
    await db.SaveChangesAsync();
    db.ChangeTracker.Clear();
    if ((await db.Set<SedgeLoan>().SingleAsync(x => x.Id == id)).Location.Shelf < 10) throw new Exception("Update lost");
}

internal sealed class FenContext(DbContextOptions<FenContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigurePlank(modelBuilder, "Plank", false);
        ConfigureReed(modelBuilder, "Reed", false);
        ConfigureMoss(modelBuilder, "Moss", true);
        ConfigureSedge(modelBuilder, "Sedge", true);
        var label = modelBuilder.Entity<Label>();
        label.Property(x => x.CodeValue).HasMaxLength(24);
        label.ComplexProperty(x => x.Zone, b => b.ToJson());
        label.Property(x => x.SlotBay).HasMaxLength(24);
        label.HasKey(x => x.CodeValue);
        label.HasIndex(x => x.SlotBay);
        label.Property<string>("ZoneBay").HasComputedColumnSql("CAST(JSON_VALUE([Zone], '$.Bay') AS nvarchar(24))", stored: true);
        label.HasIndex("ZoneBay");
    }
    static void ConfigurePlank(ModelBuilder model, string prefix, bool tpc)
    {
        var item = model.Entity<PlankItem>();
        if (tpc) item.UseTpcMappingStrategy(); else { item.UseTptMappingStrategy(); item.ToTable(prefix + "Items"); }
        item.Property(x => x.Id).ValueGeneratedNever();
        var location = item.Property(x => x.Location)
            .HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Location>(v, (JsonSerializerOptions?)null)!);
        location.Metadata.SetValueComparer(new ValueComparer<Location>(
            (a, b) => a!.Bay == b!.Bay && a.Shelf == b.Shelf,
            v => HashCode.Combine(v.Bay, v.Shelf),
            v => new Location { Bay = v.Bay, Shelf = v.Shelf }));
        model.Entity<PlankLoan>().ToTable(prefix + "Loans");
        model.Entity<PlankDeposit>().ToTable(prefix + "Deposits");
    }
    static void ConfigureReed(ModelBuilder model, string prefix, bool tpc)
    {
        var item = model.Entity<ReedItem>();
        if (tpc) item.UseTpcMappingStrategy(); else { item.UseTptMappingStrategy(); item.ToTable(prefix + "Items"); }
        item.Property(x => x.Id).ValueGeneratedNever();
        var location = item.Property(x => x.Location)
            .HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Location>(v, (JsonSerializerOptions?)null)!);
        location.Metadata.SetValueComparer(new ValueComparer<Location>(
            (a, b) => a!.Bay == b!.Bay && a.Shelf == b.Shelf,
            v => HashCode.Combine(v.Bay, v.Shelf),
            v => new Location { Bay = v.Bay, Shelf = v.Shelf }));
        model.Entity<ReedLoan>().ToTable(prefix + "Loans");
        model.Entity<ReedDeposit>().ToTable(prefix + "Deposits");
    }
    static void ConfigureMoss(ModelBuilder model, string prefix, bool tpc)
    {
        var item = model.Entity<MossItem>();
        if (tpc) item.UseTpcMappingStrategy(); else { item.UseTptMappingStrategy(); item.ToTable(prefix + "Items"); }
        item.Property(x => x.Id).ValueGeneratedNever();
        var location = item.Property(x => x.Location)
            .HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Location>(v, (JsonSerializerOptions?)null)!);
        location.Metadata.SetValueComparer(new ValueComparer<Location>(
            (a, b) => a!.Bay == b!.Bay && a.Shelf == b.Shelf,
            v => HashCode.Combine(v.Bay, v.Shelf),
            v => new Location { Bay = v.Bay, Shelf = v.Shelf }));
        model.Entity<MossLoan>().ToTable(prefix + "Loans");
        model.Entity<MossDeposit>().ToTable(prefix + "Deposits");
    }
    static void ConfigureSedge(ModelBuilder model, string prefix, bool tpc)
    {
        var item = model.Entity<SedgeItem>();
        if (tpc) item.UseTpcMappingStrategy(); else { item.UseTptMappingStrategy(); item.ToTable(prefix + "Items"); }
        item.Property(x => x.Id).ValueGeneratedNever();
        var location = model.Entity<SedgeLoan>().Property(x => x.Location)
            .HasConversion(v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Location>(v, (JsonSerializerOptions?)null)!);
        location.Metadata.SetValueComparer(new ValueComparer<Location>(
            (a, b) => a!.Bay == b!.Bay && a.Shelf == b.Shelf,
            v => HashCode.Combine(v.Bay, v.Shelf),
            v => new Location { Bay = v.Bay, Shelf = v.Shelf }));
        model.Entity<SedgeLoan>().ToTable(prefix + "Loans");
        model.Entity<SedgeDeposit>().ToTable(prefix + "Deposits");
    }
}
internal abstract class PlankItem
{
    public int Id { get; set; }
    public Location Location { get; set; } = new();
}
internal sealed class PlankLoan : PlankItem { public int Days { get; set; } }
internal sealed class PlankDeposit : PlankItem { public int Quantity { get; set; } }
internal abstract class ReedItem
{
    public int Id { get; set; }
    public Location Location { get; set; } = new();
}
internal sealed class ReedLoan : ReedItem { public int Days { get; set; } }
internal sealed class ReedDeposit : ReedItem { public int Quantity { get; set; } }
internal abstract class MossItem
{
    public int Id { get; set; }
    public Location Location { get; set; } = new();
}
internal sealed class MossLoan : MossItem { public int Days { get; set; } }
internal sealed class MossDeposit : MossItem { public int Quantity { get; set; } }
internal abstract class SedgeItem
{
    public int Id { get; set; }
}
internal sealed class SedgeLoan : SedgeItem { public int Days { get; set; } public Location Location { get; set; } = new(); }
internal sealed class SedgeDeposit : SedgeItem { public int Quantity { get; set; } }
internal sealed class Location { public string Bay { get; set; } = ""; public int Shelf { get; set; } }
internal sealed class Label { public string CodeValue { get; set; } = ""; public Location Zone { get; set; } = new(); public string SlotBay { get; set; } = ""; }
// endsnippet:complete
