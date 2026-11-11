# Express and check unsafe memory boundaries (experimental)

## Goal

Use this recipe when reviewing a low-level API whose caller must guarantee memory remains readable. The experimental updated rules distinguish declaring a pointer from accessing its memory, and let an `unsafe` member pass an audit obligation to its callers. They do not validate addresses, buffer lengths, ownership or lifetime for you.

Before introducing this experimental model, obtain explicit user opt-in. Existing authorization for that introduction remains valid; inspecting or explaining code already opted in needs no repeated approval. Compiler switches alone are not evidence of user consent.

## Requirements and stability

This edition targets **net11.0**, SDK **11.0**, and **C# preview**, with `Features=$(Features);updated-memory-safety-rules` and `AllowUnsafeBlocks=true`. The measured baseline is SDK **11.0.100**. These are experimental rules, not ordinary C# 15 behavior; the GA compiler still requires preview. No newer-SDK/older-target route is offered. Use the declared compiler mode in both the library and consumers that need caller checking.

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

The library overview suggests pointer-API annotation removal removes the project setting even when `unsafe` syntax remains. **Do not apply that instruction to this verified baseline.** A forced GA rebuild rejects unsafe syntax without `/unsafe` (`CS0227`); keep `AllowUnsafeBlocks=true` for the code above. Pointer declarations, address-taking and pinning alone compile without that setting in preview. Memory access still needs an unsafe context (`CS9360`).

The actual GA `ReadOnlySpan<byte>(void*, int)` call also requires an unsafe context (`CS9362`), and succeeds when explicitly acknowledged. This is separate from C# accessibility. One constructor check does not establish the annotation state of every API listed in the overview. Recheck changed reference assemblies/compiler artifacts before widening this finding. Current-main C# 16 compatibility notes are not a stable C# 15 contract; SDK 11.0.100 reproduces these experimental preview rules; C# 15 rejects them with CS8652.

## Migration

### Check the effective language version first

The TFM supplies the default language version; `global.json` selects the SDK.
An explicit `LangVersion` or inherited build properties can override that default.
Inspect the project and `Directory.Build.props`/targets. Temporarily add
`#error version` to a compiled source file and build: CS8304 reports the compiler
and selected language. Remove the directive, then rebuild successfully. For net11.0,
omit `LangVersion` for its stable default or explicitly choose `15.0` for stable
code. This experimental recipe deliberately uses `preview` plus its feature switch.
Avoid `LangVersion=latest`, which changes with the installed compiler; it does not
mean preview. Playbook's `targetVersion=latest` selects the newest catalogue target,
not a C# language setting. No newer-language/older-TFM route is supported here.

### Declaration and caller migration reference

These are bounded preview migration checks, not stable C# 15 promises. The actual
SDK 11.0.100 probes in `eng/Test-UnsafeBoundaries.ps1` take precedence over evolving
proposal text. Keep the checked beacon wrapper as the normal application surface.

