# Encode registered identifiers in certificate subject alternatives

## Goal

Put object identifiers (OIDs) into a certificate's subject alternative name (SAN)
extension and read them back as registeredID entries. A Reedbank survey permit
contains a DNS label and several fictional classification identifiers. These are
separate name forms; an OID does not become a DNS name, an application permission
or a trusted identity merely by appearing in a certificate.

## Older approach

.NET 10 has no SubjectAlternativeNameBuilder.AddRegisteredId. Its framework
ASN.1 writer can build the same DER extension without a package. The manual route
remains valid: GeneralNames is a SEQUENCE and registeredID uses the **implicit,
primitive context-specific tag [8]**, byte 0x88. Do not wrap a universal OID in an
explicit constructed [8] or encode its decimal text as a string. The DNS entry is
IA5String under primitive [2]. This small writer expects an already-ASCII DNS
name; it does not implement the builder's international-name conversion.

{{snippet:older}}

The comparison project uses net10.0, nullable/implicit usings and C# 14.0. It needs
System.Formats.Asn1 and System.Security.Cryptography from the shared framework,
with no NuGet package or workload. It adds no .NET 10 catalogue foundation.

## Newer approach

Use the .NET 11 SAN builder to combine the supported name forms. **The actual
RC2 signature is AddRegisteredId(string)**, taking dotted decimal notation.
The audit's draft spelling AddRegisteredId(Oid) is stale. Do not pass an Oid object
or assume its friendly-name lookup validates the value.

{{snippet:newer}}

For example, pass permit.reedbank.test and the identifiers 1.3.6.1.4.1.32473.41
and 1.3.6.1.4.1.32473.42. The 32473 enterprise arc is reserved for documentation;
these values are fictional and confer no production registration or authority.

## Migration

1. Retain the old encoder if its broader extension construction is still needed.
   Otherwise use net11.0 with the .NET 11 SDK/framework and the newer helper above.
   The canonical project declares C# 15.0, nullable and implicit usings. There is no
   package, workload, provider selector or experimental opt-in for AddRegisteredId
   in the reviewed contract. Exact RC2 SDK/runtime versions are verification pins.
2. Add each dotted OID separately, alongside existing DNS/IP/URI entries. The
   builder preserves insertion order and does not replace unrelated entries.
   AddRegisteredId rejects null/empty input with ArgumentOutOfRangeException and
   malformed OIDs with CryptographicException. The manual writer reports malformed
   values as ArgumentException. Keep validation at the input boundary and do not
   parse exception messages. A rejected builder addition leaves prior entries intact.
3. Call Build(critical: false) for this example, then add that extension to the
   CertificateRequest before signing. Build(true) is available when your certificate
   profile requires criticality; it is not a general recommendation. Replace an
   existing SAN deliberately rather than adding duplicate extensions.
4. Verify the exported certificate, not just the builder. This complete local
   helper issues a short-lived certificate with an ephemeral P-256 key and reads
   back this example's **one DNS entry followed by OIDs** layout. It is deliberately
   not a general decoder for SAN extensions containing every possible name form.

{{snippet:roundtrip}}

Call CertificateRoundTrip.Read with the extension returned by either encoder.
The tuple contains the DNS name, dotted identifiers and actual saved extension
DER. No root installation, chain build or private-key persistence is involved.

## Constraints and alternatives

Encoding a registeredID does not implement chain trust, TLS hostname matching,
DNSSEC, authorization, name constraints or certificate-policy enforcement. Use
the certificate profile and relying party's rules for those decisions. Do not
put an OID in AddDnsName to obtain registeredID encoding. ASN.1 permits large
arcs; do not validate OIDs by splitting them into fixed-width integers.

The encoder uses shared framework APIs. The round-trip fixture additionally
needs an available ECDSA P-256 certificate-signing provider; Windows x64 execution
is measured. Other providers/platforms and network identity validation are not
verified here. No UI/rendering integration is required. RC2 evidence is not GA
verification; PIN-962 retains that reconciliation and PIN-959 stays open.

## Evidence

SDK 10.0.402/runtime 10.0.13 and SDK 11.0.100-rc.2.26504.105/runtime 11RC2 compile and
run the real consumers with no package dependencies and warnings as errors.
Both export/reload certificates and assert the SAN OID 2.5.29.17, noncritical flag,
primitive [2]/[8] tags, unchanged DNS label, three exact OIDs (including a very
large arc), no trailing data and byte-identical manual/new DER. Empty, invalid
first/second arc, empty/negative arc and non-OID text reject. Current null input,
failed-append preservation and explicit criticality are separately checked.

The saved sample records include commands, exact inputs, compiler/CoreLib,
audit-matching cryptography references and supplemental ASN.1 reference hashes.
Refresh with eng/Test-IdentityRecords.ps1 -Record -VerifySources. These checks
establish encoding, not trust. No recurring native matrix or production resource
is added. Exactly eight .NET 10 foundations remain unchanged.
