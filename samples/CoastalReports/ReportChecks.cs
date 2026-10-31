using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using CoastalReports;

Console.WriteLine($"runtime={RuntimeInformation.FrameworkDescription}; os={RuntimeInformation.OSDescription}; arch={RuntimeInformation.OSArchitecture}");
string fixturePath = Path.Combine(AppContext.BaseDirectory, "legacy-fixture.json");
using var fixture = JsonDocument.Parse(File.ReadAllBytes(fixturePath));
byte[] payload = Convert.FromBase64String(fixture.RootElement.GetProperty("payload").GetString()!);
byte[] signature = Convert.FromBase64String(fixture.RootElement.GetProperty("signature").GetString()!);
byte[] spki = Convert.FromBase64String(fixture.RootElement.GetProperty("publicKeySpki").GetString()!);
Console.WriteLine($"INPUT fixtureSha256={Hash(File.ReadAllBytes(fixturePath))}; payloadSha256={Hash(payload)}; publicSpkiSha256={Hash(spki)}; DSA1024/SHA1/P1363 signatureBytes={signature.Length}");
#if NET10_0
Check(LegacyReport.Verify(payload, signature, spki), "imported legacy DSA fixture");
byte[] damaged = (byte[])payload.Clone(); damaged[0] ^= 1;
Check(!LegacyReport.Verify(damaged, signature, spki), "legacy tamper rejection");
#else
if (OperatingSystem.IsMacOS())
{
    try { using DSA unsupported = DSA.Create(); throw new Exception("Expected macOS DSA removal"); }
    catch (PlatformNotSupportedException) { Console.WriteLine("PASS actual macOS DSA.Create platform failure"); }
}
else
{
    using DSA imported = DSA.Create();
    imported.ImportSubjectPublicKeyInfo(spki, out _);
    Check(imported.VerifyData(payload, signature, HashAlgorithmName.SHA1,
        DSASignatureFormat.IeeeP1363FixedFieldConcatenation), "fixture remains DSA-verifiable on this host only");
}
#endif
using ECDsa independentlyIssued = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var request = new CertificateRequest("CN=Gannet Survey Disposable Signer", independentlyIssued, HashAlgorithmName.SHA256);
using X509Certificate2 signer = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddHours(1));
using X509Certificate2 trusted = X509CertificateLoader.LoadCertificate(signer.Export(X509ContentType.Cert));
const string keyId = "gannet-test-2026";
SignedSurvey report = CurrentReport.Sign(payload, keyId, signer);
Check(CurrentReport.Verify(report, keyId, trusted) == SurveyVerification.Valid, "ECDSA certificate private/public access and DER signature");
// A wire peer exports/imports a public SPKI and explicitly verifies the same DER contract.
using ECDsa peer = ECDsa.Create();
peer.ImportSubjectPublicKeyInfo(independentlyIssued.ExportSubjectPublicKeyInfo(), out _);
Check(peer.VerifyData(report.Payload.Span, report.Signature.Span, HashAlgorithmName.SHA256,
    DSASignatureFormat.Rfc3279DerSequence), "independent SPKI peer verifies DER");
byte[] tampered = (byte[])payload.Clone(); tampered[0] ^= 1;
Check(CurrentReport.Verify(report with { Payload = tampered }, keyId, trusted) == SurveyVerification.Invalid, "ECDSA payload tamper rejection");
Check(CurrentReport.Verify(report with { Signature = new byte[] { 0x30, 0x00 } }, keyId, trusted) == SurveyVerification.Invalid, "malformed signature rejection");
Check(CurrentReport.Verify(report with { Algorithm = "ecdsa-unspecified" }, keyId, trusted) == SurveyVerification.Invalid, "unknown algorithm rejection");
Check(CurrentReport.Verify(report with { KeyId = "untrusted" }, keyId, trusted) == SurveyVerification.Invalid, "unknown key rejection");
byte[] p1363 = independentlyIssued.SignData(payload, HashAlgorithmName.SHA256,
    DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
Check(CurrentReport.Verify(report with { Signature = p1363 }, keyId, trusted) == SurveyVerification.Invalid, "P1363 is not DER");
Check(CurrentReport.Verify(new(CurrentReport.LegacyAlgorithm, "archive-test", payload, signature), keyId, trusted)
    == SurveyVerification.UnsupportedLegacy, "legacy report explicitly unsupported; no ECDSA reinterpretation");
using ECDsa unrelatedKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var otherRequest = new CertificateRequest("CN=Other Disposable Signer", unrelatedKey, HashAlgorithmName.SHA256);
using X509Certificate2 unrelated = otherRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddHours(1));
Check(CurrentReport.Verify(report, keyId, unrelated) == SurveyVerification.Invalid, "unrelated ECDSA certificate rejection");
Console.WriteLine($"PASS new report contract; ECDSA certificateSha256={Hash(trusted.RawData)}; no trust-store writes; private keys disposed");
static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
static void Check(bool success, string claim)
{
    if (!success) throw new Exception(claim);
    Console.WriteLine($"PASS {claim}");
}
