# Select cryptographic providers for key operations

## Goal

Rookery Observatory signs balloon instructions using an existing key identified
by an OpenSSL store URI. Load the providers required by that store explicitly,
and apply a property query to opening it. This configures key access; it does
not provision a hardware key or establish FIPS compliance.

## Older approach

.NET 10 provides the single-provider overload. For an ordinary PEM key supported
by the default provider, this remains a valid route. The caller owns the returned
handle and must dispose it. The canonical internal helper is called from the
same nullable/implicit-usings assembly.

{{snippet:older}}

The older overload cannot express an ordered provider list or a store property
query. Repeated single-provider calls are not an equivalent shared context.
If the actual store needs multiple providers, retain its supported integration
or upgrade rather than inventing a .NET 10 overload or unsafe native workaround.
No down-level package route is demonstrated.

## Newer approach

.NET 11 adds the IEnumerable<string> overload with optional propertyQuery.
The original fixture uses an ordinary file URI with default and base providers;
replace these only with the names, URI and query documented for your store.

{{snippet:newer}}

For example, call RookerySigner.Open("file:/proof/rookery.pem"), put the result
in a using declaration and construct an RSAOpenSsl owner from it. Sign the
observatory message with the protocol's established SHA256 and PKCS1 signature
padding, then verify using a separately imported public key. RSAOpenSsl takes its
own reference: disposing the input safe handle does not dispose that RSA owner.
Dispose both. Never log private material or retain test keys in production.

## Migration

1. Target net11.0 with a .NET 11 SDK/runtime. Samples pin C#15.0, compared with
   net10.0/C#14.0. These are framework APIs with no extra NuGet dependency,
   workload, rendering mode or experimental opt-in. Actual RC2 pins below are
   verification baselines, not minimum-version promises.
2. Require a supported OpenSSL platform and provider installation. This route
   executes on Linux x64 with OpenSSL3.0.13; Android, browser, iOS, tvOS and
   Windows are marked unsupported by these OpenSSL key-open APIs. Do not use a
   Windows CNG result to infer OpenSSL support.
3. Load a small, trusted, ordered list. Names must be nonempty, contain no NUL
   and be unique; the list must be nonempty. Order is preserved in the context
   cache identity. It is not a universal algorithm preference or failover rule.
4. Supply a provider-assigned URI and a store property query. Here
   provider=default permits the file store, while provider=rookery-missing
   rejects it. The query is passed to OSSL_STORE_open_ex, not a blanket guarantee
   about every later cryptographic operation. Provider loading and algorithm
   selection are distinct. Do not substitute FIPS or HSM claims for real checks.
5. Keep failures explicit. Missing provider, missing key and an unsatisfied query
   fail this fixture with CryptographicException. Do not silently retry using a
   different provider or key. Provision access/authorization separately; providers
   with interactive PIN requirements need their own supported flow and evidence.
6. Dispose keys and algorithm owners. Provider contexts are cached for process
   lifetime; disposing a key is not provider unloading or cache eviction. Avoid
   unbounded caller-supplied provider combinations. Preserve the existing protocol,
   trust policy and private-key storage protections.
7. Run signing/verification, tamper and failure controls with the real store.
   The old default-provider example remains valid. This adds no .NET 10 foundation.

## Constraints

Provider names, URI and query are deployment configuration, not untrusted request
fields. Require actual key permissions and provider capabilities before selecting
this route. Default/base execution establishes neither hardware isolation nor
validation of a different provider's security policy.

## Separate Windows CNG property case

.NET 11 also adds CngProperty's ReadOnlySpan<byte> constructor. This only avoids
an intermediate caller array when setting up property bytes; it is not an OpenSSL
provider route or a new algorithm. The paired RookeryProperty sample uses the
old byte[] constructor versus input.AsSpan(), then mutates the original input and
GetValue result. Both constructors and GetValue preserve independent copies.
This Windows case tests managed property values only, not CNG key persistence or
native provider behavior. No unsafe switch or experimental opt-in is needed.

## Evidence and limits

eng/Test-ProviderRequests.ps1 -Record verifies immutable official sources,
audit-matching reference hashes and compiler identities, and records existing
sample-evidence receipts. Paired SDK10.0.403/runtime10.0.13 and
SDK11.0.100-rc.3.26522.148/runtime11.0.0-rc.2.26504.105 consumers compile on
Windows; the OpenSSL consumers execute in a network-none, pinned Linux container.
The fixture generates a fresh disposable RSA2048 PEM key, verifies signatures
and tamper rejection, missing provider/key, independent safe-handle ownership,
disposed-owner rejection, both ordered default/base lists and query rejection.
Exact image, archive, native library hashes and OpenSSL provider versions are
retained in provider.evidence.json. A context-order test is not a routing priority
benchmark. Windows CNG property-copy checks execute separately.

No FIPS module, HSM, TPM, custom-provider access, other OS, performance or final-GA
proof is claimed. PIN-959 and PIN-962 remain open. Official references are supplied
in metadata, including both pinned reference contracts, managed implementation
and native OpenSSL store integration. Guidance and canonical snippets are offline.
