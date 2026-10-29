# Retarget an embedded template-engine host

## Goal

Move the application or library that **runs the template engine** to a framework
accepted by its complete dependency graph. This does not require changing the
framework written into applications that the template creates.

The original Cwm Archive host below embeds a collection-index template. Both
versions create `CwmArchive.csproj` and a program that prints
`archive=CwmArchive;collection=MAPS-1932`. The historical host library targets
`netstandard2.0`; the current host can run on ordinary **.NET 10**, or .NET 11.
Every generated application still targets **net10.0**.

## Choose the host route

| Consumer | Actual packages and selected assets | Bounded result |
| --- | --- | --- |
| Historical `netstandard2.0` library, executed by a net10 runner | Template packages 10.0.402 select `lib/netstandard2.0` while compiling the library. The runner independently selects their `lib/net10.0` runtime assets. | Library compiles; real engine creates the template; generated application builds and runs. |
| Current ordinary net10 host — preferred when the host can stay on .NET 10 | Template packages 11.0.100-rc.2.26504.105 select `lib/net10.0`; NuGet 8.0.0-rc.605 selects `lib/net10.0`. | Independent SDK 10.0.402 restore/build/execution, including template creation. |
| Current net11 host | The same template package versions select `lib/net11.0`; NuGet dependencies still select `lib/net10.0`. | Independent SDK 11.0.100-rc.2.26504.105 restore/build/execution, including template creation. |
| Current packages in netstandard2.0 | No compatible .NET Standard asset. Default SDK fallback may choose Framework assets and issue NU1701; strict restore without that fallback fails NU1202. | Do not mistake a warning or a suppressed warning for compatibility. |

The target-10 catalogue route is conditional on explicitly adopting the newer
template packages; it does not add a ninth .NET 10 foundation. Keep the .NET 11
edition identity when using it on .NET 10.

### All eight removed routes

All sixteen original ZIPs were downloaded again and their SHA-256 values matched
the source audit. **Each** old package below was independently referenced and built
by a real netstandard2.0 consumer. **Each** corresponding RC2 package failed a
strict netstandard2.0 restore with NU1202.

| Exact package ID | 10.0.402 ZIP frameworks | 11.0.100-rc.2.26504.105 ZIP frameworks |
| --- | --- | --- |
| Microsoft.TemplateEngine.Abstractions | netstandard2.0, net8.0, net10.0, net472 | net10.0, net11.0, net472 |
| Microsoft.TemplateEngine.Core | netstandard2.0, net8.0, net10.0, net472 | net10.0, net11.0, net472 |
| Microsoft.TemplateEngine.Core.Contracts | netstandard2.0, net8.0, net10.0, net472 | net10.0, net11.0, net472 |
| Microsoft.TemplateEngine.Edge | netstandard2.0, net8.0, net10.0, net472 | net10.0, net11.0, net472 |
| Microsoft.TemplateEngine.Orchestrator.RunnableProjects | netstandard2.0, net8.0, net10.0, net472 | net10.0, net11.0, net472 |
| Microsoft.TemplateEngine.Utils | netstandard2.0, net8.0, net10.0, net472 | net10.0, net11.0, net472 |
| Microsoft.TemplateEngine.IDE | netstandard2.0, net8.0, net10.0, net472 | net10.0, net11.0, net472 |
| Microsoft.TemplateEngine.TemplateLocalizer.Core | netstandard2.0, net8.0, net10.0, net472 | net10.0, net11.0, net472 |

These directories describe artifacts, not eight executed IDE/localization
workflows or Framework compatibility. The executable example needs Edge and
RunnableProjects plus their transitive closure; it does not need IDE or the
localizer. The note abbreviates the localizer's displayed name; the last row is
its actual package ID.

The official note says the remaining modern floor is **net9.0**. These exact RC2
packages contain **net10.0**, not net9.0. The selected net10/net11 consumer assets
resolve that discrepancy for this recipe. There is no advertised net9 route.
Framework `net472` directories remain, but no Framework consumer was executed;
this recipe therefore offers no Framework route.

## Original example files

