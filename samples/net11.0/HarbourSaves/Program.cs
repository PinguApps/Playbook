// snippet:complete
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using System.Net;
using PlaybookCosmos;

string Required(string name) => Environment.GetEnvironmentVariable(name) ?? throw new InvalidOperationException(name);
var endpoint = Required("PLAYBOOK_COSMOS_ENDPOINT");
var database = Required("PLAYBOOK_COSMOS_DATABASE");
using var http = EmulatorTransport.CreateClient(endpoint, Required("PLAYBOOK_COSMOS_CERT_SHA256"));
var options = new DbContextOptionsBuilder<DispatchContext>().UseCosmos(endpoint, Required("PLAYBOOK_COSMOS_KEY"), database,
    c => c.ConnectionMode(ConnectionMode.Gateway).LimitToEndpoint().HttpClientFactory(() => http)).Options;
await using var setup = new DispatchContext(options);
await setup.Database.EnsureCreatedAsync();
foreach (var behavior in new[] { AutoTransactionBehavior.WhenNeeded, AutoTransactionBehavior.Never, AutoTransactionBehavior.Always })
foreach (var split in new[] { false, true })
{
    var prefix = $"{behavior}-{split}";
    await using var db = new DispatchContext(options);
    db.Database.AutoTransactionBehavior = behavior;
    db.Add(new Dispatch { Id = prefix + "-a", Harbour = "west" });
    db.Add(new Dispatch { Id = prefix + "-b", Harbour = split ? "east" : "west" });
    var rejected = false;
    try { await db.SaveChangesAsync(); }
    catch (InvalidOperationException) when (behavior == AutoTransactionBehavior.Always && split) { rejected = true; }
    if (rejected != (behavior == AutoTransactionBehavior.Always && split)) throw new InvalidOperationException("Unexpected boundary outcome.");
    await using var read = new DispatchContext(options);
    var count = await read.Set<Dispatch>().CountAsync(x => x.Id.StartsWith(prefix));
    if (count != (rejected ? 0 : 2)) throw new InvalidOperationException("Unexpected persisted count.");
    Console.WriteLine($"PASS {behavior}, split partition {split}: {count} saved");
}
foreach (var behavior in new[] { AutoTransactionBehavior.WhenNeeded, AutoTransactionBehavior.Never, AutoTransactionBehavior.Always })
{
    await using var db = new DispatchContext(options);
    db.Database.AutoTransactionBehavior = behavior;
    db.Add(new Dispatch { Id = behavior + "-container", Harbour = "west" });
    db.Add(new Receipt { Id = behavior + "-receipt", Harbour = "west" });
    var rejected = false;
    try { await db.SaveChangesAsync(); }
    catch (InvalidOperationException) when (behavior == AutoTransactionBehavior.Always) { rejected = true; }
    if (rejected != (behavior == AutoTransactionBehavior.Always)) throw new InvalidOperationException("Container boundary mismatch.");
    Console.WriteLine($"PASS {behavior}, two containers: rejected {rejected}");
}
using var sdkHttp = EmulatorTransport.CreateClient(endpoint, Required("PLAYBOOK_COSMOS_CERT_SHA256"));
using var sdk = new CosmosClient(endpoint, Required("PLAYBOOK_COSMOS_KEY"), new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway, LimitToEndpoint = true, HttpClientFactory = () => sdkHttp });
foreach (var behavior in new[] { AutoTransactionBehavior.WhenNeeded, AutoTransactionBehavior.Never, AutoTransactionBehavior.Always })
{
    var container = sdk.GetContainer(database, "dispatches");
    var prefix = behavior + "-conflict";
    await container.CreateItemAsync(new { id = prefix + "-z", Harbour = "west" });
    await using var db = new DispatchContext(options);
    db.Database.AutoTransactionBehavior = behavior;
    db.Add(new Dispatch { Id = prefix + "-a", Harbour = "west" });
    db.Add(new Dispatch { Id = prefix + "-z", Harbour = "west" });
    var failed = false;
    try { await db.SaveChangesAsync(); }
    catch (DbUpdateException) { failed = true; }
    if (!failed) throw new InvalidOperationException("Expected conflicting save.");
    var persisted = true;
    try { await container.ReadItemAsync<object>(prefix + "-a", new PartitionKey("west")); }
    catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound) { persisted = false; }
    if (persisted != (behavior == AutoTransactionBehavior.Never)) throw new InvalidOperationException("Unexpected rollback outcome.");
    Console.WriteLine($"PASS {behavior} conflict: preceding document persisted {persisted}");
}

sealed class DispatchContext(DbContextOptions<DispatchContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Dispatch>();
        entity.ToContainer("dispatches");
        entity.HasKey(x => x.Id);
        entity.HasPartitionKey(x => x.Harbour);
        entity.HasNoDiscriminator();
        var receipt = modelBuilder.Entity<Receipt>();
        receipt.ToContainer("receipts");
        receipt.HasKey(x => x.Id);
        receipt.HasPartitionKey(x => x.Harbour);
        receipt.HasNoDiscriminator();
    }
}
sealed class Dispatch { public required string Id { get; set; } public required string Harbour { get; set; } }
sealed class Receipt { public required string Id { get; set; } public required string Harbour { get; set; } }
// endsnippet:complete
