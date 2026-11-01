using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Playbook.Samples;

#region permit-manifest
internal static class PermitManifest
{
    // Call once at process startup, before any cryptographic XML operation initializes its cached settings.
    public static void ConfigureAtStartup() =>
        AppContext.SetData("System.Security.Cryptography.MaxReferencesPerSignedInfo", 8);

    public static bool Verify(XmlDocument document, RSA trustedKey)
    {
        var namespaces = new XmlNamespaceManager(document.NameTable);
        namespaces.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
        var signatures = document.SelectNodes("/Permits/ds:Signature", namespaces)!;
        if (signatures.Count != 1) throw new CryptographicException("Expected one signature.");
        var verifier = new SignedXml(document);
        // The configured parser limit rejects excess references here, before digest/signature checking.
        verifier.LoadXml((XmlElement)signatures[0]!);
        if (verifier.SignedInfo!.References.Count == 0) throw new CryptographicException("No permit references.");
        return verifier.CheckSignature(trustedKey);
    }
}
#endregion
