// snippet:manual-san
using System.Formats.Asn1;
using System.Security.Cryptography.X509Certificates;

namespace Playbook.Samples;

public static class ManualSan
{
    public static X509Extension Create(string dnsName, IEnumerable<string> identifiers)
    {
        var writer = new AsnWriter(AsnEncodingRules.DER);
        using (writer.PushSequence())
        {
            writer.WriteCharacterString(UniversalTagNumber.IA5String, dnsName,
                new Asn1Tag(TagClass.ContextSpecific, 2));
            foreach (string identifier in identifiers)
                writer.WriteObjectIdentifier(identifier, new Asn1Tag(TagClass.ContextSpecific, 8));
        }
        return new X509Extension("2.5.29.17", writer.Encode(), critical: false);
    }
}
// endsnippet:manual-san
