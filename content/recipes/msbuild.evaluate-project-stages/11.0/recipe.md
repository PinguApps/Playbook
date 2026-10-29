# Evaluate only the project information a tool needs

## Goal

Use this recipe when a project-inspection tool needs evaluated properties or
items but does not need to discover or execute targets. Bracken Quay Museum's
inventory utility first asks for a collection code, then for object identities
and their gallery metadata. Its project imports the collection from another file
and really resolves `Microsoft.NET.Sdk`.

The stages belong to the **MSBuild host**, independently of the framework of the
project it inspects. Both examples inspect the same net10.0 project. The newer
utility itself targets net11.0; this edition advertises only that host route.

## Requirements and measured artifacts

The qualified newer route uses SDK **11.0.100-rc.2.26504.105**, runtime/reference
pack **11.0.0-rc.2.26504.105**, C# **15.0** and the SDK's MSBuild
**18.12.0-1.26504.105**. The comparison uses SDK **10.0.402**, runtime/reference
pack **10.0.13**, C# **14.0**, and MSBuild **18.9.15**. Assembly version
`15.1.0.0` identifies neither product build. These are exact tested Windows x64
artifacts, not a promise about every MSBuild release or an actual-GA qualification.

MSBuild Locator **1.11.2** is independently distributed. Its actual package has
net8.0 and net46 assets and no transitive package dependencies; these hosts use
the net8.0 asset. That does not establish partial evaluation on an older host.
The locked package closure contains only `Microsoft.Build.Locator/1.11.2`.
`Microsoft.Build` and Framework are compilation references to the actual SDK,
with `Private="false"`; the **complete installed SDK** supplies the execution
closure. No independent Microsoft.Build NuGet package replaces those SDK assets.

The public Locator registration route is intended for external applications
loading projects in an installed SDK's context. Register it **before** any
MSBuild type loads, keep the registration method separate from methods using
those types, and start a fresh process for a different SDK. Its registration
sets process-only MSBuild environment values. Do not share this utility process
with a different build host, copy a lone Microsoft.Build DLL into its output, or
mistake API presence for a working SDK resolver. Obtain the complete matching SDK
and runtime from official .NET distributions; no Visual Studio install is needed
for this .NET-hosted example. An apphost launch is outside this sample: invoke its
DLL using the matching **absolute dotnet executable**, since the bootstrap derives
the SDK directory from that executable's location.

The sample's `artifact-pins.json` records SHA-256/product identities for the audited
MSBuild DLLs, Roslyn/C# compiler, reference assembly, dotnet/hostfxr/CoreCLR,
MSBuild dependency/runtime manifests and loaded SDK helper/resolver dependencies.
Fresh evidence also records every loaded assembly path/hash and actual imported
build file path/hash. Roslyn is 5.9 in the comparison versus 5.12 in RC2; MSBuild
VMR commits are `a8dd0577c5c13f43a2f57fe33b4ff11b90f6371e` and
`820230c386db5b0106c5430b3e5aee92a7826bf5`. Those observations are separate from
the supported acquisition/registration requirements above. Keep the full SDK,
including its `Sdks`, resolvers and dependency closure, available at runtime.

There are no provider, rendering-mode, workload or optional native-tool
requirements. Partial evaluation has no experimental feature switch in these
artifacts. This is a **preview** host qualification; do not add preview language
mode, experimental `-mt`, or a broad warning suppression to use it.

## Project being inspected

Save these two canonical inputs together as `Museum.csproj` and
`Collection.props`. `InventoryStatus` changes only when `CountInventory` executes;
registering or discovering that target does not execute it.

{{snippet:museum}}

{{snippet:collection}}

## Older approach

SDK 10's actual Microsoft.Build does not expose `ProjectEvaluationStage` or
`ProjectOptions.EvaluationStage`. Full evaluation remains a valid working
alternative for this goal. The following complete `InventoryReader.cs` reads the
same imported values and counts the registered targets without running them.

{{snippet:legacy}}

