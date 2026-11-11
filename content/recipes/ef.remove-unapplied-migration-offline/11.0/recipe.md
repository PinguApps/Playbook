# Remove an unpublished migration without database access

## Goal

Remove a local Quarry warehouse migration that you know has never been applied or shared, even when its configured database is unavailable. Offline removal cannot establish that knowledge for you.

## Older approach

EF10 has no offline switch. For the first, known-unpublished migration in this isolated project, delete its generated migration, designer and snapshot together while preserving model edits. For a later migration, restore the coherent predecessor snapshot and remove only the unpublished pair, preferably from version control. Never delete an entire real migration directory as a general removal technique. Connected migrations remove remains useful when database-state checks are wanted.

{{snippet:n10-program-cs}}

## Newer approach

The canonical factory defaults to an unreachable loopback SQL endpoint when no approved connection is supplied. Scaffold `dotnet ef migrations add Unpublished`, then run `dotnet ef migrations remove --offline`. Inspect removal of the generated migration/designer and restoration or deletion of its snapshot as appropriate. Model source edits remain. The tool does not connect to the database or check migration history. This says nothing about whether a shared environment applied the migration.

{{snippet:n11-program-cs}}

## Migration

1. Use matching local EF/Design/tool pins from the complete project. eng/Test-QuarryMigrations.ps1 runs actual offline removal with the unreachable default endpoint, checks resulting files, then scaffolds ForceControl and verifies that `migrations remove --offline --force` fails without removing it. Offline and force are mutually exclusive: force can require undoing an applied database change and cannot supply an offline history check. The runner then removes ForceControl offline before beginning its separate database verification.

When connection selection is needed instead, EF11 supports `dotnet ef migrations remove --connection <connection-string>` for a connected check; this is not the offline route, and the runner verifies its failure against the selected unreachable endpoint with the generated files preserved; successful connected removal is not claimed. Supply credentials through the consumer's secure process configuration, not source or shell history. Applied migrations require a planned database rollback or corrective migration and appropriate authorization. Do not use this recipe to erase shared history.

## Constraints

Offline excludes force; applied state is not checked offline. Only known-unpublished local scaffolds are in scope. EF11 requires net11.0/SDK11 and matching dotnet-ef/Design; historical EF10 comparison remains net10.0. Windows x64 execution, no renderer/workload, no service contact for the offline step. GA evidence does not establish GA or PMC execution.

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
