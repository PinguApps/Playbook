// snippet:required-server
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Authentication.ExtendedProtection;

internal static class GannetProtection
{
    public static NegotiateAuthentication Create(ChannelBinding? binding)
    {
        if (binding is null) throw new AuthenticationException("A live TLS endpoint binding is required.");
        return new(new NegotiateAuthenticationServerOptions
        {
            Package = "Kerberos",
            Credential = CredentialCache.DefaultNetworkCredentials,
            Binding = binding,
            Policy = new ExtendedProtectionPolicy(PolicyEnforcement.Always),
            RequiredProtectionLevel = ProtectionLevel.EncryptAndSign
        });
    }
}
// endsnippet:required-server
