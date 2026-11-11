# Create and apply a migration in one command

## Goal

For a Quarry warehouse prototype, retain a reviewable migration in source while applying its schema to a deliberately selected disposable database. This command changes the database immediately. It is appropriate only when that mutation is intended; generating a migration for review remains a separate useful workflow.

## Older approach

EF10 uses two explicit steps. With matching dotnet-ef10.0.13 installed locally, run `dotnet ef migrations add CreateQuarry`, inspect the generated Up/Down, then `dotnet ef database update`. The separation is useful when review must precede database changes.

{{snippet:n10-program-cs}}

## Newer approach

With matching dotnet-ef11 GA, run `dotnet ef database update CreateQuarry --add`. This is a database-update option, not a migrations-add option. It scaffolds the named migration then applies it, retaining the migration, designer and snapshot. Scaffolding success alone does not prove application success: inspect database history and schema, and keep files if application fails so recovery can be deliberate. Do not blindly rerun a named scaffold after failure.

{{snippet:n11-program-cs}}

## Migration

1. Copy the complete project for the intended release. Restore its pinned direct packages and use the matching local dotnet-ef tool. Set PLAYBOOK_SQL_CONNECTION_STRING only in the child environment through the approved SqlServer.ps1 Run route. The check eng/Test-QuarryMigrations.ps1 creates unique marked test databases and removes them in finally. It runs the older two-step workflow and current combined command, then verifies all three generated files, a history row, the table and foreign-key column index through the actual database. The sample intentionally omits the database foreign-key constraint; see ef.omit-database-foreign-key for integrity ownership.

PMC has the corresponding `Update-Database -Migration CreateQuarry -Add` spelling. This contribution executes dotnet-ef, not Visual Studio PMC; PIN-957 retains that independent check. Never imply PMC execution from the CLI result.

## Constraints

This mutates a live schema. Explicit database selection and owner authorization are required in a consumer workflow. No production deployment strategy, script review gate or rollback policy is replaced. Samples require the exact SDK/runtime/provider pins listed in applicability: EF11 targets net11.0, EF10 targets net10.0. Console only, no renderer/workload, Windows x64 SQL2025 proof. Stable API guidance uses GA artifacts; GA checks are limited to the recorded scope.

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

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Canonical compilation and focused real local execution are distinct from source review, mocks and cross-compilation. Exact SDKs, packages, input hashes and outputs are retained in the evidence records. Installed offline list/get checks full body, applicability and target11/edition11; target10 has no applicable edition. No cloud parity or GA execution is claimed.

## References

[Official EF11 migration changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew) and [CLI reference](https://learn.microsoft.com/en-us/ef/core/cli/dotnet).
