# Make legacy RSA encryption padding explicit

## Goal

Replace obsolete Boolean RSA padding calls while preserving an existing peer's
ciphertext contract and CAPI key access. The fictional Gannet instrument gateway
must still decrypt older readings and emit readings the older receiver accepts.
This is a compatibility migration, not a new encryption protocol design.

## Older approach

The gateway already supplies an RSACryptoServiceProvider and an agreed useOaep
flag. True means OAEP with SHA-1; false means PKCS#1 v1.5 encryption padding.
Both calls remain available in .NET11, where they produce SYSLIB0064.

{{snippet:older}}

These calls compile without that diagnostic on .NET10. The explicit overloads
also work there; preserving an older target does not require retaining Boolean
overloads. Neither legacy mapping is recommended for a new protocol.

## Newer approach

Keep the same provider, key and peer setting; make the existing padding choice
explicit in both directions. Do not fall back from a failed padding mode to another.

{{snippet:newer}}

Put this helper in a net11.0 console project with Nullable/ImplicitUsings enabled
and C#15 sample mode. Pass the application's already selected CAPI key rather than
opening or replacing a container inside the helper. The canonical example uses no
warning suppression and builds with warnings as errors.

For an isolated test, create an RSA2048 RSACryptoServiceProvider with provider type24,
CspProviderFlags.CreateEphemeralKey and PersistKeyInCsp=false. The behavior fixture
exports that disposable private CAPI blob only into its temporary directory so
separate .NET10 and11 processes can use the same key. It removes the directory,
disposes the provider and never opens a named or production key container.

## Migration

1. Record each peer's padding meaning and existing key/provider access. Keep the
   plaintext byte encoding and key unchanged. Confirm that the Boolean flag is
   the framework fOAEP flag, rather than another application's inverted setting.
2. Replace every Encrypt and Decrypt Boolean overload with the explicit mapping:
   true uses OaepSHA1, false uses Pkcs1. Keep the choice in one boundary such as
   GatewayCipher so encrypt/decrypt agree. Build without suppressing SYSLIB0064.
3. With a disposable key, encrypt in the old runtime and decrypt in the new one,
   then reverse the direction for **both** flag values. Compare recovered plaintext,
   not ciphertext equality: encryption is randomized. Keep wire transport/base64
   encoding and any negotiated padding metadata unchanged.
4. Check wrong-padding and damaged-ciphertext rejection. A CryptographicException
   is a failed operation; do not return partial plaintext or retry using a different
   padding. At a service boundary return a uniform failure, keeping diagnostics
   internal rather than exposing a padding oracle.
5. Test OAEP-SHA256 on this same provider and observe its failure. A stronger
   algorithm/provider change needs a separate peer agreement, compatible key
   import/access and positive interoperability proof. Do not replace the provider
   with RSA.Create and assume an existing CAPI container or its permissions follow.

## Constraints

The application route uses the .NET11 runtime/reference family, .NET11 SDK and
C#15 canonical sample mode with an actual Windows CAPI RSACryptoServiceProvider.
The bounded fixture measures Microsoft Enhanced RSA and AES Cryptographic Provider,
type24, ephemeral RSA2048. Different providers/keys/permissions are not established
by that result. The helper also runs on .NET10. No package, workload, rendering
mode, service, experimental API/opt-in or public resource is involved.

This provider accepts Pkcs1 and OaepSHA1; its explicit overloads reject OaepSHA256.
The diagnostic's general stronger-hash advice therefore needs a different,
separately verified provider/protocol route. That redesign is outside this slice.
The migration preserves legacy security properties; it does not add authenticated
encryption or make PKCS#1 v1.5/SHA-1 a recommendation for new designs. RSA plaintext
limits still apply (for a k-byte modulus, at most k-11 for PKCS#1 v1.5 and k-42 for
OAEP-SHA1); the fixture's short reading fits both. Production container migration,
HSMs, hardware keys, Linux emulation and final .NET11 GA are not verified.

This new guide has only target11/edition11. Its working .NET10 comparison preserves
the exact eight foundations and existing recipe/edition identities.

## Evidence

Record-SignaturePadding.ps1 captures actual SDK10.0.402/C#14/reference-runtime10.0.13
and SDK11.0.100-rc.2.26504.105/C#15/reference-runtime11RC2. Saved records contain
exact commands, input hashes, no resolved dependencies/workloads, compiler and
cryptography reference/implementation hashes, CoreLib and Windows native library
versions/hashes. Requirements above are separate from those reproduction pins.

Three real CAPI processes share only a temporary disposable test key: old-send,
new-exchange, old-receive. Both false/Pkcs1 and true/OaepSHA1 cross-decrypt in both
directions. Each runtime also checks canonical versus explicit overloads, wrong
padding, a zeroed copy of a real ciphertext, and same-provider OAEP-SHA256
encrypt/decrypt failure. Rejection checks do not prove ciphertext authenticity or
exhaustively detect arbitrary corruption. Ciphertext/public-key/input hashes and
provider/OS/runtime are recorded; private keys are not logged or committed.

The unchanged older source is separately built against the current target with
warnings as errors and fails specifically with SYSLIB0064 on Encrypt and Decrypt,
without suppression. The explicit current consumer builds with zero warnings/errors.
Versioned official reference/implementation sources are reconciled with real
assets and behavior, rather than treated as execution proof.

The ordinary packed/local-feed-installed tool is checked through real offline
stdio list/get: discovery ID/title/goal, complete canonical Markdown, target11 /
edition11, applicability and fresh evidence. Windows x64 CoreCLR execution is
measured; other platforms and actual-GA proof remain outside this evidence.
PIN-959/PIN-962 and existing Human gates remain open. No native CI matrix is added.
