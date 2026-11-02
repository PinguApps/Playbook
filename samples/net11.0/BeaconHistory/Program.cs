// snippet:complete
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var connection = Environment.GetEnvironmentVariable("PLAYBOOK_SQL_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Use the isolated SQL authoring runner.");
await using var db = new BeaconContext(connection);
await db.Database.EnsureCreatedAsync();
var stamp = new DateTimeOffset(2026, 4, 8, 23, 42, 19, TimeSpan.FromHours(2));
db.Beacons.Add(new Beacon { Label = "North shoal", RecordedAt = stamp });
await db.SaveChangesAsync();
db.ChangeTracker.Clear();
var first = await db.Beacons.SingleAsync();
Require(first.ValidFrom > new DateTime(2026, 1, 1) && first.ValidTo.Year == 9999, "Database-generated bounds");
await Task.Delay(100);
first.Label = "Outer shoal";
await db.SaveChangesAsync();
db.ChangeTracker.Clear();
var versions = await db.Beacons.TemporalAll().OrderBy(b => b.ValidFrom).ToListAsync();
Require(versions.Count == 2 && versions[0].Label == "North shoal" && versions[1].Label == "Outer shoal", "History round trip");
Require(versions[0].ValidTo == versions[1].ValidFrom && versions[0].ValidFrom < versions[0].ValidTo, "Adjacent periods");
try
{
    await db.Database.ExecuteSqlRawAsync("UPDATE dbo.Beacons SET ValidFrom = '20000101' WHERE Id = 1");
    throw new InvalidOperationException("Period update was accepted");
}
catch (SqlException e) when (e.Number == 13537) { }

var offset = TimeSpan.FromHours(-4);
var query = db.Beacons.Select(b => new
{
    Wall = b.RecordedAt.DateTime,
    Utc = b.RecordedAt.UtcDateTime,
    Local = b.RecordedAt.LocalDateTime,
    Minutes = b.RecordedAt.Offset.TotalMinutes,
    Shifted = b.RecordedAt.ToOffset(offset),
    Constructed = EF.Functions.DateTrunc("second", new DateTimeOffset(b.RecordedAt.DateTime, offset)),
    UtcConstructed = EF.Functions.DateTrunc("second", new DateTimeOffset(b.RecordedAt.DateTime)),
    Bucket = EF.Functions.DateTrunc("hour", b.RecordedAt)
});
var sql = query.ToQueryString();
foreach (var token in new[] { "DATETRUNC", "SWITCHOFFSET", "TODATETIMEOFFSET", "DATEPART" })
    Require(sql.Contains(token, StringComparison.OrdinalIgnoreCase), "Server translation " + token + " SQL: " + sql);
var value = await query.SingleAsync();
Require(value.Wall == stamp.DateTime && value.Utc == stamp.UtcDateTime && value.Minutes == 120, "Offset members");
Require(value.Shifted.EqualsExact(stamp.ToOffset(offset)), "Offset conversion preserves instant");
Require(value.Constructed.EqualsExact(new DateTimeOffset(stamp.DateTime, offset)), "Explicit constructor");
Require(value.Bucket.EqualsExact(new DateTimeOffset(2026, 4, 8, 23, 0, 0, stamp.Offset)), "Hour bucket");
var serverMinutes = await db.Database.SqlQuery<int>($"SELECT DATEPART(TZOFFSET, SYSDATETIMEOFFSET()) AS Value").SingleAsync();
Require(value.Local == stamp.ToOffset(TimeSpan.FromMinutes(serverMinutes)).DateTime, "Server-local conversion");

Require(value.UtcConstructed.EqualsExact(new DateTimeOffset(stamp.DateTime, TimeSpan.Zero)), "One-argument SQL constructor uses UTC");
if (TimeZoneInfo.Local.GetUtcOffset(stamp).TotalMinutes != serverMinutes)
    Require(value.Local != stamp.LocalDateTime, "Server local differs from client local");
var groups = await db.Beacons.GroupBy(b => EF.Functions.DateTrunc("hour", b.RecordedAt)).Select(g => g.Count()).ToListAsync();
Require(groups.Count == 1 && groups[0] == 1, "Server grouping");
Console.WriteLine($"PASS temporal generation/history/update rejection and server time translation; server offset {serverMinutes}; client offset {TimeZoneInfo.Local.GetUtcOffset(stamp).TotalMinutes}; runtime {Environment.Version}");

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

sealed class BeaconContext(string connection) : DbContext
{
    public DbSet<Beacon> Beacons => Set<Beacon>();
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer(connection, s => s.UseCompatibilityLevel(170));
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<Beacon>().ToTable("Beacons", table => table.IsTemporal(t =>
    {
        t.HasPeriodStart(nameof(Beacon.ValidFrom));
        t.HasPeriodEnd(nameof(Beacon.ValidTo));
    }));
}

sealed class Beacon
{
    public int Id { get; set; }
    public string Label { get; set; } = "";
    public DateTimeOffset RecordedAt { get; set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime ValidTo { get; private set; }
}
// endsnippet:complete
