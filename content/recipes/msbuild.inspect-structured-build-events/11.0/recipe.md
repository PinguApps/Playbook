# Consume structured assembly-conflict and server events

## Goal

Use this recipe when a build observer must retain assembly-reference relationships
or explain why an MSBuild server started, was reused or was not used. The fictional
Bracken Depot builds Dispatch and Inventory against different versions of its own
Signal and Gauge assemblies. Its observer reads event fields rather than parsing
localized diagnostic text.

## Choose the actual producer and reader

This edition is qualified with **SDK 11.0.100-rc.2.26504.105**, bundled
**MSBuild 18.12.0-1.26504.105**, **C# 15.0**, and runtime/reference pack
**11.0.0-rc.2.26504.105** on **Windows x64**. It is preview evidence, not an
actual-GA qualification. The older comparison uses **SDK 10.0.402**, MSBuild
**18.9.15**, C# **14.0**, runtime/reference pack **10.0.13**. Both Framework
assemblies identify as `15.1.0.0`; that is not the MSBuild product build.

Use a complete SDK and the matching `Microsoft.Build`, `Microsoft.Build.Framework`,
`Microsoft.Build.Utilities.Core` and `Microsoft.Build.Tasks.Core` closure. The
sample references its selected SDK with `Private="false"`; the CLI supplies these
assemblies to tasks/loggers. Its standalone proof reader binds to that same SDK
before loading event types. Do not copy a newer Framework DLL beside an older
host. The artifact pin manifest separately records the actual compiler, reference,
native host, MSBuild dependency and SDK resolver files: 98 older and 110 current
files. All are hash checked during execution. No NuGet package replacement is used;
the independently distributed public MSBuild 18.11 package is a different build
from this SDK's 18.12 assembly.

There is no external provider, rendering mode, workload, device or optional native
compiler requirement. A build logger can observe projects for other application
TFMs, but its own host/API availability is separate. This edition advertises only
the .NET 11 observer route. It adds no .NET 10 foundation and preserves the eight
approved foundations and all existing recipe/edition identities.

## Older approach

SDK 10 has working CLI builds, binary logging and server reuse. Retain the binlog
and use `BuildWarningEventArgs.Code == "MSB3277"` to classify conflicts. Show its
diagnostic messages to a person; do not reverse-engineer victor/victim relationships
from localized text. That older route actually compiles and executes in the paired
sample, including server PID controls and warning-as-error failure.

The new conflict, search and lifecycle event types and the event-taking deferred
message constructor are absent from the actual older assemblies. There is no
fabricated backport. Existing text-based deferred-message constructors remain valid.

The older project's SDK references and language/target settings are:

{{snippet:legacy-settings}}

## Newer approach

The current project's corresponding settings are:

{{snippet:settings}}

Subscribe to `AnyEventRaised` so both warning and message-derived events are seen.
Keep diagnostic verbosity: dependency/search/lifecycle records are low importance.
The following complete canonical observer is compiled in both projects; `NET11_0`
selects the newer fields. Its `Expected*` flags check the original depot fixture;
applications should choose their own policy rather than hard-code these identities.

{{snippet:reader}}

Preserve all reference fields, including nullable paths/unresolved item specs,
primary source items and each dependee's source items. Preserve search-path,
attempted-file, parent, assembly/architecture, result and AssemblyFoldersEx flags.
The sample maps its two graph directories to `<graph>/v1` and `<graph>/v2`, so two
assemblies with the same filename stay distinguishable. Other rooted paths become
`<external>/<filename>`; that is a bounded publication policy, not a lossless general
path anonymizer. Keep unredacted records private when deeper diagnosis needs them.

Conflict/search/detail constructors are internal. These are observation contracts,
not a public way to manufacture a conflict. `Message` and `FormatMessage(culture)`
are for display; the fields are the classification contract. No event API here
requires an ExperimentalAttribute suppression or preview-language switch. The
18.12 feature wave controls actual structured conflict emission; do not disable it
and then assume the record still arrives.

## Replay the actual binlog

Use `Microsoft.Build.Logging.BinaryLogReplayEventSource` from the **matched SDK**:
construct the observer, call `Initialize(replay)`, `replay.Replay(binlog)`, then
`Shutdown()` in a `finally` block. This uses the same subscription as live logging.
Old length-framed readers may skip unfamiliar records; successful old replay is
not evidence that it preserved newer structured fields. The paired sample replays
each SDK's own real binlogs with that SDK's own reader.

For a custom logger loaded by the CLI, pass
`-logger:Depot.Events.DepotEvents,<absolute-reader-assembly-path>` and
`-bl:<private-binlog-path>;ProjectImports=None` as separate process arguments.
`ProjectImports=None` avoids embedding project imports; it does **not** remove
paths, environment values, command lines or other sensitive data from a binlog.
Never publish the raw file just because imports were disabled.

## Interpret the real lifecycle

Use the supported CLI, not direct calls to `MSBuildClient` or `OutOfProcServerNode`.
Microsoft explicitly documents third-party use of those public server-host types
as unsupported. Renaming their namespace does not establish an embedding contract.

| Child-only selection | Actual observed result in this fixture |
| --- | --- |
| `MSBUILDUSESERVER=1`, `-nr:true`, fresh unique handshake salt | `Spawned`, nonzero owned PID, `ShortLived=false` |
| Same salt and compatible SDK, second build | `Reused`, same PID as the first build and task receipt |
| Server requested, `-nr:false`, without `-mt` | `NotUsed`, PID 0, `ReasonCode=node-reuse-disabled` |
| `MSBUILDUSESERVER=0`, ordinary build | No lifecycle event, because the server was never requested |

