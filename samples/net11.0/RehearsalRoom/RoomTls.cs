// snippet:options
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

internal static class RoomTls
{
    internal static SslServerAuthenticationOptions Create(
        SslStreamCertificateContext serverContext,
        X509Certificate2 approvedClientRoot,
        X509Certificate2 clientIntermediate)
    {
        var policy = new X509ChainPolicy
        {
            TrustMode = X509ChainTrustMode.CustomRootTrust,
            RevocationMode = X509RevocationMode.NoCheck, // Private offline fixture only.
            DisableCertificateDownloads = true,
            VerificationFlags = X509VerificationFlags.NoFlag
        };
        policy.CustomTrustStore.Add(approvedClientRoot);
        policy.ExtraStore.Add(clientIntermediate); // Availability does not confer trust.
        policy.ApplicationPolicy.Add(new Oid("1.3.6.1.5.5.7.3.2")); // Client authentication.
        return new SslServerAuthenticationOptions
        {
            ServerCertificateContext = serverContext,
            ClientCertificateRequired = true,
            CertificateChainPolicy = policy
        };
    }
}
// endsnippet:options
