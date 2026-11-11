# Share predictable design-time command defaults

## Goal

Give Canal contributors consistent design-time context and project selection while allowing a one-off command to inspect the archive instead.

## Older approach

EF10 has no dotnet-ef JSON defaults feature. Use explicit project, startup-project, context and configuration options, or a repository script containing those options. From a descendant directory, pass the absolute CanalDesign project directory to both project options and CanalContext to context; add --configuration Release. Explicit arguments remain useful on EF11 too.

{{snippet:n10-commands-ps1}}

{{snippet:n10-program-cs}}

## Newer approach

Run the canonical configuration script in the project directory. It writes .config/dotnet-ef.json and selects the matching project, Release build, framework and CanalContext. The project contains two factories so selecting ArchiveContext is observable rather than redundant.

{{snippet:n11-configure-ps1}}

{{snippet:n11-program-cs}}

## Migration

1. Restore/build the complete pinned project and matching local dotnet-ef11 tool. Run Configure.ps1 from the CanalDesign directory.
2. Run dotnet ef dbcontext info from that directory or a descendant. The tool walks upward from the working directory and selects the first .config/dotnet-ef.json. It does not merge all ancestor files.
3. Resolve project and startupProject relative to the parent of the containing .config directory, not the invocation directory and not .config itself. The canonical values are dots because both projects are that parent directory.
4. Run dotnet ef dbcontext info --context ArchiveContext to override the default CanalContext deliberately. Explicit CLI options take precedence. The runner uses a current Release build and --no-build solely to avoid repeating a build between introspection calls.
5. Keep credentials out of this shared file. Include only supported defaults; context-specific connection setup remains in the design-time factory. Inspect nearer configuration files when a command unexpectedly selects another project.

## Constraints

The file is interpreted by dotnet-ef11, not by the runtime or EF10 tools. This edition declares and verifies a .NET11 consumer only; it makes no older-target route claim. SQLite, Design and Tools are pinned separately; Tools does not supply Design transitively in EF11. C#15/14 and exact SDK/package pins appear below. There is no web framework, renderer, native workload or service account prerequisite.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The real EF10 CLI executes Commands.ps1 with explicit options from a nested directory. The runner executes Configure.ps1, verifies its JSON equals the committed defaults, and EF11 discovers that generated file from commands/deep, resolves both relative project paths and selects CanalContext. An explicit context option then selects ArchiveContext. The runner executes actual tool/application artifacts; it neither mocks parsing nor runs Visual Studio PMC. Windows x64 GA results do not prove GA or another provider/platform. Installed retrieval separately checks the complete offline body, applicability, target and text fallback.

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
