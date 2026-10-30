// snippet:worker
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("Quay worker requires Linux.");
var os = File.ReadAllLines("/etc/os-release")
    .Where(line => line.Contains('=', StringComparison.Ordinal))
    .Select(line => line.Split('=', 2))
    .ToDictionary(pair => pair[0], pair => pair[1].Trim('"'));
var expectedOs = Environment.GetEnvironmentVariable("QUAY_OS_VERSION")
    ?? throw new InvalidOperationException("Select the expected Azure Linux version explicitly.");
if (os["ID"] != "azurelinux" || os["VERSION_ID"] != expectedOs)
    throw new InvalidOperationException("The authored base image did not match the selected OS.");

// Native dependency is intentional: the manifest service budgets compressed batches.
var library = Environment.GetEnvironmentVariable("QUAY_ZLIB") ?? "libz.so.1";
try
{
    var handle = NativeLibrary.Load(library);
    try
    {
        var bound = Marshal.GetDelegateForFunctionPointer<CompressBound>(NativeLibrary.GetExport(handle, "compressBound"));
        var version = Marshal.GetDelegateForFunctionPointer<ZlibVersion>(NativeLibrary.GetExport(handle, "zlibVersion"));
        var bytes = bound(4096);
        if (bytes < 4096) throw new InvalidOperationException("Invalid compression budget.");
        var french = CultureInfo.GetCultureInfo("fr-FR");
        var fee = 1234.50m.ToString("F2", french);
        if (fee != "1234,50") throw new InvalidOperationException("Non-invariant globalization is required.");
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            worker = "Quay manifests", os = os["ID"], version = os["VERSION_ID"],
            runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            culture = french.Name, fee, compressionBudget = (ulong)bytes, zlib = Marshal.PtrToStringAnsi(version())
        }));
        // Identify the actual native/runtime assets loaded by this process.
        foreach (var path in File.ReadAllLines("/proc/self/maps")
            .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last())
            .Where(path => path.Length > 0 && path[0] == '/' && (path.Contains("libz.so", StringComparison.Ordinal) || path.Contains("libicu", StringComparison.Ordinal)
                || path.Contains("libc.so", StringComparison.Ordinal) || path.Contains("libcoreclr.so", StringComparison.Ordinal) || path.Contains("libSystem.Native.so", StringComparison.Ordinal)))
            .Distinct().Order(StringComparer.Ordinal))
            Console.WriteLine($"LOADED {path} sha256={Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path)))}");
    }
    finally { NativeLibrary.Free(handle); }
}
catch (DllNotFoundException)
{
    Console.Error.WriteLine($"QUAY_MISSING_NATIVE:{library}");
    return 23;
}
return 0;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate nuint CompressBound(nuint sourceLength);
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate nint ZlibVersion();
// endsnippet:worker
