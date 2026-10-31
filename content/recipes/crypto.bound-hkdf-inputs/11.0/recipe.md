# Enforce an explicit HKDF input contract on Windows

## Goal

Give telemetry peers a predictable failure when their agreed key input cannot be processed by Windows CNG. HKDF is a key derivation function: `DeriveKey` first extracts a pseudorandom key (PRK) from input keying material (IKM), then expands it using protocol context. This recipe fixes SHA-256 and a 32-byte session key. Synthetic sensor inputs are test data, not production secrets.

## Older approach: .NET 10 / C# 14

The managed HKDF route accepts the legacy 2,049-byte inputs in the paired Windows test. Direct calls are valid for inputs supported by that route. Add `using System.Security.Cryptography;` and place this class in its own file in a nullable-enabled console project.

{{snippet:older}}

## Newer approach: .NET 11 / C# 15

Windows uses CNG for `DeriveKey` and `Expand` when available, except algorithms requiring a managed fallback. In the measured SHA-256 route, 2,048 input bytes succeed and 2,049 throw `CryptographicException`. The two limits concern **IKM passed to DeriveKey** and **PRK passed to Expand**. They do not mean an output-key limit of 2,048 bytes or an identical limit on salt/context. SHA-256's HKDF output maximum is separately 255 × 32 bytes; this example always requests 32.

The boundary deliberately applies its input contract on every host, before derivation. `false` means an incompatible request: no usable key is returned. Null arguments remain programmer errors; other cryptographic failures propagate. Callers must treat both `false` and exceptions as failed derivation and must never use a previously retained key as the result of the failed request.

{{snippet:newer}}

Call `SessionKeys.TryDerive(material, salt, context, out byte[]? key)` and use `key` only when the result is true. With Expand, supply a real PRK with the protocol's agreed provenance; length checks cannot establish entropy. The minimum 32-byte PRK rule is separate from the CNG maximum. Never truncate material, rehash it, omit salt/context or silently change algorithms to make a request fit: that changes the derivation contract. Agree a protocol revision with the peer if oversized legacy input must be replaced. Clear session keys when their owner has finished using them.

## Migration

1. Inventory DeriveKey IKM and Expand PRK lengths independently, along with hash, salt, context and requested output length. Do not log real key bytes.
2. Agree the explicit maximum and incompatible-request result with both peers. Keep accepted bytes and context encoding unchanged.
3. Put the validation boundary before HKDF, then update each caller to handle rejection without exposing a key.
4. Compare accepted old/new outputs with an independent implementation or known vector; execute the 2,048/2,049 boundary on the actual Windows provider/runtime.
5. Keep oversized legacy requests as errors until protocol owners agree a replacement. Do not route them through Extract/Expand merely to bypass the contract.

## Constraints and alternatives

Framework APIs only; no NuGet package, native workload, rendering mode or experimental opt-in. Supported prerequisites are a .NET 11 Windows runtime with CNG HKDF available and SHA-256 support; measured versions below are evidence, not a promise for every Windows release or hash. Current implementation source keeps `Extract` managed and falls back when CNG is unavailable or MD5 is selected. Our independent tests record Extract separately, including 2,049-byte IKM; that is not proof that DeriveKey accepts it. No public override switch is established here. A different algorithm/provider/protocol is a separate interoperability and security decision.

## Evidence

`eng/Record-NativeBoundaries.ps1 -VerifySources` compiles real SDK 10.0.402/C# 14/ref-runtime10.0.13 and SDK 11RC2/C# 15/ref-runtime11RC2 consumers on Windows x64 build26200. CNG HKDF opens successfully; the exact current implementation's selection field is true. Native bcrypt version/hash, compiler/reference/runtime hashes and deterministic input construction are saved through ordinary sample evidence.

Raw DeriveKey IKM and Expand PRK at 2,048/2,049 bytes execute on both releases. At 2,048, both match independently calculated HMAC-SHA256 results and each other. At 2,049, older calls succeed, current CNG calls reject and the new protocol boundary returns false/null. All accepted inputs, salt and context remain unchanged. RFC5869 case1's 42-byte output is checked separately. Managed Extract matches independent HMAC at both sizes. These observations do not establish every hash, Windows version, provider or GA artifact.
