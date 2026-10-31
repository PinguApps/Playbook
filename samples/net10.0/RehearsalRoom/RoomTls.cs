// snippet:options
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

internal static class RoomTls
{
    // The context supplies the fixture's private client-root trust independently
    // of CertificateChainPolicy. This historical comparison leaves policy unset.
    internal static SslServerAuthenticationOptions Create(SslStreamCertificateContext serverContext)
        => new()
        {
            ServerCertificateContext = serverContext,
            ClientCertificateRequired = true,
            CertificateRevocationCheckMode = X509RevocationMode.NoCheck
        };
}
// endsnippet:options
