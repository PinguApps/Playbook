# Invalidate custom NuGet caches between reused builds

## Goal

Gannet Harbour runs several package-related builds in one owned process. A value
cached from the first harbour configuration otherwise survives the second build.
Invalidate it at the real end-build boundary; create its replacement only when
the next build asks for it. Keep live resources with the operation using them.

## Requirements and tested artifacts

This edition qualifies the complete Windows x64 **SDK11.0.100-rc.2.26504.105**
installation, runtime/reference11.0.0-rc.2.26504.105 and C#15 mode, compared with
SDK10.0.402, runtime/reference10.0.13 and C#14. These are exact tested pins,
not a promise for every member of either release family. No workload, provider,
account, native compiler, rendering mode or preview-language switch is needed.
The current SDK is RC2, so actual GA remains unqualified.

SDK10 carries NuGet7.9.0-rc.611 (source a8dd0577); SDK11 carries
NuGet8.0.0-rc.605 (source820230c3). Their NuGet DLL TFMs are net8.0/net10.0,
separate from this example's net10/net11 executable targets. MSBuild18.9.15 versus
18.12.0-1.26504.105 supplies actual `BuildManager` execution. Reference the matching
complete SDK closure with `Private=false`; do not mix independent package DLLs
or infer distributable package8 availability. Package dependencies here are empty.
Exact SDK/compiler/ref/runtime/native-host products and hashes are separately
retained in `samples/net11.0/GannetWorkshop/Shared/artifact-pins.json`; actual loaded SDK
assemblies are checked against it. This is an SDK-bound developer host, not a
general redistributable embedding contract.

## Older approach

SDK10's actual NuGet.Common lacks StaticState. Keep the host's explicit reset after each real EndBuild or give each build its own cache. The older consumer executes that valid route; there is no invented downlevel event API.

## Newer approach

The public current API is `NuGet.Common.StaticState.BuildEnded` and
`RaiseBuildEnded`. The older preview note's `NuGetProcessState.RegisterResetAction`
and `Reset` are unavailable; do not use them. SDK10 has no `StaticState` type.
Its valid alternative below is an explicit reset owned by the host after each
build, or an entirely per-build cache lifetime.

{{snippet:cache}}

`PackageHost` serializes its builds and cache access. It calls real public
`BeginBuild`, `BuildRequest` and `EndBuild`; its explicitly authored lifecycle
wiring then raises the actual NuGet event, including the deliberate task failure.
**This does not prove an unmodified third-party host raises the event.** Register
only when that host's actual seam is known. `RaiseBuildEnded` does not await or
drain arbitrary application operations; those lifetimes belong to their owners.

MSBuild may load the task assembly into a different `AssemblyLoadContext`.
`HostServices.RegisterHostObject` passes the actual host cache's BCL delegate
through `ITaskHost`; the task does real configuration work through that public
host-object seam. A second task-assembly static must not masquerade as the host's
cache. The current proof checks that only one NuGet.Common assembly is loaded.

The `ReadHarbour` task writes a receipt with its real PID, cached harbour and
expected task-specific harbour. The project consumes the host-supplied absolute
HarbourAssembly global property:

{{snippet:build-project}}

Create `HarbourCache(() => File.ReadAllText(absoluteConfig))` and an owned
`PackageHost(cache)`. Before each `Build`, write the next task-specific
configuration file and supply absolute project/receipt paths plus `Expected` and
`FailBuild=false` global properties. Use the matching SDK bootstrap below before
touching any MSBuild/NuGet types. The default program runs the complete original
workshop below. The evidence build additionally sets `GannetProof=true` to
compile/run separate proof fixtures; those conditional files are not required
for the default demo.

Launch the built DLL through the matching absolute dotnet executable so
Environment.ProcessPath identifies its complete SDK installation.

{{snippet:bootstrap}}

The complete demo runs both harbour builds and a benign courier child. Its
launcher dependency is included so the default project is self-contained:

