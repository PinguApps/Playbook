# Choose runtime-async compilation deliberately (experimental)

## Goal

Choose the generated async implementation when upgrading, and understand which diagnostics a change can affect.

## Older approach

An ordinary .NET 10 async method compiles to a state machine. It remains a valid implementation: it awaits the reading, adds the correction and propagates an invalid reading as a failure.

{{snippet:older}}

## Newer approach

The same survey operation can use .NET 11 runtime-managed suspension. In the public RC2 SDK, opt in with the compiler feature in the project:

{{snippet:configuration}}

{{snippet:newer}}

`Features` is a compiler-feature list; append rather than discard other entries. The installed RC2 SDK did **not** translate `UseRuntimeAsync=true` into this compiler flag. Some first-party build repositories wire that property themselves. For this public SDK, an explicit `runtime-async=off` compiler feature selects the state-machine implementation; `UseRuntimeAsync=false` alone does not override an explicit on flag. Check effective imports and emitted code rather than assuming the property has an effect. This measured distinction qualifies the cumulative documentation's property-only advice.

## Migration

1. Keep an ordinary async baseline and a genuinely pending operation. Select the .NET 11 SDK, `net11.0`, C# 15 and CoreCLR for this experimental route. No additional NuGet package or workload is needed. The exact verified SDK is 11.0.100-rc.2.26504.105 with runtime 11.0.0-rc.2.26504.105; these are evidence pins, not a claim of GA verification.
2. Add `runtime-async=on` to `Features` for the application assembly. On this target, neither `LangVersion=preview` nor `EnablePreviewFeatures=true` is required for that flag. Rebuild dependencies separately if their generated implementation also needs changing.
3. Inspect the method's implementation flags: bit `0x2000` (`MethodImplOptions.Async`) identifies runtime async. The comparison instead carries `AsyncStateMachineAttribute`. A successful compile against reference metadata is insufficient; execute pending success and failure on the matching runtime.
4. Capture live diagnostics and thrown exceptions separately. In the recorded resumed survey call, live `StackTrace` output with runtime async omitted the state-machine-box frames visible with the flag off. Both exception traces retained `Survey.ReadDepth`. This bounded observation does not promise every logical parent frame, complete debugger stepping or identical exception stacks in other applications.
5. Remove reliance on `DOTNET_RuntimeAsync` and `UNSUPPORTED_RuntimeAsync`. They are removed runtime controls; setting them to zero did not disable the already marked sample. Roll back this assembly's choice by rebuilding with `Features=runtime-async=off`, rather than trying to change its emitted methods at process startup.

## Constraints and alternatives

Runtime-managed and compiler-generated async methods interoperate through ordinary Task APIs. Opting this assembly out does not rebuild .NET 11's libraries, which may themselves use runtime async. Do not describe the entire process as state-machine-only. Existing .NET 10 task code and the .NET 11 flag-off build remain useful fallbacks.

This route is experimental CoreCLR managed execution, verified on Windows x64. No browser/Mono, Native AOT, all-platform debugger, performance improvement or older-target runtime-async route is claimed. No provider, rendering mode or native toolchain is required. F# ordinary `task {}` and `async {}` are not converted by this C# flag; the separate `fsharp.author-runtime-async-builder` recipe supplies a bounded experimental F# builder.

## Evidence

Canonical projects have fresh build/run records, including actual runtime identity and live/exception output. `eng/Test-RuntimeAsync.ps1` rebuilds flag-on/off variants, proves the bare public SDK property is inert, and launches a marked child with both removed variables set to zero. These are real compiler/runtime checks, not mocks, cross-compilation or a profiler/debugger integration test. The installed-tool check retrieves this entire body, applicability and evidence offline. GA reconciliation remains separate.

Official sources: [.NET 11 runtime guidance](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-11/runtime), [canonical removal of runtime switches](https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/preview4/runtime.md), [matching RC2 F# runtime contract](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/fsharp/docs/runtime-async.md).