The following layout keeps one canonical host implementation and three embedded
payloads. Preserve the relative paths in the project snippets. The files below
are complete offline; external links are references, not missing setup steps.

    samples/
      ArchiveTemplate/
        ArchiveTemplate.cs
        Program.cs
        template.json
        project.txt
        program.txt
        ArchiveTemplateLegacy.csproj  # historical library, pins and NuGet.Config here
      net10.0/
        ArchiveTemplateRunner/   # historical runner project
        ArchiveTemplateHost/     # current net10 project, package pins, NuGet.Config
      net11.0/
        ArchiveTemplateHost/     # current net11 project, same current pins/config

`ArchiveTemplate.cs` extracts this trusted embedded template into its own
workspace, registers the folder in virtualized engine settings, discovers its
exact identity, and performs real instantiation. It neither modifies the user's
`dotnet new` template registrations nor runs post-actions.

{{snippet:host}}

Use this driver as shared `samples/ArchiveTemplate/Program.cs`. With no argument
it deletes its own temporary workspace. A single explicit workspace argument
retains the generated files for inspection; choose a new empty directory.

{{snippet:driver}}

Save the following complete configuration as `samples/ArchiveTemplate/template.json`:

{{snippet:template-config}}

Save the next two payloads as `project.txt` and `program.txt` in that same directory.
The host embeds them under explicit resource names and writes them as a project
and C# source before discovery. `sourceName` changes the project filename and
archive label; the parameter replaces the collection token.

{{snippet:generated-project}}

{{snippet:generated-program}}

## Older approach

Use this `ArchiveTemplateLegacy.csproj` and adjacent `Directory.Packages.props`:

{{snippet:older-project}}

{{snippet:older-packages}}

The old set restores from nuget.org. Place this `NuGet.Config` beside the library
and runner projects:

{{snippet:older-sources}}

`ArchiveTemplateRunner.csproj` executes the actual netstandard library on .NET 10:

{{snippet:older-runner}}

The library compiles against the old netstandard engine and NuGet **6.13.2**
closure. The net10 runner resolves its own compatible package assets; it does
not convert the library's TFM to net10. Old netstandard asset availability is
preserved honestly, without promising continuing support for that pinned package
set or every runtime that implements .NET Standard.

## Newer approach

For the preferred ordinary .NET 10 host use `ArchiveTemplateHost.csproj`:

{{snippet:current10-project}}

For a host already moving to .NET 11 use this independent project instead:

{{snippet:current11-project}}

Both current projects use this adjacent `Directory.Packages.props`:

{{snippet:current-packages}}

Both also need this `NuGet.Config`. The public dotnet-tools feed supplies the
actual NuGet **8.0.0-rc.605** dependencies. A fresh nuget.org-only restore of this
RC2 host failed NU1102 for those unavailable dependencies. Package source mapping
deliberately confines `NuGet.*` to that feed for the current set; use the separate
historical configuration for NuGet 6.13.2. A ZIP on nuget.org alone does not prove
that its closure is available there.

{{snippet:current-sources}}

The resolved lock files/evidence retain all dependencies and hashes. The current
net10 closure includes six TemplateEngine assemblies, seven NuGet 8 assemblies,
Newtonsoft.Json 13.0.4, and the selected Microsoft.Extensions/System packages.
The net11 closure is resolved independently. Do not replace a dependency with an
SDK-root DLL, downgrade NuGet to make restore pass, or infer compatibility from
package major versions. No native toolchain, workload, external provider, account,
browser or rendering mode is required for this local managed template scenario.

## Migration

1. Identify the engine **host** project and its callers. Record the current TFM,
   package versions, selected compile/runtime assets and SDK pin separately from
   the TFMs in the generated templates. Keep non-engine reusable contracts in a
   separate netstandard library if callers still need them; that library cannot
   directly retain these new engine dependencies.
2. Choose ordinary net10 when it meets the host's requirements. Update the host
   and any in-process callers that cannot consume its new TFM. A netstandard
   caller cannot reference a net10 library. If retargeting callers is impossible,
   evaluate an explicit process boundary to a supported host; no process protocol
   or Framework fallback is implemented or verified here.
3. Replace the old package set with the exact current pins and current source
   configuration above. Keep packages isolated from unrelated central package
   policies. For reproduction, pin `global.json` to SDK `10.0.402` with
   `rollForward: disable` for both net10 projects and the historical library/runner;
   pin the net11 host to `11.0.100-rc.2.26504.105` with the same policy. The JSON is
   `{"sdk":{"version":"10.0.402","rollForward":"disable"}}` for the former;
   substitute only the version for the latter. These are measured toolchains,
   not a general claim that all future package patches are compatible.
