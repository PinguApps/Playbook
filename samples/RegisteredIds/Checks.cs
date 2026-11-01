using System.Formats.Asn1;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace Playbook.Samples;

internal static class Program
{
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public static void Main()
    {
        AssertRuntimeBaseline();
        // Documentation arc: these identifiers are fictional, not assigned production identities.
        string[] identifiers = ["1.3.6.1.4.1.32473.41", "1.3.6.1.4.1.32473.42", "2.999.123456789012345678901234567890"];
        const string dns = "permit.reedbank.test";
        X509Extension extension = Create(dns, identifiers);
        var roundTrip = CertificateRoundTrip.Read(extension);
        Require(roundTrip.DnsName == dns && roundTrip.Identifiers.SequenceEqual(identifiers) && roundTrip.Der.SequenceEqual(extension.RawData), "Canonical certificate round trip changed SAN.");
        Require(extension.RawData.SequenceEqual(ManualSan.Create(dns, identifiers).RawData), "Manual/builder DER differs.");
        Require(extension.Oid?.Value == "2.5.29.17" && !extension.Critical, "SAN identity/criticality changed.");
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var request = new CertificateRequest("CN=Reedbank survey permit", key, HashAlgorithmName.SHA256);
        request.CertificateExtensions.Add(extension);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddHours(1));
        using var loaded = X509CertificateLoader.LoadCertificate(certificate.Export(X509ContentType.Cert));
        X509Extension saved = loaded.Extensions.Single(item => item.Oid?.Value == "2.5.29.17");
        Require(saved.RawData.SequenceEqual(extension.RawData) && !saved.Critical, "Certificate changed SAN bytes.");
        var reader = new AsnReader(saved.RawData, AsnEncodingRules.DER);
        AsnReader names = reader.ReadSequence();
        Require(names.PeekTag() == new Asn1Tag(TagClass.ContextSpecific, 2), "DNS tag is not primitive [2].");
        Require(names.ReadCharacterString(UniversalTagNumber.IA5String, new Asn1Tag(TagClass.ContextSpecific, 2)) == dns, "Unrelated DNS name changed.");
        foreach (string identifier in identifiers)
        {
            Require(names.PeekTag() == new Asn1Tag(TagClass.ContextSpecific, 8), "registeredID is not primitive [8].");
            Require(names.ReadObjectIdentifier(new Asn1Tag(TagClass.ContextSpecific, 8)) == identifier, "OID changed.");
        }
        names.ThrowIfNotEmpty();
        reader.ThrowIfNotEmpty();
        foreach (string invalid in new[] { "", "3.1", "1.40.1", "1.2..3", "1.2.-1", "urn:reedbank" })
        {
            try { Create(dns, [invalid]); throw new InvalidOperationException("Invalid OID accepted: " + invalid); }
            catch (Exception error) when (error is ArgumentException or CryptographicException)
            { Console.WriteLine($"REJECT oid='{invalid}' {error.GetType().Name}"); }
        }
#if NET11_0_OR_GREATER
        var builder = new SubjectAlternativeNameBuilder();
        try { builder.AddRegisteredId(null!); throw new InvalidOperationException("Null accepted."); }
        catch (ArgumentOutOfRangeException) { }
        builder.AddRegisteredId(identifiers[0]);
        byte[] before = builder.Build().RawData;
        try { builder.AddRegisteredId("1.2..3"); throw new InvalidOperationException("Invalid OID accepted."); }
        catch (CryptographicException) { }
        Require(before.SequenceEqual(builder.Build().RawData), "Failed append changed builder.");
        Require(builder.Build(true).Critical, "Explicit critical flag lost.");
#endif
        Console.WriteLine($"PASS {Environment.Version} certificate DER={Convert.ToHexString(saved.RawData)}; DNS=[2] {dns}; registeredID=[8] {string.Join(',', identifiers)}; invalid inputs rejected; no trust/hostname validation performed.");
    }

    private static X509Extension Create(string dns, string[] identifiers)
    {
#if NET11_0_OR_GREATER
        return PermitSan.Create(dns, identifiers);
#else
        return ManualSan.Create(dns, identifiers);
#endif
    }

    private static void AssertRuntimeBaseline()
    {
        string coreLib = typeof(object).Assembly.Location;
        Console.WriteLine("RUNTIME " + JsonSerializer.Serialize(new
        {
            framework = RuntimeInformation.FrameworkDescription,
            coreLibPath = coreLib,
            coreLibVersion = FileVersionInfo.GetVersionInfo(coreLib).ProductVersion,
            coreLibSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(coreLib))).ToLowerInvariant(),
        }));
#if NET11_0_OR_GREATER
        const string expected = ".NET 11.0.0-rc.2.26504.105";
#else
        const string expected = ".NET 10.0.13";
#endif
        Require(RuntimeInformation.FrameworkDescription == expected, "Runtime baseline drift: " + RuntimeInformation.FrameworkDescription + "; expected " + expected);
    }
}
