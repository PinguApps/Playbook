# Cryptography platform migrations: PIN-686

Reviewed **22 October 2026**. Source-only audit of all five entries in the .NET 11 cryptography compatibility section, the cryptographic entry in the general obsolete index, and adjacent TLS certificate-download policy. Six assigned migration goals and one adjacent goal were submitted for delivery approval. The reconciliations below supply D (PIN-1060), E (PIN-1061) and G (PIN-1063); the original audit remains source evidence.

## Scope and evidence

[PIN-686](https://linear.app/pinguapps/issue/PIN-686), `SPEC.md` at `0a8989814ea11346354464b845c79c15bb71c5e9`, stories 47, 101, 158 and 159; approved proposal `b3778cc`, key `SH_Q06`; stable inventory ID `migration.crypto-platforms`. Sources consulted: the full target and empty comments/attachments, cited spec/proposal, current specification, planning README, coverage/implementation/verification/release plans, tool contract, glossary, contribution/dependency policies and ADRs 0001–0004. Root PLAN.md, CONTEXT.md and CONTEXT-MAP.md are absent. No explicit requirement-bearing pointer is unavailable. Historical claims that implementation has not begun predate the current host and installed-tool harness.

Linear boundary: PinguApps team `62f80270-b641-4fa3-b60c-46be4c4df2c8`, Playbook project `b22380c7-6fde-49f4-8826-e5631dc5df7a`. No native blocker. An archived-inclusive, paginated overlap query returned 317 issues; both IDs were checked on every result. Full descriptions and comments for PIN-705, PIN-750, PIN-751, PIN-752, PIN-689 and PIN-959 were read after scoped discovery. Their existing scopes are preserved; none already delivers the seven goals below. No related issue is changed or created.

The [30-file manifest](cryptography-platforms.sources.json) pins immutable URLs, SHA-256 and source headings:

- dotnet/docs `9b5f5f81de8fd9851fc46273e93e8664d9cc3556`: complete compatibility index, all five full cryptography notes, general obsolete index, both diagnostic pages, full TLS AIA note and cumulative library notes. Read behavior, introduction, rationale, mitigation and affected APIs, not only titles. The index explicitly remains incomplete.
- dotnet/dotnet `4099f83977f4d29604a9aeb971fb0a7bc1d5f699` / `820230c386db5b0106c5430b3e5aee92a7826bf5`: previous .NET 10 / .NET 11 RC2 source snapshots used by the existing family audits. Compare cryptography reference declarations, platform project selection, Windows composite dispatch, OpenSSL chain construction and TLS validation; qualify them with selected implementation files. This is source inspection, **not a new binary/API asset comparison**; PIN-705 retains that broader obligation.

Source inclusion is not proof of consumer compilation, native algorithm availability or a successful operation. No SDK was installed and no inspected code was executed. Hash replay establishes the reviewed bytes, not semantic correctness or complete release coverage. Windows CNG/Insider availability, actual macOS/OpenSSL behavior, package assets and final GA behavior need delivery evidence.

## Full migration dispositions

The source-review decisions are resolved within this boundary; implementation and publication obligations remain open. Letters refer to approval-only delivery drafts below. Source IDs refer to exact manifest records.

| Finding / introduction | Authored migration and exact limits | Disposition and source qualification |
| --- | --- | --- |
| [macOS DSA removal][dsa], Preview 1 | Finite-field DSA operations, including `DSA.Create`, `DSACryptoServiceProvider` construction and `DSACertificateExtensions.GetDSAPrivateKey`, `GetDSAPublicKey`, `CopyWithPrivateKey`, can throw `PlatformNotSupportedException`. ECDSA is distinct and unaffected. Reissue keys/certificates and agree signature formats with peers; existing DSA keys/signatures cannot be converted by changing the class name. | **Included → A.** `crypto-project-10/11` selects SecurityTransforms previously and `DSA.Create.NotSupported.cs` on current macOS. The former supported only imported 1024-bit/SHA-1 operations, not key generation. iOS/tvOS/MacCatalyst never supported DSA. Public declarations remain: this is platform implementation removal, not deletion of the DSA API across .NET. |
| [Windows Composite ML-DSA native dispatch][composite], Preview 7 | Check `CompositeMLDsa.IsAlgorithmSupported` for the exact parameter set before `GenerateKey`/import/signing. The documented CNG subset is `MLDsa44WithECDsaP256`, `MLDsa65WithECDsaP256`, `MLDsa65WithECDsaP384`, `MLDsa87WithECDsaP384`. RSA combinations lose the prior managed path; EdDSA combinations were already unavailable on Windows. | **Included → B.** `composite-windows-10/11` changes managed composition to CNG; `composite-parameters` maps exactly these four sets. A declared member does not imply native support. The API remains experimental **SYSLIB5006** in both source references. Ordinary Windows versions are not promised to support the Insider composite provider. Composite certificate APIs remain unsupported on Windows; PIN-751's ML-KEM/HPKE/CMS goals are different. |
| [Windows HKDF native dispatch][hkdf], Preview 1 | `HKDF.DeriveKey` input keying material and `HKDF.Expand` pseudorandom-key input encounter documented 2,048-byte CNG limits; oversized inputs can throw `CryptographicException`. These are input limits, not an output-length limit or a universal cap on salt/info. Validate the application's input contract and handle an incompatible request. | **Delivered → PIN-1059 / C.** `hkdf-windows` uses CNG when available, but `Extract` stays managed; unavailable CNG and MD5 take managed paths. The broad Windows headline needs these qualifications. No new public override switch was established. Do not truncate/hash inputs, change derivation or silently select a weaker hash as a compatibility fix; that can change keys or protocol security. |
| [OpenSSL AIA chain-building limit][aia], Preview 7 | `X509Chain.Build(X509Certificate2)` has a two-download issuer-fetch budget. Supply missing intermediate certificates through `ChainPolicy.ExtraStore` or provision an intermediate store. Keep trust validation: an intermediate in ExtraStore is not automatically a trusted root. | **Delivered → PIN-1060 / D.** `aia-chain-11` decrements a two-attempt loop before downloading; failures/no progress can stop earlier. It is not a two-certificate maximum chain depth, nor a guarantee of two successful fetches. The prior loop lacked that budget. No public limit-raising switch was established in this reviewed path. Keep this separate from TLS defaults below and from revocation-network policy. |
| [SYSLIB0065][raw], Preview 6 | Only the `AsnEncodedData.RawData` **setter** is newly obsolete. Construct the appropriate derived type for new values, or use its supported virtual `CopyFrom(AsnEncodedData)` for mutation. The getter remains usable. Blanket suppression does not repair stale decoded state. | **Delivered → PIN-1061 / E.** `crypto-ref-10/11` places the diagnostic on the setter. `asn-state` clones assigned bytes without derived invalidation; `basic-constraints` overrides CopyFrom and clears its decode cache. The diagnostic page supplies the semantic alternative omitted by the broad note's suppression advice. Review actual derived-type behavior, OID and Critical handling; no claim that every subclass implements identical copy semantics. |
| [SYSLIB0064][rsa], .NET 11 | `RSACryptoServiceProvider.Encrypt(byte[], bool)` / `Decrypt(byte[], bool)` become obsolete. Preserving wire compatibility maps true to explicit `RSAEncryptionPadding.OaepSHA1`, false to `Pkcs1`. A stronger OAEP hash is a separate provider/protocol migration requiring both peers and existing key access to be considered. | **Included → F.** Found in `core-obsoletions`, not the cryptography table. `rsa-csp` on Windows accepts only Pkcs1/OaepSHA1 in these padding overloads; changing this provider's argument to OaepSHA256 throws. Do not present the diagnostic page's stronger-hash advice as an implementation supported by every RSA provider. Legacy CAPI key containers are not automatically preserved by `RSA.Create`. PIN-689 owns the index map, not this semantic replacement. |
| Adjacent [TLS server AIA default][tls], Preview 3 | `SslStream.AuthenticateAsServer` / `AuthenticateAsServerAsync` disable missing-intermediate downloads for client-certificate validation when no custom chain policy is supplied. Provide the chain from the client (`ClientCertificateContext`), or use server `CertificateChainPolicy.ExtraStore` with explicit trust/download policy. | **Delivered → PIN-1063 / G; cross-reference PIN-705.** `tls-validation-11` applies the default only in the server/no-custom-policy branch. Custom `X509ChainPolicy.DisableCertificateDownloads` is respected. `tls-switch` also contains `System.Net.Security.EnableServerAiaDownloads` / `DOTNET_SYSTEM_NET_SECURITY_ENABLESERVERAIADOWNLOADS`: a compatibility escape, not the preferred migration or a bypass for Linux's per-build budget. PIN-752's caller-owned TLS I/O is a separate goal. |

## D delivery reconciliation — 30 October 2026

PIN-1060 delivers `crypto.supply-chain-intermediates` edition11.0. ArchiveManifest
uses independent Windows SDK10.0.402/C#14/ref10.0.13 and SDK11RC2/C#15/ref11RC2
compilation, then actual Linux x64 runtime10.0.13/11RC2 execution in digest-pinned
Azure Linux3/4-beta images. No package is needed. The loaded OpenSSL library's
version/path/hash and runtime/OS are captured, separately from compiler hashes.

Four fresh network-none containers per release each create random private CA
material, a loopback issuer and private HOME/cache tmpfs. Old AIA fetches three
intermediates and succeeds; current fetches two and fails PartialChain. Supplying
all intermediates with downloads disabled succeeds with zero requests on both.
A supplied but untrusted root fails UntrustedRoot; an expired signer fails
NotTimeValid. Real manifest signatures and tamper rejection plus the canonical
supplied verifier execute. No shared trust stores are edited; listeners stop and
containers/private cache/keys are removed. No TLS/online-revocation/other-platform
or actual-GA claim follows. CustomRootTrust/NoCheck belong only to the offline fixture.

`eng/Record-FileChain.ps1 -VerifySources` retains fresh compilation and separate
Linux execution through the existing sample-evidence workflow. Real ordinary
packed/local-feed-installed offline stdio list/get verifies ID/title/goal, complete
canonical Markdown, target11/edition11, applicability and evidence; see the
[walkthrough](../../inspect-guidance.md#file-load-watch-root-and-certificate-issuer-boundaries).
Other crypto delivery owners, exact eight foundations and PIN-959/PIN-962/Human
gates remain open. No native CI matrix or public publication is added.

## E/G delivery reconciliation — 31 October 2026

PIN-1061 delivers `crypto.replace-encoded-extension-data` edition11.0. Original
LaboratoryInspector consumers compile/run SDK10.0.402/C#14/ref-runtime10.0.13 and
SDK11RC2/C#15/ref-runtime11RC2 on Windows x64. The older setter reproduces stale
Basic Constraints fields despite changed raw bytes. Typed construction and CopyFrom
verify all decoded fields, bytes/OID, Critical true/false, malformed lazy-decode
rejection and getter-only code. An isolated current-target build of the unchanged
old setter fails specifically with SYSLIB0065, without suppression; supported
current code builds with warnings as errors. X509Extension input/Critical handling
is qualified separately rather than generalized to every derived type.

PIN-1063 delivers `tls.supply-client-certificate-chain` edition11.0. Separate paired
Windows compilation precedes actual Linux/OpenSSL execution in eight fresh,
network-none/read-only runtime containers, each with random private CA/server/client
certificates and private HOME/cache tmpfs. The no-custom-policy old flow fetches one
issuer and exchanges booking/ACK data; current fails PartialChain with zero fetches.
Server ExtraStore/downloads-disabled succeeds with mutual TLS/data on both; expired
clients and an unrelated trusted root are rejected with zero fetches. Trust is
private and strict, the client sends only its leaf, and NoCheck is deliberately an
offline-fixture policy rather than online-revocation evidence. TLS1.3 was negotiated.
Loaded OpenSSL3.3.7/3.5.4, libssl/libcrypto hashes, exact images, certificate inputs,
commands and listener/container/cache cleanup are saved through sample evidence.
No shared trust stores, switches, production keys or caller-owned TLS I/O are used.

`eng/Record-CertificateLinks.ps1 -VerifySources` captures affected consumer evidence
and checks the selected original/supplemental immutable sources in
[certificate-links.sources.json](certificate-links.sources.json). The ordinary
packed, isolated local-feed-installed artifact is inspected with real offline
stdio list/get, canonical Markdown, target11/edition11, applicability and freshness;
see [the walkthrough](../../inspect-guidance.md#certificate-extension-state-client-tls-chains-and-file-aliases).
Other crypto owners, PIN-705's broader audit, eight foundations, PIN-959/PIN-962 and
existing Human gates remain open. Windows TLS, other native platforms, online
revocation and actual-GA behavior are not established. No native CI matrix is added.

## C delivery reconciliation — 31 October 2026

PIN-1059 delivers `crypto.bound-hkdf-inputs`, edition11/target11. Original telemetry
consumers compile/run actual SDK10.0.402/C#14/ref-runtime10.0.13 and
SDK11RC2/C#15/ref-runtime11RC2 on Windows x64 build26200. CNG HKDF opens successfully;
the exact current implementation selects it. Native bcrypt10.0.26100.1 and
compiler/reference/runtime hashes are recorded separately from source review.

SHA-256 DeriveKey IKM and Expand PRK execute at2048/2049 bytes on both releases.
Accepted outputs match independent HMAC construction and remain identical across
releases. Older2049 inputs succeed; current raw calls reject. The canonical
boundary rejects oversized inputs before derivation, returning false/null without
rewriting input, salt or context. RFC5869 case1 is checked independently; managed
Extract remains separately recorded at both input sizes. Output length is32 for
the input-boundary cases, not2048. No silent truncation/rehashing, compatibility
bypass, all-hash/all-Windows or actual-GA claim is added.

`eng/Record-NativeBoundaries.ps1 -VerifySources` captures ordinary input-fresh
sample evidence and [bounded source identities](native-boundaries.sources.json).
The [walkthrough](../../inspect-guidance.md#pipe-handles-mobile-locking-native-webassembly-and-hkdf-inputs)
uses the actual ordinary packed/isolated-feed-installed tool over offline stdio
list/get. PIN-959 comprehensive coverage, PIN-962 actual-GA reconciliation and all
other crypto owners remain open; no resource/public package publication is added.

## Grouping, exclusions and remaining boundaries

- **Grouped ownership, not delivered coverage:** PIN-705 owns broader runtime/security reference-asset and source reconciliation; it should consume this mitigation analysis rather than repeat it. PIN-689 retains exhaustive obsolete-index ownership, with SYSLIB0064/0065 semantic work allocated here. PIN-959 retains the comprehensive gate, including delivery of every qualifying audit follow-on.
- **Preserved independent delivery:** PIN-750 owns X25519, fixed-time byte comparison and key wrapping; PIN-751 owns composite ML-KEM/HPKE/CMS encryption. None is a DSA/signature migration, HKDF-limit migration or RSA padding migration. Similar security terminology does not justify marking those tickets satisfied or expanding them silently.
- **Excluded as standalone goals:** native dispatch speed alone, unaffected ECDSA, unchanged RawData reads, and blanket warning suppression. Platform failure handling, certificate supply and explicit padding do alter authored code and remain included. Existing `networking.automatic-benefits` exclusions do not erase these concrete migrations.
- **Unresolved execution facts:** no exact Windows Insider composite-signature build/provider availability was measured, no Mac DSA removal was executed, no OpenSSL chain cache/download behavior was exercised, and no final GA artifacts were checked. These are explicit delivery prerequisites, not evidence that the source findings are excluded. A source hash or support-negative result cannot establish a positive native cryptographic round trip.

## Delivery proposals for backlog approval

**A–G are presented for Pingu's approval before tracker creation or implementation.** Proposed IDs are unpublished review labels, not changes to `migration.crypto-platforms`. Native estimates follow observed S=2, M=3, L=5. Intended destination is the existing **02 - Language and library guidance** milestone; allocate current unused `02-DDD` titles only after approval and a fresh archived-inclusive numbering check. No milestone/name changes or delivery tickets are created here.

### Common acceptance for each approved slice

Deliver one original goal-based recipe, with offline-complete prose, canonical previous/current samples, migration steps, alternatives and explicit constraints. Compare real .NET 10 / pinned .NET 11 artifacts; explain unavailable older capabilities without inventing substitutes. Record SDK/runtime/target/language, resolved package assets and relevant OS/provider versions. Preserve experimental labels and exact narrow opt-ins in discovery and detail. Normal dependency-policy and compilation/evidence freshness checks apply.

Compile both actual consumers and run each claim's real behavior checks below. Save commands, input hashes, output and native versions; source review, mocks and support-negative checks cannot substitute for required positive execution. Capture native evidence during authoring; no new recurring native CI matrix. Then build/pack once, install that ordinary artifact using the existing isolated feed/cache harness, and call real stdio `list_recipes`/`get_recipe` offline from an unrelated directory. Verify ID/title/**goal**, full canonical body, edition, platform/algorithm/stability applicability and evidence. Update the disposition register; missing required compilation or native proof blocks that slice's publication. Reconcile GA without relabelling RC results.

### A. Replace a macOS DSA signing contract — L (5)

Proposed ID `crypto.migrate-dsa-signatures`. An original coastal-survey exporter moves newly issued report signatures to ECDSA while identifying legacy DSA reports explicitly. Read the old key/certificate format, peer algorithm agreement and signature encoding before choosing replacements; do not imply old signatures can be verified by a new ECDSA key.

Prerequisites: a suitable macOS authoring machine, actual old/current runtimes, a synthetic imported DSA-1024/SHA-1 fixture generated on a supporting environment, and independently issued test ECDSA keys/certificates. Verify the .NET 10 Mac legacy verification path and .NET 11 platform failure, then real new signing/verification, tampered data rejection and the explicit unsupported legacy outcome. Do not use Mac DSA key generation as setup. Cover the listed certificate/key access boundary if claimed; scope this slice to that synthetic report contract, with no production key migration or external certificate issuer. L includes paired native and protocol-format evidence; Windows-only proof cannot complete it.

### B. Gate Windows composite signatures by the exact algorithm — L (5)

Proposed ID `crypto.select-supported-composite-signature`. A fictional observatory's signed bulletin names its agreed parameter set and fails clearly if that set is unavailable. No automatic substitution to a different algorithm or classical-only signature. Keep SYSLIB5006 labelled, with only the explicit experimental opt-in needed by the sample.

Prerequisites: measured Windows builds/providers supporting old managed composition and current native composite execution. For the same original bulletin, capture old supported RSA-composite behavior versus current rejection, actual sign/verify and tamper rejection for an agreed supported ECDSA-composite set, and a real unsupported-host/set branch. Record support probes and positive operations separately. Do not fabricate Windows certificate support or claim all four sets were exercised if only one was. L reflects native availability plus old/current algorithm behavior; inability to obtain the positive native route remains a blocker.

### C. Enforce an explicit HKDF input contract on Windows — M (3)

Proposed ID `crypto.bound-hkdf-inputs`. An original telemetry key-derivation boundary validates input lengths and reports an incompatible request before derivation. Preserve all keying bytes, salt and context for accepted inputs. Oversized legacy input is an explicit error requiring protocol agreement, not silently truncated or rehashed data.

Prerequisites: actual Windows CNG path, measured OS/runtime and deterministic synthetic inputs; SHA-256 is the bounded test algorithm. Execute DeriveKey IKM and Expand PRK cases at 2,048 and 2,049 bytes on both releases, distinguishing the PRK input from output length. Check accepted output against a known vector/independent expected result and unchanged old/new accepted outputs. Demonstrate rejection leaves no usable derived-key result. Record managed Extract separately if discussed; no all-hash/all-Windows guarantee. M covers a small validation boundary, paired real execution and complete delivery.

### D. Supply certificate intermediates without relying on excess AIA hops — M (3)

Proposed ID `crypto.supply-chain-intermediates`. A fictional archive validates a signed manifest against its isolated test root and supplies intermediate certificates explicitly. Keep root trust, intermediate availability and revocation policy separate; never fix the chain by accepting every certificate.

Prerequisites: isolated Linux/OpenSSL with real old/current runtimes, private ephemeral CA material and a loopback HTTP issuer endpoint. Use fresh process/cache state so a chain needs three downloads. Record requests and chain results for the old path, current budget failure, and current success with ExtraStore and downloads disabled. Include an untrusted-root failure and an invalid-chain case. Use CustomRootTrust only inside the isolated fixture; do not install roots or edit shared stores. M assumes one bounded chain/endpoint fixture; retain cache isolation and cleanup evidence with the sample.

### E. Update decoded ASN.1 extension state through supported APIs — S (2)

Proposed ID `crypto.replace-encoded-extension-data`. A fictional laboratory's certificate inspector keeps Basic Constraints values coherent after replacing encoded input. Compare the existing RawData assignment with a correctly typed constructor for replacement and CopyFrom for deliberate mutation; verify the Critical flag according to the chosen API instead of assuming it is copied.

Compile the old setter against .NET 10 and show the specific SYSLIB0065 diagnostic on .NET 11 without blanket suppression. Run the original stale-cache case and the constructor/CopyFrom results using distinct valid extension inputs, checking decoded fields and raw bytes. Check malformed input at the actual decode boundary. Verify getter-only code remains valid and the newer consumer compiles with warnings as errors. No network/native fixture is required; S covers one derived extension type and both supported update choices.

### F. Make legacy RSA encryption padding explicit — M (3)

Proposed ID `crypto.make-rsa-padding-explicit`. An original instrument gateway retains its existing test-key ciphertext contract while replacing Boolean padding calls. Keep compatibility migration and a stronger algorithm/provider redesign separate. Explain true/OaepSHA1 and false/Pkcs1 as legacy interoperability mappings, without recommending them for a new protocol.

Prerequisites: actual Windows CAPI provider and disposable test key, old/current runtimes, no production container access. Compile both legacy mappings and the warning-free explicit overloads; record SYSLIB0064 on the legacy current-target consumer. Execute cross-decryption of old/new ciphertext in both directions for both mappings and rejection of wrong padding/tampered input. Also execute the same provider's OAEP-SHA256 failure so the limitation is observable. If offering a stronger option, separately prove it through a supporting provider and peer/key-format agreement; otherwise leave it outside this slice. M includes interoperability and provider-negative evidence, not only a diagnostic suppression test.

### G. Validate TLS client certificates with an explicitly supplied chain — M (3)

Proposed ID `tls.supply-client-certificate-chain`; adjacent finding for PIN-705 to consume. A fictional rehearsal-room service accepts its own test client certificate only when the valid chain is available. Prefer a client-supplied intermediate context or an explicit server ExtraStore policy with downloads disabled. Scope this to one loopback mutual-TLS flow, independent of PIN-752's experimental I/O mechanism.

Prerequisites: real old/current TLS runtimes, ephemeral private CA/server/client certificates and instrumented loopback AIA endpoint, isolated from shared trust/cache state. Capture the old no-custom-policy download path, the new no-policy failure with missing intermediates, then a successful handshake and data exchange with the chosen supplied-chain route and no AIA requests. Keep invalid-client/untrusted-root rejection. Set revocation deliberately for the offline fixture and disclose that it does not prove online revocation. If teaching the compatibility switch/custom download opt-in, run and bound that behavior too; it is not necessary for the preferred recipe. M covers one authenticated transport fixture and cleanup, not production certificate provisioning.

## Reproduce source verification

PowerShell 7 and public network access only. This exact block was tested from an unrelated directory. It downloads immutable text, checks all 30 hashes and the six complete migration notes, then displays seven proposal headings. It does not build or run inspected code or the unchanged MCP server.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-13e96827'
$ErrorActionPreference = 'Stop'
$manifest = Get-Content -LiteralPath './docs/planning/audits/cryptography-platforms.sources.json' -Raw | ConvertFrom-Json
$auditTemp = Join-Path ([IO.Path]::GetTempPath()) ('pin686-' + [guid]::NewGuid())
New-Item -ItemType Directory -Path $auditTemp | Out-Null
try {
    if ($manifest.sources.Count -ne 30) { throw 'Expected 30 reviewed sources' }
    $notes = @('dsa-macos', 'composite-native', 'hkdf-cng', 'aia-limit', 'crypto-obsoletions', 'tls-aia')
    foreach ($source in $manifest.sources) {
        $file = Join-Path $auditTemp ($source.id + '.txt')
        Invoke-WebRequest -Uri $source.url -OutFile $file
        if ((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant() -ne $source.sha256) {
            throw "Source hash mismatch: $($source.id)"
        }
        if ($source.id -in $notes) {
            $body = Get-Content -LiteralPath $file -Raw
            foreach ($section in @('Previous behavior', 'New behavior', 'Recommended action', 'Affected APIs')) {
                if ($body -notmatch ('(?m)^## ' + [regex]::Escape($section) + '\r?$')) {
                    throw "Missing mitigation section: $($source.id) / $section"
                }
            }
        }
    }
    $proposals = @(Get-Content -LiteralPath './docs/planning/audits/cryptography-platforms.md' |
        Select-String -Pattern '^### [A-G]\. ')
    if ($proposals.Count -ne 7) { throw 'Expected seven approval-only proposals' }
    $proposals
    'PASS: 30 source hashes; six full migration notes; seven approval-only delivery proposals'
}
finally {
    $tempParent = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\', '/')
    if ((Split-Path -Parent ([IO.Path]::GetFullPath($auditTemp))) -ne $tempParent) {
        throw 'Unexpected source-verification directory'
    }
    Remove-Item -LiteralPath $auditTemp -Recurse -Force
}
```

Expected: A–G headings followed by PASS. A failed download, hash, section or proposal count is a failure requiring review. Hashes do not validate the migration conclusions; compare the complete notes and qualified implementation paths with the dispositions. No new served recipe means no changed discovery/Markdown/applicability response to demonstrate. Future delivery must satisfy the common real installed-MCP acceptance above. Actual checks, scope gaps and tracker state are recorded in [PIN-686 verification](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-686.md).

[dsa]: https://github.com/dotnet/docs/blob/9b5f5f81de8fd9851fc46273e93e8664d9cc3556/docs/core/compatibility/cryptography/11/dsa-removed-macos.md
[composite]: https://github.com/dotnet/docs/blob/9b5f5f81de8fd9851fc46273e93e8664d9cc3556/docs/core/compatibility/cryptography/11/compositemldsa-windows-native.md
[hkdf]: https://github.com/dotnet/docs/blob/9b5f5f81de8fd9851fc46273e93e8664d9cc3556/docs/core/compatibility/cryptography/11/hkdf-windows-cng.md
[aia]: https://github.com/dotnet/docs/blob/9b5f5f81de8fd9851fc46273e93e8664d9cc3556/docs/core/compatibility/cryptography/11/aia-fetch-limit-linux.md
[raw]: https://github.com/dotnet/docs/blob/9b5f5f81de8fd9851fc46273e93e8664d9cc3556/docs/fundamentals/syslib-diagnostics/syslib0065.md
[rsa]: https://github.com/dotnet/docs/blob/9b5f5f81de8fd9851fc46273e93e8664d9cc3556/docs/fundamentals/syslib-diagnostics/syslib0064.md
[tls]: https://github.com/dotnet/docs/blob/9b5f5f81de8fd9851fc46273e93e8664d9cc3556/docs/core/compatibility/networking/11/sslstream-aia-downloads-disabled.md