4. Restore each project, inspect `obj/project.assets.json`, and commit its
   `packages.lock.json`. Subsequent restores should use locked mode. Build and
   run the driver with the matching runtime; do not use compilation under SDK 11
   as proof that a net10 host runs on runtime 10.
5. Check the generated filename, archive name and collection replacement. Build
   and run `generated/CwmArchive.csproj` using SDK/runtime 10 even when a net11
   process created it. Its output must be
   `archive=CwmArchive;collection=MAPS-1932`. Moving the engine host does not require
   moving generated applications to .NET 11.
6. Reject the removed netstandard route explicitly. The canonical direct-consumer
   probe below replaces `PACKAGE_ID` with each of the eight exact IDs and
   `PACKAGE_VERSION` with the old or current version. Build the old variants;
   restore the current variants and expect NU1202. The explicit property disables
   the SDK's implicit .NET Framework fallback. A default Abstractions control
   instead restores net472 with NU1701; neither suppressing it nor treating a
   fallback exit code as success supplies netstandard support.

{{snippet:probe}}

## Evidence

Requires PowerShell 7, the two SDK/runtime installations and public feeds during
setup. It uses distinct old/current caches and CLI homes, independent consumer
directories, absolute child executable paths, and disables first-run global-tools
PATH integration. It compares process/user/machine PATH before and after without
restoring shared settings. No public package publication or production resources
are involved.

Run `eng/Test-TemplateEngineHost.ps1` from this checkout. Its optional `-Net10Dotnet` and `-Net11Dotnet` parameters accept the absolute SDK executable paths; the defaults are the verified installations described above.

Expect sixteen matching artifact hashes, eight old netstandard builds, eight
strict current NU1202 failures, the NU1701 control, the historical library/runner,
and independent current net10/net11 template creation. All three generated net10
applications must compile and execute with SDK/runtime 10. The saved compilation
records pin the SDK/language and complete resolved dependencies; output records
the actual host runtime separately. Temporary commands and caches are disposable.

## Constraints

The baseline uses C# 14 on SDK 10.0.402 and C# 15 on SDK 11 RC2. The host logic
does not need a C# 15-only feature. Current dependencies are **prerelease RC2**;
there is no experimental-API opt-in or stability guarantee for future NuGet-client
APIs. Exact pins and explicit prerelease source selection are deliberate choices.
Do not present RC2 evidence as final GA verification.

Execution was on Windows x64 with the actual managed dependency closure. Other
operating systems, Framework, remote template installation, IDE integration,
localization behavior, trimming and Native AOT were not exercised. The built-in
folder provider uses only this trusted embedded payload; general untrusted
template execution is outside this example. No post-action executor is supplied.
The source note's unchanged-public-API statement is not a full binary API audit.

PIN-959's qualifying coverage/evidence gate and PIN-963's actual-GA reconciliation
remain open. This delivery adds one recipe, preserves all existing recipe/edition
identities and the exact eight approved .NET 10 foundations, and adds no recurring
native/client/OS matrix.

## Official references

- [Template-engine .NET Standard removal and recommended migration](https://learn.microsoft.com/en-us/dotnet/core/compatibility/sdk/11/template-engine-netstandard).
- [Pinned Edge project and NuGet dependencies](https://github.com/dotnet/sdk/blob/b370d9f8508baee1ab99f3f8aba8bf5e1dfacf41/src/TemplateEngine/Microsoft.TemplateEngine.Edge/Microsoft.TemplateEngine.Edge.csproj).
- [Official template engine package host API source](https://github.com/dotnet/sdk/blob/b370d9f8508baee1ab99f3f8aba8bf5e1dfacf41/src/TemplateEngine/Microsoft.TemplateEngine.Edge/Settings/TemplatePackageManager.cs).
- [Custom template configuration](https://learn.microsoft.com/en-us/dotnet/core/tools/custom-templates).
- [NuGet NU1202 incompatible target diagnostic](https://learn.microsoft.com/en-us/nuget/reference/errors-and-warnings/nu1202) and [NU1701 fallback warning](https://learn.microsoft.com/en-us/nuget/reference/errors-and-warnings/nu1701).