Use the host project settings and bootstrap below with net10.0/C#14 and the
older exact SDK pin. Do not copy the newer enum into an older consumer or present
it as a backport. This comparison adds no .NET 10 foundation or edition.

## Newer approach

`Properties` includes property expansion and imports. `ItemDefinitions` adds
default item metadata; `Items` adds evaluated items; `UsingTasks` adds task
registration. `Full` includes target registration and remains the default when
the option is omitted. None of these names means that targets have executed.

The inventory utility chooses `Properties` for its first request and creates a
**second** instance at `Items` for its later request. It does not incrementally
upgrade or mutate the first instance. If both answers are always needed, choose
`Items` once and read both; two evaluations are not inherently faster than one.

Save this as `InventoryReader.cs`:

{{snippet:partial}}

Create an executable `Microsoft.NET.Sdk` project named `MuseumInventory.csproj`.
Place the following settings inside its `<Project Sdk="Microsoft.NET.Sdk">`
element, then close that element. They also copy the two inspection inputs next
to the compiled host. No repository proof-harness sources are required.

{{snippet:settings}}

Beside it, create `Directory.Packages.props` with this complete central package
boundary. Retain the generated `packages.lock.json` and use locked restore after
the initial restore. Parent repository properties, targets and NuGet configuration
can affect any project: use a clean standalone directory or inspect that inheritance.

{{snippet:packages}}

Save this bootstrap as `Program.cs`. It is shared by both compiled examples:
`NET11_0` selects the newer exact SDK; otherwise the older sample selects SDK10.
The no-inline boundary prevents JIT loading MSBuild types before registration.

{{snippet:bootstrap}}

For the newer standalone directory, save this complete `global.json`:

{{snippet:sdk}}

Run an initial `dotnet restore MuseumInventory.csproj` using the matching absolute
executable from that directory. Then run `dotnet build MuseumInventory.csproj -c
Release -p:RestoreLockedMode=true`, followed by that executable with
`bin/Release/net11.0/MuseumInventory.dll`. For the older comparison change the host
target/language to net10.0/14.0 and pin global.json to 10.0.402 with
`rollForward: disable`; use the older reader and executable. Keep
`DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` in these child environments when using an
isolated CLI home. Nothing requires installing a global tool or editing PATH.

Expect `BQ-ceramics`, then `bowl-17: Harbour` and `tile-42: Harbour`. The newer
first line includes `Properties:`. The older reader additionally prints the full
target count, whose exact value depends on the imported SDK.

## Migration

1. Inventory the utility's actual runtime, SDK references, package lock and SDK
   resolver closure. Separate its host framework from the inspected project's
   framework. Retain full evaluation while older hosts are required.
2. Register the complete selected SDK before loading MSBuild types. Rebuild the
   host against that same SDK; leave its Microsoft.Build references non-copying.
   Reject an unexpected loaded assembly or resolved SDK rather than mixing builds.
3. List the state each request consumes. Select `Properties` for property-only
   inspection or `Items` when item identities and metadata are required. Re-evaluate
   into a new instance if a later request needs a later stage; use `Full` for
   target discovery or subsequent build operations.
4. Replace mutable `Project` factories with `ProjectInstance.FromFile` for partial
   requests. Keep mutable `Project` on full evaluation: `FromFile`,
   `FromProjectRootElement` and `FromXmlReader` reject partial stages with
   `ArgumentException`. An undefined enum value is rejected when assigned to options.
5. Compare property values, imported paths and item identities/metadata with an
   explicit Full instance and an omitted-option control. Assert the returned
   `EvaluationStage`. Do not read unavailable state just to count it.
6. Invoke the matching absolute dotnet executable: `& '<absolute dotnet.exe>' msbuild Museum.csproj
   -getProperty:MuseumName,CollectionCode,InventoryStatus,MSBuildSDKsPath` and
   `& '<absolute dotnet.exe>' msbuild Museum.csproj -getItem:MuseumObject`, without `-target`. Compare the
   relevant JSON fields with the object model. As a separate control, use
   `-target:CountInventory -getProperty:InventoryStatus`; only this reports `counted`.
