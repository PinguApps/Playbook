// snippet:complete
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PlaybookCosmos;

string Required(string name) => Environment.GetEnvironmentVariable(name) ?? throw new InvalidOperationException(name);
var endpoint = Required("PLAYBOOK_COSMOS_ENDPOINT");
var key = Required("PLAYBOOK_COSMOS_KEY");
var database = Required("PLAYBOOK_COSMOS_DATABASE");
var certificate = Required("PLAYBOOK_COSMOS_CERT_SHA256");
using var http = EmulatorTransport.CreateClient(endpoint, certificate);
DbContextOptions<ReefContext> Options(HttpClient transport) => new DbContextOptionsBuilder<ReefContext>()
    .UseCosmos(endpoint, key, database, c => c.ConnectionMode(ConnectionMode.Gateway).LimitToEndpoint()
        .HttpClientFactory(() => transport)
        ).Options;
await using var writer = new ReefContext(Options(http));
await writer.Database.EnsureCreatedAsync();
// EF10 has no bulk/session facade opt-ins. The direct SDK is a supported alternative.
using var bulkHttp = EmulatorTransport.CreateClient(endpoint, certificate);
using var bulk = new CosmosClient(endpoint, key, new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway, LimitToEndpoint = true, HttpClientFactory = () => bulkHttp, AllowBulkExecution = true, ConsistencyLevel = ConsistencyLevel.Session });
var bulkContainer = bulk.GetContainer(database, "observations");
var writes = await Task.WhenAll(Enumerable.Range(0, 8).Select(n => bulkContainer.CreateItemAsync(new { id = "bulk-" + n, Owner = "reef-team", Value = n }, new PartitionKey("reef-team"))));
var conflict = false;
try { await bulkContainer.CreateItemAsync(new { id = "bulk-0", Owner = "reef-team", Value = 99 }, new PartitionKey("reef-team")); }
catch (CosmosException e) when (e.StatusCode == HttpStatusCode.Conflict) { conflict = true; }
if (!conflict || await writer.Set<Observation>().CountAsync() != 8) throw new InvalidOperationException("Bulk outcome mismatch.");
if ((await writer.Set<Observation>().SingleAsync(x => x.Id == "bulk-0")).Value != 0) throw new InvalidOperationException("Duplicate changed stored data.");
Console.WriteLine("PASS SDK bulk opt-in: eight persisted, duplicate rejected; no throughput or atomicity claim");
var ownerTokens = new Dictionary<string, string>(StringComparer.Ordinal) { ["reef-team"] = writes[7].Headers.Session };
if (string.IsNullOrWhiteSpace(ownerTokens["reef-team"])) throw new InvalidOperationException("Missing actual token.");
using var readHttp = EmulatorTransport.CreateClient(endpoint, certificate);
using var readClient = new CosmosClient(endpoint, key, new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway, LimitToEndpoint = true, HttpClientFactory = () => readHttp, ConsistencyLevel = ConsistencyLevel.Session });
var readContainer = readClient.GetContainer(database, "observations");
using var read = await readContainer.ReadItemStreamAsync("bulk-7", new PartitionKey("reef-team"), new ItemRequestOptions { SessionToken = ownerTokens["reef-team"] });
read.EnsureSuccessStatusCode();
var recent = await JsonNode.ParseAsync(read.Content);
if (recent!["Value"]!.GetValue<int>() != 7 || ownerTokens.ContainsKey("other-team")) throw new InvalidOperationException("Owner-scoped handoff failed.");
Console.WriteLine("PASS owner-scoped SDK token handoff across independent clients");
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
    var shadow = db.Entry(item).Property<JObject>("__jObject").CurrentValue!;
    if (shadow["partnerNote"]!.Value<string>() != "retain") throw new InvalidOperationException("Raw shadow read failed.");
    item.Value = 2;
    await db.SaveChangesAsync();
}
if ((await Raw("migration"))["partnerNote"]!.GetValue<string>() != "retain") throw new InvalidOperationException("Expected EF10 preservation.");
Console.WriteLine("PASS EF10 shadow raw read and unmapped-field preservation");

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



