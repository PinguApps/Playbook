// snippet:complete
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PlaybookCosmos;

string Required(string name) => Environment.GetEnvironmentVariable(name) ?? throw new InvalidOperationException(name);
var endpoint = Required("PLAYBOOK_COSMOS_ENDPOINT");
var database = Required("PLAYBOOK_COSMOS_DATABASE");
var certificate = Required("PLAYBOOK_COSMOS_CERT_SHA256");
using var http = EmulatorTransport.CreateClient(endpoint, certificate);
var options = new DbContextOptionsBuilder<CatalogContext>().UseCosmos(endpoint, Required("PLAYBOOK_COSMOS_KEY"), database,
    c => c.ConnectionMode(ConnectionMode.Gateway).LimitToEndpoint().HttpClientFactory(() => http)).Options;
await using var db = new CatalogContext(options);
await db.Database.EnsureCreatedAsync();
using var transport = new EmulatorTransport(endpoint, certificate);
var document = JObject.Parse(transport.Request("GET", $"/dbs/{database}/colls/catalog", "colls", $"dbs/{database}/colls/catalog"));
var policy = document["indexingPolicy"] ?? throw new InvalidOperationException("No policy returned.");
foreach (var field in new[] { "includedPaths", "compositeIndexes", "vectorIndexes", "fullTextIndexes" })
    if (policy[field] is not JArray items || items.Count == 0) throw new InvalidOperationException($"Missing {field}");
var included = policy["includedPaths"]!.Select(x => (string?)x["path"]).ToArray();
string[] expectedPaths = ["/Location/Bay/?", "/Shelves/[]/Bay/?", "/Title/?"];
if ((bool?)policy["automatic"] != true || !included.Order().SequenceEqual(expectedPaths.Order()))
    throw new InvalidOperationException("Automatic maintenance or selected nested paths differ.");
var composite = policy["compositeIndexes"]![0]!;
if ((string?)composite[0]?["path"] != "/Harbour" || (string?)composite[0]?["order"] != "ascending"
    || (string?)composite[1]?["path"] != "/Priority" || (string?)composite[1]?["order"] != "descending"
    || (string?)policy["vectorIndexes"]![0]?["path"] != "/Embedding" || (string?)policy["vectorIndexes"]![0]?["type"] != "flat"
    || (string?)policy["fullTextIndexes"]![0]?["path"] != "/Description")
    throw new InvalidOperationException("Composite directions or specialized index definitions differ.");
Console.WriteLine(document.ToString());
Console.WriteLine("PASS actual EF-created path/composite/vector/full-text index definitions read back; no query or performance claim.");

sealed class CatalogContext(DbContextOptions<CatalogContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<CatalogEntry>();
        entity.ToContainer("catalog");
        entity.HasKey(x => x.Id);
        entity.HasPartitionKey(x => x.Harbour);
        entity.HasNoDiscriminator();
        entity.HasAutomaticIndexing(false);
        entity.HasIndex(x => x.Title);
        entity.ComplexProperty(x => x.Location);
        entity.ComplexCollection(x => x.Shelves);
        entity.HasIndex("Location.Bay");
        entity.HasIndex("Shelves[].Bay");
        entity.HasIndex(x => new { x.Harbour, x.Priority }).IsDescending(false, true);
        entity.Property(x => x.Embedding).IsVectorProperty(DistanceFunction.Cosine, 3);
        entity.HasIndex(x => x.Embedding).IsVectorIndex(VectorIndexType.Flat);
        entity.Property(x => x.Description).EnableFullTextSearch("en-US");
        entity.HasIndex(x => x.Description).IsFullTextIndex();
    }
}
sealed class CatalogEntry
{
    public required string Id { get; set; }
    public required string Harbour { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public int Priority { get; set; }
    public required Location Location { get; set; }
    public List<Location> Shelves { get; set; } = [];
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819", Justification = "Cosmos vector property uses the provider-supported float array mapping.")]
    public float[] Embedding { get; set; } = [];
}
sealed class Location { public required string Bay { get; set; } }
// endsnippet:complete
