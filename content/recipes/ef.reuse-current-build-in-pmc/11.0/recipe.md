# Run a design-time operation using an already-current build

## Goal

Reuse a current CanalDesign build for a Visual Studio migration command without accidentally scaffolding or applying yesterday's model.

## Older approach

Use the normal EF10 PMC commands, which build as part of the operation. EF10's CLI also has --no-build, but a CLI route is not evidence for Visual Studio's Package Manager Console.

{{snippet:n10-pmc-ps1}}

{{snippet:n10-program-cs}}

## Newer approach

EF11 Add-Migration and Update-Database accept NoBuild. Load this complete CanalDesign project in Visual Studio and select the intended Release configuration. The script records the two separate operations and the required intervening rebuild; it is a manual PMC recipe, not an ordinary PowerShell script to run in a terminal.

{{snippet:n11-pmc-ps1}}

{{snippet:n11-program-cs}}

## Migration

1. Install matching Tools and explicit Design references, restore, save all files, select the framework/configuration, and build target plus startup projects successfully. In PMC, change to the project directory so dotnet resolves its SDK pin. A solution-root SDK10 pin can otherwise cause NETSDK1045 for this net11.0 project even with NoBuild: Tools still runs ResolvePackageAssets.
2. Check that PMC resolves those exact freshly built artifacts. Use Add-Migration with NoBuild only while model, references and generated inputs remain unchanged.
3. Inspect the generated migration and snapshot. Build again before Update-Database: the assembly used to scaffold cannot already contain the newly generated migration.
4. Only against the disposable sample database, run Update-Database with NoBuild on that current build. On any model, source, package, configuration, framework, startup-project or generated-file change, rebuild or omit NoBuild.
5. The PIN-957 check generated the expected Moorings table, then added BasinCode to source without rebuilding. NoBuild produced an empty migration from the old assembly. After rebuilding, another NoBuild migration added the column. Build again before applying that migration. Repeat this check when the declared inputs change.

## Constraints

This is Visual Studio on Windows with actual NuGet Package Manager Console and matching EF11 Tools. NoBuild is claimed only for Add-Migration and Update-Database here. The original consumer requires .NET11/C#15 and SQLite; the EF10 comparison uses .NET10/C#14. No UI renderer, web host, external account or service is required. Exact SDKs and transitive package assets are recorded by the locked consumers. Current GA compilation and owner PMC proof are recorded separately.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Both original consumers compile and run. On 11 November 2026 Pingu repeated the actual Visual Studio PMC check with SDK 11.0.100 and EF Tools 11.0.0 GA. Add-Migration and Update-Database with NoBuild succeeded with the intervening Release builds. The unbuilt BasinCode change produced an empty migration; after rebuilding, the next migration added the column. A repeated migration name was correctly rejected. Agent inspection of generated sources and a read-only SQLite query confirmed Id, Label, BasinCode and all three applied migrations with ProductVersion 11.0.0. The PMC record distinguishes owner execution from agent artifact/database inspection; it does not establish every Visual Studio version.

## Complete canonical projects

{{snippet:n10-canaldesign-csproj}}

{{snippet:n10-directory-build-props}}

{{snippet:n10-directory-build-targets}}

{{snippet:n10-directory-packages-props}}

{{snippet:n10-global-json}}

{{snippet:n11-canaldesign-csproj}}

{{snippet:n11-directory-build-props}}

{{snippet:n11-directory-build-targets}}

{{snippet:n11-directory-packages-props}}

{{snippet:n11-global-json}}


## References

Official EF11 change notes and CLI/breaking-change contracts are listed in structured sources. [EF11 changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/whatsnew), [CLI defaults](https://learn.microsoft.com/en-us/ef/core/cli/dotnet), [breaking changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/breaking-changes).
