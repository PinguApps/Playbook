# Wrap encryption keys in the negotiated AES format

## Goal

A fictional harbour dispatch service protects a randomly generated dispatch key under a separate key-encryption key. Select the agreed wrapping format and reject a changed wrapped key before using the recovered secret. Key wrapping protects key material; it is not a substitute for encrypting arbitrary message content.

## Older approach

.NET 10 added AES Key Wrap with Padding, RFC 5649 (KWP). It supports nonempty key lengths that are not multiples of eight. This implementation is a valid padded format, including for a 17-byte key. It is not the unpadded RFC 3394 format even when the input happens to be aligned.

{{snippet:older}}

## Newer approach

.NET 11 adds the unpadded AES-KW methods implementing RFC 3394. The calling shape is similar, but the wire format and permitted sizes differ. Use these methods only when the receiver expects KW. A 32-byte dispatch key wraps to 40 bytes; GetKeyWrapLength reports the required size. Padded methods remain available for a protocol that specifies KWP.

{{snippet:newer}}

## Migration

1. Create a net11.0 console project with C# 15.0, nullable and implicit usings. Import System.Security.Cryptography and Harbour.Keys. The framework supplies Aes; no package, optional workload or account is required.
2. Obtain a secret AES key-encryption key of 16, 24 or 32 bytes through your key-management boundary. For a local demonstration use RandomNumberGenerator.GetBytes(32) for it and independently for the dispatch key. Never derive either by encoding a human password directly or store the wrapping key beside the wrapped key without separate protection.
3. Confirm the receiver's format and authenticate the surrounding algorithm/version metadata. For RFC 3394 call Protect and persist/transmit the returned ciphertext; call Recover with the same wrapping key. KW requires at least 16 plaintext bytes in multiples of eight. GetKeyWrapLength and the destination-taking overloads support caller-owned buffers. Ciphertext has eight extra bytes.
4. If your existing protocol is KWP, retain EncryptKeyWrapPadded/DecryptKeyWrapPadded instead. KWP accepts shorter/nonaligned nonempty input and uses a different initial value and length encoding. Do not migrate by merely renaming methods while leaving the wire format label unchanged; do not automatically retry another format after an integrity failure.
5. Catch a failed unwrap as a failed operation. Do not use an output buffer after TryDecryptKeyWrap returns false. Dispose Aes and erase all caller-owned wrapping/recovered/plaintext key arrays with CryptographicOperations.ZeroMemory in finally blocks. Wrapped bytes are not the same as an unwrapped secret, but still avoid unnecessary logging.

## Constraints

These framework APIs use the platform AES implementation. Actual verification covers Windows .NET 10 KWP and Linux/OpenSSL .NET 11 KW, not every provider or FIPS configuration. No Bcl package on an older target is claimed. There is no caller IV to choose for KW/KWP; their construction is specified by their respective RFCs.

Wrapping is deterministic under a given wrapping key. It detects invalid key material during unwrap, but supplies neither sender authentication nor associated-data binding and does not protect the surrounding database record. Keep access controls, key rotation and the protocol's metadata protection separate. It is not a general message AEAD.

## Evidence

Both actual projects compile and execute key round trips and wrong-key/tampering rejection. The older comparison additionally wraps a 17-byte input; the newer rejects a 15-byte KW input, checks the 32-to-40 sizing and matches the published RFC 3394 section 4.1 conformance vector. The vector is attributed standards test data; the application code and prose are original. RC2 artifact execution is not GA/provider-matrix verification.
