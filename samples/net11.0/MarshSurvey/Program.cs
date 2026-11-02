// snippet:complete
using Microsoft.EntityFrameworkCore;

var connection = Environment.GetEnvironmentVariable("PLAYBOOK_SQL_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Use an isolated playbook_ database through the authoring route.");
await using var db = new SurveyContext(new DbContextOptionsBuilder<SurveyContext>().UseSqlServer(connection).Options);
if (!db.Database.GetDbConnection().Database.StartsWith("playbook_", StringComparison.Ordinal))
    throw new InvalidOperationException("Requires an exclusively owned disposable database.");
await db.Database.EnsureCreatedAsync();
await db.Database.OpenConnectionAsync();
var north = new Station { Id = 1, Name = "North reedbed" };
var south = new Station { Id = 2, Name = "South lagoon" };
db.AddRange(north, south);
db.AddRange(new Reading { Id = 11, Station = north, Score = 4 },
    new Reading { Id = 12, Station = north, Score = 9 },
    new Reading { Id = 13, Station = north, Score = 9 },
    new Reading { Id = 21, Station = south, Score = 7 });
db.Planned.AddRange(new Planned { Id = 1, Code = "A" }, new Planned { Id = 2, Code = "B" });
db.Observed.AddRange(new Observed { Id = 1, Code = "B" }, new Observed { Id = 2, Code = "C" });
await db.SaveChangesAsync();

// snippet:winner
var winners = db.Readings.GroupBy(r => r.StationId)
    .Select(g => g.OrderByDescending(r => r.Score).ThenBy(r => r.Id).First());
var selected = winners.Select(r => new { r.Id, r.Station.Name });
var actual = await selected.OrderBy(r => r.Id).ToArrayAsync();
// endsnippet:winner
Check(actual.Select(r => r.Id).SequenceEqual([12, 21]), "Winner IDs and deterministic ties");
Check(actual.Select(r => r.Name).SequenceEqual(["North reedbed", "South lagoon"]), "Navigation after group winner");
Console.WriteLine(selected.ToQueryString());

// snippet:reconciliation
var reconciliation = db.Planned.FullJoin(db.Observed, p => p.Code, o => o.Code,
    (p, o) => new { Planned = p == null ? null : p.Code, Observed = o == null ? null : o.Code });
var rows = await reconciliation.ToArrayAsync();
// endsnippet:reconciliation
Check(rows.Length == 3, "Full reconciliation count");
Check(rows.Any(r => r.Planned == "A" && r.Observed == null), "Left-only row");
Check(rows.Any(r => r.Planned == "B" && r.Observed == "B"), "Matched row");
Check(rows.Any(r => r.Planned == null && r.Observed == "C"), "Right-only row");
Console.WriteLine(reconciliation.ToQueryString());
Console.WriteLine($"PASS: real group winner/navigation and full reconciliation; runtime {Environment.Version}; EF {typeof(DbContext).Assembly.GetName().Version}; SQL {db.Database.GetDbConnection().ServerVersion}");

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
sealed class SurveyContext(DbContextOptions<SurveyContext> options) : DbContext(options)
{
    public DbSet<Reading> Readings => Set<Reading>();
    public DbSet<Planned> Planned => Set<Planned>();
    public DbSet<Observed> Observed => Set<Observed>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Station>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<Reading>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<Planned>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<Observed>().Property(x => x.Id).ValueGeneratedNever();
    }
}
sealed class Station { public int Id { get; set; } public required string Name { get; set; } }
sealed class Reading
{
    public int Id { get; set; }
    public int StationId { get; set; }
    public required Station Station { get; set; }
    public int Score { get; set; }
}
sealed class Planned { public int Id { get; set; } public required string Code { get; set; } }
sealed class Observed { public int Id { get; set; } public required string Code { get; set; } }
// endsnippet:complete
