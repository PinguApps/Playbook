using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace Harbour.Envelopes;

// snippet:encapsulate
public static class QuantumDispatch
{
    // Pure ML-KEM is available; this is NOT a composite/classical hybrid.
    public static MLKem CreateKey()
    {
        if (!MLKem.IsSupported) throw new PlatformNotSupportedException("ML-KEM provider required.");
        return MLKem.GenerateKey(MLKemAlgorithm.MLKem768);
    }

    public static byte[] Send(MLKem publicKey, out byte[] capsule)
    {
        publicKey.Encapsulate(out capsule, out byte[] secret);
        return secret;
    }
}
// endsnippet:encapsulate

// snippet:cms
public static class DispatchEnvelope
{
    // RSA CMS is the previous-release comparison, not post-quantum protection.
    public static byte[] Encrypt(ReadOnlySpan<byte> dispatch, X509Certificate2 trustedRecipient)
    {
        EnvelopedCms envelope = new(new ContentInfo(dispatch.ToArray()),
            new AlgorithmIdentifier(new Oid("2.16.840.1.101.3.4.1.42"))); // AES-256-CBC
        envelope.Encrypt(new CmsRecipient(trustedRecipient, RSAEncryptionPadding.OaepSHA256));
        return envelope.Encode();
    }

    public static byte[] Decrypt(byte[] encoded, RSA privateKey)
    {
        EnvelopedCms envelope = new();
        envelope.Decode(encoded);
        envelope.Decrypt(envelope.RecipientInfos[0], privateKey);
        return envelope.ContentInfo.Content;
    }
}
// endsnippet:cms
