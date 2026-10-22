# Author a small runtime-async workflow (experimental)

## Goal

Define a deliberately small F# workflow over runtime suspension, with explicit failure and asynchronous resource lifetime rules.

## Older approach

The ordinary .NET 10 task builder already supports the survey operation and asynchronous disposal. It remains the appropriate default when an experimental custom builder is unnecessary.

{{snippet:older}}

## Newer approach

There is no shipped `FSharp.Core.runtimeTask`. The upstream name belongs to compiler-test support. The following independently authored support defines only the operations needed here. Add `open System`, `open System.Threading.Tasks`, `open System.Runtime.CompilerServices` and `open Microsoft.FSharp.Core.CompilerServices.StateMachineHelpers` before it.

{{snippet:builder}}

{{snippet:newer}}

`Run` introduces a compiler-recognized return marker. The other inline fragments keep the waits inside that marked method after specialization. The intrinsic implementations are throwing compiler stubs: never call them as ordinary runtime helper functions. `return!` in this support explicitly awaits Task or ValueTask; it does not return an unawaited nested task. Inputs are already-started tasks and bindings wait sequentially; this is not a concurrency limiter.

## Migration

1. Keep the ordinary task comparison. Use .NET 11 SDK and `net11.0` with `LangVersion=preview`, on a .NET 11 CoreCLR runtime that implements this feature. This RC2 compiler rejects the feature without preview (FS3350); later main-branch F# 11.2 rules are not this compiler's gate.
2. Reference FSharp.Core 11 and select its `lib/net10.0/FSharp.Core.dll` asset. Set `DisableImplicitFSharpCoreReference=true` and add an explicit FSharp.Core PackageReference (or a central PackageVersion). Set `RestorePackagesWithLockFile=true` and retain the lockfile. The samples also set `DisableImplicitLibraryPacksFolder=true` so verification restores the actual NuGet package rather than the SDK's bundled library copy. Exact evidence pins are SDK 11.0.100-rc.2.26504.105, runtime 11.0.0-rc.2.26504.105 and Core 11.0.101-rc2.26504.105. The asset's net10.0 name does not establish a supported net10 application route. The older comparison uses SDK 10.0.402/Core 10.1.402 (netstandard2.1).
3. Include the complete support above and keep its combinators inline. Choose SurveyTask, SurveyValueTask, SurveyTaskUnit or SurveyValueTaskUnit according to the actual generic/non-generic return carrier. Do not infer that an arbitrary opaque callback or custom awaiter can suspend safely.
4. Verify a genuinely pending first wait, second wait, return-from result, propagated failure and delayed DisposeAsync before adopting it. The checks also cover failure followed by pending cleanup, preserving the same body exception when cleanup succeeds and disposing exactly once. Ordinary finally precedence applies if cleanup itself throws; this support does not aggregate failures.
5. Inspect emitted `MethodImplOptions.Async` bit 0x2000, then execute on the matching runtime. A reference-assembly capability check cannot inspect the eventual executing JIT. The negative probes reject unavailable target assets and lack of the reference marker, plus prohibited lifetime/suspension shapes.

## Constraints and alternatives

- This is experimental compiler/runtime coupling, with no general-purpose production-builder, complete debugging, Native AOT or performance claim. Ordinary `task {}` and `async {}` retain their existing implementation.
- Do not retain a byref, Span/byref-like value or pinned pointer across suspension. The actual RC2 compiler reports **FS3919** for these probes; its source document calls the diagnostic FS3917. Suspension outside a marked method is rejected. Tail calls, stack allocation and synchronized runtime-async methods remain prohibited runtime shapes. Captured pointer provenance is not reliably diagnosed; a passing compilation does not certify lifetime safety.
- The bounded non-generic Task check uses a real non-generic Task. A pending Task<int> upcast to Task produced InvalidProgramException on the recorded RC2 runtime; keep that value on the generic binding path. This limitation is visible here rather than advertised as supported interoperability.
- Async disposal requires an actual IAsyncDisposable; synchronous IDisposable, loops, cancellation policy, parallel sources, custom awaiters and asynchronous sequences are outside this deliberately small support. A non-cancellable await does not become cancellable.
- Managed checks ran on Windows x64. No UI, provider, rendering mode, workload or native toolchain is required. See `runtime.choose-async-codegen` for shared runtime settings/live-versus-exception-stack distinctions and `fsharp.bound-async-work` for ordinary concurrency control.

## Evidence

Both original projects compile and execute against their actual pinned dependencies. Fresh records cover four carriers, pending waits, return!, exception identity, normal/exceptional asynchronous disposal and emitted markers. `eng/Test-RuntimeAsync.ps1` records real negative compiler diagnostics, not metadata-only execution claims. The existing packed-tool harness checks complete body, experimental discovery labels, edition 11.0 and applicability offline. No GA or other-platform proof is inferred.

Official sources: [matching runtime-async contract](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/fsharp/docs/runtime-async.md), [actual intrinsic declarations](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/fsharp/src/FSharp.Core/resumable.fsi), [RC2 language gates](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/fsharp/src/Compiler/Facilities/LanguageFeatures.fs).
