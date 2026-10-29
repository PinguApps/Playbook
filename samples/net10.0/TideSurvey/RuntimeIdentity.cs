// snippet:complete
using System.Reflection;
using System.Security.Cryptography;

internal static class RuntimeIdentity
{
    public static void Verify()
    {
#if NET10_0
        const string framework = "10.0.13";
#else
        const string framework = "11.0.0-rc.2.26504.105";
#endif
        foreach (var assembly in new[] { typeof(object).Assembly, typeof(WebApplication).Assembly, typeof(Microsoft.AspNetCore.Http.Features.IHttpRequestFeature).Assembly })
        {
            if (new DirectoryInfo(Path.GetDirectoryName(assembly.Location)!).Name != framework)
                throw new InvalidOperationException("Unexpected loaded framework");
            Console.WriteLine($"LOADED {assembly.GetName().Name} | {assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion} | {assembly.Location} | {Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(assembly.Location)))}");
        }
    }
}
// endsnippet:complete
