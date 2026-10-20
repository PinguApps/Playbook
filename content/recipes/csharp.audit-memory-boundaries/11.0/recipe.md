# Express and check unsafe memory boundaries (experimental)

## Goal

Use this recipe when reviewing a low-level API whose caller must guarantee memory remains readable. The experimental updated rules distinguish declaring a pointer from accessing its memory, and let an `unsafe` member pass an audit obligation to its callers. They do not validate addresses, buffer lengths, ownership or lifetime for you.

## Requirements and stability

This edition targets **net11.0**, SDK **11.0**, and **C# preview**, with `Features=$(Features);updated-memory-safety-rules` and `AllowUnsafeBlocks=true`. The measured baseline is SDK **11.0.100-rc.2.26504.105**. These are experimental rules, not ordinary C# 15 behavior or GA verification. No newer-SDK/older-target route is offered. Use the declared compiler mode in both the library and consumers that need caller checking.

No NuGet package, package asset, provider, workload, rendering mode, account or native service is needed. The console examples run on a platform supported by their .NET runtime. The safe wrapper is equally usable by framework code; it does not establish browser, native UI or AOT behavior.

## Older approach: .NET 10 / C# 14

A fictional coastal beacon sends a two-byte little-endian reading. The wrapper checks the available length and pins the managed storage for the whole read. Its raw API requires the caller to supply two readable bytes. An ordinary `unsafe` method establishes the body's unsafe context; pointer-bearing call sites need an unsafe context too.

{{snippet:older}}

This remains a valid approach. There was no equivalent compiler-enforced caller obligation for an unsafe method with a pointer-free signature in C# 14. Documentation and careful wrappers remain necessary; the older modifier alone does not impose that new contract.

## Newer approach: make the two audit boundaries explicit

Under the updated rules, `unsafe` on `ReadRaw` declares a caller obligation. It does **not** authorize memory access inside that method: the body needs its own `unsafe` block. The wrapper's pointer declaration and `fixed` statement can remain outside an unsafe context; only the call is acknowledged with `unsafe(expression)`.

{{snippet:newer}}

`Decode` exposes a checked, pointer-free surface. For valid managed spans, its length check and pinning discharge the raw reader's stated obligation. A span originally fabricated from an invalid pointer is still the creator's responsibility. For this two-byte operation, ordinary span indexing or `BinaryPrimitives.ReadUInt16LittleEndian` is usually preferable; the pointer example teaches an existing low-level boundary, not a performance recommendation.

Explicit-layout fields need a deliberate `safe` or `unsafe` declaration under the updated model. This independent, non-overlapping value field has no extra caller invariant, so the author marks its access `safe`. The .NET 10 equivalent uses the same attributes and field without the `safe` modifier. It already supports the same ordinary value read/write; the new declaration records the safety decision.

{{snippet:layout}}

## Build and try the complete example

Create a console project with the selected `BeaconFrame` implementation above in `Frames.cs` and this `Program.cs`:

{{snippet:program}}

Create the project with `dotnet new console --framework net11.0`, replace its `PropertyGroup` with the following, then run `dotnet run -c Release` with the .NET 11 SDK. Expect 4661:

{{snippet:newer-project}}

For the older comparison, use the .NET 10 implementation, target `net10.0`, set `LangVersion` to `14.0`, remove `Features`, and retain `AllowUnsafeBlocks=true`. Keep `ImplicitUsings` and `Nullable` enabled. Passing an empty or one-byte frame must throw `ArgumentException` before any raw read. Do not test an invalid pointer by executing it: that can crash or corrupt the process.

## Constraints: accessibility and caller obligations

- `public`, `internal` and `private` still control who can name a member. An unsafe context grants no access to a private declaration. Keep raw methods private/internal unless external callers actually need them; this example uses `public` to demonstrate a cross-assembly contract.
- An opted-in caller must surround a requires-unsafe call with an unsafe block or expression, even if its signature contains no pointer. Merely marking the caller method `unsafe` propagates the obligation; its body still needs a context.
- In the measured output, the module carries `System.Runtime.CompilerServices.MemorySafetyRulesAttribute(2)` and `ReadRaw` carries `System.Diagnostics.CodeAnalysis.RequiresUnsafeAttribute`. Let the compiler emit these markers; do not hand-apply attributes as a substitute for the feature switch.
- A consumer using legacy rules does not enforce a pointer-free requires-unsafe contract. Changing library annotations does not upgrade other projects. The rules are an audit aid, not runtime access control.
- The preview's `safe` modifier records a deliberate safety assertion, notably on extern/layout declarations. It neither changes accessibility nor establishes a safe lexical block. Do not label an unchecked raw reader `safe`. The layout example is an ordinary value read/write; it does not establish native marshalling behavior.

## Resolve the AllowUnsafeBlocks conflict

The library overview suggests pointer-API annotation removal removes the project setting even when `unsafe` syntax remains. **Do not apply that instruction to this verified baseline.** A forced RC2 rebuild rejects unsafe syntax without `/unsafe` (`CS0227`); keep `AllowUnsafeBlocks=true` for the code above. Pointer declarations, address-taking and pinning alone compile without that setting in preview. Memory access still needs an unsafe context (`CS9360`).

The actual RC2 `ReadOnlySpan<byte>(void*, int)` call also requires an unsafe context (`CS9362`), and succeeds when explicitly acknowledged. This is separate from C# accessibility. One constructor check does not establish the annotation state of every API listed in the overview. Recheck changed reference assemblies/compiler artifacts before widening this finding. Current-main C# 16 compatibility notes are not a stable C# 15 contract; this edition describes only reproduced RC2 preview behavior.

## Migration

1. Keep a working .NET 10 baseline and inventory each raw operation's length, lifetime, ownership and pinning preconditions.
2. Opt the .NET 11 library and relevant consumers into preview plus `updated-memory-safety-rules`. Keep `AllowUnsafeBlocks=true` where unsafe syntax is used.
3. Review every declaration modifier. Add explicit body contexts around actual operations; use member `unsafe` only when the caller has a real obligation. Preserve the narrow checked wrapper and existing public compatibility commitments.
4. Rebuild every changed project. Confirm valid readings and truncated-input rejection, then compile consumers missing caller/body contexts or violating declaration visibility. Expect compiler rejection, not a runtime safety guarantee.

## Evidence and limits

The canonical .NET 10 and .NET 11 projects compile and execute identical checks for byte order, boundary readings, unchanged input and truncated frames. Saved records report exact SDKs, language modes, inputs, commands and runtime output. `eng/Test-UnsafeBoundaries.ps1` separately rebuilds real consumers to check emitted contracts, accessibility, preview gating, missing body/caller contexts, pointer-only relaxation, pointer span construction and legacy-rule limitations. These are compilation and actual execution checks, not mocks or cross-compilation. No native resource, external account or required Human proof is involved. Broader API annotation removal and final GA behavior remain outside the measured claim.

Official sources are included with the recipe response. Essential steps and code above are complete offline; references provide provenance rather than missing implementation.
