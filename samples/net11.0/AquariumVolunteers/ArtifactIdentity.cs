// snippet:complete
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;

internal static class ArtifactIdentity
{
    public static void Verify()
    {
#if NET10_0
        const string sdk = "10.0.402";
        const string framework = "10.0.13";
        const string uiHash = "fbd7bbc2665767a342fdec21e1c13edb89f5e33b342a37c5b18487cf7613ba00";
#else
        const string sdk = "11.0.100-rc.2.26504.105";
        const string framework = "11.0.0-rc.2.26504.105";
        const string uiHash = "5dc1a5919cf11e62214f32537c7ea8fa4a56623e2d0ebaa51a00d21d24203249";
#endif
        var identity = typeof(SignInManager<IdentityUser>).Assembly;
        var installation = Directory.GetParent(Path.GetDirectoryName(identity.Location)!)!.Parent!.Parent!.FullName;
        foreach (var loaded in new[] { typeof(object).Assembly, identity, typeof(IdentityUser).Assembly,
            typeof(Microsoft.Extensions.DependencyInjection.IdentityServiceCollectionUIExtensions).Assembly,
            typeof(Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext).Assembly,
            typeof(SqliteConnection).Assembly })
        {
            Console.WriteLine($"LOADED {loaded.GetName().Name} | {loaded.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion} | {Hash(loaded.Location)}");
        }
        if (new DirectoryInfo(Path.GetDirectoryName(identity.Location)!).Name != framework ||
            new DirectoryInfo(Path.GetDirectoryName(typeof(object).Assembly.Location)!).Name != framework)
            throw new InvalidOperationException("Unexpected runtime/framework baseline");
        var ui = typeof(Microsoft.Extensions.DependencyInjection.IdentityServiceCollectionUIExtensions).Assembly;
        if (Hash(ui.Location) != uiHash) throw new InvalidOperationException("Identity.UI bytes differ from PIN-709 audit");
        Console.WriteLine($"AUDIT-MATCH Identity.UI | {uiHash}");
        var compiler = Path.Combine(installation, "sdk", sdk, "Roslyn", "bincore", "csc.dll");
        Console.WriteLine($"COMPILER {sdk} | {FileVersionInfo.GetVersionInfo(compiler).ProductVersion} | {Hash(compiler)}");
        foreach (var path in new[]
        {
            Path.Combine(installation, "dotnet" + (OperatingSystem.IsWindows() ? ".exe" : "")),
            Path.Combine(installation, "shared", "Microsoft.NETCore.App", framework, OperatingSystem.IsWindows() ? "coreclr.dll" : "libcoreclr.so"),
            Path.Combine(installation, "host", "fxr", framework, OperatingSystem.IsWindows() ? "hostfxr.dll" : "libhostfxr.so")
        }) Console.WriteLine($"NATIVE-HOST {Path.GetFileName(path)} | {FileVersionInfo.GetVersionInfo(path).ProductVersion} | {Hash(path)}");
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        Console.WriteLine($"SQLITE {connection.ServerVersion} | {RuntimeInformation.OSDescription} | {RuntimeInformation.ProcessArchitecture}");
        foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
            if (Path.GetFileName(module.FileName).Contains("e_sqlite3", StringComparison.OrdinalIgnoreCase))
                Console.WriteLine($"NATIVE-PROVIDER {Path.GetFileName(module.FileName)} | {Hash(module.FileName)}");
    }
    private static string Hash(string path) => Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path)));
}
// endsnippet:complete
