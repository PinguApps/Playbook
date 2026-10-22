using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace Harbour.Envelopes;

// snippet:encapsulate
public static class QuantumDispatch
{
    public static CompositeMLKem CreateKey()
    {
        CompositeMLKemAlgorithm algorithm = CompositeMLKemAlgorithm.MLKem768WithECDiffieHellmanP256;
        if (!CompositeMLKem.IsAlgorithmSupported(algorithm))
            throw new PlatformNotSupportedException("The negotiated composite algorithm is unavailable.");
        return CompositeMLKem.GenerateKey(algorithm);
    }

    public static byte[] Send(CompositeMLKem publicKey, out byte[] capsule)
    {
        publicKey.Encapsulate(out capsule, out byte[] secret);
        return secret;
    }
}
// endsnippet:encapsulate

// snippet:hpke
public static class ClassicalDispatch
{
    public static Hpke CreateKey()
    {
        HpkeSuite suite = new(HpkeKem.DHKEM_P256_HKDF_SHA256,
            HpkeKdf.HKDF_SHA256, HpkeAead.AES_128_GCM);
        if (!Hpke.IsSupported(suite)) throw new PlatformNotSupportedException("HPKE suite unavailable.");
        return Hpke.GenerateKey(suite);
    }

    public static byte[] Send(Hpke trustedPublicKey, ReadOnlySpan<byte> dispatch, out byte[] capsule)
    {
        trustedPublicKey.Seal(dispatch, out capsule, out byte[] ciphertext,
            associatedData: "dispatch-header-v1"u8, info: "harbour-hpke-v1"u8);
        return ciphertext;
    }

    public static byte[] Receive(Hpke privateKey, byte[] capsule, byte[] ciphertext) =>
        privateKey.Open(capsule, ciphertext, "dispatch-header-v1"u8, "harbour-hpke-v1"u8);
}
// endsnippet:hpke

// snippet:cms
public static class DispatchEnvelope
{
    public static byte[] Encrypt(ReadOnlySpan<byte> dispatch, X509Certificate2 trustedRecipient)
    {
        if (OperatingSystem.IsWindows() || !MLKem.IsSupported)
            throw new PlatformNotSupportedException("Pure ML-KEM CMS needs the non-Windows provider route.");
        EnvelopedCms envelope = new(new ContentInfo(dispatch.ToArray()),
            new AlgorithmIdentifier(new Oid("2.16.840.1.101.3.4.1.42"))); // AES-256-CBC
        // Present-but-empty UKM; new CmsRecipient(cert) instead omits UKM entirely.
        envelope.Encrypt(CmsRecipient.CreateForKeyEncapsulation(trustedRecipient, []));
        return envelope.Encode();
    }

    public static byte[] Decrypt(byte[] encoded, MLKem privateKey)
    {
        EnvelopedCms envelope = new();
        envelope.Decode(encoded);
        if (envelope.RecipientInfos.Count != 1 || envelope.RecipientInfos[0] is not KemRecipientInfo recipient)
            throw new CryptographicException("Expected one KEM recipient.");
        envelope.Decrypt(recipient, privateKey);
        return envelope.ContentInfo.Content;
    }
}
// endsnippet:cms
