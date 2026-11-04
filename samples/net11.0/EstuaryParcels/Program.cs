// snippet:complete
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using PlaybookCosmos;

string Required(string key) => Environment.GetEnvironmentVariable(key) ?? throw new InvalidOperationException(key);
var endpoint = Required("PLAYBOOK_COSMOS_ENDPOINT");
var database = Required("PLAYBOOK_COSMOS_DATABASE");
var reuse = args.Contains("--reuse");

DbContextOptions<ParcelContext> Options(bool? content, HttpClient http) => new DbContextOptionsBuilder<ParcelContext>()
    .UseCosmos(endpoint, Required("PLAYBOOK_COSMOS_KEY"), database, cosmos =>
    {
        cosmos.ConnectionMode(ConnectionMode.Gateway).LimitToEndpoint().HttpClientFactory(() => http);
#if !DIAGNOSTIC_PROBE
#pragma warning disable CS0618 // Deliberate old-option execution control; remove the call in production.
#endif
        if (content.HasValue) cosmos.ContentResponseOnWriteEnabled(content.Value);
#pragma warning restore CS0618
    }).Options;
foreach (var content in new bool?[] { true, false, null })
{
    using var http = EmulatorTransport.CreateClient(endpoint, Required("PLAYBOOK_COSMOS_CERT_SHA256"));
    var options = Options(content, http);
    var extension = options.Extensions.Single(x => x.GetType().GetProperty("EnableContentResponseOnWrite") != null);
    Console.WriteLine($"OPTION stored={extension.GetType().GetProperty("EnableContentResponseOnWrite")!.GetValue(extension) ?? "null"}");
    await using var context = new ParcelContext(options);
    await context.Database.EnsureCreatedAsync();
    var entity = context.Model.FindEntityType(typeof(Parcel))!;
    if (!entity.GetPartitionKeyPropertyNames().SequenceEqual(new[] { "Depot" })) throw new Exception("Key order changed.");
#if NET10_0
#pragma warning disable CS0618
    if (entity.GetPartitionKeyPropertyName() != "Depot" || entity.GetPartitionKeyProperty()!.Name != "Depot") throw new Exception("Legacy metadata differs.");
#pragma warning restore CS0618
#endif
    var id = "parcel-" + (content?.ToString() ?? "omitted");
    if (!reuse) { context.Add(new Parcel { Id = id, Depot = "estuary", Count = 4 }); await context.SaveChangesAsync(); }
    await using var first = new ParcelContext(options);
    await using var stale = new ParcelContext(options);
    var saved = await first.Set<Parcel>().WithPartitionKey("estuary").SingleAsync(x => x.Id == id);
    var rival = await stale.Set<Parcel>().WithPartitionKey("estuary").SingleAsync(x => x.Id == id);
    var tag = saved.ETag;
    if (string.IsNullOrWhiteSpace(tag) || saved.Count != (reuse ? 5 : 4)) throw new Exception("Save/reload missing value or ETag.");
    saved.Count = reuse ? 6 : 5;
    await first.SaveChangesAsync();
    if (saved.ETag == tag) throw new Exception("ETag did not refresh.");
    rival.Count = 99;
    try { await stale.SaveChangesAsync(); throw new Exception("Stale write succeeded."); }
    catch (DbUpdateConcurrencyException) { }
    await using var reload = new ParcelContext(options);
    if ((await reload.Set<Parcel>().WithPartitionKey("estuary").SingleAsync(x => x.Id == id)).Count != (reuse ? 6 : 5)) throw new Exception("Update lost.");
    if ((await reload.Set<Parcel>().WithPartitionKey("wrong").Where(x => x.Id == id).ToListAsync()).Count != 0) throw new Exception("Incorrect partition found document.");
    Console.WriteLine($"PASS Cosmos reuse={reuse} content={content?.ToString() ?? "omitted/null"}; key order=Depot; save/reload/update/ETag/stale-write/wrong-key");
}
sealed class ParcelContext(DbContextOptions<ParcelContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var parcel = modelBuilder.Entity<Parcel>();
        parcel.ToContainer("parcels").HasNoDiscriminator();
        parcel.HasKey(x => new { x.Id, x.Depot });
        parcel.Property(x => x.Id).ToJsonProperty("id");
        parcel.Property(x => x.ETag).IsETagConcurrency();
#if NET10_0
#pragma warning disable CS0618
        parcel.Metadata.SetPartitionKeyPropertyName(nameof(Parcel.Depot));
#pragma warning restore CS0618
#else
        parcel.Metadata.SetPartitionKeyPropertyNames(new[] { nameof(Parcel.Depot) });
#endif
    }
}
sealed class Parcel
{
    public string Id { get; set; } = "";
    public string Depot { get; set; } = "";
    public int Count { get; set; }
    public string? ETag { get; set; }
}
// endsnippet:complete



