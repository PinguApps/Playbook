using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlaybookCosmos;

var endpoint = Required("PLAYBOOK_COSMOS_ENDPOINT");
var key = Required("PLAYBOOK_COSMOS_KEY");
var name = Required("PLAYBOOK_COSMOS_DATABASE");
var certificate = Required("PLAYBOOK_COSMOS_CERT_SHA256");
using var http = EmulatorTransport.CreateClient(endpoint, certificate);
var options = new DbContextOptionsBuilder<ReadingContext>()
    .UseCosmos(endpoint, key, name, cosmos => cosmos.ConnectionMode(ConnectionMode.Gateway)
        .LimitToEndpoint().RequestTimeout(TimeSpan.FromSeconds(15)).HttpClientFactory(() => http))
    .Options;
await using var context = new ReadingContext(options);
await context.Database.EnsureCreatedAsync();
context.Add(new Reading { Id = "reef-1", Fleet = "north", Value = 17 });
await context.SaveChangesAsync();
context.ChangeTracker.Clear();
var saved = await context.Readings.WithPartitionKey("north").SingleAsync(x => x.Id == "reef-1");
if (saved.Value != 17) throw new InvalidOperationException("EF persisted value differed.");
saved.Value = 18;
await context.SaveChangesAsync();
context.ChangeTracker.Clear();
if (await context.Readings.WithPartitionKey("north").Where(x => x.Value == 18).CountAsync() != 1)
    throw new InvalidOperationException("EF server query/update failed.");
Console.WriteLine($"PASS: {context.Database.ProviderName} {typeof(DbContext).Assembly.GetName().Version}; actual async insert/query/update.");

using var directHttp = EmulatorTransport.CreateClient(endpoint, certificate);
using var client = Client(directHttp);
var container = client.GetContainer(name, "readings");
var read = await container.ReadItemAsync<Reading>("reef-1", new PartitionKey("north"));
var session = read.Headers.Session;
if (string.IsNullOrWhiteSpace(session)) throw new InvalidOperationException("No session token returned.");
using var peerHttp = EmulatorTransport.CreateClient(endpoint, certificate);
using var peer = Client(peerHttp);
var peerRead = await peer.GetContainer(name, "readings").ReadItemAsync<Reading>("reef-1", new PartitionKey("north"), new ItemRequestOptions { SessionToken = session });
if (peerRead.Resource.Value != 18) throw new InvalidOperationException("Independent client read failed.");
Console.WriteLine("PASS: actual session token handed to an independent client; local read-your-writes only, no distributed-consistency proof.");

using (var batch = await container.CreateTransactionalBatch(new PartitionKey("north"))
    .CreateItem(new Reading { Id = "rollback", Fleet = "north", Value = 99 })
    .CreateItem(new Reading { Id = "reef-1", Fleet = "north", Value = 99 }).ExecuteAsync())
{
    if (batch.StatusCode != HttpStatusCode.Conflict) throw new InvalidOperationException("Expected conflicting batch.");
}
try
{
    await container.ReadItemAsync<Reading>("rollback", new PartitionKey("north"));
    throw new InvalidOperationException("Conflicting batch partially persisted.");
}
catch (CosmosException exception) when (exception.StatusCode == HttpStatusCode.NotFound) { }
Console.WriteLine("PASS: real same-partition transactional batch conflict rolls back its preceding write.");

using (var bulkHttp = EmulatorTransport.CreateClient(endpoint, certificate))
using (var bulk = Client(bulkHttp, bulk: true))
{
    var target = bulk.GetContainer(name, "readings");
    await Task.WhenAll(Enumerable.Range(0, 8).Select(i => target.CreateItemAsync(
        new Reading { Id = $"bulk-{i}", Fleet = "bulk", Value = i }, new PartitionKey("bulk"))));
    using var query = target.GetItemQueryIterator<int>(new QueryDefinition("SELECT VALUE COUNT(1) FROM c WHERE c.fleet = @fleet").WithParameter("@fleet", "bulk"));
    if ((await query.ReadNextAsync()).Single() != 8) throw new InvalidOperationException("Bulk rows did not persist.");
}
Console.WriteLine("PASS: eight actual opted-in SDK bulk writes persisted; no throughput claim.");

