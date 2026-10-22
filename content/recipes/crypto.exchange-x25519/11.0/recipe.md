# Exchange keys with the platform X25519 implementation

## Goal

Two fictional harbour dispatchers need the same key-encryption key without sending it over the wire. Use the platform's X25519 implementation when the agreed protocol requires that curve. A shared secret alone does not identify the other dispatcher.

## Older approach

.NET 10 has ECDiffieHellman for supported curves, but no X25519DiffieHellman abstraction. This P-256 comparison demonstrates two-party agreement using authenticated public keys. It is a valid alternative protocol when both peers agree; its public-key encoding and hash derivation are not interchangeable with X25519. Existing X25519 integrations need a reviewed third-party/native implementation; none is invented or verified here.

{{snippet:older}}

## Newer approach

.NET 11 provides X25519DiffieHellman, including the platform-selected GenerateKey, public/private key import/export and raw secret derivation. Check IsSupported before offering the protocol. This example rejects a zero agreement, derives a purpose-specific key with HKDF and clears the temporary raw secret even on failure. The fixed salt and information are public domain separators for this demonstration, not passwords or secret storage.

{{snippet:newer}}

## Migration

1. Use a net11.0 console project with C# 15.0, nullable and implicit usings enabled. Import System.Security.Cryptography and the example's Harbour.Keys namespace. The framework supplies the APIs; no package or workload is needed.
2. Each dispatcher calls DockExchange.CreateKey and sends only ExportPublicKey to its peer. Authenticate that public key through the surrounding protocol before passing it to Derive. Never transmit ExportPrivateKey or an unencrypted PKCS#8 private key as part of agreement.
3. Each side calls DockExchange.Derive with its own private-key object and the trusted remote 32-byte public key. Use the returned 32-byte key only for the named key-encryption purpose. Both sides must use the same KDF, salt and information. A real protocol must also bind the negotiated algorithms, both identities, roles and transcript; this small primitive is not a complete authenticated handshake.
4. Dispose each key object. After consuming a derived byte array, call CryptographicOperations.ZeroMemory in a finally block. For public-key containers use ExportSubjectPublicKeyInfo/ImportSubjectPublicKeyInfo rather than confusing SPKI with the raw 32 bytes. The PKCS#8 methods similarly use a different private-key container.
5. Negotiate X25519 separately from any retained P-256 format. Reject an unsupported provider or malformed/low-order peer input; do not silently switch curves or bypass failed agreement. Do not use the raw shared secret directly as an AES key.

## Constraints

The source supplies Windows CNG, OpenSSL, Apple and Android paths with differing availability. IsSupported is the runtime decision, not the OS name or SDK version. The recorded round trip runs on Linux x64 with OpenSSL 3.5.5; Windows was exercised during authoring, but the saved canonical evidence is Linux. No browser/WASI route, native handle ownership, platform matrix, persistent key store or authenticated exchange is claimed. X25519 is classical cryptography and does not provide post-quantum resistance.

An all-zero check rejects an invalid result; it does not prove a valid peer identity. The library may reject the low-order input before this application's check. Memory clearing limits one managed buffer's lifetime; it cannot prove removal of every copy from the process.

## Evidence

The actual .NET 10 P-256 and .NET 11 X25519 examples compile and execute. Checks compare independently derived keys using FixedTimeEquals, round-trip public SPKI and private PKCS#8 containers, and reject malformed and zero peer input. Pinned SDK 11.0.100-rc.2.26504.105 and OpenSSL 3.5.5 are measured artifacts, not GA evidence or minimum versions. No mock/native shim or certificate-trust proof substitutes for these operations.
