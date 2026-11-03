// snippet:complete
using Newtonsoft.Json.Linq;
using PlaybookCosmos;
string Required(string name) => Environment.GetEnvironmentVariable(name) ?? throw new InvalidOperationException(name);
var database = Required("PLAYBOOK_COSMOS_DATABASE");
using var transport = new EmulatorTransport(Required("PLAYBOOK_COSMOS_ENDPOINT"), Required("PLAYBOOK_COSMOS_CERT_SHA256"));
// EF10 can use externally provisioned policies. Apply this JSON through the Cosmos data-plane API.
const string definition = """
{"id":"catalog","partitionKey":{"paths":["/Harbour"],"kind":"Hash"},
 "indexingPolicy":{"automatic":true,"indexingMode":"consistent",
 "includedPaths":[{"path":"/Title/?"}],"excludedPaths":[{"path":"/*"},{"path":"/Embedding/*"}],
 "compositeIndexes":[[{"path":"/Harbour","order":"ascending"},{"path":"/Priority","order":"descending"}]],
 "vectorIndexes":[{"path":"/Embedding","type":"flat"}],"fullTextIndexes":[{"path":"/Description"}]},
 "vectorEmbeddingPolicy":{"vectorEmbeddings":[{"path":"/Embedding","dataType":"float32","distanceFunction":"cosine","dimensions":3}]},
 "fullTextPolicy":{"defaultLanguage":"en-US","fullTextPaths":[{"path":"/Description","language":"en-US"}]}}
""";
transport.Request("POST", $"/dbs/{database}/colls", "colls", $"dbs/{database}", definition);
var saved = JObject.Parse(transport.Request("GET", $"/dbs/{database}/colls/catalog", "colls", $"dbs/{database}/colls/catalog"));
foreach (var field in new[] { "includedPaths", "compositeIndexes", "vectorIndexes", "fullTextIndexes" })
    if (saved["indexingPolicy"]?[field] is not JArray items || items.Count == 0) throw new InvalidOperationException(field);
Console.WriteLine(saved.ToString());
Console.WriteLine("PASS externally provisioned policy read-back; no query/performance claim.");
// endsnippet:complete
