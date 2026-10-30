// snippet:verify
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

internal static class ManifestVerifier
{
    internal static bool Accept(byte[] manifest, byte[] signature,
        X509Certificate2 signer, X509Certificate2 testRoot,
        IEnumerable<X509Certificate2> intermediates)
    {
        // Supply every missing intermediate. This does not make any of them a trusted root.
        using var chain = ArchivePolicy.Create(testRoot, intermediates, disableDownloads: true);
        using var key = signer.GetRSAPublicKey();
        return chain.Build(signer) && key is not null && key.VerifyData(manifest,
            signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
}
// endsnippet:verify
