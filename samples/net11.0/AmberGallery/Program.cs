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
    foreach (var scenario in new[] { "absent", "null", "empty", "wins", "yields", "equal", "invalid", "overflow", "number", "convention-wins", "convention-yields" })
    {
        // Copy the SDK-produced envelope and the exact uncompressed asset descriptor.
        // Retain its AssetFile, headers and properties; never edit obj or the generated output.
        var manifest = generated.DeepClone().AsObject();
        var asset = original.DeepClone().AsObject();
        asset.Remove("Order");
        var authored = JsonSerializer.Deserialize<StaticAssetDescriptor>(asset.ToJsonString())!;
        authored.Order = scenario switch
        {
            "null" => null,
            "empty" => "",
            "wins" => "-200",
            "yields" => "50",
            "equal" => "0",
            "invalid" => "curator-first",
            "overflow" => "2147483648",
            _ => null
        };
        asset = JsonSerializer.SerializeToNode(authored)!.AsObject();
        if (scenario == "null") asset["Order"] = null;
        if (scenario == "number") asset["Order"] = JsonValue.Create(50);
        var competing = JsonSerializer.Deserialize<StaticAssetDescriptor>(label.ToJsonString())!;
        competing.Route = route;
        competing.Order = "0";
        manifest["Endpoints"] = new JsonArray(asset, JsonSerializer.SerializeToNode(competing));
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
        var ambiguousMatches = 0;
        app.Use(async (context, next) =>
        {
            try { await next(context); }
            catch (Exception error) when (scenario == "equal" && error.GetType().Name == "AmbiguousMatchException")
            {
                ambiguousMatches++;
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        });
        app.UseRouting();
        StaticAssetsEndpointConventionBuilder mapped;
        try { mapped = app.MapStaticAssets(path); }
        catch (JsonException) when (scenario == "number")
        {
            Check(Hash(await File.ReadAllBytesAsync(path)) == Hash(input), "rejected input unchanged");
            Console.WriteLine($"PASS number: sha256={Hash(input)} JSON number rejected; Order must be a string or null.");
            continue;
        }
        Check(scenario != "number", "wrong JSON kind must fail");
        if (scenario.StartsWith("convention-", StringComparison.Ordinal))
        {
            mapped.Add(endpoint =>
            {
                // Conventions change the route builder, not the frozen descriptor.
                if (endpoint is RouteEndpointBuilder routed &&
                    endpoint.Metadata.OfType<StaticAssetDescriptor>().Any(item => item.AssetPath == route))
                    routed.Order = scenario == "convention-wins" ? -200 : 50;
            });
        }
        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>().Where(item => item.Metadata.GetMetadata<StaticAssetDescriptor>() is not null).ToArray();
        Check(endpoints.Length == 2, "two actual manifest endpoints");
        var endpoint = endpoints.Single(item => item.Metadata.GetMetadata<StaticAssetDescriptor>()?.AssetPath == route);
        Check(endpoints.Single(item => item != endpoint).Order == 0, "competing manifest order");
        var expectedOrder = scenario is "wins" or "convention-wins" ? -200 :
            scenario is "yields" or "convention-yields" ? 50 : scenario == "equal" ? 0 : -100;
        Check(endpoint.Order == expectedOrder, "framework manifest order");
        var descriptor = endpoint.Metadata.GetMetadata<StaticAssetDescriptor>()!;
        try { descriptor.Order = "-300"; throw new InvalidOperationException("Frozen descriptor accepted mutation"); }
        catch (InvalidOperationException error) when (error.Message.Contains("frozen", StringComparison.Ordinal)) { }
        await app.StartAsync();
        try
        {
            var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using var client = new HttpClient { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(10) };
            using var response = await client.GetAsync(new Uri("/" + route, UriKind.Relative));
            var body = await response.Content.ReadAsStringAsync();
            if (scenario == "equal") Check(response.StatusCode == HttpStatusCode.InternalServerError && ambiguousMatches == 1, "equal literal orders are ambiguous");
            else
            {
                var selectedAsset = expectedOrder < 0 ? route : "labels/amber.html";
                var expected = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "wwwroot", selectedAsset));
                Check(response.StatusCode == HttpStatusCode.OK && body == expected, "actual competing-route response");
            }
            using var head = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, "/" + route));
            if (scenario != "equal") Check(head.StatusCode == HttpStatusCode.OK && (await head.Content.ReadAsByteArrayAsync()).Length == 0, "static HEAD");
            Console.WriteLine($"PASS {scenario}: input={manifest.ToJsonString()} sha256={Hash(input)} order={endpoint.Order} HTTP={(int)response.StatusCode} body={JsonSerializer.Serialize(body)} frozen=reject");
        }
        finally { await app.StopAsync(); }
        Check(Hash(await File.ReadAllBytesAsync(path)) == Hash(input), "isolated input unchanged");
    }
    Check(Hash(await File.ReadAllBytesAsync(generatedPath)) == Hash(generatedBytes), "SDK output unchanged");
    Console.WriteLine("PASS AmberGallery: generated manifest, string/default/invalid order, public conventions, real HTTP and frozen descriptor.");
}
finally { Directory.Delete(temporary, recursive: true); }

static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
// endsnippet:complete
