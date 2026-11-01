using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Playbook.Samples;

internal static class PlanetariumChecks
{
    public static void Run()
    {
        // Resolve an actual type from each moved assembly, without an implementation package.
        Type[] contracts = [typeof(IDistributedCache), typeof(IConfiguration), typeof(IServiceCollection),
            typeof(IMetricsBuilder), typeof(IFileProvider), typeof(IHost), typeof(ILogger),
            typeof(IOptions<>), typeof(StringValues)];
        foreach (Type contract in contracts)
        {
            Assembly assembly = contract.Assembly;
            string path = assembly.Location;
            Console.WriteLine("ASSEMBLY " + JsonSerializer.Serialize(new { name = assembly.GetName().Name,
                path, version = System.Diagnostics.FileVersionInfo.GetVersionInfo(path).ProductVersion,
                sha256 = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path))) }));
        }
        StringValues slots = new(["dusk", "midnight"]);
        if (slots.Count != 2 || Options.Create("dusk").Value != "dusk") throw new InvalidOperationException("Schedule contract failed");
        Console.WriteLine("PASS PlanetariumContracts: nine actual assemblies; dusk/midnight schedule");
    }
}
