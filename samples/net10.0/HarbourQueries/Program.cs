// snippet:complete
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
#pragma warning disable CA1849 // Deliberate historical/removal probes only.
using PlaybookCosmos;

string Required(string name) => Environment.GetEnvironmentVariable(name) ?? throw new InvalidOperationException(name);
var endpoint = Required("PLAYBOOK_COSMOS_ENDPOINT");
var key = Required("PLAYBOOK_COSMOS_KEY");
var database = Required("PLAYBOOK_COSMOS_DATABASE");
using var http = EmulatorTransport.CreateClient(endpoint, Required("PLAYBOOK_COSMOS_CERT_SHA256"));
var builder = new DbContextOptionsBuilder<HarbourContext>().UseCosmos(endpoint, key, database,
    c => c.ConnectionMode(ConnectionMode.Gateway).LimitToEndpoint().HttpClientFactory(() => http));
builder.ConfigureWarnings(w => w.Ignore(CosmosEventId.SyncNotSupported)); // Historical EF10 opt-in only.
await using var db = new HarbourContext(builder.Options);
await db.Database.EnsureCreatedAsync();
db.Add(new Mooring { Id = "berth-seven", Harbour = "south", Name = "Lantern" });
await db.SaveChangesAsync();
db.ChangeTracker.Clear();
var discriminator = db.Model.FindEntityType(typeof(Mooring))!.FindDiscriminatorProperty()!;
if (discriminator.Name != "$type" || discriminator.GetJsonPropertyName() != "$type")
    throw new InvalidOperationException("Unexpected model/storage discriminator mapping.");
var rows = await db.Set<Mooring>().Where(x => EF.Property<string>(x, "$type") == "Mooring").ToListAsync();
if (rows.Count != 1 || rows[0].Name != "Lantern") throw new InvalidOperationException("Corrected query failed.");
using var rawHttp = EmulatorTransport.CreateClient(endpoint, Required("PLAYBOOK_COSMOS_CERT_SHA256"));
using var sdk = new CosmosClient(endpoint, key, new CosmosClientOptions {
    ConnectionMode = ConnectionMode.Gateway, LimitToEndpoint = true, HttpClientFactory = () => rawHttp });
using var response = await sdk.GetContainer(database, "moorings").ReadItemStreamAsync("berth-seven", new PartitionKey("south"));
response.EnsureSuccessStatusCode();
using var json = await JsonDocument.ParseAsync(response.Content);
if (json.RootElement.GetProperty("$type").GetString() != "Mooring" || json.RootElement.TryGetProperty("Discriminator", out _))
    throw new InvalidOperationException("Stored JSON name changed.");
Console.WriteLine($"PASS model {discriminator.Name}, stored $type; corrected query executed without JSON rename");
_ = db.Set<Mooring>().ToList();
db.Add(new Mooring { Id = "berth-eight", Harbour = "south", Name = "Beacon" });
db.SaveChanges();
await db.SaveChangesAsync();
if (await db.Set<Mooring>().CountAsync() != 2) throw new InvalidOperationException("Async save/query failed.");
Console.WriteLine("PASS async query/save and historical EF10 synchronous opt-in");

sealed class HarbourContext(DbContextOptions<HarbourContext> options) : DbContext(options) {
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        var e = modelBuilder.Entity<Mooring>();
        e.ToContainer("moorings"); e.HasKey(x => x.Id); e.HasPartitionKey(x => x.Harbour);
    }
}
sealed class Mooring {
    public required string Id { get; set; }
    public required string Harbour { get; set; }
    public required string Name { get; set; }
}
// endsnippet:complete


