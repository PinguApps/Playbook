// snippet:complete
using System.Net;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using PlaybookCosmos;
string Required(string name) => Environment.GetEnvironmentVariable(name) ?? throw new InvalidOperationException(name);
var endpoint = Required("PLAYBOOK_COSMOS_ENDPOINT");
using var http = EmulatorTransport.CreateClient(endpoint, Required("PLAYBOOK_COSMOS_CERT_SHA256"));
using var client = new CosmosClient(endpoint, Required("PLAYBOOK_COSMOS_KEY"), new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway, LimitToEndpoint = true, HttpClientFactory = () => http });
var database = client.GetDatabase(Required("PLAYBOOK_COSMOS_DATABASE"));
var container = (await database.CreateContainerAsync("dispatches", "/Harbour")).Container;
using var result = await container.CreateTransactionalBatch(new PartitionKey("west"))
    .CreateItem(new { id = "first", Harbour = "west" })
    .CreateItem(new { id = "second", Harbour = "west" }).ExecuteAsync();
if (!result.IsSuccessStatusCode) throw new InvalidOperationException(result.ErrorMessage);
using var conflict = await container.CreateTransactionalBatch(new PartitionKey("west"))
    .CreateItem(new { id = "rollback", Harbour = "west" })
    .CreateItem(new { id = "first", Harbour = "west" }).ExecuteAsync();
if (conflict.StatusCode != HttpStatusCode.Conflict) throw new InvalidOperationException("Expected conflict.");
try { await container.ReadItemAsync<JObject>("rollback", new PartitionKey("west")); throw new InvalidOperationException("Partial batch persisted."); }
catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound) { }
Console.WriteLine("PASS explicit SDK batch success and conflict rollback; EF10 SaveChanges has no transactional batch equivalent.");
// endsnippet:complete
