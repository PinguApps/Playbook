using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Playbook.Samples;

// snippet:certificate-roundtrip
public static class CertificateRoundTrip
{
    // Reads this example's one-DNS-then-registeredIDs layout, not every possible SAN.
    public static (string DnsName, string[] Identifiers, byte[] Der) Read(X509Extension extension)
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var request = new CertificateRequest("CN=Reedbank survey permit", key, HashAlgorithmName.SHA256);
        request.CertificateExtensions.Add(extension);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddHours(1));
        using var loaded = X509CertificateLoader.LoadCertificate(certificate.Export(X509ContentType.Cert));
        X509Extension saved = loaded.Extensions.Single(item => item.Oid?.Value == "2.5.29.17");
        var reader = new AsnReader(saved.RawData, AsnEncodingRules.DER);
        AsnReader names = reader.ReadSequence();
        string dnsName = names.ReadCharacterString(UniversalTagNumber.IA5String, new Asn1Tag(TagClass.ContextSpecific, 2));
        var identifiers = new List<string>();
        while (names.HasData)
            identifiers.Add(names.ReadObjectIdentifier(new Asn1Tag(TagClass.ContextSpecific, 8)));
        reader.ThrowIfNotEmpty();
        return (dnsName, identifiers.ToArray(), saved.RawData);
    }
}
// endsnippet:certificate-roundtrip
