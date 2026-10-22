# Negotiate experimental composite key encapsulation

## Goal

A fictional harbour dispatcher needs a shared secret from a negotiated combination of a post-quantum and a classical algorithm. Use the provided composite construction and check that exact algorithm before generating keys. Separately, distinguish HPKE's name, Hybrid Public Key Encryption, from a post-quantum hybrid KEM: the word hybrid alone does not promise quantum resistance.

## Older approach

.NET 10 supports pure ML-KEM. This comparison uses ML-KEM-768 to establish a shared secret; it is not the same composite wire format. There is no built-in CompositeMLKem or Hpke equivalent in this baseline. Retaining pure ML-KEM is a protocol choice; hand-concatenating two secrets does not reproduce the newer standardized/draft composite construction.

{{snippet:older}}

Use MLKem.ImportEncapsulationKey(MLKemAlgorithm.MLKem768, recipient.ExportEncapsulationKey()) to give the sender only the public key. This raw-key route avoids the .NET 10 experimental SPKI import/export members. The previous-release sample compiles without suppressing experimental diagnostics.

## Newer approach

CompositeMLKem is experimental (SYSLIB5006). The example explicitly chooses ML-KEM-768 plus P-256 and fails when IsAlgorithmSupported returns false. The library owns the combiner and encoding; both peers must agree on this exact algorithm and authenticate the public key.

{{snippet:newer}}

Hpke is separately experimental (SYSLIB5009). RC2 implements classical DHKEM suites; its ML-KEM and composite enum names do not establish implemented post-quantum HPKE. This original example deliberately uses supported P-256/HKDF-SHA256/AES-128-GCM with matching public information and associated data on both sides.

{{snippet:hpke}}

## Migration

1. Use a net11.0 console project, C# 15.0, nullable/implicit usings and System.Security.Cryptography. Set the project's NoWarn to append SYSLIB5006 and SYSLIB5009 to its existing value only after accepting both experimental contracts. These are explicit opt-ins, not evidence that the APIs are stable. No NuGet package or workload is required for these framework APIs.
2. Call QuantumDispatch.CreateKey for the receiver. Import its ExportSubjectPublicKeyInfo output using CompositeMLKem.ImportSubjectPublicKeyInfo to create a public-only sender key. Authenticate that key and the negotiated algorithm before encapsulating. Call Send to obtain a shared secret and a capsule; transmit only the capsule. The receiver calls its private key's Decapsulate(capsule). The two returned secrets must agree.
3. Pass the shared secret through the protocol's specified KDF with transcript/purpose binding before treating it as application key material. Zero the caller-owned shared-secret arrays after use and dispose every key. Do not log/export decapsulation keys or invent your own combiner. Encapsulation alone does not encrypt an application message or authenticate its sender.
4. For the separate classical HPKE example, call ClassicalDispatch.CreateKey for the receiver, then Hpke.ImportEncapsulationKey(receiver.Suite, receiver.ExportEncapsulationKey()) for the sender. Send returns ciphertext and capsule; Receive requires both. These public context strings must match; choose application-specific bindings in a real protocol. Base-mode HPKE authenticates the ciphertext, not the sender's identity.
5. Check Hpke.IsSupported for the selected suite as well as CompositeMLKem.IsAlgorithmSupported for a composite KEM. Handle false as unavailable; do not silently drop the post-quantum component to make a request succeed. Keep the two constructions and their wire formats distinct.

## Constraints

Experimental SYSLIB5006/SYSLIB5009 contracts may change even after .NET GA. Composite availability depends on both the platform ML-KEM provider and its classical component. This exact composite and classical HPKE suite were executed on Linux x64/OpenSSL 3.5.5. Windows CNG availability differs; Apple/browser/mobile support is not established by this evidence. Query the support APIs on the deployment host, including its crypto-policy configuration.

The CMS recipe accepts pure ML-KEM, not this composite format. Do not pass CompositeMLKem to CMS merely because a public decryption overload exists: in RC2 it throws PlatformNotSupportedException. No older-target package route, certificate trust, interoperability with another implementation or complete protocol security proof is claimed.

## Evidence

Actual .NET 10 pure ML-KEM and .NET 11 composite samples compile and execute with public-only sender keys, real encapsulation/decapsulation, fixed-time equality and malformed-capsule rejection. The newer sample also executes classical HPKE seal/open and ciphertext-tampering rejection, and asserts the selected MLKEM768_P256 HPKE suite is unsupported. No mock stands in for the post-quantum operation. Exact dependencies, RC2 SDK, Linux image and OpenSSL version are recorded separately from GA readiness.
