using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Playbook.Samples;

#region permit-manifest
internal static class PermitManifest
{
    public static bool Verify(XmlDocument document, RSA trustedKey, int budget = 8)
    {
        if (budget is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(budget));
        var namespaces = new XmlNamespaceManager(document.NameTable);
        namespaces.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
        // Application policy rejects excess direct references before SignedXml parses them.
        var signatures = document.SelectNodes("/Permits/ds:Signature", namespaces)!;
        if (signatures.Count != 1) throw new CryptographicException("Expected one signature.");
        var signature = (XmlElement)signatures[0]!;
        int count = signature.SelectNodes("ds:SignedInfo/ds:Reference", namespaces)!.Count;
        if (count < 1 || count > budget) throw new CryptographicException("Permit reference budget exceeded.");
        var verifier = new SignedXml(document);
        verifier.LoadXml(signature);
        return verifier.CheckSignature(trustedKey);
    }
}
#endregion
