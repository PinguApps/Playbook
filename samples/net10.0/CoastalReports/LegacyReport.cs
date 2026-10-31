// snippet:legacy
using System.Security.Cryptography;

namespace CoastalReports;

public static class LegacyReport
{
    // The archive contract specifies imported DSA-1024 SPKI, SHA-1 and 40-byte P1363.
    public static bool Verify(byte[] payload, byte[] signature, byte[] publicKeySpki)
    {
        using DSA key = DSA.Create();
        key.ImportSubjectPublicKeyInfo(publicKeySpki, out int consumed);
        if (consumed != publicKeySpki.Length || key.KeySize != 1024 || signature.Length != 40)
            return false;
        return key.VerifyData(payload, signature, HashAlgorithmName.SHA1,
            DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
    }
}
// endsnippet:legacy
