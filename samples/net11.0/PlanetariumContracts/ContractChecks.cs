// snippet:complete
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.OpenApi;

namespace Planetarium;

internal static class ContractChecks
{
    public static async Task RunAsync(WebApplication app, string scenario, bool newer)
    {
        // GetDocument runs all real registration, but owns the mock server lifecycle.
        if (Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider")
        {
            await app.RunAsync();
            return;
        }
        Assembly openApi = typeof(OpenApiOptions).Assembly;
        Require((openApi.GetType("Microsoft.AspNetCore.OpenApi.IAdditionalOpenApiDocumentNameResolver") is not null) == newer, "actual resolver availability");
        Require(typeof(Microsoft.Extensions.DependencyInjection.OpenApiServiceCollectionExtensions).GetMethods()
            .Count(method => method.Name == "AddOpenApiCore") == (newer ? 1 : 0), "actual core registration availability");
        // Observe the same real provider used reflectively by GetDocument, not the resolver alone.
        Type providerType = openApi.GetType("Microsoft.Extensions.ApiDescriptions.IDocumentProvider", throwOnError: true)!;
        object provider = app.Services.GetRequiredService(providerType);
        string[] names = ((IEnumerable<string>)providerType.GetMethod("GetDocumentNames")!.Invoke(provider, null)!).ToArray();
        string[] expected = scenario switch
        {
            "missing" => ["public"],
            "case" => ["public", "staff", "PUBLIC"],
            _ => ["public", "staff"]
        };
        Require(names.SequenceEqual(expected, StringComparer.Ordinal), "provider names/order/exact duplicate elimination");
        Console.WriteLine("PROVIDER=" + string.Join(",", names));
        Console.WriteLine("ARTIFACT=" + openApi.FullName + "; runtime=" + Environment.Version);
        app.Urls.Add("http://127.0.0.1:0");
        await app.StartAsync();
        try
        {
            string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
            using HttpClient http = new() { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(30) };
            foreach (string name in new[] { "public", "staff" })
            {
                using HttpResponseMessage response = await http.GetAsync(new Uri("/openapi/" + name + ".json", UriKind.Relative));
                if (name == "staff" && scenario == "missing")
                {
                    Require(response.StatusCode == HttpStatusCode.NotFound, "configured options without discovered name gives 404");
                    continue;
                }
                response.EnsureSuccessStatusCode();
                string title = "Planetarium " + name + (scenario == "explicit" && name == "public" ? " (registered)" : "");
                CheckDocument(await response.Content.ReadAsStringAsync(), name, title);
            }
            using HttpResponseMessage absent = await http.GetAsync(new Uri("/openapi/absent.json", UriKind.Relative));
            Require(absent.StatusCode == HttpStatusCode.NotFound, "unknown name gives 404");
            using HttpResponseMessage upper = await http.GetAsync(new Uri("/openapi/PUBLIC.json", UriKind.Relative));
            upper.EnsureSuccessStatusCode();
            CheckDocument(await upper.Content.ReadAsStringAsync(), "public", "Planetarium public" + (scenario == "explicit" ? " (registered)" : ""));
            Require((await http.GetStringAsync(new Uri("/planetarium/shows", UriKind.Relative))).Contains("Winter sky", StringComparison.Ordinal), "public handler");
            Require((await http.GetStringAsync(new Uri("/planetarium/projector", UriKind.Relative))).Contains("312", StringComparison.Ordinal), "staff handler");
            Console.WriteLine("PASS runtime " + scenario + ": names, 3.0 options, titles, distinct paths, missing/unknown 404 and case-insensitive HTTP lookup; handlers remain callable.");
        }
        finally { await app.StopAsync(); await app.DisposeAsync(); }
    }

    private static void CheckDocument(string json, string name, string title)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;
        Require(root.GetProperty("openapi").GetString()!.StartsWith("3.0.", StringComparison.Ordinal), "named version");
        Require(root.GetProperty("info").GetProperty("title").GetString() == title, "named title");
        string[] paths = root.GetProperty("paths").EnumerateObject().Select(property => property.Name).ToArray();
        Require(paths.SequenceEqual(new[] { name == "public" ? "/planetarium/shows" : "/planetarium/projector" }), "audience path isolation");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) { throw new InvalidOperationException(message); }
    }
}
// endsnippet:complete
