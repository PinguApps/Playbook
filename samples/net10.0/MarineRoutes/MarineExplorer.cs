// snippet:complete
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

namespace MarineRoutes;

[ApiController]
[Route("mvc")]
public sealed class MarineController : ControllerBase
{
    [HttpGet("specimens/{id:int:min(1)}")]
    public IActionResult Specimen() => new JsonResult(MarineValues.Read(HttpContext));
    [HttpGet("expeditions/{season?}")]
    public IActionResult Expedition() => new JsonResult(MarineValues.Read(HttpContext));
    [HttpGet("charts/{**sheet}")]
    public IActionResult Chart() => new JsonResult(MarineValues.Read(HttpContext));
    [HttpGet("stations/{basin=pelagic}")]
    public IActionResult Station() => new JsonResult(MarineValues.Read(HttpContext));
}

internal static class MarineValues
{
    public static Dictionary<string, string?> Read(HttpContext context) => context.Request.RouteValues
        .Where(pair => pair.Key is not ("controller" or "action"))
        .ToDictionary(pair => pair.Key, pair => pair.Value is null ? null : Convert.ToString(pair.Value, CultureInfo.InvariantCulture));
}

internal sealed record MarineParameter(string Name, bool Optional, bool CatchAll, string? Default, IReadOnlyList<string> Policies);
internal sealed record MarineContract(string Source, IReadOnlyList<MarineParameter> Parameters, Dictionary<string, string?> Defaults);

internal static class MarineExplorerExtensions
{
    public static MarineContract? ReadMarineContract(this ApiDescription description)
    {
        ArgumentNullException.ThrowIfNull(description);
        // MVC display paths discard constraints. Prefer its actual authored attribute template.
        var template = (description.ActionDescriptor as ControllerActionDescriptor)?.AttributeRouteInfo?.Template;
        var source = template is null ? "RelativePath (provider-dependent)" : "MVC attribute template";
        template ??= description.RelativePath;
        if (template is null) return null;
        RoutePattern pattern;
        try { pattern = RoutePatternFactory.Parse(template); }
        catch (RoutePatternException) { return null; }
        if (pattern is null) return null;
        return new MarineContract(source, pattern.Parameters.Select(parameter => new MarineParameter(
            parameter.Name, parameter.IsOptional, parameter.IsCatchAll,
            parameter.Default is null ? null : Convert.ToString(parameter.Default, CultureInfo.InvariantCulture),
            parameter.ParameterPolicies.Select(policy => policy.Content ??
                policy.ParameterPolicy?.GetType().FullName ?? "<unspecified policy>").ToArray())).ToArray(),
            pattern.Defaults.ToDictionary(pair => pair.Key,
                pair => pair.Value is null ? null : Convert.ToString(pair.Value, CultureInfo.InvariantCulture)));
    }
}

internal static class ArtifactIdentity
{
    public static void Verify()
    {
        const string sdk = "10.0.402";
        const string framework = "10.0.13";
        var asp = typeof(ApiDescription).Assembly;
        var runtimeRoot = Directory.GetParent(Path.GetDirectoryName(asp.Location)!)!.Parent!.Parent!.FullName;
        foreach (var assembly in new[] { typeof(object).Assembly, asp, typeof(IApiDescriptionGroupCollectionProvider).Assembly, typeof(RoutePattern).Assembly })
        {
            var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
            if (new DirectoryInfo(Path.GetDirectoryName(assembly.Location)!).Name != framework ||
                (!version.StartsWith(framework + "+", StringComparison.Ordinal) &&
                 !version.StartsWith(framework + "-", StringComparison.Ordinal) && version != framework))
                throw new InvalidOperationException($"Unexpected loaded artifact {assembly.Location}: {version}");
            Console.WriteLine($"LOADED {assembly.Location} | {version} | {Hash(assembly.Location)}");
        }
        var references = new Dictionary<string, string>
        {
            ["Microsoft.AspNetCore.Mvc.Abstractions.dll"] = "ee11b2cb01859705b117836a91351fcef16ead0d396bbd00c90ab022fd38ce61",
            ["Microsoft.AspNetCore.Mvc.ApiExplorer.dll"] = "3abbd398391c4779d94a0ccdf6635da48702660c6c6c9600b817901b00c7778d",
            ["Microsoft.AspNetCore.Routing.dll"] = "479d28d8dfb738800f32eab08f92a7b70872b5a24971fb57b477807a5039995d",
        };
        foreach (var pair in references)
        {
            var file = Path.Combine(runtimeRoot, "packs", "Microsoft.AspNetCore.App.Ref", framework, "ref", "net10.0", pair.Key);
            if (Hash(file) != pair.Value) throw new InvalidOperationException("Audited reference hash mismatch: " + file);
            Console.WriteLine($"AUDIT-MATCH {file} | {pair.Value}");
        }
        var compiler = Path.Combine(runtimeRoot, "sdk", sdk, "Roslyn", "bincore", "csc.dll");
        Console.WriteLine($"COMPILER {compiler} | {System.Diagnostics.FileVersionInfo.GetVersionInfo(compiler).ProductVersion} | {Hash(compiler)}");
        var property = typeof(ApiDescription).GetProperty("RoutePattern");
        if (property is not null) throw new InvalidOperationException("Unexpected .NET10 backport; revisit scope");
    }
    private static string Hash(string path) => Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path)));
}
// endsnippet:complete