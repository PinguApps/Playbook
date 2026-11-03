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
// EF10's warning opt-in is deliberately absent: EF11 cannot restore synchronous I/O.
await using var db = new HarbourContext(builder.Options);
await db.Database.EnsureCreatedAsync();
db.Add(new Mooring { Id = "berth-seven", Harbour = "south", Name = "Lantern" });
await db.SaveChangesAsync();
db.ChangeTracker.Clear();
var discriminator = db.Model.FindEntityType(typeof(Mooring))!.FindDiscriminatorProperty()!;
if (discriminator.Name != "Discriminator" || discriminator.GetJsonPropertyName() != "$type")
    throw new InvalidOperationException("Unexpected model/storage discriminator mapping.");
var rows = await db.Set<Mooring>().Where(x => EF.Property<string>(x, "Discriminator") == "Mooring").ToListAsync();
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
var syncQueryFailed = false;
try { _ = db.Set<Mooring>().ToList(); } catch (InvalidOperationException) { syncQueryFailed = true; }
db.Add(new Mooring { Id = "berth-eight", Harbour = "south", Name = "Beacon" });
var syncSaveFailed = false;
try { db.SaveChanges(); } catch (InvalidOperationException) { syncSaveFailed = true; }
if (!syncQueryFailed || !syncSaveFailed) throw new InvalidOperationException("Expected removed synchronous I/O failures.");
await db.SaveChangesAsync();
if (await db.Set<Mooring>().CountAsync() != 2) throw new InvalidOperationException("Async save/query failed.");
Console.WriteLine("PASS async query/save; synchronous query/save both rejected");

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

