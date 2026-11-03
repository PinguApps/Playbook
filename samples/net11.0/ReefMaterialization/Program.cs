// snippet:complete
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using PlaybookCosmos;

string Required(string name) => Environment.GetEnvironmentVariable(name) ?? throw new InvalidOperationException(name);
var endpoint = Required("PLAYBOOK_COSMOS_ENDPOINT");
var key = Required("PLAYBOOK_COSMOS_KEY");
var database = Required("PLAYBOOK_COSMOS_DATABASE");
using var http = EmulatorTransport.CreateClient(endpoint, Required("PLAYBOOK_COSMOS_CERT_SHA256"));
var options = new DbContextOptionsBuilder<SurveyContext>().UseCosmos(endpoint, key, database,
    c => c.ConnectionMode(ConnectionMode.Gateway).LimitToEndpoint().HttpClientFactory(() => http)).Options;
await using var db = new SurveyContext(options);
await db.Database.EnsureCreatedAsync();
// No CLR initializer: the provider must decide how the stored empty array materializes.
db.Add(new Survey { Id = "empty", Fleet = "reef", Stops = [] });
db.Add(new Survey { Id = "one", Fleet = "reef", Stops = [new Stop { Name = "west" }], Note = new Note { Text = "checked" } });
await db.SaveChangesAsync();
db.ChangeTracker.Clear();
var empty = await db.Set<Survey>().SingleAsync(x => x.Id == "empty");
var one = await db.Set<Survey>().SingleAsync(x => x.Id == "one");
Console.WriteLine($"OBSERVED empty owned list null={empty.Stops is null}; optional reference null={empty.Note is null}");
if (empty.Note is not null || one.Note?.Text != "checked" || one.Stops?.Single().Name != "west") throw new InvalidOperationException("Owned reference/list round-trip failed.");
if (empty.Stops is null || empty.Stops.Count != 0) throw new InvalidOperationException("EF11 must materialize an empty list.");
// Real server arithmetic produces fractional values despite integer operand types.
foreach (var numerator in new[] { 7, -7, 5, -5, 9, -9 }) {
    db.Add(new Survey { Id = "n" + numerator, Fleet = "reef", Numerator = numerator, Denominator = 4, Stops = [] });
}
await db.SaveChangesAsync();
var query = db.Set<Survey>().Where(x => x.Denominator == 4);
var ints = await query.Select(x => new { x.Id, Value = x.Numerator / x.Denominator }).ToListAsync();
var longs = await query.Select(x => new { x.Id, Value = (long)x.Numerator / x.Denominator }).ToListAsync();
var decimals = await query.Select(x => new { x.Id, Value = (decimal)x.Numerator / x.Denominator }).ToListAsync();
var rounded = await query.Select(x => new { x.Id, Value = (int)Math.Round((double)x.Numerator / x.Denominator) }).ToListAsync();
Console.WriteLine(query.Select(x => x.Numerator / x.Denominator).ToQueryString());
foreach (var row in ints) {
    var n = int.Parse(row.Id[1..], System.Globalization.CultureInfo.InvariantCulture);
    var fraction = n / 4.0;
    var expected = (int)fraction;
    var actualLong = longs.Single(x => x.Id == row.Id).Value;
    var actualDecimal = decimals.Single(x => x.Id == row.Id).Value;
    var actualRounded = rounded.Single(x => x.Id == row.Id).Value;
    Console.WriteLine($"OBSERVED {fraction}: int={row.Value}, long={actualLong}, decimal={actualDecimal}, explicitRound={actualRounded}");
    if (row.Value != expected || actualLong != expected || actualDecimal != (decimal)fraction || actualRounded != (int)Math.Round(fraction)) throw new InvalidOperationException($"Numeric policy changed: fraction={fraction}, expected={expected}, int={row.Value}, long={actualLong}, decimal={actualDecimal}, rounded={actualRounded}, roundExpected={Math.Round(fraction)}");
}
Console.WriteLine("PASS real owned collection/reference round-trips and int/long/decimal projections with explicit rounding");

sealed class SurveyContext(DbContextOptions<SurveyContext> options) : DbContext(options) {
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        var e = modelBuilder.Entity<Survey>();
        e.ToContainer("surveys"); e.HasKey(x => x.Id); e.HasPartitionKey(x => x.Fleet); e.HasNoDiscriminator();
        e.OwnsMany(x => x.Stops); e.OwnsOne(x => x.Note);
    }
}
sealed class Survey {
    public required string Id { get; set; }
    public required string Fleet { get; set; }
    public List<Stop> Stops { get; set; } = null!;
    public Note? Note { get; set; }
    public int Numerator { get; set; }
    public int Denominator { get; set; }
}
sealed class Stop { public required string Name { get; set; } }
sealed class Note { public required string Text { get; set; } }
// endsnippet:complete

