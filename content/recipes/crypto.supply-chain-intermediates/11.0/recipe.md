# Supply certificate intermediates without relying on excess AIA hops

## Goal

An archive accepts a signed manifest only when its signer chains to the intended
test root and the manifest signature matches. Make missing intermediate certificates
available without depending on three successive issuer downloads.

## Older approach

On the measured Linux .NET10 runtime, the archive's five-certificate chain can fetch
three missing intermediates through Authority Information Access (AIA) HTTP URLs.
That is a valid older fallback, subject to network availability and chain policy.
Explicitly supplied intermediates work on the older runtime as well.

{{snippet:older}}

Pass the original manifest bytes, an RSA/SHA-256 PKCS#1 signature, the signer
certificate, the fixture root and the known intermediates to `Accept`. For the old
download-only comparison the intermediate sequence is empty. This is a fixed
synthetic archive signature contract, not a recommendation to change another
protocol's algorithm or signature encoding.

Both versions use this complete private-fixture policy helper:

{{snippet:policy}}

`testRoot` is an isolated test CA selected by the caller, not a root learned from
the untrusted manifest. `NoCheck` deliberately removes revocation networking from
this offline fixture. Production callers must choose trusted roots, certificate
purpose/identity and revocation policy for their own protocol; do not copy these
test-policy choices as universal production validation.

## Newer approach

The current OpenSSL chain engine has a two-attempt issuer-download budget. It is
not a maximum of two intermediates or a guarantee of two successful HTTP responses.
Supply every missing intermediate and disable certificate downloads; still require
`Build` and the signature check to succeed.

{{snippet:newer}}

`ExtraStore` provides candidates to chain building; it does not trust them. The
sample's untrusted root is present as a candidate but absent from CustomTrustStore,
and is rejected. An expired signer is also rejected even with every intermediate
available. Never fix a chain by accepting every certificate or setting ignore-all
verification flags. A valid signature alone does not establish signer trust.

## Migration

1. Identify the intended root and complete intermediate chain from an authenticated
   provisioning source. Keep root trust, issuer availability and revocation policy
   as separate decisions. Do not trust a root simply because it arrived with data.
2. Load certificates with `X509Certificate2.CreateFromPem` or the supported loader
   matching their actual format. Keep certificates alive through validation and
   dispose them afterwards. This fixture generates private ephemeral certificates
   in memory instead; it installs no roots.
3. Add the intermediates to `ChainPolicy.ExtraStore`, and set
   `DisableCertificateDownloads=true` if the chosen validation policy must not fetch
   issuers. Keep verification flags strict. For this fixture use only its own root
   in `CustomRootTrust`; configure a real application's trust separately.
4. Require successful chain building and verify the exact manifest bytes/signature
   with the agreed algorithm. Return a visible rejection on either failure; inspect
   `ChainStatus` to diagnose missing issuer, untrusted root or invalid validity time.
5. Repeat with an empty issuer cache, unavailable download route, untrusted root,
   expired chain and tampered manifest. Do not infer TLS client-certificate defaults
   or online revocation from this standalone `X509Chain` exercise.

## Constraints

The .NET11 primary target uses the framework X509 APIs; no extra package, provider
package, workload, rendering mode or experimental opt-in is needed. The native
provider here is Linux/OpenSSL. Exact Azure Linux3/.NET10.0.13 and Azure Linux4
beta/.NET11RC2 images are digest-pinned by the recorder. Loaded OpenSSL version,
native library hash, OS/architecture, runtime and commands are captured. Separate
Windows SDK10.0.402/C#14 and SDK11RC2/C#15 consumers compile the actual reference
sets; compilation itself does not prove Linux behavior.

Only this bounded Linux chain is measured. Windows/macOS/other OpenSSL versions,
TLS handshakes, online CRL/OCSP, production key provisioning and final GA behavior
are outside the evidence. No public budget-raising switch is taught. The alternative
of managed intermediate-store provisioning needs its own lifecycle/access policy;
this recipe keeps all state private instead.

## Evidence

`eng/Record-FileChain.ps1 -VerifySources` first captures actual paired compilation,
then executes four new containers per runtime using real native chain building.
Each has network mode `none` (loopback remains available), a read-only root,
temporary private HOME/cache, random ephemeral CA keys and an instrumented loopback
HTTP issuer. No container shares certificate cache state with another.

| Case | .NET10 | .NET11 RC2 |
| --- | --- | --- |
| Three missing intermediates, AIA allowed | Success; three issuer requests | PartialChain; two issuer requests |
| All intermediates supplied, downloads disabled | Success; zero requests | Success; zero requests |
| Root supplied but untrusted | UntrustedRoot; zero requests | UntrustedRoot; zero requests |
| Expired signer, full issuer chain | NotTimeValid; zero requests | NotTimeValid; zero requests |

Real signing/verification and tamper rejection run alongside chain outcomes; the
canonical supplied-chain verifier also executes. Listeners stop and containers,
private keys and tmpfs caches disappear; no shared stores or persistent settings
are edited. `verification-evidence.json` is compilation evidence;
`linux.evidence.json` separately records execution and cleanup. The ordinary
installed tool exposes both with canonical Markdown/applicability offline.

RC evidence does not close PIN-959/PIN-962 or existing Human gates. The exact eight
foundations and recipe/edition identities remain intact; no native CI matrix or
production/public publication is introduced.
