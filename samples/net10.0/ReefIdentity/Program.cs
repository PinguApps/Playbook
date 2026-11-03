// snippet:complete
using System.Net;
using System.Text;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using PlaybookCosmos;

string Required(string name) => Environment.GetEnvironmentVariable(name) ?? throw new InvalidOperationException(name);
var endpoint = Required("PLAYBOOK_COSMOS_ENDPOINT");
var key = Required("PLAYBOOK_COSMOS_KEY");
var database = Required("PLAYBOOK_COSMOS_DATABASE");
using var http = EmulatorTransport.CreateClient(endpoint, Required("PLAYBOOK_COSMOS_CERT_SHA256"));
var options = new DbContextOptionsBuilder<ReefContext>().UseCosmos(endpoint, key, database,
    c => c.ConnectionMode(ConnectionMode.Gateway).LimitToEndpoint().HttpClientFactory(() => http)).Options;
await using var db = new ReefContext(options);
await db.Database.EnsureCreatedAsync();
using var sdkHttp = EmulatorTransport.CreateClient(endpoint, Required("PLAYBOOK_COSMOS_CERT_SHA256"));
using var sdk = new CosmosClient(endpoint, key, new CosmosClientOptions {
    ConnectionMode = ConnectionMode.Gateway, LimitToEndpoint = true, HttpClientFactory = () => sdkHttp });
var container = sdk.GetContainer(database, "labels");
// These are the real historical composite IDs, including the old collision.
async Task Seed(string id, string code, string suffix = "") {
    var json = System.Text.Json.JsonSerializer.Serialize(new { id, Fleet = "reef", Code = code, Revision = "v1" });
    json = json[..^1] + suffix + "}";
    using var body = new MemoryStream(Encoding.UTF8.GetBytes(json));
    using var response = await container.CreateItemStreamAsync(body, new PartitionKey("reef"));
    response.EnsureSuccessStatusCode();
}
await Seed("bay^2Fwest|v1", "bay/west");
await Seed("present|v1", "present", ",\"Detail\":{\"Note\":\"surveyed\",\"Survey\":{\"Rank\":7}}");
await Seed("null|v1", "null", ",\"Detail\":{\"Note\":null,\"Survey\":{\"Rank\":8}}");

// Process-wide compatibility is deliberately bounded to this migration consumer.

var legacy = await db.Set<Label>().FindAsync("reef", "bay/west", "v1");
if (legacy?.Code != "bay/west") throw new InvalidOperationException("Compatibility read failed.");
db.ChangeTracker.Clear();
db.Add(new Label { Code = "bay^2Fwest", Revision = "v1", Fleet = "reef" });
var collided = false;
try { await db.SaveChangesAsync(); } catch (DbUpdateException) { collided = true; }
if (!collided) throw new InvalidOperationException("Expected old ID collision.");
db.ChangeTracker.Clear();
Console.WriteLine("PASS compatibility reaches historical document; literal ^2F collides with slash");

var defined = await db.Set<Label>().Where(x => EF.Functions.IsDefined(x.Detail!.Note))
    .Select(x => new { x.Detail!.Note }).ToListAsync();
if (defined.Count != 2 || defined.Count(x => x.Note is null) != 1) throw new InvalidOperationException("Defined/null distinction failed.");
var fallback = await db.Set<Label>().Select(x => new { Note = EF.Functions.CoalesceUndefined(x.Detail!.Note, "unrecorded") }).ToListAsync();
if (fallback.Count != 3 || fallback.Count(x => x.Note == "unrecorded") != 1 || fallback.Count(x => x.Note is null) != 1)
    throw new InvalidOperationException("Undefined fallback changed JSON null.");
Console.WriteLine("PASS IsDefined excludes absent path, retains JSON null; CoalesceUndefined replaces only absent path");
var failed = false;
try {
    var ranks = await db.Set<Label>().Select(x => new { x.Detail!.Survey!.Rank }).ToListAsync();
    if (ranks.Count != 2 || !ranks.Select(x => x.Rank).Order().SequenceEqual([7, 8]))
        throw new InvalidOperationException("Expected exactly the two present nested ranks.");
}
catch (InvalidOperationException) { failed = true; }
if (failed) throw new InvalidOperationException("EF10 single projection should filter undefined.");
Console.WriteLine("PASS EF10 single projection filters undefined");

sealed class ReefContext(DbContextOptions<ReefContext> options) : DbContext(options) {
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        var e = modelBuilder.Entity<Label>();
        e.ToContainer("labels"); e.HasKey(x => new { x.Fleet, x.Code, x.Revision });
        e.HasPartitionKey(x => x.Fleet); e.HasNoDiscriminator(); e.OwnsOne(x => x.Detail, d => d.OwnsOne(x => x.Survey));
    }
}
sealed class Label {
    public required string Code { get; set; }
    public required string Revision { get; set; }
    public required string Fleet { get; set; }
    public Detail? Detail { get; set; }
}
sealed class Detail { public string? Note { get; set; } public Survey? Survey { get; set; } }
sealed class Survey { public int Rank { get; set; } }
// endsnippet:complete
