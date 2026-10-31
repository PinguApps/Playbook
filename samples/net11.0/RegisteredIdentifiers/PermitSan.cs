using System.Security.Cryptography.X509Certificates;

namespace Playbook.Samples;

// snippet:builder-san
public static class PermitSan
{
    public static X509Extension Create(string dnsName, IEnumerable<string> identifiers)
    {
        var builder = new SubjectAlternativeNameBuilder();
        builder.AddDnsName(dnsName);
        foreach (string identifier in identifiers)
            builder.AddRegisteredId(identifier);
        return builder.Build(critical: false);
    }
}
// endsnippet:builder-san
