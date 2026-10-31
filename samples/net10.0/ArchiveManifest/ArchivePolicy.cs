// snippet:policy
using System.Security.Cryptography.X509Certificates;

internal static class ArchivePolicy
{
    // Custom trust and NoCheck belong only to this private, offline test CA.
    internal static X509Chain Create(X509Certificate2? testRoot,
        IEnumerable<X509Certificate2> intermediates, bool disableDownloads)
    {
        var chain = new X509Chain();
        chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        if (testRoot is not null) chain.ChainPolicy.CustomTrustStore.Add(testRoot);
        chain.ChainPolicy.ExtraStore.AddRange(intermediates.ToArray());
        chain.ChainPolicy.DisableCertificateDownloads = disableDownloads;
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
        chain.ChainPolicy.UrlRetrievalTimeout = TimeSpan.FromSeconds(3);
        return chain;
    }
}
// endsnippet:policy
