# Adapt an opaque curried callback once

## Goal

A fictional seed laboratory folds a batch of readings through a supplied callback. Keep a clear curried API for literal lambdas and callbacks returned by another function. When the enclosing inline function is expanded and the callback is opaque, permit one optimized-closure adaptation before the loop instead of repeated fast-call adaptation inside it.

## Older approach

SDK10.0.402/F#10 with FSharp.Core10.1.402 has no `OptimizeClosureIfNotInlinedAttribute`. A valid inline fold can use `InlineIfLambda` alone. Keep that working approach; do not add an unknown attribute to the old Core or claim the old version cannot process batches.

Create sibling `SeedFoldLibrary` and `SeedFold` directories. Save each source as `SeedFold.fs`; use the exact project filenames below because the consumer's project reference points at its sibling. Save `Directory.Packages.props` in each directory and pin `sdk.version:10.0.402`, `sdk.rollForward:disable` in each `global.json`. These actual older projects run on10.0.13. They explicitly disable the SDK's implicit Core reference so the package pin owns the bytes.

`SeedFoldLibrary/SeedFoldLibrary.fsproj` and its package file:

{{snippet:project-10-seedfoldlibrary}}

{{snippet:packages-10-seedfoldlibrary}}

`SeedFoldLibrary/SeedFold.fs`:

{{snippet:source-10-seedfoldlibrary}}

`SeedFold/SeedFold.fsproj` and its package file:

{{snippet:project-10-seedfold}}

{{snippet:packages-10-seedfold}}

`SeedFold/SeedFold.fs`:

{{snippet:source-10-seedfold}}

The consumer is a separate assembly. A literal lambda captures bias4; the non-inlined factory records creation before returning its callback, making this a genuine supplied-function case rather than a factory call flattened into every loop iteration. Both folds return28 for readings2,5,9, preserve effect order and stop with the original exception on reading5. The older approach is an executed fallback, not an emulation of the new attribute.

## Newer approach

Use separate sibling directories from the older comparison. Pin SDK11.0.100-rc.2.26504.105 and FSharp.Core11.0.101-rc2.26504.105 as shown; the measured runtime/reference closure is11 RC2. F# **11.0**, rather than `preview`, is the declaration gate. The attribute is public, sealed, parameter-only and has a parameterless constructor. It must accompany `InlineIfLambda` on a **separately curried parameter** of a suitable inline function or method. That parameter must have a curried F# function type of arity **2–5**; a tupled unary function is not an arity-two callback.

`SeedFoldLibrary/SeedFoldLibrary.fsproj` and its package file:

{{snippet:project-11-seedfoldlibrary}}

{{snippet:packages-11-seedfoldlibrary}}

`SeedFoldLibrary/SeedFold.fs`:

{{snippet:source-11-seedfoldlibrary}}

`SeedFold/SeedFold.fsproj` and its package file:

{{snippet:project-11-seedfold}}

{{snippet:packages-11-seedfold}}

`SeedFold/SeedFold.fs`:

{{snippet:source-11-seedfold}}

The `foldControl` function preserves the same body and `InlineIfLambda` without the new attribute. In the optimized separate consumer, `opaque` creates the callback once and calls `OptimizedClosures.FSharpFunc.Adapt` once before its loop; the loop calls the adapted `Invoke`. The control retains an `InvokeFast` call in its loop. The known capturing literal needs no `Adapt` in its observed method. This structural evidence establishes this compiler transform, not elimination of every closure or a universal throughput/allocation percentage. An opaque factory does not become inlined merely because its returned function is adapted.

## Migration

1. Identify an existing inline function with a saturated curried callback call in a loop. Preserve result, call order, exception and empty-input contracts before changing its annotations.
2. Check the actual compiler and Core package. The declaration needs F#11 and the qualified Core containing the attribute; an older Core cannot acquire it from a language-mode setting.
3. Add `OptimizeClosureIfNotInlined` alongside `InlineIfLambda` on the separate callback parameter. Do not place it on a tupled method argument, non-inline declaration, non-function value, unary callback or callback with more than five arguments. A separately curried method parameter is different from a tupled argument list.
4. Retain a literal capturing lambda and a callback from a non-inlined factory in a separate consumer. Run `dotnet restore`, `dotnet build -c Release` and `dotnet run -c Release --no-build` from `SeedFold` using that installation's absolute executable. Restore initially produces lock files; retain both projects' locks and use locked restore on subsequent builds.
5. Expect the PASS line reporting28, ordered effects2,5,9, exception stop at5 and empty result0. Compare against the unannotated control. Inspect optimized emitted IL for the relevant consumer loop before making a transform claim; an assembly-wide occurrence of the name `Adapt` is insufficient.
6. The actual newer compiler rejects the attributed declaration in F#10 mode with FS3350. Unsupported shapes produce FS3916. Correct the declaration or retain the older valid fold; do not suppress these errors or fabricate an older attribute.

## Constraints

Only target11/edition11 is served; the original target10 comparison preserves the exact eight foundations. Measured Core11 assets include net10.0 and netstandard variants, but an asset label is not a tested older-target route. This recipe advertises no new attribute route on an older target or old compiler. The separate import check builds an attributed library with the newer compiler, then consumes it using that **same newer compiler in F#8 mode** and runs on11 RC2. That proves imported metadata consumption for those conditions, not old-compiler/runtime compatibility. Use the genuinely old library/consumer above when retaining the older toolchain.

The framework/package/SDK pins are deliberately separate: the SDK's bundled Core and NuGet Core can have different hashes despite related versions. Saved records identify actual packages and selected assets; the new consumers select Core `lib/net10.0`, the older ones `lib/netstandard2.1`. No provider, rendering mode, workload, native toolchain, preview-language switch or experimental diagnostic suppression is required. Verification ran on Windows x64. The package/SDK are RC2; final GA reconciliation, comprehensive coverage and existing Human gates remain open. No performance benchmark or cross-platform matrix is claimed.

## Evidence

`eng/Test-CompilerInspections.ps1 -Record` captures actual older/newer library compilation and separate-consumer execution through the existing sample-evidence workflow. The additional current execution record preserves decoded call offsets and backward-loop targets, requiring one `Adapt` before `opaque`'s loop, no `InvokeFast` there, a repeated `InvokeFast` in `control`, and no literal adaptation. It also preserves actual FS3350, seven invalid-shape FS3916 probes, newer-compiler/F#8-mode imported-library execution, and compiler/reference/runtime/Core hashes. These are original fictional-domain checks, not upstream tests relabelled as execution. Installed stdio inspection of the exact ordinary packed artifact verifies discovery ID/title/goal, full canonical Markdown, target11/edition11, applicability and fresh evidence offline from an unrelated directory; target10 has no applicable edition. Proxy independence is not network isolation. PIN-959 and PIN-961 remain open.
