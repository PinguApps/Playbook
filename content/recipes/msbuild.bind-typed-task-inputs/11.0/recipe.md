# Bind typed file inputs in a custom MSBuild task

## Goal

Use this recipe when a custom task reads project-relative files and must also
work under experimental multithreaded MSBuild. The fictional Meridian Ridge
observatory has north and south projects, each with `frames/meridian.txt` and a
different `Filter` item metadata value. Each produces its own report containing
the filter and exposure text; projects never share an output file.

## Choose the host before changing the task

This edition targets .NET 11 and uses **SDK 11.0.100-rc.2.26504.105**,
**MSBuild 18.12.0-1.26504.105**, **C# 15.0**, and the matching .NET 11 RC2 runtime
and reference pack. `-mt` is experimental and explicitly opt-in. This is RC2
evidence, not an actual-GA qualification. The older comparison uses SDK
**10.0.402**, MSBuild **18.9.15**, C# **14.0**, runtime/reference pack **10.0.13**.
Both MSBuild assembly identities are `15.1.0.0`; that is not their product version.

The pinned SDK 10 already supplies `AbsolutePath`, `TaskEnvironment`,
`IMultiThreadableTask` and multithread infrastructure. It does **not** supply
`ITaskItem<T>` or `TaskItem<T>`, and its host does not select the environment
constructor. Do not describe `-mt` itself as new in .NET 11. A .NET 10 application
built by SDK 11 is different from loading a net11 task into an older host. This
recipe advertises no down-level typed-task route and adds no .NET 10 foundation.

The tasks reference the selected SDK's actual `Microsoft.Build.Framework.dll`
and `Microsoft.Build.Utilities.Core.dll` with `Private="false"`; the host supplies
them at execution. There are no NuGet packages, external providers, workloads,
rendering modes or optional native dependencies. Do not bundle a newer Framework
DLL next to a task as a substitute for upgrading its host. Independent NuGet
replacements, Visual Studio/.NET Framework MSBuild.exe, native AOT, other OSes and
arbitrary MSBuild versions are outside the measured Windows x64 route.

The sample artifact pins reconcile all three MSBuild DLLs against the source
audit's exact hashes and VMR commits: older `a8dd0577c5c13f43a2f57fe33b4ff11b90f6371e`,
newer `820230c386db5b0106c5430b3e5aee92a7826bf5`. Fresh evidence also checks
SHA-256 and records product versions for Roslyn/csc (5.9 versus 5.12), System.Runtime
reference assembly, dotnet.exe, hostfxr and CoreCLR. Those identities are measured
artifacts, separate from the prerequisites above. The resolved package closure is
empty. Neither API metadata nor these hashes alone prove binding or concurrency.

## Older approach

The older task accepts ordinary `ITaskItem[]`. Its caller supplies an **absolute**
`ProjectDirectory="$(MSBuildProjectDirectory)"` and an explicit `ReportDirectory`.
It resolves every relative input and output against that directory rather than
depending on process CWD. It does not claim thread-safety eligibility: under `-mt`
it can continue running in an isolated sidecar task host.

Include `using Microsoft.Build.Framework;`, `using Microsoft.Build.Utilities;`
and `namespace Observatory;` before this class. Enable implicit usings and nullable
annotations in the task project.

{{snippet:legacy}}

Use the same item/target shape below for the older task, adding
`ProjectDirectory="$(MSBuildProjectDirectory)" ReportDirectory="night-reports"`
to its task invocation. Keep the older task in its own net10.0 assembly with SDK
10 references; it is an alternative implementation of the same goal.

## Newer approach

Include `using Microsoft.Build.Framework;` and `namespace Observatory;` before
this class. `ITaskItem<FileInfo>.Value` carries the bound input while ordinary
item metadata remains available. `TaskItem<AbsolutePath>` creates an output item
with an absolute value and a copied filter; both generic types live in
**Microsoft.Build.Framework**, not Utilities.

{{snippet:typed}}

Create a `Microsoft.NET.Sdk` class-library project, set `OutputType` to `Library`,
and insert these settings inside its `<Project>` element. Keep implicit usings
and nullable enabled. The repository's executable proof harness adds linked test
sources separately; those are unnecessary in your task library.

{{snippet:settings}}

Pin `global.json` to `11.0.100-rc.2.26504.105`, `rollForward: disable` and
`allowPrerelease: true` in this qualification environment. Build in that directory
with the matching dotnet executable. Only the two SDK references are required;
leave their `Private` flags false. Copy the complete class above into the library.

Each project supplies the same relative item identity with its own contents.
Set `ObservatoryTaskAssembly` to the absolute built task DLL path:

{{snippet:invocation}}

