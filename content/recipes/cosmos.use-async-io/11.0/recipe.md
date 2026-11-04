# Read and save Cosmos documents asynchronously

## Goal

Replace blocking Cosmos query and save calls when upgrading to EF11, where no opt-in restores synchronous I/O.

## Older approach

EF10 already recommends asynchronous database operations. The historical comparison deliberately suppresses CosmosEventId.SyncNotSupported to demonstrate the old opt-in; this is migration context, not a recommendation to introduce blocking I/O.

{{snippet:n10-program-cs}}

## Newer approach

Await ToListAsync and SaveChangesAsync throughout the call chain. Keep the caller asynchronous, propagate cancellation in application code, and avoid Result, Wait or GetAwaiter().GetResult() wrappers. EF11 removes synchronous I/O completely: suppressing diagnostics or setting an AppContext switch cannot restore it. Add and other in-memory tracking operations remain synchronous where no database I/O occurs.

{{snippet:n11-program-cs}}

## Migration

1. Find synchronous terminal query operators and saves, including repository wrappers. Change signatures to Task/Task<T>, await the provider operations and update callers. Remove the old sync-warning suppression. The sample deliberately tests both removed paths, then persists and queries through async operations. Do not confuse synchronous LINQ over an already materialized collection with database I/O.

## Constraints

Cosmos; no switch restores sync in EF Core 11. Requires EF11/net11.0/SDK11/C#15; historical EF10.0.13/net10.0/SDK10.0.402/C#14 comparison. Exact package assets, provider and prerequisites are in applicability. Windows x64 execution; no renderer or additional workload. Stable contract, RC2 evidence, no experimental opt-in. GA reconciliation remains pending.

## Complete projects

### .NET 10

{{snippet:n10-directory-build-props}}

{{snippet:n10-directory-build-targets}}

{{snippet:n10-directory-packages-props}}

{{snippet:n10-global-json}}

{{snippet:n10-harbourqueries-csproj}}

{{snippet:n10-emulatortransport-cs}}

### .NET 11

{{snippet:n11-directory-build-props}}

{{snippet:n11-directory-build-targets}}

{{snippet:n11-directory-packages-props}}

{{snippet:n11-global-json}}

{{snippet:n11-harbourqueries-csproj}}

{{snippet:n11-emulatortransport-cs}}

## Evidence

The official EF11 breaking-change reference establishes that no opt-in restores synchronous I/O. The executable sample verifies default-path rejection and working asynchronous operations; it does not enumerate configuration switches.

Canonical compilation and focused real local execution are distinct from source review, mocks and cross-compilation. Exact SDKs, packages, input hashes and outputs are retained in the evidence records. Installed offline list/get checks full body, applicability and target11/edition11; target10 has no applicable edition. No cloud parity or GA execution is claimed.

## References

[Official EF Cosmos guidance](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/breaking-changes).
