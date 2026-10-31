using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

Console.WriteLine($"runtime={RuntimeInformation.FrameworkDescription}; os={RuntimeInformation.OSDescription}; arch={RuntimeInformation.ProcessArchitecture}");
var before = new X509BasicConstraintsExtension(false, false, 0, false);
var incoming = new X509BasicConstraintsExtension(true, true, 2, true);
Console.WriteLine($"INPUT old={Convert.ToHexString(before.RawData)}; incoming={Convert.ToHexString(incoming.RawData)}; critical={incoming.Critical}");
Check(!before.CertificateAuthority && !before.HasPathLengthConstraint && before.PathLengthConstraint == 0, "prime decoded cache");
#if NET10_0
ExtensionUpdate.Assign(before, incoming.RawData);
Check(before.RawData.SequenceEqual(incoming.RawData), "setter changes raw bytes");
Check(!before.CertificateAuthority && !before.HasPathLengthConstraint && before.PathLengthConstraint == 0 && !before.Critical, "setter retains stale decoded fields and original Critical");
Console.WriteLine("PASS stale-cache: raw=CA/path2; decoded=end-entity/path0; Critical=False");
var replacement = new X509BasicConstraintsExtension(incoming, incoming.Critical);
before.CopyFrom(incoming);
#else
var replacement = ExtensionUpdate.Replace(incoming);
ExtensionUpdate.Mutate(before, incoming);
#endif
Validate(replacement, incoming, "constructor");
Validate(before, incoming, "CopyFrom");
var endEntity = new X509BasicConstraintsExtension(false, false, 0, false);
before.CopyFrom(endEntity);
Check(!before.CertificateAuthority && !before.HasPathLengthConstraint && before.PathLengthConstraint == 0 && !before.Critical && before.RawData.SequenceEqual(endEntity.RawData), "second mutation clears fields and copies Critical=False");
Console.WriteLine("PASS reverse mutation: end-entity/path0; Critical=False");
byte[] malformed = [0x30, 0x03, 0x01];
var badInput = new X509Extension("2.5.29.19", malformed, false);
var lazy = new X509BasicConstraintsExtension(badInput, false);
Check(lazy.RawData.SequenceEqual(malformed), "constructor stores malformed bytes before decoding");
RejectDecode(lazy, "constructor decode");
before.CopyFrom(badInput);
Check(before.RawData.SequenceEqual(malformed), "CopyFrom stores malformed bytes before decoding");
RejectDecode(before, "CopyFrom decode");
try { before.CopyFrom(new AsnEncodedData(incoming.RawData)); throw new InvalidOperationException("Expected extension type rejection"); }
catch (ArgumentException) { Console.WriteLine("PASS CopyFrom requires X509Extension input"); }
// Getter-only use deliberately compiles with warnings as errors on both targets.
byte[] snapshot = incoming.RawData.ToArray();
Check(snapshot.SequenceEqual(incoming.RawData), "getter-only snapshot");
Console.WriteLine("PASS getter-only; constructor/CopyFrom raw bytes, OID, Critical and all decoded fields; malformed boundary");

static void Validate(X509BasicConstraintsExtension value, X509BasicConstraintsExtension input, string operation)
{
    Check(value.CertificateAuthority && value.HasPathLengthConstraint && value.PathLengthConstraint == 2 && value.Critical && value.Oid?.Value == "2.5.29.19" && value.RawData.SequenceEqual(input.RawData), operation);
    Console.WriteLine($"PASS {operation}: CA=True; hasPath=True; path=2; Critical=True; raw={Convert.ToHexString(value.RawData)}");
}
static void RejectDecode(X509BasicConstraintsExtension value, string operation)
{
    try { _ = value.CertificateAuthority; throw new InvalidOperationException("Expected malformed decode rejection"); }
    catch (CryptographicException ex) { Console.WriteLine($"PASS {operation}: {ex.GetType().Name}"); }
}
static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
