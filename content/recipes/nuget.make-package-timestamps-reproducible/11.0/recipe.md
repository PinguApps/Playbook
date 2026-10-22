# Produce repeatable package timestamps

## Goal

Rebuild an unsigned package with the same archive timestamps even when the input files have different modification times. A depot distributes a small manifest; its release process chooses one timestamp for the source revision and reuses it for every reproduction.

## Older approach

The .NET 10 SDK does not provide the newer pack timestamp property. Deterministic compilation remains useful, but it does not control every ZIP entry's timestamp. An existing release step can normalize an **unsigned** archive after packing. This original helper sets every entry to the same representable time:

{{snippet:older}}

{{snippet:normalize}}

Build with `dotnet pack Depot.csproj -c Release -o ./packages`, then call `Normalize.UnsignedArchive(packagePath)` from the packaging utility before signing. This controls entry times only; it does not promise identical metadata names, ordering, compression or bytes. Keeping an established reproducible packaging pipeline is also valid.

## Newer approach

With the .NET 11 SDK, import this property group into the project being packed, or put it in the effective shared build properties:

{{snippet:newer}}

Run `dotnet pack Depot.csproj -c Release -o ./packages`. The fixed RFC3339 value opts in and is reused for archive entries. Replace the illustrative date with your release's chosen, repeatable value. Unix seconds such as `946684800` represent the same instant and were also exercised.

`DeterministicTimestamp=true` uses the current build time for all entries in the measured RC2 implementation. It standardizes entry times **within** that package; it does not freeze time across separate builds. Use a fixed timestamp to reproduce another build. Omitting the property, or setting it to `false`, retains file modification times even with `Deterministic=true`. Do not assume the container `SOURCE_DATE_EPOCH` setting enables NuGet pack timestamps: RC1 removed that automatic fallback.

## Migration

1. Inspect `global.json`, imported properties and the actual pack task. Use the declared .NET 11 SDK route; upgrading the consuming application's TFM alone does not update pack tooling.
2. Choose a timestamp within ZIP's representable range (1980 through 2107), using an even whole second to avoid ZIP's two-second precision truncation. Save the chosen value with the build inputs; never choose a fresh clock value for a reproduction.
3. Add `Deterministic=true` and the fixed `DeterministicTimestamp` to the effective pack project. Keep package identity/version, dependencies, contents and toolchain fixed too.
4. Pack twice to different local output directories after changing a payload file's modification time. Read every archive entry's `LastWriteTime` and compare the results. Compare hashes separately when whole-package reproducibility is required.
5. Remove any redundant timestamp rewrite only after validating the new package. Apply signing after all archive changes; do not mutate a signed package.

## Constraints

This is SDK/MSBuild-based packaging guidance, not a new runtime API or a container timestamp recipe. There is no language feature, provider, UI rendering mode, workload, account or service prerequisite. Configuration applies across project languages; the verification utility is C#.

Repeatable timestamps do not make arbitrary builds reproducible. Generated files, dependency resolution, assembly paths, package metadata/order, compression toolchains, source content, signing and signature timestamps can still change the artifact. The measured fixed-input RC2 packages had equal bytes; that bounded observation is not a general guarantee. The older normalization helper must be used before signing and only on an archive whose contents you own.

The recipe is catalogued for target 11. Its .NET 10 sample is comparison evidence, not an additional .NET 10 foundation or an advertised SDK 11/older-target route. The SDK change is not publisher-designated experimental; actual verification is RC2, so GA reconciliation is still required.

## Evidence

Original C# utilities compiled and ran on Windows x64 with SDKs 10.0.402 and 11.0.100-rc.2.26504.105. They created real local packages using the selected SDK, isolated feed/cache/home and unrelated temporary directories. Older builds verified post-pack normalized entry times. Current builds verified repeated fixed-RFC3339 entry times and bytes, equivalent Unix seconds, `true` build time, and default/false preservation of payload modification times.

The attached current input-hashed records contain commands, runtime output and package SHA-256 values. These are actual compilation and execution, not mock builds or cross-compilation. No package was published; no Linux/macOS, signing-service or GA execution is claimed. Packed-tool checks separately establish offline list/get delivery, not reproducibility of all consumer packages.
