# Gate Windows composite signatures by the exact algorithm

## Goal

Use this when a signed bulletin must retain its agreed composite signature contract across a Windows runtime upgrade.

## Older approach

.NET10 uses managed composition over its component providers on Windows. This can support RSA combinations that .NET11's native dispatch rejects. The same exact-set gate is useful on both releases; it is not a promise that support remains unchanged.

{{snippet:older}}

## Newer approach

.NET11 dispatches composite signatures to Windows CNG. Agree the parameter set and trusted public key with the peer before signing; never choose an alternative merely because a probe returns false.

{{snippet:newer}}

## Migration

1. Record the bulletin's exact payload bytes, agreed parameter set and trusted public key identity. Retain that metadata alongside each signature.
2. Probe that exact set on the machine that will perform the operation. Fail clearly when unavailable; capability discovery is not signature verification.
3. Generate an ephemeral test key, sign and verify the original bulletin, then alter a byte and require verification to fail. Repeat with the actual deployed provider before using production keys.
4. Treat rejected RSA combinations as an explicit protocol migration decision. Negotiate a new algorithm/key separately; retain historical signatures and their original verification contract.
5. Keep the previous application and records available for rollback. Do not relabel RSA or classical-only signatures as ECDSA composite output.

## Constraints

Experimental **SYSLIB5006** applies to both releases. The console projects opt in only to this diagnostic through NoWarn; no experimental language syntax, packages, workloads or rendering mode is needed. Current net11.0/C#15 builds with SDK11; net10.0/C#14 is the independent older consumer. Exact SDK pins are verification baselines, not minimum supported Windows versions.

The documented native mappings are MLDsa44WithECDsaP256, MLDsa65WithECDsaP256, MLDsa65WithECDsaP384 and MLDsa87WithECDsaP384. Declared members do not establish provider support. Windows composite certificate APIs are not supported by this route. No import, certificate provisioning, trust-store modification, production key migration or classical fallback is taught. Applications must authenticate public keys independently.

## Evidence

On Windows10.0.26200, actual .NET10.0.13 and .NET11RC2 consumers both sign/verify and reject tampered data using44/P256. Only that ECDSA set was exercised. The older RSA2048/PSS composite succeeds; the current exact-set probe returns false and the canonical boundary throws PlatformNotSupportedException. Other native sets and Windows builds are not claimed.

Canonical projects: samples/net10.0/ObservatoryBulletins and samples/net11.0/ObservatoryBulletins. Record-Evidence.ps1 captures actual SDK/runtime output, input identity and native DLL versions/hashes. RC2 does not establish GA behavior. The original immutable dispatch, mapping and migration-note references accompany the offline metadata.
