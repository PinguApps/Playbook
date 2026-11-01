# Inspect an encryption instance's key size

## Goal

Report how many key bytes an existing AES-GCM or AES-CCM instance was configured
with. Fictional Mossbank's parcel diagnostics need this metadata without keeping
secret key material just to answer the question. This is inspection, not key
selection, key extraction or a cipher-liveness check.

## Older approach

.NET 10 has no KeySizeInBytes property on these two types. If your code creates
the cipher, retain the validated constructor key length alongside it. These
original owners dispose their cipher and never retain the caller's key buffer.
The read-only Cipher property keeps the metadata paired with that same instance.
Copy the snippet into a nullable/implicit-usings console project; its internal
types are called from the same assembly.

{{snippet:older}}

Construct a GcmParcelKey or CcmParcelKey using a securely generated key and read
its KeySizeInBytes. Use its Cipher for existing operations and dispose the owner.
The assignment happens after successful cipher construction, so rejected key
lengths cannot produce an apparently valid owner. Existing bookkeeping remains
valid after upgrading. If you receive only an arbitrary existing .NET 10 cipher,
there is no equivalent public instance getter demonstrated here: obtain trusted
metadata from its owner instead of guessing from a tag or ciphertext.

## Newer approach

.NET 11 exposes KeySizeInBytes on AesGcm and AesCcm. Read it from the actual
instance; no wrapper or duplicate size field is needed for this purpose.

{{snippet:newer}}

Call ParcelKeyInfo.Describe(existingGcm) or Describe(existingCcm) from the same
assembly. The result is 16, 24 or 32 bytes for supported AES keys, corresponding
to 128, 192 or 256 bits. Multiply by eight only when a display needs bits.
The getter neither exports the secret nor changes encryption or decryption.

## Migration

1. Target net11.0 with a .NET 11 SDK/runtime. The canonical sample declares
   C#15.0; the old net10.0 consumer declares C#14.0. These are framework APIs:
   no NuGet dependency, optional workload, rendering mode, provider configuration,
   unsafe code or experimental opt-in is required. No down-level package getter
   route is claimed. Exact RC2 test pins below are distinct from these requirements.
2. Require AesGcm.IsSupported or AesCcm.IsSupported for the mode you use.
   Unsupported modes need an explicit application decision; do not silently
   replace encryption with plaintext. This evidence requires both modes to run.
3. Replace only duplicate size bookkeeping with the instance getter. Keep
   established key generation/storage, tag policy, associated data, authentication
   failure handling and disposal. Keep a unique nonce for every encryption under
   one key; reporting key length does not make nonce reuse safe.
4. Keep units and concepts separate: this sample uses a 12-byte nonce with a
   16-byte GCM tag or an 8-byte CCM tag for every tested key size. GCM's
   TagSizeInBytes and the static TagByteSizes/NonceByteSizes describe other things.
   Ciphertext length is not key length. Check supported nonce/tag sizes for your
   actual platform and protocol rather than deriving them from this getter.
5. Do not use a successful getter as evidence that the object is still usable.
   On the tested RC2 Windows implementation the value remains readable after
   Dispose, while Encrypt and Decrypt throw ObjectDisposedException. Lifetime
   still belongs to the owner; do not use disposal probing as synchronization.
6. Exercise every key size your protocol supports with real encryption/decryption
   and tampered-tag rejection. Retain the valid old owner on net10.0; this recipe
   adds no .NET 10 foundation or compatible .NET 10 getter edition.

## Constraints and alternatives

A valid key length is not proof of key quality, authorization, correct protocol
parameters or secure storage. Do not log key bytes. Dispose cipher owners and
clear temporary key buffers when appropriate. An ownership wrapper may still be
useful for other policy/lifetime responsibilities; remove it only if truly unused.
No FIPS, HSM, custom-provider, performance or universal OS support is claimed.
Actual positive execution here uses Windows x64 CNG. Other providers/platforms
were not executed. The audited property is not marked experimental; RC2 execution
does not establish final-GA verification.

## Evidence

eng/Test-AttachmentFacts.ps1 -Record -VerifySources uses the existing sample
evidence workflow. ParcelKeys builds and runs with SDK10.0.402/runtime10.0.13
and SDK11.0.100-rc.2.26504.105/runtime11.0.0-rc.2.26504.105, zero warnings/errors
and no package dependencies or workloads. Saved records capture input hashes,
exact commands, compiler/CoreLib identity, loaded BCrypt/bcryptPrimitives versions
and hashes, and audit-matching System.Runtime/System.Security.Cryptography
reference hashes. The bounded attachment-facts manifest pins official paired
contracts and current constructor/property/Windows-lifetime implementations.

Both editions execute 16/24/32-byte keys after clearing the caller's input buffer,
GCM/CCM round trips with associated data, tampered-tag rejection, invalid key
lengths, size metadata before/after disposal and disposed Encrypt/Decrypt failure.
The .NET 11 getter and old explicitly tracked size agree for the tested cases.
Generated test keys are ephemeral; no production key or service is used.

The ordinary installed package is checked through isolated-feed/cache stdio
list/get from an unrelated directory: discovery ID/title/goal, complete expanded
canonical snippets, target11/edition11 applicability and current evidence. This
is offline content retrieval, not consumer-platform verification. PIN-959/PIN-962,
other Human gates and the exact eight .NET 10 foundations remain unchanged.