{{snippet:demo}}

{{snippet:launcher}}

Both canonical executable projects inherit `OutputType=Exe`, nullable/implicit
usings and warnings-as-errors from this explicit sample boundary:

{{snippet:boundary}}

Their exact older/current project settings are:

{{snippet:old-settings}}

{{snippet:settings}}

Use `global.json` with `rollForward=disable`, exact SDK version above and
`allowPrerelease=true` for11. Clear package sources in `NuGet.Config`; these
projects restore no packages. The shared compile includes are canonical repository
paths; the test fixture and artifact manifest are source inputs, not hidden
downloaded implementations.

## Constraints

`Invalidate` only installs a new `Lazy<string>`; it never reads the ending build's
environment/configuration. The next build applies its own inputs before the first
`Value` access. Do not turn this into eager recomputation at `BuildEnded`.

This cache deliberately owns no disposable live resource. An operation holds its
own file, connection or semaphore until its work finishes. Never replace and
dispose a shared semaphore while older users retain it: old and new holders can
then run together, defeating synchronization. Concurrent or overlapping builds
need a deliberately scoped/thread-safe design; the serialized sample is no proof
for them.

Dispose subscribers to unsubscribe, even on failure. Reset handlers must guard
their own external-resource cleanup without silently swallowing programming bugs;
the source event invokes delegates directly. This handler does no external
cleanup. A deliberately throwing subscriber in the real-build control confirms
that such a bug surfaces and is removed in `finally`.

## Migration

1. Establish the actual reused process, loaded NuGet assembly and real end-build
   seam. Retain explicit per-build reset/lifetime on an older host.
2. Move configuration-derived immutable values into an owned cache. Subscribe
   once to the actual event, and remove the subscription during host teardown.
3. Invalidate after the real build finishes. Apply the next task's configuration
   before lazy first use; do not recompute with the ending task's values.
4. Move live resources into operation scopes. Drain/dispose only resources you
   own, without killing shared build servers or resetting persistent settings.
5. Run two actual builds in one process, then failing-build, repeat,
   unsubscription and in-flight-resource controls. Manually firing an event alone
   is only a unit check, and two CLI processes are not a reused-host proof.

## Evidence and limits

`eng/Record-PackageWorkshop.ps1` uses the existing sample-evidence workflow.
Both actual SDK consumers reproduce `moor-A` leaking into build2 expecting
`moor-B` with reset disabled, then produce correct `quay-A`/`quay-B` receipts in
the same host/task PID with reset enabled. Creation counts prove invalidation
does not recompute; a failing real task still reaches reset. A real open
operation-owned file remains writable/readable across reset and throws after its
own disposal. Current-only controls run a real build after unsubscribe and a
throwing reset subscriber. Old absence/explicit-reset controls are independent.
Raw paths/commands stay ignored; retained evidence redacts private paths and pins
fresh input bytes. No automatic third-party/server reset, concurrency support,
universal performance, OS matrix or GA claim is made.

The ordinary exact installed Playbook artifact returns full canonical Markdown,
ID/title/goal, target11/edition11, applicability and fresh paired evidence through
real stdio list/get from an unrelated directory. Target10 has no applicable
edition; its valid explicit-reset comparison remains here. Retrieval executes no
build. Proxy independence is not network isolation. The exact eight approved
.NET10 foundations, PIN-959 qualifying coverage/evidence, PIN-963 actual-GA and
unrelated Human gates remain open.

## Official references

- [Exact public event and its timing/resource rules](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/nuget-client/src/NuGet.Core/NuGet.Common/StaticState.cs), SHA-256 `b32e6a9780c60875a1856f2f40d9d76a28e4f25c6d158649485eb2bc6422ced0`.
- [Public build lifecycle](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/msbuild/src/Build/BackEnd/BuildManager/BuildManager.cs).
- [Public host-object registration](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/msbuild/src/Build/Instance/HostServices.cs).
