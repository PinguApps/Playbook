# Bound XML signature references

## Goal

Reject signatures with more references than a deliberate application budget.
Fictional Merewick accepts at most eight signed permit references per signature.
Use a parser limit to bound this part of XML signature processing; it does not
replace input-size limits, trusted-key selection or authorization.

## Older approach

System.Security.Cryptography.Xml10.0.13 has a fixed parser limit of 100 direct
Reference children per SignedInfo. A stricter application policy remains valid:
count direct references before passing the Signature to SignedXml.LoadXml.
Malformed XML is still rejected by the cryptographic parser.

{{snippet:older}}

The application accepts only a positive budget up to the old parser's fixed100.
This precheck is an application boundary, not a patch to the library. Setting
the new configuration key on the measured older package does not lower its limit.
Use a nullable/implicit-usings net10.0/C#14 project with the Xml10.0.13 package;
the lock resolves matching Pkcs10.0.13 transitively. The helper is internal and
called within its assembly.

## Newer approach

The current Xml package reads a process-wide AppContext data value. Configure it
at process startup before any cryptographic XML work initializes cached settings.
This is SetData with an integer, not SetSwitch with a Boolean. The exact key has
no .Xml segment: System.Security.Cryptography.MaxReferencesPerSignedInfo.

{{snippet:newer}}

Call PermitManifest.ConfigureAtStartup() once before Verify or other XML crypto
operations. Configure all process users consistently; this cannot implement a
different per-request/tenant budget. Keep the older application check too when
different consumers require different limits. The fixture uses original locally
generated RSA2048/SHA256 signatures over same-document permit IDs.

## Migration

1. Use the demonstrated net11.0/C#15 route with a .NET 11 SDK/runtime and
   System.Security.Cryptography.Xml11 release family. Actual evidence pins
   11.0.0-rc.2.26504.105, selects lib/net11.0 and resolves matching Pkcs. The
   package also contains a net10.0 asset, but this recipe does not claim a verified
   older-target/new-package route. There is no workload, unsafe or experimental
   opt-in or rendering requirement. The tested provider is Windows x64 CNG RSA.
2. Choose a small positive budget from the protocol's needs, here8. Leave the
   default100 when that is the intended policy. The setting controls direct
   references in one SignedInfo, not document bytes, total signatures, transforms,
   nesting, decryption operations or total cryptographic work.
3. Set the value before first use. It is cached by the library's static settings;
   changing AppContext later does not reconfigure an initialized process. Restart
   with a new setting and test in separate processes. Prefer validating your
   deployment configuration rather than depending on conversion fallbacks.
4. Parse untrusted input with DtdProcessing.Prohibit, XmlResolver=null and a
   suitable XmlReaderSettings.MaxCharactersInDocument before loading a
   PreserveWhitespace=true XmlDocument whose XmlResolver is also null. An
   already constructed document is the helper's input; it does not undo unsafe
   parsing. Bound the byte stream before constructing it too. Preserve other XML
   protections and the library's safe transform/canonicalization defaults.
5. Call LoadXml and handle CryptographicException as rejection. Over-budget
   SignedInfo fails during parsing, before signature verification. A valid parsed
   signature must still pass CheckSignature with the application's trusted key;
   a document-supplied KeyInfo is not a trust decision. Handle false as rejection.
6. Bind the verified reference IDs to the exact application records used next.
   Enforce the expected document schema, unique IDs, signature location/count and
   permitted same-document references/transforms. This example enforces one
   direct signature under Permits and at least one reference; it is a budget
   example, not a complete XML signature-wrapping defense or permission policy.
   A valid signature alone never authorizes survey access.

## Constraints and alternatives

The measured default is100: valid1/8/9/100 signatures parse and verify, while101
is rejected. With the current setting8, valid1/8 still verify and9/100/101 fail
both SignedInfo.LoadXml and SignedXml.LoadXml. The old parser keeps its fixed100
despite setting8, while its application helper rejects9. No raised/unlimited
budget is recommended. Use a positive integer; no unlimited-budget mode is taught
here. An unconvertible value falls back to100, as tested in a fresh process; do not rely on
invalid configuration to enforce a stricter budget.

Other serviced depth/DTD/transform protections already exist in the old baseline;
they are not all new .NET 11 features. MaxDecryptedDataElements was already spelled
that way in the old source; no configuration-key rename is required. Do not
disable XML security limits or truncated-HMAC protections to make a fixture pass.
Use application validation on older packages or when process-global configuration
cannot express the required policy. Keep package/security maintenance deliberate.

## Evidence

eng/Test-LifetimeSignatures.ps1 -Record -VerifySources compiles/runs PermitManifest
on SDK10.0.402/runtime10.0.13/Xml10.0.13 and
SDK11.0.100-rc.2.26504.105/runtime11RC2/Xml11RC2. Locked dependency sets include
matching Pkcs. Recorded reference/package archive/selected/executed XML hashes
match the runtime-security audit; compiler/CoreLib/CoreCLR and Windows crypto
binary identities are retained separately from supported prerequisites.

Separate default/strict/late/invalid processes generate and hash original local
RSA2048/SHA256 fixtures at1/8/9/100/101 references, parse SignedInfo, load actual
SignedXml and check accepted signatures. Malformed extra SignedInfo children,
tampered signed content and a different RSA key reject. The late setting leaves9
accepted under the cached100 limit. Required valid signatures remain unaffected.
Keys are disposable process-local test keys; no production key or service is used.

The ordinary package's installed offline stdio list/get verifies discovery,
complete canonical Markdown, target11/edition11 applicability, fresh paired
evidence and target10 rejection. The eight .NET 10 foundations are unchanged.
Windows x64 RC2 is measured; other providers/platforms and final GA remain
unverified. PIN-959/PIN-962 and existing Human gates remain open; no recurring
native/client/OS matrix, public publication or persistent setting change is added.
