// snippet:complete
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace ArchiveContracts;

internal static class ContractChecks
{
    private static readonly string[] ExpectedMedia = ["application/json", "text/plain"];
    public static async Task RunAsync(WebApplication app, bool newer)
    {
        app.Urls.Add("http://127.0.0.1:0");
        await app.StartAsync();
        try
        {
            string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using HttpClient client = new() { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(30) };
            using JsonDocument document = JsonDocument.Parse(await client.GetStringAsync(new Uri("/openapi/v31.json", UriKind.Relative)));
            JsonElement root = document.RootElement;
            Require(root.GetProperty("openapi").GetString()!.StartsWith("3.1.", StringComparison.Ordinal), "3.1 document");
            Require(root.GetProperty("paths").GetProperty("/archive/restricted").GetProperty("get").GetProperty("responses").TryGetProperty("401", out _), "unauthorized result metadata");
            using HttpResponseMessage denied = await client.GetAsync(new Uri("/archive/restricted", UriKind.Relative));
            Require(denied.StatusCode == HttpStatusCode.Unauthorized, "unauthorized response");
            foreach (string prefix in new[] { "/archive", "/mvc/archive" })
            {
                foreach (string suffix in new[] { "/file", "/stream" })
                {
                    string path = prefix + suffix;
                    JsonElement schema = Resolve(root, Content(root, path, "get").GetProperty("application/zip").GetProperty("schema"));
                    Require(schema.GetProperty("type").GetString() == "string" && schema.GetProperty("format").GetString() == "binary", path + " binary schema");
                    using HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative));
                    Require(response.StatusCode == HttpStatusCode.OK, path + " status");
                    Require(response.Content.Headers.ContentType?.MediaType == "application/zip", path + " media");
                    Require(response.Content.Headers.ContentDisposition?.FileNameStar == "soundings.zip", path + " filename");
                    Require((await response.Content.ReadAsByteArrayAsync()).SequenceEqual(Archive.Bytes), path + " bytes");
                }
                if (newer)
                {
                    JsonElement content = Content(root, prefix + "/summary", "get");
                    Require(content.EnumerateObject().Select(p => p.Name).Order().SequenceEqual(ExpectedMedia), "all media, no stale declarations");
                    JsonElement[] cases = content.GetProperty("application/json").GetProperty("schema").GetProperty("anyOf").EnumerateArray().ToArray();
                    Require(cases.Length == 2, "both same-media schemas");
                    Require(cases.Any(c => Resolve(root, c).GetProperty("properties").TryGetProperty("count", out _)), "reading schema");
                    Require(cases.Any(c => Resolve(root, c).GetProperty("properties").TryGetProperty("reason", out _)), "empty schema");
                    Require(Resolve(root, content.GetProperty("text/plain").GetProperty("schema")).GetProperty("type").GetString() == "string", "text schema");
                }
                foreach (string kind in new[] { "json", "empty", "text" })
                {
                    string path = newer ? prefix + "/summary?kind=" + kind : prefix + "/summary/" + kind;
                    if (!newer) { Require(Content(root, path, "get").TryGetProperty(kind == "text" ? "text/plain" : "application/json", out _), "older separate contract"); }
                    await ArchiveClient.ReadAsync(client, path, kind);
                }
            }

