// snippet:complete
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using PlaybookCosmos;
string Required(string key) => Environment.GetEnvironmentVariable(key) ?? throw new Exception(key);
var endpoint = Required("PLAYBOOK_COSMOS_ENDPOINT");
using var http = EmulatorTransport.CreateClient(endpoint, Required("PLAYBOOK_COSMOS_CERT_SHA256"));
var options = new DbContextOptionsBuilder<MarshContext>().UseCosmos(endpoint, Required("PLAYBOOK_COSMOS_KEY"), Required("PLAYBOOK_COSMOS_DATABASE"),
    c => c.ConnectionMode(ConnectionMode.Gateway).LimitToEndpoint().HttpClientFactory(() => http)).Options;
await using var db = new MarshContext(options);
await db.Database.EnsureCreatedAsync();
db.AddRange(new Reading { Id = "valid", Coast = "west", Text = "12.5", Depth = 12.5 },
    new Reading { Id = "null", Coast = "west", Text = null },
    new Reading { Id = "invalid", Coast = "west", Text = "mud" },
    new Reading { Id = "range", Coast = "west", Text = "1e100", Depth = 1e100 });
await db.SaveChangesAsync(); db.ChangeTracker.Clear();
var query = db.Set<Reading>().Where(x => x.Id == "valid")
    .Where(x => (double)(int)x.Depth + 2 > 13)
    .Select(x => "depth=" + MathF.Floor((float)x.Depth));
Console.WriteLine(query.ToQueryString());
var result = await query.ToListAsync();
if (result.Count != 1 || result[0] != "depth=12") throw new Exception("Composed server result differs");
Console.WriteLine("PASS COMPOSED " + string.Join(";", result));
var nullRows = await db.Set<Reading>().Where(x => x.Id == "null" && x.Text == null).Select(x => x.Text).ToListAsync();
if (nullRows.Count != 1 || nullRows[0] != null) throw new Exception("Null control failed");
var rangeRows = await db.Set<Reading>().Where(x => x.Id == "range").Select(x => "depth=" + Math.Abs((int)x.Depth)).ToListAsync();
Console.WriteLine("RANGE " + string.Join(";", rangeRows));
if (rangeRows.Count != 1 || !rangeRows[0].Contains("1E+100", StringComparison.OrdinalIgnoreCase)) throw new Exception("Expected service numeric range to survive no-op cast");
try {
    _ = await db.Set<Reading>().Where(x => Convert.ToDouble(x.Text) > 0).ToListAsync();
    throw new Exception("Unexpected string conversion support");
} catch (InvalidOperationException ex) when (ex.Message.Contains("could not be translated", StringComparison.Ordinal)) {
    Console.WriteLine("PASS System.Convert string conversion rejected before service execution");
}
Console.WriteLine("PASS null, invalid conversion and service numeric range controls");
sealed class MarshContext(DbContextOptions<MarshContext> options) : DbContext(options) {
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        var e = modelBuilder.Entity<Reading>(); e.ToContainer("readings"); e.HasKey(x => new { x.Id, x.Coast }); e.HasPartitionKey(x => x.Coast); e.HasNoDiscriminator();
    }
}
sealed class Reading { public string Id { get; set; } = ""; public string Coast { get; set; } = ""; public string? Text { get; set; } public double Depth { get; set; } }
// endsnippet:complete