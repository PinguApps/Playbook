// snippet:complete
using System.Net;
using System.Text;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using PlaybookCosmos;

AppContext.SetSwitch("Microsoft.EntityFrameworkCore.EscapeIllegalCosmosIdCharacters", !args.Contains("--modern", StringComparer.Ordinal));
string Required(string name) => Environment.GetEnvironmentVariable(name) ?? throw new InvalidOperationException(name);
var endpoint = Required("PLAYBOOK_COSMOS_ENDPOINT");
var key = Required("PLAYBOOK_COSMOS_KEY");
var database = Required("PLAYBOOK_COSMOS_DATABASE");
using var http = EmulatorTransport.CreateClient(endpoint, Required("PLAYBOOK_COSMOS_CERT_SHA256"));
var options = new DbContextOptionsBuilder<ReefContext>().UseCosmos(endpoint, key, database,
    c => c.ConnectionMode(ConnectionMode.Gateway).LimitToEndpoint().HttpClientFactory(() => http)).Options;
await using var db = new ReefContext(options);
// Run in a separate process so the AppContext switch cannot leak across modes.
if (args.Contains("--modern", StringComparer.Ordinal)) {
    var unreachable = false;
    try { unreachable = await db.Set<Label>().FindAsync("reef", "bay/west", "v1") is null; }
    catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.BadRequest) { unreachable = true; }
    if (!unreachable) throw new InvalidOperationException("Default EF11 unexpectedly reached escaped ID.");
    db.Add(new Label { Fleet = "reef", Code = "bay-west", Revision = "v2" });
    await db.SaveChangesAsync();
    db.ChangeTracker.Clear();
    if (await db.Set<Label>().FindAsync("reef", "bay-west", "v2") is null) throw new InvalidOperationException("Legal new key unreachable.");
    Console.WriteLine("PASS default EF11 cannot point-read old escaped key; legal new composite key round-trips");
    return;
}
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

var childInfo = new System.Diagnostics.ProcessStartInfo(Environment.ProcessPath!) { UseShellExecute = false };
childInfo.ArgumentList.Add(typeof(ReefContext).Assembly.Location);
childInfo.ArgumentList.Add("--modern");
using (var child = System.Diagnostics.Process.Start(childInfo) ?? throw new InvalidOperationException("Child startup failed.")) {
    await child.WaitForExitAsync();
    if (child.ExitCode != 0) throw new InvalidOperationException("Default EF11 identity check failed.");
}
// Remove only the legal-key fixture created by the child, keeping projection counts focused.
await using (var cleanup = new ReefContext(options)) {
    var legal = await cleanup.Set<Label>().FindAsync("reef", "bay-west", "v2");
    cleanup.Remove(legal!);
    await cleanup.SaveChangesAsync();
}
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
// TODO GA: retest the documented single- and multi-property undefined exceptions
// against .NET11/EF11 GA artifacts. RC2/RC3 omit the absent single-property row.
// Pingu explicitly approved shipping the documented contract with this gap disclosed.
var singleFailed = false;
try {
    var ranks = await db.Set<Label>().Select(x => new { x.Detail!.Survey!.Rank }).ToListAsync();
    Console.WriteLine($"OBSERVED single-property projection returned {ranks.Count} rows; SQL: "
        + db.Set<Label>().Select(x => new { x.Detail!.Survey!.Rank }).ToQueryString());
}
catch (InvalidOperationException exception) {
    singleFailed = true;
    Console.WriteLine("OBSERVED single-property exception: " + exception.Message);
}
var multiFailed = false;
try { _ = await db.Set<Label>().Select(x => new { x.Code, x.Detail!.Survey!.Rank }).ToListAsync(); }
catch (InvalidOperationException exception) {
    multiFailed = true;
    Console.WriteLine("OBSERVED multi-property exception: " + exception.Message);
}
Console.WriteLine($"GA RETEST REQUIRED: documented undefined exceptions; single={singleFailed}, multi={multiFailed}. RC observations are not GA proof.");
if (args.Contains("--verify-ga", StringComparer.Ordinal) && (!singleFailed || !multiFailed))
    throw new InvalidOperationException("Documented GA undefined-projection contract was not observed.");

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
