# Choose a result for absent Cosmos document paths

## Goal

Filter absent paths or supply a fallback when optional JSON navigation can evaluate to undefined.

## Older approach

The EF10 comparison uses the same original reef labels and actual provider. The actual EF10 single-property comparison omits the undefined row and asserts the two present ranks. Microsoft documents that older multi-property projections could throw; this comparison does not execute that older query shape. Do not rely on a query shape to implement a business rule. Both explicit functions are useful in the older provider too.

{{snippet:n10-program-cs}}

## Newer approach

An absent navigation segment produces Cosmos undefined, which is different from an explicitly stored JSON null. The documented EF11 contract throws InvalidOperationException for both single-property and multi-property anonymous/DTO projections when a path evaluates to undefined. Treat that as the upgrade contract; the measured GA discrepancy is recorded below. Filter with IsDefined when an absent value should exclude the record; CoalesceUndefined supplies a value while retaining the record. JSON null remains null in both examples. If the business rule also needs a fallback for null, add an explicit null-coalescing rule and test it separately.

{{snippet:n11-program-cs}}

## Migration

1. Inventory projections that navigate optional document objects. Decide separately whether an absent path should exclude a record or contribute a fallback; define the rule for explicit JSON null too.

2. Apply IsDefined in the predicate to exclude undefined values, or CoalesceUndefined in the projection to retain those records with a fallback. Keep the behavior independent of the number of projected properties.

3. Copy the complete release-specific projects and run eng/Test-ReefIdentity.ps1 -Record against the disposable local emulator. Compare absent, JSON-null and present fixtures. Refresh GA package/SDK pins and run the documented exception checks after GA; The measured GA discrepancy does not override the documented contract; use the verified workaround.

## Constraints

EF Cosmos11 requires net11.0, SDK11 and C#15 in this sample; the EF10 comparison uses net10.0, SDK10 and C#14. Exact pins are below and in applicability. Stable API guidance uses the recorded GA SDK and package evidence. Windows x64 and the approved legacy Linux emulator only; no browser, renderer, cloud parity or performance claim.

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

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Paired exact-artifact records retain actual SDKs, dependencies, input hashes, compilation and local-emulator execution. They cover historical composite IDs, compatibility/collision checks and explicit undefined filtering/fallback versus JSON null. The EF11 consumer additionally checks default reachability and a legal new composite ID in a separate process.

**Measured GA discrepancy:** SDK 11.0.100 and EF11.0.0 still emit `SELECT VALUE` for the single-property projection and omit the absent row. The multi-property projection throws the documented undefined-value exception. The refreshed real-provider evidence records both results. Pingu's 3 November 2026 approval concerned the earlier documented discrepancy; it is retained as history, rather than a new approval of GA behavior. The optional `--verify-ga` regression gate requires both exceptions and therefore remains unsatisfied by this release. Keep the verified IsDefined/CoalesceUndefined workaround and recheck that gate after a provider update, using an owned disposable database.

No mocks, cloud service parity, production data migration or performance result is claimed. Installed offline retrieval separately checks canonical full Markdown, applicability and target11/edition11; older comparison samples add no target10 edition.

## References

[EF11 breaking changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/breaking-changes).
