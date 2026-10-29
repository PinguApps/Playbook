// snippet:complete
using MarineRoutes;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
await using var app = builder.Build();
app.MapControllers();
app.MapGet("/minimal/specimens/{id:int:min(1)}", (HttpContext context) => MarineValues.Read(context));
app.MapGet("/minimal/expeditions/{season?}", (HttpContext context) => MarineValues.Read(context));
app.MapGet("/minimal/charts/{**sheet}", (HttpContext context) => MarineValues.Read(context));
app.MapGet("/minimal/stations/{basin=pelagic}", (HttpContext context) => MarineValues.Read(context));
await app.StartAsync();
try
{
    // The collection is produced by the actual MVC and endpoint providers after startup.
    var descriptions = app.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>()
        .ApiDescriptionGroups.Items.SelectMany(group => group.Items).ToArray();
    Check(descriptions.Length == 8, "eight discovered routes");
    foreach (var description in descriptions)
    {
        var contract = description.ReadMarineContract();
        Console.WriteLine(JsonSerializer.Serialize(new { description.RelativePath, contract }));
        Check(contract is not null, "built-in provider supplies usable source");
    }
    foreach (var prefix in new[] { "mvc", "minimal" })
    {
        ApiDescription Find(string part) => descriptions.Single(item =>
            item.RelativePath?.StartsWith(prefix + "/" + part + "/", StringComparison.Ordinal) == true);
        var constrained = Find("specimens").ReadMarineContract()!;
        var id = constrained.Parameters.Single();
        Check(id.Name == "id" && id.Policies.Count == 2 && id.Policies[0] == "int" && id.Policies[1] == "min(1)", "constraint declarations");
        Check(Find("expeditions").ReadMarineContract()!.Parameters.Single().Optional, "optional marker");
        Check(Find("charts").ReadMarineContract()!.Parameters.Single().CatchAll, "catch-all marker");
        var defaults = Find("stations").ReadMarineContract()!;
        Check(defaults.Defaults["basin"] == "pelagic", "parsed default dictionary");
        Check(defaults.Parameters.Single().Default == "pelagic", "parameter default");
    }
    // RelativePath is presentation metadata. MVC has removed its source constraints.
    var displayed = descriptions.Single(item => item.RelativePath == "mvc/specimens/{id}");
    var lossy = RoutePatternFactory.Parse(displayed.RelativePath!);
    Check(lossy.Parameters.Single().ParameterPolicies.Count == 0, "display parsing loses MVC constraints");
    Check(displayed.ReadMarineContract()!.Parameters.Single().Policies.Count == 2, "extension retains source constraints");
    var legacy = new ApiDescription { RelativePath = null };
    Check(legacy.ReadMarineContract() is null, "legacy null control");
    legacy.RelativePath = "legacy/{id:int}";
    Check(legacy.ReadMarineContract()!.Parameters.Single().Policies.Single() == "int", "bounded display fallback");
    legacy.RelativePath = "legacy/{broken";
    Check(legacy.ReadMarineContract() is null, "malformed legacy display is unavailable");

    var address = app.Services.GetRequiredService<IServer>().Features
        .Get<IServerAddressesFeature>()!.Addresses.Single();
    using var client = new HttpClient { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(15) };
    foreach (var prefix in new[] { "mvc", "minimal" })
    {
        await Match($"/{prefix}/specimens/7", "id", "7");
        await Reject($"/{prefix}/specimens/0");
        await Reject($"/{prefix}/specimens/not-a-number");
        await Match($"/{prefix}/expeditions", "season", null);
        await Match($"/{prefix}/expeditions/winter", "season", "winter");
        await Match($"/{prefix}/charts", "sheet", null);
        await Match($"/{prefix}/charts/deep/shelf/atlas", "sheet", "deep/shelf/atlas");
        await Match($"/{prefix}/stations", "basin", "pelagic");
        await Match($"/{prefix}/stations/coastal", "basin", "coastal");
    }
    ArtifactIdentity.Verify();
    Console.WriteLine("PASS: actual MVC + Minimal discovery, extension constraints/optional/catch-all/defaults, lossy display/null controls; 18 HTTP matching/nonmatching requests.");

    async Task Match(string path, string parameter, string? expected)
    {
        using var response = await client.GetAsync(new Uri(path, UriKind.Relative));
        Check(response.StatusCode == HttpStatusCode.OK, $"200 {path}");
        var values = (await response.Content.ReadFromJsonAsync<Dictionary<string, string?>>())!;
        values.TryGetValue(parameter, out var actual);
        Check(actual == expected, $"route value {path}: {parameter}={expected ?? "<absent>"}");
        Console.WriteLine($"HTTP {path}: 200 {parameter}={actual ?? "<absent>"}");
    }
    async Task Reject(string path)
    {
        using var response = await client.GetAsync(new Uri(path, UriKind.Relative));
        Check(response.StatusCode == HttpStatusCode.NotFound, $"constraint rejects {path}");
        Console.WriteLine($"HTTP {path}: 404");
    }
}
finally { await app.StopAsync(); }

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

// endsnippet:complete
