using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using TelemetryKeys;

if (!OperatingSystem.IsWindows()) { throw new PlatformNotSupportedException("This check requires Windows CNG."); }
bool current = Environment.Version.Major == 11;
Console.WriteLine($"runtime={RuntimeInformation.FrameworkDescription}; os={RuntimeInformation.OSDescription}; arch={RuntimeInformation.ProcessArchitecture}");
int status = Native.BCryptOpenAlgorithmProvider(out nint algorithm, "HKDF", null, 0);
if (status != 0) { throw new Exception($"CNG HKDF unavailable: 0x{status:X8}"); }
Native.BCryptCloseAlgorithmProvider(algorithm, 0);
if (current && typeof(HKDF).GetField("s_hasCngImplementation", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) is not true)
{
    throw new Exception("The exact current implementation did not select CNG.");
}
string native = Path.Combine(Environment.SystemDirectory, "bcrypt.dll");
Console.WriteLine($"CNG HKDF open=0; selectedCurrentCng={current}; bcrypt={FileVersionInfo.GetVersionInfo(native).ProductVersion}; sha256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(native)))}");
byte[] salt = "gannet-sensors/session-7"u8.ToArray();
byte[] context = "telemetry/encryption/v1"u8.ToArray();
foreach (int size in new[] { 2048, 2049 })
{
    byte[] material = Enumerable.Range(0, size).Select(i => (byte)(i * 17 + 5)).ToArray();
    byte[] snapshot = material.ToArray();
    byte[] saltSnapshot = salt.ToArray();
    byte[] contextSnapshot = context.ToArray();
    byte[] expectedPrk = HMACSHA256.HashData(salt, material);
    byte[] expectedDerived = ExpandFirstBlock(expectedPrk, context);
    byte[] expectedExpanded = ExpandFirstBlock(material, context);
    foreach (bool expand in new[] { false, true })
    {
        byte[] expected = expand ? expectedExpanded : expectedDerived;
        byte[]? raw = null;
        bool rejected = false;
        try
        {
            raw = expand ? HKDF.Expand(HashAlgorithmName.SHA256, material, 32, context)
                : HKDF.DeriveKey(HashAlgorithmName.SHA256, material, 32, salt, context);
        }
        catch (CryptographicException) { rejected = true; }
        Check(rejected == (current && size == 2049), "raw boundary");
        if (!rejected) { Check(raw!.SequenceEqual(expected), "independent HMAC result"); }
        else { Check(raw is null, "no raw result escaped failure"); }
        bool accepted = expand ? SessionKeys.TryExpand(material, context, out byte[]? key)
            : SessionKeys.TryDerive(material, salt, context, out key);
        Check(accepted == !(current && size == 2049), "protocol acceptance");
        if (accepted) { Check(key!.SequenceEqual(expected), "canonical result"); }
        else { Check(key is null, "rejection returns no usable key"); }
        Console.WriteLine($"{(expand ? "Expand PRK" : "DeriveKey IKM")} input={size}; outputLength=32; rawRejected={rejected}; protocolAccepted={accepted}; expected={Convert.ToHexString(expected)}");
    }
    Check(material.SequenceEqual(snapshot) && salt.SequenceEqual(saltSnapshot) && context.SequenceEqual(contextSnapshot), "inputs preserved");
    Check(HKDF.Extract(HashAlgorithmName.SHA256, material, salt).SequenceEqual(expectedPrk), "Extract remains separate");
    Console.WriteLine($"Extract managed input={size}; outputLength=32; matched independent HMAC");
}
byte[] rfcIkm = Enumerable.Repeat((byte)0x0b, 22).ToArray();
byte[] rfcSalt = Convert.FromHexString("000102030405060708090a0b0c");
byte[] rfcInfo = Convert.FromHexString("f0f1f2f3f4f5f6f7f8f9");
byte[] vector = HKDF.DeriveKey(HashAlgorithmName.SHA256, rfcIkm, 42, rfcSalt, rfcInfo);
Check(Convert.ToHexString(vector) == "3CB25F25FAACD57A90434F64D0362F2A2D2D0A90CF1A5A4C5DB02D56ECC4C5BF34007208D5B887185865", "RFC5869 case1");
Console.WriteLine("PASS SHA256 RFC5869 case1, independent boundary outputs, unchanged inputs and explicit rejection; no all-hash/all-Windows/GA claim");

static byte[] ExpandFirstBlock(byte[] prk, byte[] info) => HMACSHA256.HashData(prk, (byte[])[.. info, 1]);
static void Check(bool condition, string claim) { if (!condition) { throw new Exception(claim); } }
internal static class Native
{
    [DllImport("bcrypt.dll", CharSet = CharSet.Unicode)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern int BCryptOpenAlgorithmProvider(out nint handle, string name, string? implementation, uint flags);
    [DllImport("bcrypt.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern int BCryptCloseAlgorithmProvider(nint handle, uint flags);
}
