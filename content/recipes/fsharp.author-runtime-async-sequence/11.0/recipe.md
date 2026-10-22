# Produce pull-driven asynchronous results (experimental)

## Goal

Build a deliberately small F# asynchronous producer whose caller requests one result at a time and owns cancellation and awaited cleanup.

## Older approach

Ordinary .NET 10 tasks can implement IAsyncEnumerable directly. This original two-reading survey keeps a position and owns a fresh lease per enumeration. Its calls must be serial. Add `open System`, `open System.Collections.Generic`, `open System.Threading` and `open System.Threading.Tasks`.

{{snippet:older}}

This valid older approach has no third-party sequence dependency. It remains appropriate when experimental compiler/runtime coupling is unnecessary. It is distinct from C# async-LINQ transformations over an existing stream.

## Newer approach

Keep the imports above and add `open System.Runtime.CompilerServices` and `open Microsoft.FSharp.Core.CompilerServices.StateMachineHelpers`. The following independently authored inline entry point uses the two actual compiler intrinsics. Ordinary `seq` syntax describes the statically known recipe; the compiler lowers it into asynchronous move/dispose methods. No shipped `asyncSeq2`, `runtimeAsyncSeq` or general-purpose builder is assumed.

{{snippet:newer}}

Acquisition of each lease happens on the first move, inside that enumeration. `relay` explicitly forwards the caller's token into the nested asynchronous source and awaits its cleanup before the outer cleanup. The ordinary `for` syntax over a synchronous nested sequence does not magically make that sequence asynchronous.

Raw generated iterators do not support overlapping moves or move/dispose. Use one owner and serialize every call. This bounded adapter rejects overlap before touching that unsupported state; it adds ordinary task wrappers and makes no throughput claim:

{{snippet:serial}}

For a minimal consumer, create a resource factory and use the guarded source:

{{snippet:consumer}}

Call `inspectFirst ()` from the entry point. The first move waits until the gate
is released; the consumer reads one value and awaits early disposal.

## Migration

1. Keep the ordinary task implementation as the comparison/default. Use the .NET 11 SDK, `TargetFramework=net11.0` and `LangVersion=preview`, with a .NET 11 CoreCLR runtime that actually implements runtime async. A reference-assembly capability check cannot establish executing runtime support. RC2 requires preview; later main-branch F# 11.2 wording is not this baseline.
2. Set `DisableImplicitFSharpCoreReference=true`, `DisableImplicitLibraryPacksFolder=true` and `RestorePackagesWithLockFile=true`. Add a FSharp.Core 11 PackageReference, include the source in `Compile`, restore and retain the lockfile. Exact evidence uses SDK `11.0.100-rc.2.26504.105`, runtime `11.0.0-rc.2.26504.105` and package `11.0.101-rc2.26504.105`. The intrinsics ship in **lib/net10.0/FSharp.Core.dll**, not the inspected netstandard assets. That label does not create a supported net10 application route.
3. Keep the delayed recipe statically visible through the inline `soundings` entry point. Do not invoke an intrinsic as an ordinary helper: its library implementation is a throwing compiler stub. Do not hide the recipe behind an opaque function or assume optimized tail `yield!` handoff is supported. Both tested unsupported shapes fail with **FS3922**; missing preview gives FS3350 and unavailable runtime-reference support gives FS3351.
4. Retain one enumeration owner. Await each MoveNextAsync once before reading Current or issuing another operation. Await DisposeAsync when stopping early or after failure. `serially` rejects concurrent calls and makes repeated disposal a no-op; it does not make raw concurrent operations supported. Current is only meaningful after a successful move and before the next operation.
5. Pass the current enumeration token to nested sources and cancellable operations. A token check after `AsyncHelpers.Await ready` cannot interrupt a non-cancellable Task: cancellation remains pending until that work finishes. The example checks again afterward. Do not dispose while a move is pending to force cancellation; cancel the operation, await the move's outcome, then dispose serially.
6. Exercise genuinely pending first moves, incremental values, normal exhaustion, pending early disposal, nested cancellation, non-cancellable waiting, body faults and delayed nested cleanup before adoption. In this route, inner cleanup runs before outer cleanup; an outer cleanup failure replaces an inner cleanup failure, which replaces a body fault. When cleanup succeeds, the original body exception object is retained. Failures are not aggregated.

## Constraints and alternatives

- This route is experimental and deliberately bounded. It is not a drop-in TaskSeq replacement, a shipped high-level FSharp.Core builder, or a performance/debugger-stepping guarantee. The serial guard's ordinary wrappers are a correctness boundary, not a runtime-async optimization.
- No Native AOT, Mono/browser or legacy runtime route is claimed. Byref/byref-like/pinned values must not survive suspension. Opaque suspension fragments, optimized tail handoff and suspending producer try/with handlers remain outside this example. Keep cleanup in the supported statically lowered try/finally forms.
- Supply fresh resource factories for independent enumerations. Work supplied as an already-started Task can be shared; move the work's creation into the recipe too when each enumeration needs independent work.
- Ordinary manual task enumerators or an explicitly chosen maintained library remain alternatives. Declaring that library would require restoring and validating its actual dependencies; this sample does not silently assume one.
- RC2 execution is separate from GA reconciliation. Shared runtime-async restrictions also apply to the task-builder recipe, but this goal has its own executed producer ownership and lifecycle evidence.

## Evidence

The embedded older/newer records contain fresh compilation and real execution. The newer program verifies pending moves and disposal, overlap rejection at the guard, nested cancellation, cancellation limits, and delayed nested exception precedence. The focused script compiles the negative recipe/language/runtime probes. See `docs/verification/PIN-982-984.md`. Matching official intrinsic declarations, runtime-async source and language gates are listed in the structured sources; upstream test builders were reference evidence, not copied example code.
