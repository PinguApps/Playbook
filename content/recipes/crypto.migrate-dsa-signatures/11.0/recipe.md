# Replace a legacy DSA report-signing contract

## Goal

Issue new coastal-survey reports with an agreed ECDSA contract while keeping
archived DSA reports identifiable. A new key cannot verify an old signature.
Changing DSA to ECDsa is a protocol/key change, not an in-place key conversion.

.NET11 removes finite-field DSA support on macOS; it does not remove ECDSA or
DSA on every operating system. The affected macOS boundaries include DSA.Create,
DSACryptoServiceProvider construction and DSACertificateExtensions.GetDSAPrivateKey,
GetDSAPublicKey and CopyWithPrivateKey. The public declarations still exist.
Those macOS operations are source-reviewed here and **not-tested**, including
certificate access. Windows compilation/execution below is separate evidence.

## Older approach

The fictional Gannet Shoal archive contract uses an imported 1024-bit DSA public
key in DER SubjectPublicKeyInfo (SPKI), SHA-1, and a 40-byte IEEE P1363 signature
(fixed-width r followed by s). Its exact signed bytes include the report's trailing
newline. This old verification path remains a valid historical comparison; it
is not a recommendation to issue more DSA/SHA-1 signatures.

{{snippet:older}}

The previous macOS SecurityTransforms implementation supported only imported
DSA-1024/SHA-1 operations and never supported DSA key generation. Generate the
synthetic fixture on a supporting environment; do not make macOS DSA.Create(1024)
key generation a setup step. The retained public-only fixture was generated on
Windows, contains payload/SPKI/signature and has no private key. Importing that
fixture on .NET10/macOS and observing .NET11/macOS failure have not been executed.
iOS, tvOS and MacCatalyst did not support this DSA route previously.

## Newer approach

Agree the new contract with every signing/verifying peer first: NIST P-256,
SHA-256, ASN.1 DER sequence signatures, an explicit versioned algorithm name and a
key identifier resolved through trusted configuration. DER signatures have variable
length; ECDSA P1363 signatures for this curve are 64 bytes and are a different
encoding. Do not rely on an overload's default format.

{{snippet:contract}}

{{snippet:newer}}

Put both declarations in separate files in a net11.0 console project with
Nullable and ImplicitUsings enabled, using C#15 sample mode. Supply an independently
issued ECDSA certificate with its accessible private key to Sign. Give Verify the
corresponding **already trusted** public certificate and configured key identifier.
The certificate is a key carrier here; these helpers do not establish certificate
trust, expiry, revocation or identity. The sample uses short-lived, self-signed,
disposable certificates without adding them to a trust store.

Payload is the exact signed byte sequence, not a reconstructed text/JSON object.
The verifier only accepts its configured algorithm/key; it does not resolve a
certificate from an untrusted report. Legacy reports return UnsupportedLegacy
before any ECDSA verification. That result means this new-format verifier does
not authenticate the report; it must never be treated as success. The same
ECDSA helpers also compile/run in the .NET10 comparison: ECDSA is not new in11.

## Migration

1. Inventory the existing DSA key/certificate representation, key size, hash,
   signature encoding and exact signed report bytes. Confirm peers currently use
   SPKI/SHA-1/P1363 before applying this bounded comparison; PKCS#8/private-key
   files, certificates and SPKI are different containers, not interchangeable bytes.
2. Define a separate ECDSA report version with peers. Provision independent P-256
   keys/certificates and distribute authenticated public-key/identifier mappings.
   Do not import DSA parameters as an ECDSA key, rename archived signatures or
   assume replacing a certificate preserves the old private key.
3. Add SignedSurvey/SurveyVerification and CurrentReport. Route new issuance to
   Sign and configure the matching trusted public certificate at the verifier.
   Serialize algorithm, key ID, payload and signature without changing payload
   bytes; encode binary fields consistently, for example Base64 at the transport.
4. Keep legacy records tagged dsa1024-sha1-p1363-v0. On the new macOS11 route,
   report UnsupportedLegacy and stop. If archival verification is still required,
   retain a separately supported verifier and original DSA public material under
   an explicitly agreed retention policy. A new ECDSA key cannot recover that
   verification. No external verifier service is implemented in this slice.
5. Verify a new signature with the configured certificate and an independent SPKI
   peer. Reject modified payloads, unrelated certificates, unknown algorithm/key
   IDs and the wrong signature format. Treat native/key-access failures as an
   unavailable signing/verification operation, never as verified data or a reason
   to retry another algorithm.

## Constraints

Application route: .NET11 runtime/reference family and SDK, C#15 canonical sample
mode, macOS ECDSA provider with usable P-256 certificate/key access. Exact pins
below describe measured Windows evidence, not a requirement to use an RC in an
application. This is stable framework API guidance with no experimental opt-in,
NuGet package, workload, rendering mode, production key migration or external
certificate issuer. Host console build is workload-free; it is not a Mac app build.

Both canonical consumers are compiled/executed on Windows x64 CoreCLR. **Not-tested
on macOS:** .NET10 imported DSA verification/tamper rejection, .NET11 DSA.Create /
DSACryptoServiceProvider / DSA certificate access failures, and ECDSA key/certificate
access, new signatures, encoding/tamper rejection and unsupported-legacy outcome.
PIN-699 excludes those unavailable native checks without requiring another machine.
No Apple native binary was inspected or executed. iOS/tvOS/MacCatalyst, other
curves/key stores/encodings, certificate chain validation and final11 GA are outside
the proof. This guide adds only target11/edition11; the comparison preserves the
exact eight .NET10 foundations and other existing identities.

## Evidence

Record-SignaturePadding.ps1 captures SDK10.0.402/C#14/reference-runtime10.0.13 and
SDK11.0.100-rc.2.26504.105/C#15/reference-runtime11RC2, commands, input hashes,
empty dependency/workload closures and compiler/reference/managed/native Windows
artifact identities. The versioned source manifest qualifies the old/current
macOS dispatch and removal independently of those Windows artifacts.

Windows runs verify the imported synthetic DSA fixture (and old tamper rejection),
real new ECDSA signing/verification through certificate private/public access,
independent exported/imported SPKI verification, modified data, malformed/wrong
encoding signatures, unrelated certificates, unknown algorithm/key and explicit
unsupported legacy routing. The old/current code uses the same report input.
The fixture's exact bytes and generation source are retained; fresh evidence hashes
include them. None of these observations establishes the excluded macOS results.

Ordinary packed/local-feed-installed MCP retrieval checks full canonical Markdown,
ID/title/goal, target11/edition11, applicability, not-tested disclosures and fresh
saved evidence offline. RC evidence leaves PIN-959/PIN-962 and existing Human
gates open; it does not establish GA or comprehensive release readiness.
