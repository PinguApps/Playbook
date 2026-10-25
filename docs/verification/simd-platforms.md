# Simd platforms evidence

Historical October 2026 authoring observations. Follow the [evidence reuse policy](../contributing.md#choose-the-check-and-retain-its-result).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-739-740.md).

The Windows x64 authoring run executed AVX2/scalar agreement checks. Arm SVE/SVE2
and AVX-VNNI-512 execution were unavailable. Required-mode checks exited with
INCOMPLETE; the recipe retains pending Human proof, which still blocks release.
This accepted delivery limitation does not establish hardware-path execution.

## Future native verification

There is no currently available route; this is an accepted PR limitation. Do not
provision resources or change hardware requirements implicitly. If suitable
hardware becomes available, use the same source/input hashes,
declared SDK/runtime and sample on a suitable isolated measured host. Run the
canonical sample in its required mode and retain feature flags and scalar
agreement output. SVE2/SHA3/SM4 reporting establishes detection only; no
cryptographic or general SVE2 algorithm is claimed by these examples.

The current .NET 11 DLL accepts --require-sve and --require-vnni512. Both exit
nonzero with INCOMPLETE on the available host. A plain PASS establishes only
the paths named in its output. Required native results must be captured and
reviewed before changing humanProof to verified, refreshing affected inputs,
rebuilding/retesting and claiming native verification. The combined PR is
authorized with this gap. RC2 evidence is not GA
verification. No mocks or cross-compilation replace this missing execution.
