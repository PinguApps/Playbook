// snippet:contexts
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

internal static class RoomContexts
{
    internal static SslStreamCertificateContext Server(X509Certificate2 serverWithKey, X509Certificate2 approvedClientRoot)
    {
        var trust = SslCertificateTrust.CreateForX509Collection(
            new X509Certificate2Collection(approvedClientRoot), sendTrustInHandshake: false);
        return SslStreamCertificateContext.Create(serverWithKey, null, offline: true, trust);
    }

    internal static SslClientAuthenticationOptions Client(X509Certificate2 clientWithKey, X509Certificate2 approvedServerCertificate)
    {
        var policy = new X509ChainPolicy
        {
            TrustMode = X509ChainTrustMode.CustomRootTrust,
            RevocationMode = X509RevocationMode.NoCheck,
            DisableCertificateDownloads = true
        };
        policy.CustomTrustStore.Add(approvedServerCertificate);
        return new SslClientAuthenticationOptions
        {
            TargetHost = "room.test", // The private server certificate has this DNS SAN.
            ClientCertificateContext = SslStreamCertificateContext.Create(clientWithKey, null, offline: true),
            CertificateChainPolicy = policy
        };
    }
}
// endsnippet:contexts