| Review surface | Migration decision and measured boundary |
| --- | --- |
| Native declarations | Every extern needs an explicit `safe` assertion or `unsafe` caller obligation (CS9389). The safe `LibraryImport` partial declaration also compiles with the real SDK generator; inspect generated declarations too. Generation does not validate the native signature or calling convention. Native execution is outside this example. |
| Layout and fields | Explicit layout without a field decision produces CS9392. Review packed/nonzero-size layouts too: the current proposal requires annotations, but the pinned compiler accepts the unannotated Pack=1 probe. Treat that as missing enforcement, not evidence of safety. Writing an unsafe field requires an audit context (CS9362), just like reading it. |
| Properties and events | Accessor obligations are distinct from backing-field obligations. Marking a property/event unsafe does not mark its backing field. Choose a property modifier or individual accessor modifiers, not both (CS9396); do not repeat the same modifier on every accessor. Event add/remove accessors cannot have independent modifiers (CS1609); annotate the event. |
| Constructors | A parameterless unsafe constructor needs an audit when satisfying `new()` in an expression (CS9376). The `struct` constraint probe also reports CS9376; the audited new-constraint call succeeds. Declaration-position combinations remain source-reviewed; inspect them before changing generic public contracts. An unsafe block around a declaration is not a compatibility strategy. |
| Polymorphism | Do not add caller obligations through an override (CS9364) or implicit interface implementation (CS9365): callers can still use the safe base/interface declaration. |
| Delegates and nested code | Converting an unsafe method to a delegate needs an audit (CS9362). The delegate does not retain a caller obligation, so the conversion must discharge it. A local function needs its own unsafe modifier to propagate an obligation; an enclosing method modifier does not do this. Lambdas cannot declare requires-unsafe contracts; audit operations in their bodies. |
| Partial declarations | Keep safe/unsafe decisions consistent between both parts; the mismatched unsafe probe produces CS0764. Review source-generator output as well as authored code. |
| Contextual names | Escape an existing type named safe as `@safe` (or qualify it) where the contextual keyword would change parsing. The escaped-name probe compiles. |
| Expression and async scopes | `unsafe(expression)` acknowledges only the enclosed expression. `await unsafe(ReadAsync())` keeps the acknowledgement around the call. Never let a pointer outlive its allocation/pin; awaiting inside fixed is rejected (CS9398). Later proposal language about suspension does not extend storage lifetimes. |
| Stack storage | Under SkipLocalsInit, an uninitialized stackalloc span requires audit (CS9361), even if the next statement clears it. Prefer an initializer; the initialized probe executes safely. Never inspect uninitialized bytes to test this rule. |
| Legacy assemblies | Recompile callers as well as libraries. The real C# 14 library probes reject calls to pointer and function-pointer-returning methods with CS9363. Do not infer protection from pointer-free annotations. These are focused shapes, not an exhaustive legacy compatibility guarantee. |

Diagnostic numbers are compiler-specific evidence. CS9363 identifies legacy
pointer-signature compatibility; CS9376 constructor constraints; CS9392 explicit
layout fields. CS9401 has no named ErrorCode in this pinned compiler; do not
substitute it for observed CS9361. Rules not independently probed here (including the full
LibraryImport generator surface, declaration-position constraints and all legacy
function-pointer shapes) remain source-reviewed migration considerations, not
verified compiler/runtime guarantees. Recheck those exact consumers when migrating.

1. Keep a working .NET 10 baseline and inventory each raw operation's length, lifetime, ownership and pinning preconditions.
2. After obtaining explicit user opt-in, opt the .NET 11 library and relevant consumers into preview plus `updated-memory-safety-rules`. Keep `AllowUnsafeBlocks=true` where unsafe syntax is used.
3. Review every declaration modifier. Add explicit body contexts around actual operations; use member `unsafe` only when the caller has a real obligation. Preserve the narrow checked wrapper and existing public compatibility commitments.
4. Rebuild every changed project. Confirm valid readings and truncated-input rejection, then compile consumers missing caller/body contexts or violating declaration visibility. Expect compiler rejection, not a runtime safety guarantee.

## Written safety contracts

Document every requires-unsafe API with caller-facing XML remarks covering readable length, allocation lifetime, ownership and pinning. Put a local `SAFETY` comment beside each unsafe operation or safety assertion explaining how those obligations are discharged. The canonical raw reader remains a two-byte synchronous read; it never owns, retains or frees storage. The checked span wrapper stays the normal entry point. Keep raw APIs private/internal unless a cross-assembly caller requires them.

These comments and XML remarks are review recommendations. The compiler checks the opted-in unsafe boundaries and accessibility, not the truth or presence of this prose; no new analyzer enforces it. XML comments in the canonical source reach agents as part of the embedded snippet, without requiring generated XML documentation files.

## Evidence and limits

The canonical .NET 10 and .NET 11 projects compile and execute identical checks for byte order, boundary readings, unchanged input and truncated frames. Saved records report exact SDKs, language modes, inputs, commands and runtime output. `eng/Test-UnsafeBoundaries.ps1` separately rebuilds real consumers to check emitted contracts, accessibility, preview gating, missing body/caller contexts, pointer-only relaxation, pointer span construction and legacy-rule limitations. These are compilation and actual execution checks, not mocks or cross-compilation. No native resource, external account or required Human proof is involved. Broader API annotation removal and broader GA API behavior remain outside the measured claim.

Official sources are included with the recipe response. Essential steps and code above are complete offline; references provide provenance rather than missing implementation.

GA refresh (PIN-1287): the declared .NET 11 sample was rebuilt and executed with SDK 11.0.100 and runtime 11.0.0. This refresh does not establish broader platform or API behavior.
