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
try { Console.WriteLine(query.ToQueryString()); throw new Exception("Expected EF10 translation rejection"); } catch (InvalidOperationException ex) { Console.WriteLine("PASS EF10 rejects server conversion: " + ex.Message); }
var roots = await db.Set<Reading>().Where(x => x.Id == "valid").ToListAsync();
var result = roots.Where(x => (double)(int)x.Depth + 2 > 13).Select(x => "depth=" + MathF.Floor((float)x.Depth)).ToList();
if (result.Count != 1 || result[0] != "depth=12") throw new Exception("Client result differs");
Console.WriteLine("PASS CLIENT COMPOSED " + string.Join(";", result));
foreach (var id in new[] { "null", "invalid", "range" }) {
    try {
        var rows = await db.Set<Reading>().Where(x => x.Id == id).ToListAsync();
        var values = rows.Select(x => Convert.ToDouble(x.Text)).ToList();
        if (values.Count != 1 || values[0] != (id == "null" ? 0 : 1e100) || id == "invalid") throw new Exception("Client boundary control failed");
        Console.WriteLine("PASS " + id + " => " + string.Join(";", values));
    } catch (FormatException ex) when (id == "invalid") { Console.WriteLine("PASS invalid => " + ex.GetType().Name); }
}
sealed class MarshContext(DbContextOptions<MarshContext> options) : DbContext(options) {
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        var e = modelBuilder.Entity<Reading>(); e.ToContainer("readings"); e.HasKey(x => new { x.Id, x.Coast }); e.HasPartitionKey(x => x.Coast); e.HasNoDiscriminator();
    }
}
sealed class Reading { public string Id { get; set; } = ""; public string Coast { get; set; } = ""; public string? Text { get; set; } public double Depth { get; set; } }
// endsnippet:complete