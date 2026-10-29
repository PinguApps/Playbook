// snippet:complete
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.AspNetCore.StaticAssets;

internal static class ArtifactIdentity
{
    public static void Verify()
    {
        const string sdk = "11.0.100-rc.2.26504.105";
        const string framework = "11.0.0-rc.2.26504.105";
        const string target = "net11.0";
        const string auditedReference = "45474da98fbd627eaaa354b75b7ab8ebd63112f4d2400d6c0c6f6035c9d177ba";
        var assembly = typeof(StaticAssetDescriptor).Assembly;
        var installation = Directory.GetParent(Path.GetDirectoryName(assembly.Location)!)!.Parent!.Parent!.FullName;
        foreach (var loaded in new[] { typeof(object).Assembly, assembly })
        {
            var version = loaded.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
            if (new DirectoryInfo(Path.GetDirectoryName(loaded.Location)!).Name != framework)
                throw new InvalidOperationException("Unexpected loaded framework: " + loaded.Location);
            Console.WriteLine($"LOADED {loaded.Location} | {version} | {Hash(loaded.Location)}");
        }
        var reference = Path.Combine(installation, "packs", "Microsoft.AspNetCore.App.Ref", framework, "ref", target, "Microsoft.AspNetCore.StaticAssets.dll");
        if (Hash(reference) != auditedReference) throw new InvalidOperationException("StaticAssets differs from PIN-709 audit");
        Console.WriteLine($"AUDIT-MATCH {reference} | {auditedReference}");
        foreach (var relative in new[]
        {
            "Roslyn/bincore/csc.dll",
            "Sdks/Microsoft.NET.Sdk.StaticWebAssets/tasks/net11.0/Microsoft.NET.Sdk.StaticWebAssets.Tasks.dll",
            "Sdks/Microsoft.NET.Sdk.StaticWebAssets/targets/Microsoft.NET.Sdk.StaticWebAssets.targets"
        })
        {
            var path = Path.Combine(installation, "sdk", sdk, relative);
            Console.WriteLine($"SDK-ARTIFACT {path} | {FileVersionInfo.GetVersionInfo(path).ProductVersion} | {Hash(path)}");
        }
        foreach (var path in new[]
        {
            Path.Combine(installation, "dotnet" + (OperatingSystem.IsWindows() ? ".exe" : "")),
            Path.Combine(installation, "shared", "Microsoft.NETCore.App", framework, OperatingSystem.IsWindows() ? "coreclr.dll" : "libcoreclr.so"),
            Path.Combine(installation, "host", "fxr", framework, OperatingSystem.IsWindows() ? "hostfxr.dll" : "libhostfxr.so")
        }) Console.WriteLine($"NATIVE-HOST {path} | {FileVersionInfo.GetVersionInfo(path).ProductVersion} | {Hash(path)}");
        Console.WriteLine($"PLATFORM {RuntimeInformation.OSDescription} | {RuntimeInformation.ProcessArchitecture}; packages=[] workloads=[]");
        var property = typeof(StaticAssetDescriptor).GetProperty("Order");
        if (property?.PropertyType != typeof(string) || !property.CanRead || !property.CanWrite ||
            new NullabilityInfoContext().Create(property).ReadState != NullabilityState.Nullable ||
            new NullabilityInfoContext().Create(property).WriteState != NullabilityState.Nullable)
            throw new InvalidOperationException("Expected nullable string Order get/set");
        if (property.GetCustomAttributesData().Any(attribute => attribute.AttributeType.Name == "ExperimentalAttribute"))
            throw new InvalidOperationException("Revisit experimental requirements");
    }

    private static string Hash(string path) => Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path)));
}
// endsnippet:complete