using var transport = new EmulatorTransport(endpoint, certificate);
CheckPolicy("paths", "\"indexingPolicy\":{\"automatic\":true,\"indexingMode\":\"consistent\",\"includedPaths\":[{\"path\":\"/*\"}],\"excludedPaths\":[],\"compositeIndexes\":[[{\"path\":\"/fleet\",\"order\":\"ascending\"},{\"path\":\"/value\",\"order\":\"descending\"}]]}", "compositeIndexes");
CheckPolicy("vector", "\"vectorEmbeddingPolicy\":{\"vectorEmbeddings\":[{\"path\":\"/embedding\",\"dataType\":\"float32\",\"distanceFunction\":\"cosine\",\"dimensions\":3}]},\"indexingPolicy\":{\"vectorIndexes\":[{\"path\":\"/embedding\",\"type\":\"flat\"}]}", "vectorIndexes");
CheckPolicy("fulltext", "\"fullTextPolicy\":{\"defaultLanguage\":\"en-US\",\"fullTextPaths\":[{\"path\":\"/text\",\"language\":\"en-US\"}]},\"indexingPolicy\":{\"fullTextIndexes\":[{\"path\":\"/text\"}]}", "fullTextIndexes");

void CheckPolicy(string id, string policy, string expected)
{
    var created = false;
    try
    {
        transport.Request("POST", $"/dbs/{name}/colls", "colls", $"dbs/{name}",
            $"{{\"id\":\"{id}\",\"partitionKey\":{{\"paths\":[\"/fleet\"],\"kind\":\"Hash\"}},{policy}}}");
        created = true;
        var actual = transport.Request("GET", $"/dbs/{name}/colls/{id}", "colls", $"dbs/{name}/colls/{id}");
        var index = JObject.Parse(actual)["indexingPolicy"]?[expected] as JArray;
        if (index == null || index.Count == 0)
            throw new InvalidOperationException($"{id} policy was accepted but {expected} was not returned; baseline regression.");
        else Console.WriteLine($"PASS: {id} policy applied and read back; index query execution/performance remains recipe-specific.");
    }
    finally { if (created) transport.Request("DELETE", $"/dbs/{name}/colls/{id}", "colls", $"dbs/{name}/colls/{id}"); }
}

CosmosClient Client(HttpClient transport, bool bulk = false) => new(endpoint, key, new CosmosClientOptions
{
    ConnectionMode = ConnectionMode.Gateway,
    HttpClientFactory = () => transport,
    AllowBulkExecution = bulk,
    LimitToEndpoint = true,
    RequestTimeout = TimeSpan.FromSeconds(15),
    ConsistencyLevel = ConsistencyLevel.Session
});
static string Required(string variable) => Environment.GetEnvironmentVariable(variable)
    ?? throw new InvalidOperationException("Run through eng/Cosmos.ps1 -Action Run.");

public sealed class Reading
{
    [JsonProperty("id")] public string Id { get; set; } = "";
    [JsonProperty("fleet")] public string Fleet { get; set; } = "";
    [JsonProperty("value")] public int Value { get; set; }
}

public sealed class ReadingContext(DbContextOptions<ReadingContext> options) : DbContext(options)
{
    public DbSet<Reading> Readings => Set<Reading>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var reading = modelBuilder.Entity<Reading>();
        reading.ToContainer("readings");
        reading.HasPartitionKey(x => x.Fleet);
        reading.HasKey(x => x.Id);
        reading.Property(x => x.Id).ToJsonProperty("id");
        reading.Property(x => x.Fleet).ToJsonProperty("fleet");
        reading.Property(x => x.Value).ToJsonProperty("value");
    }
}
