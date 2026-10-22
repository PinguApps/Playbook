using Harbour.Envelopes;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

// Fail, rather than skip, when the evidence host cannot exercise the operation.
using var recipientKey = QuantumDispatch.CreateKey();
#if NET11_0_OR_GREATER
using var senderKey = CompositeMLKem.ImportSubjectPublicKeyInfo(recipientKey.ExportSubjectPublicKeyInfo());
#else
using var senderKey = MLKem.ImportEncapsulationKey(MLKemAlgorithm.MLKem768, recipientKey.ExportEncapsulationKey());
#endif
Expect<ArgumentException>(() => recipientKey.Decapsulate(new byte[1]));
byte[] first = QuantumDispatch.Send(senderKey, out byte[] capsule);
byte[] second = recipientKey.Decapsulate(capsule);
try { Require(CryptographicOperations.FixedTimeEquals(first, second), "encapsulation round trip"); }
finally { CryptographicOperations.ZeroMemory(first); CryptographicOperations.ZeroMemory(second); }
Console.WriteLine($"PASS: {recipientKey.GetType().BaseType?.Name} real encapsulation/decapsulation with public-only sender.");

#if NET11_0_OR_GREATER
using (var privateHpke = ClassicalDispatch.CreateKey())
using (var publicHpke = Hpke.ImportEncapsulationKey(privateHpke.Suite, privateHpke.ExportEncapsulationKey()))
{
    byte[] encrypted = ClassicalDispatch.Send(publicHpke, "berth-27-arrival"u8, out byte[] hpkeCapsule);
    Require(CryptographicOperations.FixedTimeEquals(ClassicalDispatch.Receive(privateHpke, hpkeCapsule, encrypted), "berth-27-arrival"u8), "HPKE round trip");
    encrypted[^1] ^= 1;
    Expect<CryptographicException>(() => ClassicalDispatch.Receive(privateHpke, hpkeCapsule, encrypted));
    var unsupported = new HpkeSuite(HpkeKem.MLKEM768_P256, HpkeKdf.HKDF_SHA256, HpkeAead.AES_128_GCM);
    Require(!Hpke.IsSupported(unsupported), "RC2 does not implement post-quantum HPKE");
}
Console.WriteLine("PASS: classical HPKE round trip/tamper rejection; post-quantum HPKE IsSupported=False.");
#endif

using RSA issuerKey = RSA.Create(2048);
var issuerName = new X500DistinguishedName("CN=Harbour test issuer");
var signer = X509SignatureGenerator.CreateForRSA(issuerKey, RSASignaturePadding.Pkcs1);
// Ephemeral certificates are local test identities. They establish no chain trust.
#if NET11_0_OR_GREATER
if (OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("CMS evidence must run on non-Windows.");
using MLKem cmsKey = MLKem.GenerateKey(MLKemAlgorithm.MLKem768);
PublicKey publicKey = PublicKey.CreateFromSubjectPublicKeyInfo(cmsKey.ExportSubjectPublicKeyInfo(), out _);
CertificateRequest request = new(new X500DistinguishedName("CN=Harbour receiver"), publicKey, HashAlgorithmName.SHA256);
#else
using RSA cmsKey = RSA.Create(2048);
CertificateRequest request = new("CN=Harbour receiver", cmsKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
#endif
using X509Certificate2 certificate = request.Create(issuerName, signer,
    DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(10), RandomNumberGenerator.GetBytes(16));
byte[] encoded = DispatchEnvelope.Encrypt("dispatch-berth-27"u8, certificate);
Require(CryptographicOperations.FixedTimeEquals(DispatchEnvelope.Decrypt(encoded, cmsKey), "dispatch-berth-27"u8), "CMS round trip");
#if NET11_0_OR_GREATER
EnvelopedCms decoded = new();
decoded.Decode(encoded);
Require(decoded.RecipientInfos[0] is KemRecipientInfo, "KEM recipient wire representation");
using MLKem wrongKey = MLKem.GenerateKey(MLKemAlgorithm.MLKem768);
Expect<CryptographicException>(() => DispatchEnvelope.Decrypt(encoded, wrongKey));
// This public overload exists but is intentionally unsupported in RC2.
Expect<PlatformNotSupportedException>(() => decoded.Decrypt((KemRecipientInfo)decoded.RecipientInfos[0], recipientKey));
#endif
Console.WriteLine("PASS: real CMS encode/decode/decrypt with ephemeral certificate; no certificate-trust or authentication claim.");

static void Require(bool condition, string claim)
{
    if (!condition) throw new InvalidOperationException(claim);
}

static void Expect<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new InvalidOperationException($"Expected {typeof(T).Name}.");
}
