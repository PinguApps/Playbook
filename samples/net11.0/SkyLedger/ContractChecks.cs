// snippet:complete
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace SkyLedger;

internal static class ContractChecks
{
    public static async Task RunAsync(WebApplication app, bool newer)
    {
        // The build-time tool replaces Kestrel; leave its host startup untouched.
        if (System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider")
        {
            await app.RunAsync();
            return;
        }
        app.Urls.Add("http://127.0.0.1:0");
        await app.StartAsync();
        try
        {
            string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using HttpClient http = new() { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(30) };
            foreach ((string name, string version) in new[] { ("v1", newer ? "3.2." : "3.1."), ("v31", "3.1."), ("v30", "3.0."), ("client", "3.0.") })
            {
                using JsonDocument document = JsonDocument.Parse(await http.GetStringAsync(new Uri("/openapi/" + name + ".json", UriKind.Relative)));
                Require(document.RootElement.GetProperty("openapi").GetString()!.StartsWith(version, StringComparison.Ordinal), name + " version");
                if (newer && name == "v1")
                {
                    JsonElement media = document.RootElement.GetProperty("paths").GetProperty("/observatory/events").GetProperty("get").GetProperty("responses").GetProperty("200").GetProperty("content").GetProperty("text/event-stream");
                    JsonElement item = media.GetProperty("itemSchema");
                    JsonElement properties = item.GetProperty("properties");
                    Require(item.GetProperty("required").EnumerateArray().Any(p => p.GetString() == "data"), "required event data");
                    string reference = properties.GetProperty("data").GetProperty("$ref").GetString()!;
                    JsonElement payload = document.RootElement.GetProperty("components").GetProperty("schemas").GetProperty(reference.Split('/')[^1]);
                    Require(HasType(payload.GetProperty("properties").GetProperty("exposure"), "integer"), "typed event payload");
                    Require(HasType(properties.GetProperty("event"), "string") && HasType(properties.GetProperty("id"), "string"), "SSE event and id schema");
                }
                if (newer && name == "v31")
                {
                    JsonElement media = document.RootElement.GetProperty("paths").GetProperty("/observatory/events").GetProperty("get").GetProperty("responses").GetProperty("200").GetProperty("content").GetProperty("text/event-stream");
                    Require(!media.TryGetProperty("itemSchema", out _) && media.TryGetProperty("x-oai-itemSchema", out _), "older spec extension is not standard itemSchema");
                }
            }
            using HttpResponseMessage response = await http.GetAsync(new Uri("/observatory/events", UriKind.Relative), HttpCompletionOption.ResponseHeadersRead);
            Require(response.Content.Headers.ContentType?.MediaType == "text/event-stream", "SSE media");
            using Stream stream = await response.Content.ReadAsStreamAsync();
            using StreamReader reader = new(stream);
            List<string> frames = [];
            string frame = "";
            while (await reader.ReadLineAsync() is string line)
            {
                if (line.Length == 0) { if (frame.Length > 0) { frames.Add(frame); frame = ""; } }
                else { frame += line + "\n"; }
            }
            Require(frames.Count == 2, "two separate SSE frames");
            for (int index = 0; index < frames.Count; index++)
            {
                string[] lines = frames[index].Split('\n', StringSplitOptions.RemoveEmptyEntries);
                Require(lines.Contains("event: alignment") && lines.Contains("id: scan-" + (41 + index)), "event name and id");
                using JsonDocument data = JsonDocument.Parse(lines.Single(l => l.StartsWith("data: ", StringComparison.Ordinal))[6..]);
                Require(data.RootElement.GetProperty("dome").GetString() == "West dome" && data.RootElement.GetProperty("exposure").GetInt32() == 7 + index, "real event payload");
            }
            using HttpResponseMessage raw = await http.GetAsync(new Uri("/observatory/raw", UriKind.Relative));
            Require(raw.Content.Headers.ContentType?.MediaType == "application/json", "raw async enumerable is JSON");
            using JsonDocument rawBody = JsonDocument.Parse(await raw.Content.ReadAsStringAsync());
            Require(rawBody.RootElement.ValueKind == JsonValueKind.Array && rawBody.RootElement.GetArrayLength() == 2, "raw JSON array");
            Require(await http.GetFromJsonAsync<DomeStatus>("/observatory/status") == new DomeStatus("West dome", 7), "ordinary typed endpoint");
            using JsonDocument environment = JsonDocument.Parse(await http.GetStringAsync(new Uri("/openapi/environment.json", UriKind.Relative)));
            Require(environment.RootElement.GetProperty("info").GetProperty("title").GetString() == "Sky ledger: " + app.Environment.EnvironmentName, "runtime transformer environment");
            Require(environment.RootElement.GetProperty("paths").TryGetProperty("/observatory/calibration", out _) == app.Environment.IsDevelopment(), "runtime environment endpoint inclusion");
            if (Environment.GetEnvironmentVariable("SKY_LEDGER_SERVE") == "1")
            {
                Console.WriteLine("SKY_ADDRESS=" + address);
                await app.WaitForShutdownAsync();
            }
            Console.WriteLine($"PASS {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}: real SSE frames/data/id, raw JSON, generated default and explicit versions; itemSchema checked={newer}.");
        }
        finally { await app.StopAsync(); await app.DisposeAsync(); }
    }

    private static void Require(bool condition, string claim)
    {
        if (!condition) { throw new InvalidOperationException(claim); }
    }

    private static bool HasType(JsonElement schema, string expected)
    {
        JsonElement type = schema.GetProperty("type");
        return type.ValueKind == JsonValueKind.Array ? type.EnumerateArray().Any(t => t.GetString() == expected) : type.GetString() == expected;
    }
}
// endsnippet:complete
