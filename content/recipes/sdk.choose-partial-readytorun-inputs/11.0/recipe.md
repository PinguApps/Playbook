# Choose partial ReadyToRun assemblies and preserve JIT fallback

## Goal

Skylark's dispatch command separates ordinary calculations from rarely used label
logic. Select one assembly for partial ReadyToRun compilation, keep another as IL,
and compare the result with a full compilation. Select this route for control of
native preparation, without assuming a startup or throughput improvement.

## Requirements and actual artifacts

This edition is a Windows x64 **PE** route targeting net11.0 with C#15. No provider,
rendering mode, optional workload or production resource is involved. Keep the
JIT-capable runtime and IL. This is not NativeAOT or an ELF/Mach-O demonstration.

| Layer | Older comparison | Current example |
| --- | --- | --- |
| SDK / compiler | 10.0.402 / 5.9.0-1.26505.111 | 11.0.100-rc.2.26504.105 / 5.12.0-1.26504.105 |
| Target / C# setting | net10.0 / 14.0 | net11.0 / 15.0 |
| Reference/runtime packs and Crossgen2 | 10.0.13, win-x64 | 11.0.0-rc.2.26504.105, win-x64 |

Use complete official SDK/runtime distributions and the matching
`Microsoft.NETCore.App.Runtime.win-x64` and
`Microsoft.NETCore.App.Crossgen2.win-x64` packages. Their actual restored versions
and native Crossgen2 hash are recorded separately from the SDK pin. A copied
compiler or task assembly does not establish a compatible target/runtime route.
Both samples are framework-dependent and disable apphost generation; run their
DLLs with the matching absolute dotnet executable. RC2 does not qualify actual GA.

## Older approach

SDK10 supports ordinary full ReadyToRun and excluding whole assemblies. It does
not expose the current partial-assembly item. Excluding HotRules and ColdRules
keeps them available to the JIT; it is a useful older alternative with a different
native-code set, not an implementation of partial compilation.

Save `SkylarkReadyToRun.csproj` under `samples/net10.0/SkylarkReadyToRun`:

{{snippet:older}}

Save its sibling projects as `Hot/Skylark.HotRules.csproj` and
`Cold/Skylark.ColdRules.csproj`:

{{snippet:hot-project-10}}

{{snippet:cold-project-10}}

Use `global.json` version **10.0.402**, with `rollForward: "disable"`.

## Newer approach

Save the matching project tree under `samples/net11.0/SkylarkReadyToRun`, with
`global.json` version **11.0.100-rc.2.26504.105**, `rollForward: "disable"`:

{{snippet:newer}}

{{snippet:hot-project-11}}

{{snippet:cold-project-11}}

The linked sources live under `samples/net10.0/SkylarkReadyToRun`: `Program.cs`,
`HotRules.cs` and `ColdRules.cs` respectively. Both release pairs use these original sources.
The late-bound calculation and excluded label code exercise executable fallback.

{{snippet:program}}

{{snippet:hot}}

{{snippet:cold}}

`PublishReadyToRunPartialAssemblies` is an **item list of assembly file names**,
not a Boolean or a list of methods. The SDK passes it into the compiler's partial
mode. Unlisted eligible assemblies still receive full preparation; use
`PublishReadyToRunExclude` to leave an assembly entirely as IL. Partial compilation
does not promise that every method receives native code, nor that the JIT is avoided.
This deliberately unprofiled example produces a partial container with **zero
native method bodies** in HotRules; its calculations execute through JIT fallback.
The full control has native methods. A profile-driven partial compilation is a
separate input policy; no useful hot-method set or speedup is inferred here.

The project property is **PublishReadyToRunContainerFormat**, here `pe`.
**Crossgen2ContainerFormat** is the SDK task parameter receiving it; setting an
MSBuild property with that task-parameter name does not configure this route.
The selected format must match the target/runtime. Other native envelopes require
their own supported compiler, linker, runtime and execution evidence.

## Migration

1. Create the shown project/source tree and exact SDK pins. Preserve explicit
   target/language versions and project-reference paths. Restore with each
   release's absolute dotnet executable from its own directory.
2. Publish the full control with
   `dotnet publish SkylarkReadyToRun.csproj -c Release -p:SkylarkMode=full -o <full-directory>`.
   All three application assemblies should have real ReadyToRun native headers.
3. Run `dotnet clean SkylarkReadyToRun.csproj -c Release` before changing the
   selection, then publish to a fresh selected directory with `-p:SkylarkMode=selected`.
   Current HotRules is partial, ColdRules stays IL and the command remains full.
   In the older selected control both rule assemblies stay IL.
4. Inspect the actual output PE files: the CLR ManagedNativeHeader points to an
   RTR header for prepared assemblies. The current selected HotRules has the
   partial flag (0x4); excluded ColdRules has no such header. Check actual bytes,
   not merely evaluated MSBuild items or the existence of a DLL.
   Its RuntimeFunctions section is empty without profile data, while the full
   control's section is nonempty. The helper verifies this boundary explicitly.
5. Execute each published `SkylarkReadyToRun.dll` with the matching runtime.
   Expect direct=21, late=35 and cold=SK-7. Repeat in a disposable child with
   `DOTNET_ReadyToRun=0` to check full JIT fallback. Preserve IL and runtime files.
6. Exercise a deliberate invalid container value and retain its failure diagnostic.
   Return to `pe` before publishing. Compare measured size or startup only if your
   application needs that decision, with a separately specified measurement.

## Constraints

Changing only the selection can reuse prior native intermediate outputs in this
SDK. Clean before changing profiles; a new publish directory alone is insufficient.

These settings change preparation, not application semantics or dependency
availability. Misspelt file names can leave the intended item unselected; exclusions
are separate from partial selection. Matched runtime/Crossgen2 assets and a
supported target execution host are prerequisites. Forced-JIT success demonstrates
fallback behavior, not which native method executed during the ordinary run.

No IL stripping, composite-image contract, other OS/architecture, native envelope,
performance improvement or SDK11-on-net10 route is claimed. The older comparison
remains valid and adds no ninth foundation. PIN-959/PIN-963/GA gates remain open.

## Evidence

`eng/Test-SkylarkBuild.ps1 -Record` builds both real dependency sets, publishes
full/selected controls, inspects PE machine/native headers and partial flags, and
runs every output normally and with ReadyToRun disabled. It also executes the
current invalid-format failure. Saved input hashes, locks, exact native compiler
and runtime package identities distinguish actual execution from source inspection.
It checks zero partial native-method output without a profile against nonzero
full native-method output; it does not claim native execution of a particular method.

Ordinary build/pack verifies fresh sample evidence. The installed offline MCP
check retrieves target11/edition11 with full canonical snippets and applicability;
target10 is intentionally unavailable. Official deployment guidance and the pinned
SDK target/task sources above support the bounded configuration contract.
