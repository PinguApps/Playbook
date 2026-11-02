// snippet:complete
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

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
    db.Add(new Label { Code = new() { Value = "FN-42" }, Zone = new() { Bay = "Marsh", Shelf = 8 }, Slot = new() { Bay = "Quay", Shelf = 2 } });
    await db.SaveChangesAsync();
}
await using (var schema = connection.CreateCommand())
{
    schema.CommandText = "SELECT COUNT(*) FROM sys.indexes WHERE object_id = OBJECT_ID(N'Label') AND name IN (N'PK_Label', N'IX_Label_Slot_Bay', N'IX_Label_Zone_Bay')";
    if (Convert.ToInt32(await schema.ExecuteScalarAsync()) != 3) throw new Exception("Missing physical key or lookup index");
}
Console.WriteLine("PASS: physical nested primary key, scalar index and SQL Server JSON path index");
await using (var db = new FenContext(options))
{
    await CheckPlank(db, 11, "West");
    await CheckReed(db, 21, "East");
    await CheckMoss(db, 31, "North");
    await CheckSedge(db, 41, "South");
    var label = await db.Set<Label>().SingleOrDefaultAsync(x => x.Code.Value == "FN-42") ?? throw new Exception("Nested identity failed");
    if ((await db.Set<Label>().SingleAsync(x => x.Zone.Bay == "Marsh")).Code.Value != label.Code.Value) throw new Exception("Lookup failed");
    if (!ReferenceEquals(label, await db.Set<Label>().SingleAsync(x => x.Code.Value == "FN-42"))) throw new Exception("Identity resolution failed");
    if ((await db.Set<Label>().SingleAsync(x => x.Slot.Bay == "Quay")).Code.Value != "FN-42") throw new Exception("Scalar lookup failed");
    label.Zone.Shelf = 9;
    await db.SaveChangesAsync();
}
await using (var db = new FenContext(options))
{
    if ((await db.Set<Label>().SingleAsync()).Zone.Shelf != 9) throw new Exception("JSON update failed");
    var duplicate = new Label { Code = new() { Value = "FN-42" }, Zone = new() { Bay = "Other", Shelf = 1 }, Slot = new() { Bay = "Quay", Shelf = 3 } };
    db.ChangeTracker.Clear();
    db.Add(duplicate);
    try { await db.SaveChangesAsync(); throw new Exception("Duplicate accepted"); }
    catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2627 or 2601 }) { }
}
Console.WriteLine("PASS: TPT/TPC scalar/JSON insert-read-update; nested identity, lookup and duplicate rejection");

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
        ConfigurePlank(modelBuilder, "Plank", false, false);
        ConfigureReed(modelBuilder, "Reed", false, true);
        ConfigureMoss(modelBuilder, "Moss", true, false);
        ConfigureSedge(modelBuilder, "Sedge", true, true);
        var label = modelBuilder.Entity<Label>();
        label.ComplexProperty(x => x.Code).Property(x => x.Value).HasMaxLength(24);
        label.ComplexProperty(x => x.Zone, b => b.ToJson());
        label.ComplexProperty(x => x.Slot).Property(x => x.Bay).HasMaxLength(24);
        label.HasKey(x => x.Code.Value);
        label.HasIndex(x => x.Slot.Bay);
        label.HasIndex(x => x.Zone.Bay);
    }
    static void ConfigurePlank(ModelBuilder model, string prefix, bool tpc, bool json)
    {
        var item = model.Entity<PlankItem>();
        if (tpc) item.UseTpcMappingStrategy(); else { item.UseTptMappingStrategy(); item.ToTable(prefix + "Items"); }
        item.Property(x => x.Id).ValueGeneratedNever();
        var location = item.ComplexProperty(x => x.Location);
        if (json) location.ToJson();
        model.Entity<PlankLoan>().ToTable(prefix + "Loans");
        model.Entity<PlankDeposit>().ToTable(prefix + "Deposits");
    }
    static void ConfigureReed(ModelBuilder model, string prefix, bool tpc, bool json)
    {
        var item = model.Entity<ReedItem>();
        if (tpc) item.UseTpcMappingStrategy(); else { item.UseTptMappingStrategy(); item.ToTable(prefix + "Items"); }
        item.Property(x => x.Id).ValueGeneratedNever();
        var location = item.ComplexProperty(x => x.Location);
        if (json) location.ToJson();
        model.Entity<ReedLoan>().ToTable(prefix + "Loans");
        model.Entity<ReedDeposit>().ToTable(prefix + "Deposits");
    }
    static void ConfigureMoss(ModelBuilder model, string prefix, bool tpc, bool json)
    {
        var item = model.Entity<MossItem>();
        if (tpc) item.UseTpcMappingStrategy(); else { item.UseTptMappingStrategy(); item.ToTable(prefix + "Items"); }
        item.Property(x => x.Id).ValueGeneratedNever();
        var location = item.ComplexProperty(x => x.Location);
        if (json) location.ToJson();
        model.Entity<MossLoan>().ToTable(prefix + "Loans");
        model.Entity<MossDeposit>().ToTable(prefix + "Deposits");
    }
    static void ConfigureSedge(ModelBuilder model, string prefix, bool tpc, bool json)
    {
        var item = model.Entity<SedgeItem>();
        if (tpc) item.UseTpcMappingStrategy(); else { item.UseTptMappingStrategy(); item.ToTable(prefix + "Items"); }
        item.Property(x => x.Id).ValueGeneratedNever();
        var location = model.Entity<SedgeLoan>().ComplexProperty(x => x.Location);
        if (json) location.ToJson();
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
internal sealed class Code { public string Value { get; set; } = ""; }
internal sealed class Label { public Code Code { get; set; } = new(); public Location Zone { get; set; } = new(); public Location Slot { get; set; } = new(); }
// endsnippet:complete
