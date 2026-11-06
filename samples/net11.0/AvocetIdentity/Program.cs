// snippet:complete
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using PlaybookCosmos;
string Required(string key) => Environment.GetEnvironmentVariable(key) ?? throw new Exception(key);
var endpoint = Required("PLAYBOOK_COSMOS_ENDPOINT");
var database = Required("PLAYBOOK_COSMOS_DATABASE");
using var http = EmulatorTransport.CreateClient(endpoint, Required("PLAYBOOK_COSMOS_CERT_SHA256"));
var options = new DbContextOptionsBuilder<AvocetContext>().UseCosmos(endpoint, Required("PLAYBOOK_COSMOS_KEY"), database,
    c => c.ConnectionMode(ConnectionMode.Gateway).LimitToEndpoint().HttpClientFactory(() => http)).Options;
await using var db = new AvocetContext(options);
await db.Database.EnsureCreatedAsync();
db.Add(new Logbook { Id = "a", Coast = "west", Detail = new Detail { Note = "ringed" } });
db.Add(new Logbook { Id = "b", Coast = "west", Detail = new Detail { Note = "unringed" } });
await db.SaveChangesAsync();
db.ChangeTracker.Clear();
using var sdkHttp = EmulatorTransport.CreateClient(endpoint, Required("PLAYBOOK_COSMOS_CERT_SHA256"));
using var sdk = new CosmosClient(endpoint, Required("PLAYBOOK_COSMOS_KEY"), new CosmosClientOptions {
    ConnectionMode = ConnectionMode.Gateway, LimitToEndpoint = true, HttpClientFactory = () => sdkHttp });
foreach (var (id, expectedNote) in new[] { ("a", "ringed"), ("b", "unringed") }) {
    using var stored = await sdk.GetContainer(database, "logbooks").ReadItemStreamAsync(id, new PartitionKey("west"));
    stored.EnsureSuccessStatusCode();
    using var json = await System.Text.Json.JsonDocument.ParseAsync(stored.Content);
    if (json.RootElement.GetProperty("Detail").GetProperty("Note").GetString() != expectedNote)
        throw new Exception("Stored document control failed for " + id);
}
Console.WriteLine("PASS both stored documents have distinct expected owned values");
var omittedRejected = false;
try {
    _ = await db.Set<Logbook>().AsNoTrackingWithIdentityResolution().Select(x => x.Detail).ToListAsync();
} catch (InvalidOperationException ex) {
    if (!ex.Message.Contains("primary key properties", StringComparison.Ordinal) ||
        !ex.Message.Contains("Id", StringComparison.Ordinal) ||
        !ex.Message.Contains("Coast", StringComparison.Ordinal))
        throw;
    omittedRejected = true;
    Console.WriteLine("OMITTED rejected: " + ex.Message);
}
if (!omittedRejected) throw new Exception("EF11 omitted-key projection unexpectedly succeeded");
var keyed = await db.Set<Logbook>().AsNoTrackingWithIdentityResolution()
    .Select(x => new { x.Id, x.Coast, First = x.Detail, Second = x.Detail }).ToListAsync();
foreach (var row in keyed) Console.WriteLine($"KEYED {row.Id} {row.Coast} first={row.First?.Note ?? "NULL"} second={row.Second?.Note ?? "NULL"}");
if (keyed.Count != 2 || keyed.Any(x => !ReferenceEquals(x.First, x.Second)) ||
    keyed.Single(x => x.Id == "a").First.Note != "ringed" ||
    keyed.Single(x => x.Id == "b").First.Note != "unringed" || ReferenceEquals(keyed[0].First, keyed[1].First))
    throw new Exception("Keyed projection identity/value failed");
Console.WriteLine("PASS keyed projection, duplicate identity and persisted values");
sealed class AvocetContext(DbContextOptions<AvocetContext> options) : DbContext(options) {
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        var e = modelBuilder.Entity<Logbook>();
        e.ToContainer("logbooks"); e.HasKey(x => new { x.Id, x.Coast }); e.HasPartitionKey(x => x.Coast);
        e.HasNoDiscriminator(); e.OwnsOne(x => x.Detail);
    }
}
sealed class Logbook { public string Id { get; set; } = ""; public string Coast { get; set; } = ""; public Detail Detail { get; set; } = new(); }
sealed class Detail { public string Note { get; set; } = ""; }
// endsnippet:complete
