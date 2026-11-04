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

1. Install matching Tools and explicit Design references, restore, save all files, select the framework/configuration, and build target plus startup projects successfully in Visual Studio.
2. Check that PMC resolves those exact freshly built artifacts. Use Add-Migration with NoBuild only while model, references and generated inputs remain unchanged.
3. Inspect the generated migration and snapshot. Build again before Update-Database: the assembly used to scaffold cannot already contain the newly generated migration.
4. Only against the disposable sample database, run Update-Database with NoBuild on that current build. On any model, source, package, configuration, framework, startup-project or generated-file change, rebuild or omit NoBuild.
5. For H_PMC_PROOF, compare the generated migration with the expected model, then edit a mapped property without rebuilding and observe the stale-artifact risk in the real PMC. Rebuild and confirm the new model is used. Retain actual Visual Studio/NuGet host versions and results through PIN-957. These manual actions remain pending; CLI introspection is not their substitute.

## Constraints

This is Visual Studio on Windows with actual NuGet Package Manager Console and matching EF11 Tools. NoBuild is claimed only for Add-Migration and Update-Database here. The original consumer requires .NET11/C#15 and SQLite; the EF10 comparison uses .NET10/C#14. No UI renderer, web host, external account or service is required. Exact SDKs and transitive package assets are recorded by the locked consumers. RC2 compilation is not GA proof.

## Evidence

Both original consumers compile and run; the matching Tools package source exposes NoBuild and forwards it to skipBuild for both commands. Actual interactive PMC execution, current/stale artifact outcomes and product integration remain pending H_PMC_PROOF / PIN-957. Pingu explicitly approved publishing this PR with PMC proof pending on 4 November 2026. The pending flag continues to block release-ready packaging; this exception does not claim that Human proof passed. Installed list/get verifies the full offline recipe and retains that limitation.

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
