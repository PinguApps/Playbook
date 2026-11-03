// snippet:complete
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PlaybookCosmos;

string Required(string name) => Environment.GetEnvironmentVariable(name) ?? throw new InvalidOperationException($"Missing {name}; use the isolated emulator route.");
var endpoint = Required("PLAYBOOK_COSMOS_ENDPOINT");
var key = Required("PLAYBOOK_COSMOS_KEY");
var database = Required("PLAYBOOK_COSMOS_DATABASE");
var certificate = Required("PLAYBOOK_COSMOS_CERT_SHA256");
if (!database.StartsWith("playbook_", StringComparison.Ordinal)) throw new InvalidOperationException("Disposable database required.");
using var http = EmulatorTransport.CreateClient(endpoint, certificate);
var options = new DbContextOptionsBuilder<ManifestContext>().UseCosmos(endpoint, key, database,
    c => c.ConnectionMode(ConnectionMode.Gateway).LimitToEndpoint().HttpClientFactory(() => http)).Options;
await using var db = new ManifestContext(options);
await db.Database.EnsureCreatedAsync();
db.Add(new Manifest { Id = "reef", Region = "north", Location = new Location { Bay = "Cove" },
    Samples = [new Specimen { Code = "Kelp", Count = 2 }, new Specimen { Code = "Coral", Count = 3 }] });
await db.SaveChangesAsync();
db.ChangeTracker.Clear();
var saved = await db.Manifests.WithPartitionKey("north").SingleAsync(x => x.Id == "reef");
if (saved.Location.Bay != "Cove" || saved.Samples.Select(x => x.Count).Sum() != 5) throw new InvalidOperationException("Round trip failed.");
if (db.ChangeTracker.Entries().Count() != 1) throw new InvalidOperationException("Complex values must not be entity entries.");

saved.Location = new Location { Bay = "Lagoon" };
saved.Samples[0].Count = 7;
saved.Samples.Add(new Specimen { Code = "Sponge", Count = 4 });
await db.SaveChangesAsync();
db.ChangeTracker.Clear();
var updated = await db.Manifests.WithPartitionKey("north").SingleAsync(x => x.Id == "reef");
if (updated.Location.Bay != "Lagoon" || updated.Samples.Count != 3 || updated.Samples[0].Count != 7) throw new InvalidOperationException("Value update failed.");
using var sdkHttp = EmulatorTransport.CreateClient(endpoint, certificate);
using var sdk = new CosmosClient(endpoint, key, new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway, LimitToEndpoint = true, HttpClientFactory = () => sdkHttp });
var document = (await sdk.GetContainer(database, "manifests").ReadItemAsync<JObject>("reef", new PartitionKey("north"))).Resource;
if ((string?)document["Location"]?["Bay"] != "Lagoon" || document["Samples"] is not JArray array || array.Count != 3)
    throw new InvalidOperationException("Stored nested object/array shape differs.");
Console.WriteLine(document.ToString());
Console.WriteLine($"PASS real Cosmos nested values, updates, tracking and document shape; EF {typeof(DbContext).Assembly.GetName().Version}; runtime {Environment.Version}");

sealed class ManifestContext(DbContextOptions<ManifestContext> options) : DbContext(options)
{
    public DbSet<Manifest> Manifests => Set<Manifest>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Manifest>();
        entity.ToContainer("manifests");
        entity.HasKey(x => x.Id);
        entity.HasPartitionKey(x => x.Region);
        entity.HasNoDiscriminator();
// snippet:mapping
        entity.ComplexProperty(x => x.Location);
        entity.ComplexCollection(x => x.Samples);
// endsnippet:mapping

    }
}
sealed class Manifest { public required string Id { get; set; } public required string Region { get; set; } public required Location Location { get; set; } public List<Specimen> Samples { get; set; } = []; }
sealed class Location { public required string Bay { get; set; } }
sealed class Specimen { public required string Code { get; set; } public int Count { get; set; } }
// endsnippet:complete

