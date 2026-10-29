# Register a closed set of build extensions in an AOT host

## Goal

Use this recipe when a tool embeds MSBuild and knows its permitted extensions at
compile time. North Quay's permit tool evaluates an original local SDK project,
then runs one known task and checks its output through a known logger. The native
image contains that task, resolver and logger. Installing another DLL beside it
does not extend its allowed set.

**This is a bounded preview route with a retained IL2026 diagnostic.** It is not
an analyzer-clean build or a claim that MSBuild, arbitrary tasks, SDK projects,
property functions or plugins are generally NativeAOT-safe. Keep the verified
managed alternative when a project needs anything outside the compiled set.

## Requirements and actual artifacts

The host framework is independent of the project it evaluates. Here the input is
a small trusted MSBuild project with no .NET framework build or external service.
The measured machine is Windows x64. No provider, UI rendering mode, database,
container, workload or production resource is involved.

| Layer | Older managed comparison | Current managed/native host |
| --- | --- | --- |
| SDK / Roslyn | 10.0.402 / 5.9.0-1.26505.111 | 11.0.100-rc.2.26504.105 / 5.12.0-1.26504.105 |
| Framework, reference pack, managed runtime | net10.0 / 10.0.13 | net11.0 / 11.0.0-rc.2.26504.105 |
| C# compiler setting | 14.0 | 15.0; no preview-language switch |
| Embedded MSBuild | Installed SDK 18.9.15, via Locator 1.11.2 net8.0 | Independently distributed Microsoft.Build, Framework, Utilities.Core and StringTools **18.11.0**, net11.0 assets |
| Native compiler/runtime | No older native route claimed | ILC and NativeAOT runtime pack 11.0.0-rc.2.26504.105, win-x64 |
| Platform tools | Not required | Visual Studio 18 Build Tools, MSVC 14.51.36231; Windows Kit 10.0.26100.0 |

These are exact tested inputs, separately from the supported prerequisites:
NativeAOT on Windows needs Visual Studio 2022 or later with Desktop development
with C++ and its SDK components. This specific route is qualified only with the
table's compiler, linker, libraries and RC2 SDK. Acquire the complete SDKs from
official .NET distributions and the public NuGet packages from nuget.org; a lone
SDK assembly is insufficient. The older alternative needs the complete SDK10
runtime/SDK context. The native executable includes its runtime, but this example
still selects the installed SDK11 toolset context through `MSBUILD_EXE_PATH`.

Do not confuse product builds with assembly version `15.1.0.0`. The audited SDK11
bundles MSBuild **18.12.0-1.26504.105**, VMR `820230c3…`; the chosen public NuGet
package **18.11.0** contains product **18.11.0-1.26512.6**, source commit
`1b17a019a34ed35480e479aaf42fa3a7835cf45f`. They are different artifacts. The
package's public registration contract and real consumer, rather than a bundled
DLL's metadata, establish this route. SDK10's build comes from VMR `a8dd0577…`.
The source audit's compiler/framework/native-host hashes are reconciled in the
sample pins; independently acquired packages and native tools have separate hashes.

The locked current closure also includes `NuGet.Frameworks/7.9.0` (net8.0),
`Microsoft.VisualStudio.SolutionPersistence/1.0.52` (net8.0), and
`System.Configuration.ConfigurationManager`, `System.Diagnostics.EventLog`,
`System.Reflection.MetadataLoadContext` and `System.Security.Cryptography.ProtectedData`
**10.0.10**. Native compiler/linker package versions are pinned by the RC2 SDK.
The two explicit helper packages close framework/solution-reader references;
including them does not qualify solution builds. Keep `packages.lock.json` and
restore with `RestoreLockedMode=true`. All sample dependencies remain manually
maintained; no automatic version advancement is granted.

## Older approach

SDK10 has no `SdkResolver.Register` or `Task.RegisterTask` equivalent. Its complete
managed host still resolves a deliberately laid-out local SDK directory and loads
the declared task assembly through `UsingTask`. This remains a working option;
it is not an older NativeAOT implementation. Locator must register before any
MSBuild type loads, in a separate method/process from other SDK selections.

