// snippet:complete
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure;
using PlaybookCosmos;

string Required(string name) => Environment.GetEnvironmentVariable(name) ?? throw new InvalidOperationException(name);
var endpoint = Required("PLAYBOOK_COSMOS_ENDPOINT");
var key = Required("PLAYBOOK_COSMOS_KEY");
var database = Required("PLAYBOOK_COSMOS_DATABASE");
var certificate = Required("PLAYBOOK_COSMOS_CERT_SHA256");
using var http = EmulatorTransport.CreateClient(endpoint, certificate);
DbContextOptions<ReefContext> Options(HttpClient transport, bool bulk = false) => new DbContextOptionsBuilder<ReefContext>()
    .UseCosmos(endpoint, key, database, c => c.ConnectionMode(ConnectionMode.Gateway).LimitToEndpoint()
        .HttpClientFactory(() => transport)
        .SessionTokenManagementMode(bulk ? SessionTokenManagementMode.FullyAutomatic : SessionTokenManagementMode.Manual).BulkExecutionAllowed(bulk)).Options;
await using var writer = new ReefContext(Options(http, bulk: true));
await writer.Database.EnsureCreatedAsync();
writer.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
foreach (var n in Enumerable.Range(0, 8)) writer.Add(new Observation { Id = "bulk-" + n, Owner = "reef-team", Value = n });
await writer.SaveChangesAsync();
writer.ChangeTracker.Clear();
if (await writer.Set<Observation>().CountAsync() != 8) throw new InvalidOperationException("Bulk persistence failed.");
writer.Add(new Observation { Id = "bulk-0", Owner = "reef-team", Value = 99 });
var conflict = false;
try { await writer.SaveChangesAsync(); } catch (DbUpdateException) { conflict = true; }
if (!conflict) throw new InvalidOperationException("Expected duplicate failure.");
writer.ChangeTracker.Clear();
if ((await writer.Set<Observation>().SingleAsync(x => x.Id == "bulk-0")).Value != 0) throw new InvalidOperationException("Duplicate changed stored data.");
Console.WriteLine("PASS bulk opt-in: eight persisted, duplicate rejected; no throughput or atomicity claim");

using var sessionHttp = EmulatorTransport.CreateClient(endpoint, certificate);
await using var sessionWriter = new ReefContext(Options(sessionHttp));
sessionWriter.Add(new Observation { Id = "session-recent", Owner = "reef-team", Value = 7 });
await sessionWriter.SaveChangesAsync();
// The authenticated caller determines the owner key. Never accept another caller's token map.
var ownerTokens = new Dictionary<string, IReadOnlyDictionary<string, string?>>(StringComparer.Ordinal)
    { ["reef-team"] = new Dictionary<string, string?>(sessionWriter.Database.GetSessionTokens()) };
if (ownerTokens["reef-team"].Count == 0) throw new InvalidOperationException("Missing actual tokens.");
using var readHttp = EmulatorTransport.CreateClient(endpoint, certificate);
await using var reader = new ReefContext(Options(readHttp));
if (ReferenceEquals(sessionWriter.Database.GetCosmosClient(), reader.Database.GetCosmosClient())) throw new InvalidOperationException("Clients must be independent.");
reader.Database.UseSessionTokens(ownerTokens["reef-team"]);
var recent = await reader.Set<Observation>().WithPartitionKey("reef-team").SingleAsync(x => x.Id == "session-recent");
if (recent.Value != 7 || ownerTokens.ContainsKey("other-team")) throw new InvalidOperationException("Owner-scoped handoff failed.");
Console.WriteLine("PASS owner-scoped manual token handoff across independent EF clients");

using var sdkHttp = EmulatorTransport.CreateClient(endpoint, certificate);
using var sdk = new CosmosClient(endpoint, key, new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway, LimitToEndpoint = true, HttpClientFactory = () => sdkHttp });
var container = sdk.GetContainer(database, "observations");
async Task Seed(string id) {
    using var body = new MemoryStream(Encoding.UTF8.GetBytes(new JsonObject { ["id"] = id, ["Owner"] = "reef-team", ["Value"] = 1, ["partnerNote"] = "retain" }.ToJsonString()));
    using var response = await container.CreateItemStreamAsync(body, new PartitionKey("reef-team"));
    response.EnsureSuccessStatusCode();
}
async Task<JsonObject> Raw(string id) {
    using var response = await container.ReadItemStreamAsync(id, new PartitionKey("reef-team"));
    response.EnsureSuccessStatusCode();
    return (await JsonNode.ParseAsync(response.Content))!.AsObject();
}
await Seed("migration");
using var migrationHttp = EmulatorTransport.CreateClient(endpoint, certificate);
await using (var db = new ReefContext(Options(migrationHttp))) {
    var item = await db.Set<Observation>().WithPartitionKey("reef-team").SingleAsync(x => x.Id == "migration");
    if (db.Model.FindEntityType(typeof(Observation))!.FindProperty("__jObject") is not null) throw new InvalidOperationException("Removed shadow property returned.");
    item.Value = 2;
    await db.SaveChangesAsync();
}
if ((await Raw("migration")).ContainsKey("partnerNote")) throw new InvalidOperationException("Expected EF11 unmapped-field removal.");
Console.WriteLine("PASS EF11 removes unmapped fields on replacement save");

await Seed("raw-owned");
// Own the complete document: read all fields and use the ETag to reject stale replacement.
var raw = await Raw("raw-owned");
var etag = raw["_etag"]!.GetValue<string>();
raw["Value"] = 3;
using var payload = new MemoryStream(Encoding.UTF8.GetBytes(raw.ToJsonString()));
using var replaced = await container.ReplaceItemStreamAsync(payload, "raw-owned", new PartitionKey("reef-team"), new ItemRequestOptions { IfMatchEtag = etag });
replaced.EnsureSuccessStatusCode();
var roundTrip = await Raw("raw-owned");
if (roundTrip["partnerNote"]!.GetValue<string>() != "retain" || roundTrip["Value"]!.GetValue<int>() != 3) throw new InvalidOperationException("Raw ownership lost data.");
using var staleBody = new MemoryStream(Encoding.UTF8.GetBytes(raw.ToJsonString()));
using var stale = await container.ReplaceItemStreamAsync(staleBody, "raw-owned", new PartitionKey("reef-team"), new ItemRequestOptions { IfMatchEtag = etag });
if (stale.StatusCode != HttpStatusCode.PreconditionFailed) throw new InvalidOperationException("Stale replacement should fail.");
Console.WriteLine("PASS direct JSON ownership preserves partner field and rejects stale ETag");

sealed class ReefContext(DbContextOptions<ReefContext> options) : DbContext(options) {
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        var entity = modelBuilder.Entity<Observation>();
        entity.ToContainer("observations"); entity.HasKey(x => x.Id); entity.HasPartitionKey(x => x.Owner); entity.HasNoDiscriminator();
    }
}
sealed class Observation { public required string Id { get; set; } public required string Owner { get; set; } public int Value { get; set; } }
// endsnippet:complete
