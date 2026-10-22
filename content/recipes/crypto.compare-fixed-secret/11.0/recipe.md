# Compare a secret buffer with a repeated byte

## Goal

A fictional harbour dispatcher rejects an uninitialized 32-byte token. Check whether every byte is zero using the cryptographic comparison primitive, without an application loop that exits at the first differing byte. This is a predicate about bytes, not authentication by itself.

## Older approach

.NET 10 already offers fixed-time comparison of two equal-length spans. A cleared, fixed-size stack buffer supplies the expected zero bytes. This remains a valid implementation and makes the comparison length explicit; there is no need to replace an existing correct use merely because a shorter overload exists.

{{snippet:older}}

## Newer approach

.NET 11 adds FixedTimeEquals(ReadOnlySpan<byte>, byte). The repeated-byte form avoids constructing the comparison buffer. The API's work depends on input length, not the location or values of its contents. Keep the public token-format check separate from the secret predicate.

{{snippet:newer}}

## Migration

1. Use a net11.0 console project, C# 15.0, nullable and implicit usings enabled. Import System.Security.Cryptography and Harbour.Keys. These APIs are in Microsoft.NETCore.App; there is no external package, workload, renderer or service requirement.
2. Pass the received 32-byte token to SecretChecks.IsBlank. True means all 32 bytes are zero; false means at least one is nonzero. A wrong length throws ArgumentException and is a format failure, not a valid token. Do not log the token.
3. Use this overload only when the expected value is one byte repeated throughout the span. To compare two distinct secrets, keep FixedTimeEquals(secret, expectedSecret) with the intended equal length. SequenceEqual, a string conversion or a custom early-return loop does not acquire the same timing contract.
4. Preserve the caller's actual acceptance rule. A nonzero token still needs normal authentication/verification. Treat the boolean as information intentionally revealed by this boundary; branching on its result is necessary and does not make the entire surrounding protocol constant-time.

## Constraints

The operation is synchronous and managed; the provider dimension is the framework implementation rather than OpenSSL or CNG. The fixed-length validation openly reveals the protocol's required length. General variable-length inputs reveal length, and an empty span compares equal to a repeated byte; this example excludes empty tokens through its length check.

No timing measurements, constant-time network request, JIT/disassembly proof, password-validation protocol or guarantee about other code is claimed. Erase caller-owned secrets when their lifetime ends using CryptographicOperations.ZeroMemory; this predicate does not own or clear its input.

## Evidence

Both original examples compile and execute. Checks accept an all-zero token, reject nonzero bytes at the first, middle and last positions, and reject the wrong public length. They establish functional behavior and actual use of the specified primitive. They are not a statistical timing test. The .NET 11 result uses pinned RC2, separate from pending GA reconciliation.
