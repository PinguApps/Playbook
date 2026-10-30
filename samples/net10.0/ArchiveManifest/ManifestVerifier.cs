// snippet:verify
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

internal static class ManifestVerifier
{
    internal static bool Accept(byte[] manifest, byte[] signature,
        X509Certificate2 signer, X509Certificate2 testRoot,
        IEnumerable<X509Certificate2> intermediates)
    {
        // AIA was a working fallback on Linux for longer chains; explicit supply also works.
        using var chain = ArchivePolicy.Create(testRoot, intermediates, disableDownloads: false);
        using var key = signer.GetRSAPublicKey();
        return chain.Build(signer) && key is not null && key.VerifyData(manifest,
            signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
}
// endsnippet:verify
