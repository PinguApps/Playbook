// snippet:complete
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticAssets;

const string route = "exhibits/amber.html";
ArtifactIdentity.Verify();
var generatedPath = Path.Combine(AppContext.BaseDirectory, "AmberGallery.staticwebassets.endpoints.json");
var generatedBytes = await File.ReadAllBytesAsync(generatedPath);
var generated = JsonNode.Parse(generatedBytes)!.AsObject();
Check(generated["Version"]!.GetValue<int>() == 1 && generated["ManifestType"]!.GetValue<string>() == "Build", "SDK build schema");
var original = generated["Endpoints"]!.AsArray().Single(node =>
    node!["Route"]!.GetValue<string>() == route && node["Selectors"]!.AsArray().Count == 0)!;
var label = generated["Endpoints"]!.AsArray().Single(node =>
    node!["Route"]!.GetValue<string>() == "labels/amber.html" && node["Selectors"]!.AsArray().Count == 0)!;
Console.WriteLine($"GENERATED sha256={Hash(generatedBytes)} endpoint={original.ToJsonString()}");

var temporary = Path.Combine(Path.GetTempPath(), "amber-gallery-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(temporary);
try
{
    foreach (var scenario in new[] { "absent", "ignored-order", "convention-wins", "convention-yields" })
    {
        // Copy the SDK-produced envelope and the exact uncompressed asset descriptor.
        // Retain its AssetFile, headers and properties; never edit obj or the generated output.
        var manifest = generated.DeepClone().AsObject();
        var asset = original.DeepClone().AsObject();
        asset.Remove("Order");
        if (scenario == "ignored-order") asset["Order"] = "50";
        var competing = label.DeepClone().AsObject();
        competing["Route"] = route;
        manifest["Endpoints"] = new JsonArray(asset, competing);
        var path = Path.Combine(temporary, scenario + ".json");
        var input = JsonSerializer.SerializeToUtf8Bytes(manifest);
        await File.WriteAllBytesAsync(path, input);
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = [], EnvironmentName = "Production", ContentRootPath = AppContext.BaseDirectory,
            WebRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot")
        });
        builder.Logging.ClearProviders();
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        await using var app = builder.Build();
        var mapped = app.MapStaticAssets(path);
        mapped.Add(endpoint =>
        {
            if (endpoint is RouteEndpointBuilder routed)
            {
                var descriptor = endpoint.Metadata.OfType<StaticAssetDescriptor>().Single();
                if (descriptor.AssetPath == "labels/amber.html") routed.Order = 0;
                else if (scenario.StartsWith("convention-", StringComparison.Ordinal))
                    routed.Order = scenario == "convention-wins" ? -200 : 50;
            }
        });
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>().Where(item => item.Metadata.GetMetadata<StaticAssetDescriptor>() is not null).ToArray();
        Check(endpoints.Length == 2, "two actual manifest endpoints");
        var endpoint = endpoints.Single(item => item.Metadata.GetMetadata<StaticAssetDescriptor>()?.AssetPath == route);
        Check(endpoints.Single(item => item != endpoint).Order == 0, "competing manifest order");
        var expectedOrder = scenario == "convention-wins" ? -200 :
            scenario == "convention-yields" ? 50 : -100;
        Check(endpoint.Order == expectedOrder, "framework manifest order");
        var descriptor = endpoint.Metadata.GetMetadata<StaticAssetDescriptor>()!;
        try { descriptor.Route = route; throw new InvalidOperationException("Frozen descriptor accepted mutation"); }
        catch (InvalidOperationException error) when (error.Message.Contains("frozen", StringComparison.Ordinal)) { }
        await app.StartAsync();
        try
        {
            var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using var client = new HttpClient { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(10) };
            using var response = await client.GetAsync(new Uri("/" + route, UriKind.Relative));
            var body = await response.Content.ReadAsStringAsync();
            var selectedAsset = expectedOrder < 0 ? route : "labels/amber.html";
            var expected = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "wwwroot", selectedAsset));
            Check(response.StatusCode == HttpStatusCode.OK && body == expected, "actual competing-route response");
            using var head = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, "/" + route));
            Check(head.StatusCode == HttpStatusCode.OK && (await head.Content.ReadAsByteArrayAsync()).Length == 0, "static HEAD");
            Console.WriteLine($"PASS {scenario}: input={manifest.ToJsonString()} sha256={Hash(input)} order={endpoint.Order} HTTP={(int)response.StatusCode} body={JsonSerializer.Serialize(body)} frozen=reject");
        }
        finally { await app.StopAsync(); }
        Check(Hash(await File.ReadAllBytesAsync(path)) == Hash(input), "isolated input unchanged");
    }
    Check(Hash(await File.ReadAllBytesAsync(generatedPath)) == Hash(generatedBytes), "SDK output unchanged");
    Console.WriteLine("PASS AmberGallery: older generated manifest, ignored unknown Order, public conventions, real HTTP and frozen descriptor.");
}
finally { Directory.Delete(temporary, recursive: true); }

static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
// endsnippet:complete
