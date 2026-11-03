# Choose a result for absent Cosmos document paths

## Goal

Filter absent paths or supply a fallback when optional JSON navigation can evaluate to undefined.

## Older approach

The EF10 comparison uses the same original reef labels and actual provider. The actual EF10 single-property comparison omits the undefined row and asserts the two present ranks. Microsoft documents that older multi-property projections could throw; this comparison does not execute that older query shape. Do not rely on a query shape to implement a business rule. Both explicit functions are useful in the older provider too.

{{snippet:n10-program-cs}}

## Newer approach

An absent navigation segment produces Cosmos undefined, which is different from an explicitly stored JSON null. The documented EF11 contract throws InvalidOperationException for both single-property and multi-property anonymous/DTO projections when a path evaluates to undefined. Treat that as the upgrade contract; the RC discrepancy and required GA retest are recorded below. Filter with IsDefined when an absent value should exclude the record; CoalesceUndefined supplies a value while retaining the record. JSON null remains null in both examples. If the business rule also needs a fallback for null, add an explicit null-coalescing rule and test it separately.

{{snippet:n11-program-cs}}

## Migration

1. Inventory projections that navigate optional document objects. Decide separately whether an absent path should exclude a record or contribute a fallback; define the rule for explicit JSON null too.

2. Apply IsDefined in the predicate to exclude undefined values, or CoalesceUndefined in the projection to retain those records with a fallback. Keep the behavior independent of the number of projected properties.

3. Copy the complete release-specific projects and run eng/Test-ReefIdentity.ps1 -Record against the disposable local emulator. Compare absent, JSON-null and present fixtures. Refresh GA package/SDK pins and run the documented exception checks after GA; RC observations do not override the .NET11 documentation.

## Constraints

EF Cosmos11 requires net11.0, SDK11 and C#15 in this sample; the EF10 comparison uses net10.0, SDK10 and C#14. Exact pins are below and in applicability. Stable API guidance uses RC3 evidence, not GA verification. Windows x64 and the approved legacy Linux emulator only; no browser, renderer, cloud parity or performance claim.

## Complete projects

### .NET 10 comparison project

{{snippet:n10-directory-build-props}}

{{snippet:n10-directory-build-targets}}

{{snippet:n10-directory-packages-props}}

{{snippet:n10-emulatortransport-cs}}

{{snippet:n10-global-json}}

{{snippet:n10-reefidentity-csproj}}

### .NET 11 comparison project

{{snippet:n11-directory-build-props}}

{{snippet:n11-directory-build-targets}}

{{snippet:n11-directory-packages-props}}

{{snippet:n11-emulatortransport-cs}}

{{snippet:n11-global-json}}

{{snippet:n11-reefidentity-csproj}}

## Evidence

Paired exact-artifact records retain actual SDKs, dependencies, input hashes, compilation and local-emulator execution. They cover historical composite IDs, compatibility/collision checks and explicit undefined filtering/fallback versus JSON null. The EF11 consumer additionally checks default reachability and a legal new composite ID in a separate process.

**GA retest required:** the official .NET11 documentation defines the intended contract. Actual EF11 RC2 and RC3 single-property projections still emit `SELECT VALUE` and silently omit the absent nested path. The sample records both single- and multi-property observations; it does not label them GA proof. Pingu approved delivery with this discrepancy disclosed on 3 November 2026. When .NET11 goes GA, refresh the SDK/package pins, rerun against the real provider and verify that both projection shapes throw. The sample's `--verify-ga` mode enforces both exceptions. After updating GA pins, use the approved disposable route's Run action with `-CommandArguments @('run','-c','Release','--','--verify-ga')`, the absolute GA dotnet.exe as `-Executable`, and the absolute samples/net11.0/ReefIdentity directory as `-WorkingDirectory`. Create/remove the unique marked database as described by the local route; the child receives its connection environment from that route. Keep the safe IsDefined/CoalesceUndefined guidance regardless of query shape.

No mocks, cloud service parity, production data migration, performance result or GA execution is claimed. Installed offline retrieval separately checks canonical full Markdown, applicability and target11/edition11; older comparison samples add no target10 edition.

## References

[EF11 breaking changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/breaking-changes).
