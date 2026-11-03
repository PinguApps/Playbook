using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Observatory;

if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Windows provider proof required.");
Console.WriteLine(RuntimeInformation.OSDescription + " " + RuntimeInformation.FrameworkDescription);
foreach (string name in new[] { "bcrypt.dll", "bcryptPrimitives.dll" })
{
    string path = Path.Combine(Environment.SystemDirectory, name);
    Console.WriteLine($"NATIVE {path} version={FileVersionInfo.GetVersionInfo(path).FileVersion} sha256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))}");
}
byte[] bulletin = Encoding.UTF8.GetBytes("Grey Observatory|bulletin=7|elevation=42");
var agreed = CompositeMLDsaAlgorithm.MLDsa44WithECDsaP256;
Console.WriteLine($"PROBE {agreed.Name}={CompositeMLDsa.IsAlgorithmSupported(agreed)}");
using var key = Bulletin.Create(agreed);
byte[] signature = key.SignData(bulletin);
if (!key.VerifyData(bulletin, signature)) throw new Exception("Signature rejected");
bulletin[0] ^= 1;
if (key.VerifyData(bulletin, signature)) throw new Exception("Tamper accepted");
Console.WriteLine("PASS ECDSA-composite signature and tamper rejection; only 44/P256 exercised");
var rsa = CompositeMLDsaAlgorithm.MLDsa44WithRSA2048Pss;
Console.WriteLine($"PROBE {rsa.Name}={CompositeMLDsa.IsAlgorithmSupported(rsa)}");
#if NET11_0_OR_GREATER
try { using var rejected = Bulletin.Create(rsa); throw new Exception("RSA unexpectedly available"); }
catch (PlatformNotSupportedException) { Console.WriteLine("PASS actual unsupported RSA set fails closed"); }
#else
using var legacy = Bulletin.Create(rsa);
var oldSignature = legacy.SignData(bulletin);
if (!legacy.VerifyData(bulletin, oldSignature)) throw new Exception("Legacy RSA rejected");
bulletin[0] ^= 1;
if (legacy.VerifyData(bulletin, oldSignature)) throw new Exception("Legacy tamper accepted");
Console.WriteLine("PASS older managed RSA-composite sign/verify/tamper");
#endif
