// snippet:complete
using System.Net.Http.Json;
using System.Text.Json;

namespace FenContracts;

internal static class ContractChecks
{
    public static async Task RunAsync(string address)
    {
        using var client = new HttpClient { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(20) };
        foreach (var pathBase in new[] { "", "/fen" })
        {
            foreach (var documentName in new[] { "raw", "v1", "slash" })
            {
                using var document = JsonDocument.Parse(await client.GetStringAsync(new Uri($"{pathBase}/openapi/{documentName}.json", UriKind.Relative)));
                var root = document.RootElement;
                var schemas = root.GetProperty("components").GetProperty("schemas");
                var reading = schemas.GetProperty("Reading");
                var legacy = schemas.GetProperty("LegacyReading");
                var operations = root.GetProperty("paths");
                Require(IsDeprecated(operations.GetProperty("/legacy-readings").GetProperty("get")), "obsolete operation");
                Require(!IsDeprecated(operations.GetProperty("/readings").GetProperty("get")), "current operation control");
                var expectSchemaDeprecation = true;
                Require(IsDeprecated(legacy) == expectSchemaDeprecation, $"{documentName}: obsolete type");
                Require(IsDeprecated(reading.GetProperty("properties").GetProperty("gaugeTicks")) == expectSchemaDeprecation, $"{documentName}: obsolete property");
                Require(!IsDeprecated(reading), "current type control");
                Require(!IsDeprecated(reading.GetProperty("properties").GetProperty("station")), "current property control");

                Require(root.GetProperty("info").GetProperty("title").GetString() == "Alder Fen observations", "document transformer");
                Require(root.GetProperty("x-observation-area").GetProperty("revision").GetInt32() == 7, "JSON object extension");
                Require(reading.GetProperty("x-measurement-system").GetString() == "metric", "JSON scalar extension");
                Require(reading.GetProperty("properties").GetProperty("station").GetProperty("description").GetString() == "Published observation field.", "inline schema traversal");
                var response = operations.GetProperty("/readings").GetProperty("get").GetProperty("responses").GetProperty("503");
                Require(response.GetProperty("$ref").GetString() == "#/components/responses/GaugeOffline", "local response reference");
                Require(response.GetProperty("description").GetString() == "Retry the gauge later.", "reference description override");
                Require(root.GetProperty("components").GetProperty("responses").GetProperty("GaugeOffline").GetProperty("description").GetString() == "The observation gauge is offline.", "referenced response target");

                // snippet:urls
                var server = root.GetProperty("servers")[0].GetProperty("url").GetString()!;
                var expectedServer = address + (pathBase.Length > 0 ? pathBase : documentName == "slash" ? "/" : "");
                Require(server == expectedServer, $"{documentName}: server fixture '{server}' != '{expectedServer}'");
                // This application's explicit composition preserves PathBase with either root spelling.
                var requestUrl = server.TrimEnd('/') + "/readings";
                Require(requestUrl == address + pathBase + "/readings", "composed request fixture");
                var actual = await client.GetFromJsonAsync<JsonElement>(requestUrl);
                Require(actual.GetProperty("station").GetString() == "Alder-7", "actual composed HTTP request");
                // Uri resolution with a root-relative path discards /fen: demonstrate that separate pitfall.
                if (pathBase.Length > 0)
                {
                    Require(new Uri(new Uri(server), "/readings").AbsolutePath == "/readings", "URI resolution control");
                }
                // endsnippet:urls
                Console.WriteLine($"{documentName}: PathBase='{pathBase}', server='{server}', schemaDeprecated={expectSchemaDeprecation}; transformers/reference/extensions/HTTP PASS");
            }
        }
        var retired = await client.GetFromJsonAsync<JsonElement>("/legacy-readings");
        Require(retired.GetProperty("gaugeTicks").GetInt32() == 24, "Obsolete does not remove runtime behavior");
    }

    private static bool IsDeprecated(JsonElement value) => value.TryGetProperty("deprecated", out var flag) && flag.GetBoolean();
    private static void Require(bool condition, string claim)
    {
        if (!condition) throw new InvalidOperationException($"Contract check failed: {claim}");
    }
}
// endsnippet:complete