The engine selects the public single-`TaskEnvironment` constructor before binding
parameters. An absent `OBSERVATORY_REPORT_DIR` selects `reports` beneath that
invocation's project; a child-process value `night-reports` selects that directory
instead. An explicit `ReportDirectory="selected-reports"` parameter overrides the
constructor default during binding. An ordinary SDK 11 host supplies the
process-backed fallback environment; `-mt` supplies a per-invocation environment.
Never cache a task environment in a static field.

## Preserve older-host construction deliberately

Keeping two constructors is useful only when the task's **whole API/runtime
closure** is compatible with the old host. This small compatibility task is built
against SDK 10/net10.0 and uses no generic items or newer APIs:

{{snippet:compatibility}}

The exact same compatibility DLL is executed by both real hosts. SDK 10 selects
`parameterless`; SDK 11 selects `injected`. Both compute the configured default
from the environment available during construction. This does not make the
net11 typed task compatible with SDK 10. Dropping the parameterless constructor
would forfeit this older-host fallback; do so only after raising your minimum
host and target requirements.

## Migration

1. Inventory the actual build host, target/runtime, SDK selection and task DLL
   references. Retain the explicit-path task where an older host remains required.
2. Upgrade the task host and rebuild the typed library against its matching SDK
   assemblies. Replace input parsing with `ITaskItem<FileInfo>.Value`, retain
   metadata, and create output items from already absolute values.
3. Move environment-dependent defaults into the environment constructor. Keep a
   compatible parameterless constructor where required. Use `TaskEnvironment`
   instead of mutating `Environment.CurrentDirectory` or process environment.
4. Audit all task code, helpers and shared mutable objects before adding
   `[MSBuildMultiThreadableTask]`. The attribute is an author promise used for
   routing; it cannot make unsafe code safe. It is not inherited by subclasses.
   The migration analyzer mentioned in the preview notes is **planned**, not a
   shipped prerequisite or an assurance supplied by this recipe.
5. Run the ordinary host first, then an actual `-mt -m:2 -nr:false` build with a
   parent target using `MSBuild Projects="north/site.proj;south/site.proj"
   Targets="Measure" BuildInParallel="true"`. Set `MSBUILDUSESERVER=0` only in the
   disposable child if you need to avoid sharing a long-lived server. The normal
   `-mt` default can engage MSBuild Server; this proof intentionally exercises
   thread nodes directly and makes no server-reuse/performance claim.
6. Check both absolute input/output paths, distinct report contents and filter
   metadata returned through `<Output>`. Confirm different task threads overlap
   in the **same process**, rather than counting parallel child processes as
   multithread proof. Check complete process environment/CWD before and after
   `Execute()`. Constructor defaults are also exercised, but this snapshot does
   not monitor the construction/binding phase. Under current strict mode, the process CWD may be a sentinel;
   it is deliberately different from `TaskEnvironment.ProjectDirectory`.
7. Exercise absent/configured environment defaults, the explicit output override,
   ordinary-host execution and missing files. Absolute/typed binding does not
   establish existence, permission, containment or protection against symlinks.
   Trusted build inputs and distinct outputs are assumptions of this example;
   add your own authorization/containment and I/O error policy where needed.

## Constraints

The attribute controls routing and requires a full thread-safety audit. Avoid
process CWD/environment mutations, shared unsynchronized caches and output paths
shared across projects. Use the ordinary host or unannotated sidecar route while
that audit is incomplete. Typed paths still require the application's file access
and error policy. This exact SDK/reference/runtime qualification does not imply
compatibility with arbitrary package replacements, older hosts or other platforms.

## Evidence

The saved compile-and-run records use the existing sample-evidence workflow.
The executable harness creates two actual project files in a disposable tree,
drives real MSBuild processes, checks content and metadata round-trips, then
deletes its files. Its separate checked subclass rendezvous proves two in-process
tasks overlap and asserts full process-environment/CWD equality across `Execute()`; the production
class and canonical XML are also executed directly. SDK 10's `-mt` control keeps
the unannotated task in sidecars. Missing input is a real failing build on both
hosts, not a synthetic binder call. The same net10 compatibility DLL hash is
recorded by both constructor checks. Build/run the net10.0 sample first: the
net11.0 proof intentionally loads that existing compatibility DLL. The reproduction
commands in the inspection walkthrough preserve this ordering.

No native/device/Human action is required for this bounded managed task goal.
PIN-959's comprehensive evidence gate, PIN-963/actual-GA reconciliation and all
other recipes' pending Human gates remain open. Re-run affected evidence after
changing source, pins or prerequisites; compilation alone is insufficient.

## Official references

- [Pinned Preview 7 typed binding, construction and experimental multithreading](https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/preview7/msbuild.md).
- [Pinned TaskEnvironment implementation and fallback contract](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/msbuild/src/Framework/TaskEnvironment.cs).
- [Pinned generic item implementation](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/msbuild/src/Framework/TaskItem_T.cs).
- [Pinned multithreaded routing, explicit server opt-out and strict-mode specification](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/msbuild/documentation/specs/multithreading/multithreaded-msbuild.md).
