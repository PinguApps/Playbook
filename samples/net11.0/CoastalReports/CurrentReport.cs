// snippet:current
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CoastalReports;

public static class CurrentReport
{
    public const string Algorithm = "ecdsa-p256-sha256-der-v1";
    public const string LegacyAlgorithm = "dsa1024-sha1-p1363-v0";

    public static SignedSurvey Sign(byte[] payload, string keyId, X509Certificate2 signingCertificate)
    {
        using ECDsa key = signingCertificate.GetECDsaPrivateKey()
            ?? throw new CryptographicException("An independently issued ECDSA private key is required.");
        RequireP256(key);
        byte[] snapshot = (byte[])payload.Clone();
        return new(Algorithm, keyId, snapshot, key.SignData(snapshot, HashAlgorithmName.SHA256,
            DSASignatureFormat.Rfc3279DerSequence));
    }

    // trustedKeyId/certificate come from the verifier's configuration, never the report.
    public static SurveyVerification Verify(SignedSurvey report, string trustedKeyId,
        X509Certificate2 trustedCertificate)
    {
        if (report.Algorithm == LegacyAlgorithm) return SurveyVerification.UnsupportedLegacy;
        if (report.Algorithm != Algorithm || report.KeyId != trustedKeyId) return SurveyVerification.Invalid;
        using ECDsa key = trustedCertificate.GetECDsaPublicKey()
            ?? throw new CryptographicException("The trusted certificate must contain an ECDSA key.");
        RequireP256(key);
        return key.VerifyData(report.Payload.Span, report.Signature.Span, HashAlgorithmName.SHA256,
            DSASignatureFormat.Rfc3279DerSequence) ? SurveyVerification.Valid : SurveyVerification.Invalid;
    }

    private static void RequireP256(ECDsa key)
    {
        if (key.ExportParameters(false).Curve.Oid.Value != "1.2.840.10045.3.1.7")
            throw new CryptographicException("The agreed report contract requires NIST P-256.");
    }
}
// endsnippet:current