{{snippet:legacy-settings}}

{{snippet:legacy-packages}}

The older package boundary pins `Microsoft.Build.Locator` to **1.11.2**. Its
actual net8.0 asset has no transitive packages. Set `QUAY_MSBUILD_SDK` to the
absolute complete SDK10 directory before launching the DLL with SDK10's absolute
`dotnet.exe`. The bootstrap changes only its own disposable process:

{{snippet:legacy-bootstrap}}

Both hosts use the same original project, task, output rule and logger below.
The older host obtains `Quay.Permits` from `MSBuildSDKsPath/Quay.Permits/Sdk`;
the current host obtains the same directory through its compiled resolver.

## Newer approach

Pin `global.json` to **11.0.100-rc.2.26504.105**, with `rollForward: "disable"`
and `allowPrerelease: true`. Use the following project under the repository's
sample build boundary (`OutputType=Exe`, nullable and implicit usings enabled,
warnings as errors), and this independent central-package boundary:

{{snippet:settings}}

{{snippet:packages}}

The Framework package supplies the trim-time switches that remove dynamic
resolver/plugin probing, unrestricted property functions, configuration-file
toolsets, reflective task execution/parameter-type discovery and reflective
logger loading. Its current-TFM `_._` can hide its lower-TFM transitive target.
Import the **actual public package target**, as the pinned official SDK source
does; do not copy its implementation or SDK internals. Save this as
`Directory.Build.targets` beside the project:

{{snippet:package-targets}}

Set `QUAY_MSBUILD_SDK` to the complete SDK11 directory. Registration happens
before the first evaluation/build, in a fresh process. Unlike the older route,
this host never uses Locator to dynamically load MSBuild:

{{snippet:bootstrap}}

Save these original fixture files at `Quay.proj`,
`Sdks/Quay.Permits/Sdk/Sdk.props` and `Sdks/Quay.Permits/Sdk/Sdk.targets` in the
host output. `Sdk.targets` contains only `<Project />`.

{{snippet:project}}

{{snippet:props}}

The shared implementation is compiled into each host. The conditional sections
are selected by the project's target framework, not by runtime probing:

{{snippet:engine}}

{{snippet:task}}

{{snippet:logger}}

{{snippet:resolver}}

`SdkResolver.Register` contributes a real instance alongside the default resolver,
ordered by priority. Register before the first SDK resolution: the engine captures
the available set then. It is process-wide, not per project, and has no public
unregister operation. A new process gives each negative control a fresh registry.

`Task.RegisterTask<T>()` keys registration by `typeof(T).Name`. It roots public
constructors/properties through `DynamicallyAccessedMembers`; construction and
property binding still have distinct responsibilities. Registration wins over
the project's same-named `UsingTask` and collapses runtime/architecture variants.
The generic factory overload also roots the known task and receives
`TaskEnvironment`. The string/factory overload alone cannot root an unknown
type's public properties. Prefer the generic API here.

String parameters already have supported binding. A custom item/value parameter
type may additionally need `TaskItem.RegisterTaskParameterItemType<T>()` or
`RegisterTaskParameterValueType<T>()`; class registration is not parameter-type
registration. This example does not claim arbitrary generic `TaskItem<T>` binding
under AOT: the reviewed implementation still has runtime-code-generation limits.

The logger is an already-constructed `ILogger`, supplied through
`RegisterLogger` and `BuildParameters.Loggers`. There is no new hypothetical
logger registry. A logger described by assembly/type name, remote forwarding
logger loading or a project-cache plugin follows different reflective paths.

## Migration

1. Inventory the exact tasks, SDK names and loggers your trusted project needs.
   Keep a managed host for any open-ended extension loading. Do not treat this
   example's compiled set as a security sandbox for untrusted MSBuild input.
2. Preserve the verified SDK10 managed route. Create the separate net11 host and
   pinned package boundary above; do not mix package18.11 and bundled18.12 engine
   assemblies in one process or claim a net10 host can load net11 package assets.
3. Include the fixed project/SDK files, compile the task into the host, register
   its generic type and resolver at startup, and supply real logger instances.
   Preserve the package-supplied trim defaults and the exact `UsingTask` name.