7. Keep claim-specific checks and refresh evidence after changing sources, SDK,
   package or imports. Measure your own workload before claiming any speed or
   allocation benefit; this recipe makes no universal performance claim.

## Constraints

RC2 **guards** unavailable state. Reading `Items`/`GetItems` at `Properties`, or
reading `Targets.Count` at any partial stage, throws `InvalidOperationException`.
An unavailable target count is **not zero**. The pinned Preview7 notes show a
zero-target-count example, but the actual RC2 source and execution reject that
read. Use a Full control to inspect targets. The sequence is a bounded evaluation
contract, not lazy completion of state when a getter is called.

Property-only inspection still processes imports, property functions and SDK
resolution. It is not a sandbox for untrusted project files, and cannot guarantee
that evaluation has no side effects. Read only trusted inputs. The sample checks
that its own targets did not execute; it does not prove arbitrary project safety.

The .NET11 CLI uses early evaluation for no-target `-getProperty`/`-getItem` queries;
SDK10 returns the same relevant values through its existing full-evaluation route.
Returned values alone cannot reveal the CLI's internal evaluation stage. The
object-model stage/guard checks prove the public API separately; official pinned
source supplies the CLI routing explanation. A supplied target changes the request
to execution. Do not infer target execution from a nonempty target dictionary.

No down-level partial-API route, independent Microsoft.Build package substitution,
Visual Studio/.NET Framework host, NativeAOT/trimming, server reuse, workload,
other-platform or GA compatibility is advertised. No recurring native/OS/client
matrix is added. The exact eight approved .NET10 foundations and all existing
recipe/edition identities are retained.

## Evidence

Both actual host dependency sets compile and execute through
`eng/Record-Evidence.ps1` and their saved `verification-evidence.json` records.
The proof uses the original imported SDK project, compares property sets/imports,
evaluated item values/metadata/counts, checks all partial stage identities and
guard failures, all three mutable factories, undefined stage rejection, explicit
Full versus omitted Full, actual SDK import paths and loaded assembly closure.
Real CLI property/item queries agree on the collection data, and the explicit
target control alone changes `InventoryStatus`. This is execution, not metadata
presence, mocked results or target cross-compilation.

The ordinary packed Playbook tool is installed through an isolated local feed and
cache. The existing Inspector helper exercises real installed stdio discovery and
retrieval from an unrelated directory, checks ID/title/goal, target11/edition11,
complete canonical Markdown, applicability and current evidence. Target10 does
not acquire a new partial-evaluation route. Retrieval with unusable proxies proves
proxy independence, not network isolation; setup can restore dependencies online.
The recipe itself is complete offline and retrieving it executes no consumer.

PIN-959's qualifying coverage/evidence gate, PIN-963/actual-GA reconciliation and
unrelated Human gates remain open. This bounded managed recipe needs no Human,
native device, hosted service or production resource proof.

## Official references

- [Pinned Preview7 feature description and CLI routing](https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/preview7/msbuild.md).
- [Actual RC2 stage semantics](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/msbuild/src/Build/Definition/ProjectEvaluationStage.cs), [options/default validation](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/msbuild/src/Build/Definition/ProjectOptions.cs), [instance guards](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/msbuild/src/Build/Instance/ProjectInstance.cs) and [mutable factory rejection](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/msbuild/src/Build/Definition/Project.cs).
- [Locator package](https://www.nuget.org/packages/Microsoft.Build.Locator/1.11.2), [the package's pinned external-host guidance](https://github.com/microsoft/MSBuildLocator/blob/89a9d0858a83930f5066e4fa0bf0d154e24095d7/README.md) and [RegisterMSBuildPath contract](https://github.com/microsoft/MSBuildLocator/blob/89a9d0858a83930f5066e4fa0bf0d154e24095d7/src/MSBuildLocator/MSBuildLocator.cs).
