using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace Playbook.Samples;

internal static class RuntimeIdentity
{
    public static void Assert()
    {
        string expected = Environment.Version.Major == 10 ? "10.0.13" : "11.0.0-rc.2.26504.105";
        string path = typeof(object).Assembly.Location;
        string product = FileVersionInfo.GetVersionInfo(path).ProductVersion!;
        if (RuntimeInformation.FrameworkDescription != ".NET " + expected || Path.GetFileName(Path.GetDirectoryName(path)) != expected)
            throw new InvalidOperationException($"Runtime drift: expected {expected}, got {product}.");
        Console.WriteLine("RUNTIME " + JsonSerializer.Serialize(new {
            framework = RuntimeInformation.FrameworkDescription, coreLibPath = path,
            product, sha256 = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path))) }));
    }
}