            using HttpRequestMessage query = new(new HttpMethod("QUERY"), "/archive/search") { Content = JsonContent.Create(new SearchFilter("Sound", 2)) };
            using HttpResponseMessage result = await client.SendAsync(query);
            Require(result.StatusCode == HttpStatusCode.OK, "QUERY request");
            ReadingSummary[] matches = (await result.Content.ReadFromJsonAsync<ReadingSummary[]>())!;
            Require(matches.Length == 1 && matches[0] == new ReadingSummary("Soundings", 2), "QUERY body binding and search");
            JsonElement search = root.GetProperty("paths").GetProperty("/archive/search");
            if (newer)
            {
                JsonElement operation = search.GetProperty("x-oai-additionalOperations").GetProperty("QUERY");
                CheckQueryBody(root, operation);
                Require(!search.TryGetProperty("query", out _), "no native query in 3.1");
                using JsonDocument v32 = JsonDocument.Parse(await client.GetStringAsync(new Uri("/openapi/v32.json", UriKind.Relative)));
                Require(v32.RootElement.GetProperty("openapi").GetString() == "3.2.0", "3.2 version");
                JsonElement native = v32.RootElement.GetProperty("paths").GetProperty("/archive/search");
                CheckQueryBody(v32.RootElement, native.GetProperty("query"));
                Require(!native.TryGetProperty("x-oai-additionalOperations", out _), "native QUERY instead of extension");
                using JsonDocument v30 = JsonDocument.Parse(await client.GetStringAsync(new Uri("/openapi/v30.json", UriKind.Relative)));
                JsonElement legacy = v30.RootElement.GetProperty("paths").GetProperty("/archive/search");
                CheckQueryBody(v30.RootElement, legacy.GetProperty("x-oai-additionalOperations").GetProperty("QUERY"));
                Require(!legacy.TryGetProperty("query", out _), "no native query in 3.0");
            }
            else
            {
                Require(!search.TryGetProperty("query", out _) && !search.TryGetProperty("x-oai-additionalOperations", out _), "older generator omits QUERY");
                CheckQueryBody(root, search.GetProperty("post"));
                using HttpResponseMessage post = await client.PostAsJsonAsync("/archive/search", new SearchFilter("Sound", 2));
                Require((await post.Content.ReadFromJsonAsync<ReadingSummary[]>())!.SequenceEqual(matches), "POST comparison");
            }
            // Optional durable input for the separate, real generated-client check.
            string? output = Environment.GetEnvironmentVariable("ARCHIVE_CONTRACT_OUTPUT");
            if (output is not null)
            {
                Directory.CreateDirectory(output);
                string version = newer ? "v30" : "v31";
                await File.WriteAllTextAsync(Path.Combine(output, "openapi.json"), await client.GetStringAsync(new Uri($"/openapi/{version}.json", UriKind.Relative)));
            }
            if (Environment.GetEnvironmentVariable("ARCHIVE_CONTRACT_SERVE") == "1")
            {
                Console.WriteLine("ARCHIVE_ADDRESS=" + address);
                await app.WaitForShutdownAsync();
            }
            Console.WriteLine($"PASS .NET {Environment.Version}: four binary file contracts and HTTP bytes; QUERY body and version-specific operations; Minimal API/MVC response variants and typed HttpClient consumer. No mocks or cross-compilation.");
        }
        finally { await app.StopAsync(); await app.DisposeAsync(); }
    }

    private static void CheckQueryBody(JsonElement root, JsonElement operation)
    {
        JsonElement body = operation.GetProperty("requestBody");
        Require(body.GetProperty("required").GetBoolean(), "required QUERY body");
        JsonElement schema = Resolve(root, body.GetProperty("content").GetProperty("application/json").GetProperty("schema"));
        Require(schema.GetProperty("properties").TryGetProperty("prefix", out _) && schema.GetProperty("properties").TryGetProperty("minimumCount", out _), "QUERY body schema");
        Require(operation.GetProperty("responses").TryGetProperty("200", out _), "QUERY response");
    }

    private static JsonElement Content(JsonElement root, string path, string method) =>
        root.GetProperty("paths").GetProperty(path).GetProperty(method).GetProperty("responses").GetProperty("200").GetProperty("content");

    private static JsonElement Resolve(JsonElement root, JsonElement schema) => schema.TryGetProperty("$ref", out JsonElement reference)
        ? root.GetProperty("components").GetProperty("schemas").GetProperty(reference.GetString()!.Split('/')[^1]) : schema;

    internal static void Require(bool condition, string claim)
    {
        if (!condition) { throw new InvalidOperationException("Contract mismatch: " + claim); }
    }
}

// snippet:consumer
internal static class ArchiveClient
{
    public static async Task ReadAsync(HttpClient client, string path, string kind)
    {
        using HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative));
        response.EnsureSuccessStatusCode();
        if (kind == "text")
        {
            ContractChecks.Require(response.Content.Headers.ContentType?.MediaType == "text/plain" && await response.Content.ReadAsStringAsync() == "Soundings: 2", "plain-text consumer");
        }
        else
        {
            ContractChecks.Require(response.Content.Headers.ContentType?.MediaType == "application/json", "JSON consumer media");
            using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (body.RootElement.TryGetProperty("reason", out _))
            {
                EmptySummary empty = body.RootElement.Deserialize<EmptySummary>(JsonSerializerOptions.Web)!;
                ContractChecks.Require(kind == "empty" && empty.Reason == "No readings", "empty variant consumer");
            }
            else
            {
                ReadingSummary reading = body.RootElement.Deserialize<ReadingSummary>(JsonSerializerOptions.Web)!;
                ContractChecks.Require(kind == "json" && reading == new ReadingSummary("Soundings", 2), "reading variant consumer");
            }
        }
    }
}
// endsnippet:consumer
// endsnippet:complete