Do not confuse absence with a `NotUsed` record. Retain the stable `ReasonCode` for
classification and the localized `Reason`/`Message` only for display. The sample
does not publish localized reason text because it can contain private diagnostics.
Startup events are deferred until build loggers exist; they need not have project
context. Do not discard them merely because their context is invalid/no project.

The diagnostic fixture sets a unique `MSBUILDNODEHANDSHAKESALT` in its own child
environment. The pinned handshake source incorporates that salt into endpoint
identity and explicitly excludes the session ID for servers; an isolated temp
directory or terminal session alone is insufficient. This is a pinned implementation
control, not a supported universal isolation API. It avoids other workers' private
pipes. The fixture retains the process handle identified by its own task receipt,
binds its salt/start-time receipt to rule out PID reuse before assigning cleanup
ownership, then checks executable/start time and lifecycle PID. Later validation
failures still stop that owned handle and confirm exit; a forced real failure
checks this path. No-server task PIDs must equal their launching CLI process.
It never invokes global build-server shutdown, and deletes its disposable signing
key during cleanup.

The source also distinguishes short-lived experimental `-mt` servers through
`ShortLived`. This recipe uses no `-mt`, makes no execution claim for that branch,
and does not promise that every build starts/reuses a server or runs faster.

## Deferred-message constructor migration

Retain existing text constructors when only a comment is needed. SDK 11 adds
`DeferredBuildMessage(BuildEventArgs)` so an existing event can survive deferred
startup logging instead of being flattened into a plain comment. It takes text
from `Message` (empty if null), importance from a `BuildMessageEventArgs`, otherwise
Low. These constructor controls are intentionally synthetic; the real CLI binlogs
above independently prove actual lifecycle emission through deferred startup logging.

The canonical check below lives inside the fixture class, whose `Require` helper
throws `InvalidOperationException` when a condition fails:

{{snippet:constructors}}

No mandatory source migration is needed for the old text overloads. The new overload
does not make an older host understand a newer event. In particular, actual
`-warnaserror:MSB3277` promotes the conflict to an ordinary `BuildErrorEventArgs`;
the typed warning must not be required on that failure path. Both real consumers
exercise that failure and check its stable error code.

## Migration

1. Identify the actual build/logger/replay host and pin its full SDK closure.
   Keep application target, package build and SDK product version separate.
2. Retain the old diagnostic/binlog route on SDK 10. Upgrade the observer and
   producer together before adding the .NET 11 type cases.
3. Subscribe before replay/build, retain warning, dependency and search fields,
   and leave localization to the presentation layer. Unsubscribe during cleanup.
4. Preserve typed startup events when deferring messages. Handle missing project
   context, `NotUsed` reason codes and total absence of a lifecycle record.
5. Reproduce with original strong-named references and isolated owned server
   resources. Verify emission and PID reuse, rather than treating an option value
   or a manually constructed event as execution evidence.
6. Check warning promotion and replay with matched tooling. Redact compact evidence
   while keeping graph identity; keep raw binlogs and private paths disposable.

## Evidence

Run `eng/Record-DepotEvents.ps1` with absolute paths to the two complete SDK
executables. It uses the existing sample-evidence workflow to compile and execute
both actual dependency sets, record input hashes and SDK/language/dependency
identities, then retain redacted behaviour records. It compares process/user/machine
PATH and persistent user/machine environment values without restoring shared state.
All isolated .NET children disable automatic global-tool PATH integration.

The fixture compiles original Signal/Gauge v1/v2 libraries and Dispatch/Inventory
dependees with the actual C# compiler. A primary Signal v1 defeats the v2 dependency
and emits MSB3277 with `WasNotPrimary`; Gauge v2 wins over v1 without that warning
and emits dependency details. A missing DepotMissing reference generates real
failed searches. Every normal binlog checks these expected identities/relationships,
and the separate warning-as-error binlog checks the real failure. No hand-made event
substitutes for these checks. Exact raw commands/binlogs stay under ignored
`artifacts/depot-events`; `<graph>`/`<dotnet-root>` in saved compact output stand
for the generated directory and supplied SDK installation.

## Constraints

This is managed Windows x64 RC2 proof, with no Visual Studio/.NET Framework,
independent package, NativeAOT, other-OS or universal server/reuse/performance claim.
No unavailable older route is advertised. Missing a complete SDK or safe owned
lifecycle blocks reproduction; a synthetic replacement would not qualify it.
PIN-959 qualifying coverage/evidence, PIN-963 actual-GA reconciliation and unrelated
Human gates remain open. Retrieving this offline recipe executes none of the sample.

## Official references

- [Conflict field and formatting contracts](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/msbuild/src/Framework/AssemblyReferenceConflictEventArgs.cs).
- [Search attempt fields and serialization](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/msbuild/src/Framework/AssemblyResolutionSearchTraceEventArgs.cs).
- [Real conflict emission and warning promotion](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/msbuild/src/Tasks/AssemblyDependency/ResolveAssemblyReference.cs).
- [Lifecycle kind, PID, reason and short-lived fields](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/msbuild/src/Framework/MSBuildServerLifecycleEventArgs.cs).
- [CLI deferred startup logging](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/msbuild/src/MSBuild/XMake.cs) and [deferred constructors](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/msbuild/src/Build/BackEnd/BuildManager/BuildManager.cs).
- [CLI support and unsupported third-party hosting boundary](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/msbuild/documentation/MSBuild-Server.md).
- [Handshake salt calculation](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/msbuild/src/Framework/BackEnd/Handshake.cs) and [server endpoint identity](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/msbuild/src/Framework/BackEnd/ServerNodeHandshake.cs).
