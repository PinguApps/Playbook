# Keep a relationship while omitting its database constraint

## Goal

Model the depot assigned to a Quarry crate while allowing an external import owner to manage referential integrity. Keep EF navigation metadata and the foreign-key lookup index, but deliberately omit the database constraint from migrations. The database will accept missing depot references; choose this only with an explicit integrity policy.

## Older approach

EF10 retains the relationship in the model. Scaffold a migration, then deliberately remove its generated table.ForeignKey call before applying it. The check edits only that operation in a disposable first migration; the primary key, foreign-key column and index remain. Future scaffolds still need review because the model continues to describe the relationship. Keep a coherent snapshot and review Up and Down for existing-schema transitions rather than mechanically deleting every foreign-key operation.

{{snippet:n10-program-cs}}

## Newer approach

EF11 expresses that omission directly using ExcludeForeignKeyFromMigrations on the relationship builder. This changes migration output without removing the navigation, foreign-key property or conventional index.

{{snippet:n11-program-cs}}

## Migration

1. Run the complete version-specific project with matching pinned dotnet-ef. eng/Test-QuarryMigrations.ps1 scaffolds and applies a real migration to each unique SQL Server test database through the approved local route. It verifies no foreign-key operation in the final migration, then inspects sys.foreign_keys, sys.indexes and the migration history. The compiled consumer inspects EF's retained relationship and index metadata and successfully inserts a crate with a missing depot, demonstrating changed database integrity ownership.

For an existing constrained database, scaffold and inspect a separate transition that deliberately drops the intended constraint; the first-migration check here does not prove every upgrade or rollback path. Assess current invalid rows and the ownership of all writers. EF relationship fixup and cascade configuration are not a substitute for database enforcement. Imports, scripts and concurrent writers also need a policy. Do not infer missing-principal join semantics from constraint omission; that is separate guidance.

## Constraints

Relational only; relationship/index retained; data integrity responsibility changes. This is SQL Server2025 execution with EF10.0.13 and EF11 RC2, not evidence for every relational provider. SDK/runtime/language/package assets are explicit in applicability. No rendering or workload requirement. Stable API guidance, RC2 proof, no GA claim or experimental opt-in.

## Tool setup

Copy the version-specific project files below into an independent working directory. Run restore there with the matching SDK; the project pins its direct packages and writes a package lock. The maintained canonical samples additionally retain their complete tested transitive lockfiles. A newly generated lock is not evidence that a different dependency graph was tested.

Install dotnet-ef into a local tools directory using `dotnet tool install dotnet-ef --version 11.0.0 --tool-path ./tools` for the current sample, or version `10.0.13` for the previous sample. Disable automatic global-tool PATH integration in the child environment (`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0`). No global install or PATH edit is needed. The commands below use the familiar `dotnet ef` shorthand: with this tool-path installation, invoke the matching absolute dotnet executable followed by the installed `tools/.store/dotnet-ef/<version>/dotnet-ef/<version>/tools/net10.0/any/dotnet-ef.dll` and then the stated EF arguments. Run in the project directory, or pass its explicit --project path. This is the route exercised by the check; use the actual SDK installation path for your machine.

Supply PLAYBOOK_SQL_CONNECTION_STRING only to the process that may use the chosen database. Without it, this sample deliberately selects an unreachable loopback endpoint. For an actual application substitute your approved design-time configuration; never copy this demonstration endpoint into a deployment. The sample itself performs no migration on startup. Its --inspect mode writes one deliberately invalid test row, so run that mode only against disposable data. Repository checks create and clean up their own marked databases; no supplied script is required to understand the commands and code in this recipe.

## Complete projects

### .NET 10

{{snippet:n10-directory-build-props}}

{{snippet:n10-directory-build-targets}}

{{snippet:n10-directory-packages-props}}

{{snippet:n10-global-json}}

{{snippet:n10-quarrymigrations-csproj}}

### .NET 11

{{snippet:n11-directory-build-props}}

{{snippet:n11-directory-build-targets}}

{{snippet:n11-directory-packages-props}}

{{snippet:n11-global-json}}

{{snippet:n11-quarrymigrations-csproj}}

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Canonical compilation and focused real local execution are distinct from source review, mocks and cross-compilation. Exact SDKs, packages, input hashes and outputs are retained in the evidence records. Installed offline list/get checks full body, applicability and target11/edition11; target10 has no applicable edition. No cloud parity or GA execution is claimed.

## References

[Official EF11 migration changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew) and [CLI reference](https://learn.microsoft.com/en-us/ef/core/cli/dotnet).