4. Run the managed comparison and current host. Evaluation must report
   `North Quay NQ-017` before any task runs; execution must produce
   `RECEIPT NQ-017:approved`, return that item from the target and log it.
5. Publish with the actual win-x64 ILC and platform linker. Use absolute tool
   paths and explicit child-process `LIB`/`INCLUDE`; do not run `vcvars` into shared
   environment or change PATH. Run the resulting native `.exe`, not its managed
   DLL or an apphost presented as native proof.
6. Inspect all trimming/AOT diagnostics. `BuildManager.Build` still carries
   `RequiresUnreferencedCode` for logger/project-cache initialization and emits
   **IL2026** in both compilation and native analysis. `WarningsNotAsErrors`
   retains that warning visibly; there is no `NoWarn`, pragma, root-all descriptor
   or suppression attribute in this sample. This result is a measured fixed-set
   execution with a known diagnostic, not a resolved analyzer-safety guarantee.
   Any additional diagnostic or changed extension set needs renewed qualification.
7. Run fresh-process missing-task and missing-resolver controls. The native host
   must report **MSB4283** for the omitted task and **MSB4282** for the omitted
   resolver. Do not enable reflective loading to make those failures disappear.
   `--evaluate-only` must succeed without producing a permit receipt.

## Evidence

`eng/Record-NorthQuayBuildHost.ps1` uses the existing sample evidence workflow to
compile/run both actual dependency sets, then publishes and executes the native
binary. It checks native x64 PE identity and absence of a CLR directory, records
its SHA-256 and actual compiler/linker/library hashes, retains diagnostics and
exercises success/evaluation-only/missing-registration controls from an unrelated
directory. Saved compilation and native-execution records include current input
hashes, locked dependencies, exact commands and measured SDK/runtime conditions.
Native execution is distinct from cross-compilation and metadata inspection.

## Constraints

This example qualifies only the supplied local project, string input/output task,
resolver, logger and platform. Evaluation can import files and execute allowed
property functions; it does not run targets. Target execution can have side
effects even with a fixed task set. No arbitrary tasks, workload SDKs, solution
builds, package restore in the hosted project, server reuse, project-cache
plugins, forwarding loggers, other OS/architecture or speed claim is made.

The .NET11 toolchain/registration integration remains preview-qualified, including
the retained RUC boundary and explicit package-target import. No experimental
diagnostic-ID opt-in, `-mt` or preview language feature is required. Requalify the
actual GA assets separately. The exact eight .NET10 foundations, existing recipe
identities, PIN-959 qualifying coverage/evidence, PIN-963/actual-GA reconciliation
and unrelated Human gates remain unchanged and open.

## References

- [Public task registration source and annotations](https://github.com/dotnet/msbuild/blob/1b17a019a34ed35480e479aaf42fa3a7835cf45f/src/Utilities/Task.cs).
- [Public resolver registration and timing contract](https://github.com/dotnet/msbuild/blob/1b17a019a34ed35480e479aaf42fa3a7835cf45f/src/Framework/Sdk/SdkResolver.cs).
- [Direct logger registration](https://github.com/dotnet/msbuild/blob/1b17a019a34ed35480e479aaf42fa3a7835cf45f/src/Build/Definition/ProjectCollection.cs).
- [Remaining public build boundary and plugin work](https://github.com/dotnet/msbuild/blob/1b17a019a34ed35480e479aaf42fa3a7835cf45f/documentation/aot/follow-up-work.md).
- [Actual SDK's public-package target import](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/sdk/src/Cli/dotnet-aot/AotDependencies.props).
- [Microsoft.Build 18.11.0 package](https://www.nuget.org/packages/Microsoft.Build/18.11.0).
- [NativeAOT prerequisites and publication](https://learn.microsoft.com/dotnet/core/deploying/native-aot/).
- [Interpret AOT warnings and test the native result](https://learn.microsoft.com/dotnet/core/deploying/native-aot/fixing-warnings).
- [Locator external-host guidance](https://github.com/microsoft/MSBuildLocator/blob/89a9d0858a83930f5066e4fa0bf0d154e24095d7/README.md).
