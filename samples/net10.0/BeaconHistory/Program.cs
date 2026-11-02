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
var bounds = await db.Beacons.Select(b => new { Start = EF.Property<DateTime>(b, "ValidFrom"), End = EF.Property<DateTime>(b, "ValidTo") }).SingleAsync();
Require(bounds.Start > new DateTime(2026, 1, 1) && bounds.End.Year == 9999, "Database-generated shadow bounds");
await Task.Delay(100);
first.Label = "Outer shoal";
await db.SaveChangesAsync();
db.ChangeTracker.Clear();
var versions = await db.Beacons.TemporalAll().OrderBy(b => EF.Property<DateTime>(b, "ValidFrom"))
    .Select(b => new { b.Label, Start = EF.Property<DateTime>(b, "ValidFrom"), End = EF.Property<DateTime>(b, "ValidTo") }).ToListAsync();
Require(versions.Count == 2 && versions[0].Label == "North shoal" && versions[1].Label == "Outer shoal", "History round trip");
Require(versions[0].End == versions[1].Start && versions[0].Start < versions[0].End, "Adjacent periods");
try
{
    await db.Database.ExecuteSqlRawAsync("UPDATE dbo.Beacons SET ValidFrom = '20000101' WHERE Id = 1");
    throw new InvalidOperationException("Period update was accepted");
}
catch (SqlException e) when (e.Number == 13537) { }

// EF10 can keep these expressions on the server with a parameterized SQL query.
var offset = "-04:00";
var value = await db.Database.SqlQuery<TimeReport>($"""
    SELECT CONVERT(datetime2, RecordedAt) AS Wall,
           CONVERT(datetime2, SWITCHOFFSET(RecordedAt, '+00:00')) AS Utc,
           SWITCHOFFSET(RecordedAt, {offset}) AS Shifted,
           DATETRUNC(hour, RecordedAt) AS Bucket
    FROM dbo.Beacons WHERE Id = {1}
    """).SingleAsync();
Require(value.Wall == stamp.DateTime && value.Utc == stamp.UtcDateTime, "SQL wall/UTC conversion");
Require(value.Shifted.EqualsExact(stamp.ToOffset(TimeSpan.FromHours(-4))), "SQL offset conversion");
Require(value.Bucket.EqualsExact(new DateTimeOffset(2026, 4, 8, 23, 0, 0, stamp.Offset)), "SQL hour bucket");
Console.WriteLine($"PASS EF10 shadow temporal generation/history/update rejection and parameterized server time query; runtime {Environment.Version}");

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
        t.HasPeriodStart("ValidFrom");
        t.HasPeriodEnd("ValidTo");
    }));
}

sealed class Beacon
{
    public int Id { get; set; }
    public string Label { get; set; } = "";
    public DateTimeOffset RecordedAt { get; set; }
}
sealed class TimeReport
{
    public DateTime Wall { get; set; }
    public DateTime Utc { get; set; }
    public DateTimeOffset Shifted { get; set; }
    public DateTimeOffset Bucket { get; set; }
}
// endsnippet:complete
