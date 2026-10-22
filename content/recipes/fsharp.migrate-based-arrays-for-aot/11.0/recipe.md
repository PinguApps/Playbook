# Preserve chart coordinates with zero-based array storage

## Goal

Replace based two-dimensional arrays with zero-based storage and explicit coordinate offsets before publishing a Native AOT application. Harbour chart coordinates remain unchanged.

## Older approach

This original .NET 10 JIT implementation uses the array's lower bounds as chart coordinates. It remains valid on a supported JIT runtime. It is not evidence that arbitrary based arrays work under Native AOT.

{{snippet:older}}

## Newer approach

Add `open System`. The chart stores the origin separately. `zeroCreateBased`, `createBased` and `initBased` become `zeroCreate`, `create` and `init`; the initializer translates storage indexes back to chart coordinates. `depthAt` translates in the other direction.

{{snippet:newer}}

For example, `let chart = surveyed -4 7 2 3 (fun north east -> north * 100 + east)` gives `depthAt -4 7 chart = -393`. Its storage entry is `chart.Cells[0,0]`. Callers must use the coordinate-aware accessor; merely replacing allocation calls changes indexing semantics.

## Migration

1. Inventory all three public Based calls and every use of lower bounds, indexed mapping and direct indexing. Preserve the external origin and dimensions before changing representation. Audit code accepting arrays from elsewhere too.
2. Add the original support above to `Grid.fs`. Use a project with `OutputType=Exe`, `TargetFramework=net11.0`, `LangVersion=11.0`, `DisableImplicitFSharpCoreReference=true`, `DisableImplicitLibraryPacksFolder=true` and `RestorePackagesWithLockFile=true`; include `Grid.fs` in `Compile` and reference FSharp.Core 11 explicitly. The reproducible package pin is `11.0.101-rc2.26504.105`. Restore and retain the lockfile. Select its actual **lib/net10.0/FSharp.Core.dll** asset, not a netstandard fallback or a guessed net11.0 asset.
3. Move coordinate translation to the boundary. `map`, `mapi` and `copy` now operate on zero-based cells. When `mapi` needs chart coordinates, add the saved origin. `rebase` already produces zero-based storage; rebasing without retaining the origin loses its meaning. Keep values, not just array lengths, in regression checks.
4. Check both empty dimensions independently and together, copied storage independence and every translated value. The original and migrated programs both check create/init/zeroCreate and map/mapi/copy/rebase. The newer package's based helper semantics are separately executed under JIT only.
5. On a suitable Windows x64 machine with Visual C++ x64 tools and Windows SDK, publish with the .NET 11 SDK: `dotnet publish -c Release -r win-x64 -p:PublishAot=true -p:IlcTreatWarningsAsErrors=true`. Execute the file in `bin/Release/net11.0/win-x64/publish/`. Exact evidence pins are SDK `11.0.100-rc.2.26504.105`, runtime/compiler packs `11.0.0-rc.2.26504.105`, MSVC `14.51.36231` and Windows SDK `10.0.26100.0`; those are measured baselines, not universal exact consumer requirements. Other native platforms need their own supported toolchain and verification.
6. Keep warnings as errors. All three public Based entry points carry RequiresDynamicCode in the selected package, including calls whose bases are literally zero. Real Native AOT analysis reports **IL3050** for ZeroCreateBased, CreateBased and InitializeBased. Fix call sites rather than suppressing the warnings. Review the whole application: safe array allocation cannot make unrelated reflection or dynamic code AOT-compatible.

## Constraints and alternatives

- `map`, `mapi` and `copy` preserve their input bounds. Their lack of a public warning is not a promise that arbitrary non-zero-based inputs work under AOT. The selected net10.0 asset uses a different auxiliary allocation path from the netstandard assets.
- The complete migrated program published and ran natively without suppression. A plain class and concrete integer string formatting avoid rooting the record/printf reflection formatting paths encountered during authoring; retain those choices or separately verify replacements.
- Zero-based storage and offsets also work on .NET 10; they are not a newly invented .NET 11 language feature. This edition addresses the newer public annotation and migration. No additional .NET 10 catalogue foundation is added.
- If based arrays are part of an unavoidable interface, retain a supported JIT deployment and document it. A flattened one-dimensional buffer is another option when its indexing and overflow rules are justified; it is unnecessary for this example.
- F# 11 ordinary syntax needs no preview language opt-in. The verification used RC2 packages and runtime, separately from final GA reconciliation. No Native AOT claim is made for FSI or runtime-async sequences.

## Evidence

The embedded evidence includes exact dependencies, source hashes, fresh builds, original JIT execution and actual native publish/run. The native build executes its published program with `--require-native`; the supplemental record checks that the produced PE has no managed metadata. The focused authoring check captures all three IL3050 call sites, including literal zero bases. See `docs/verification/PIN-982-984.md` for reproduction and limits. The matching official Array2D source/signatures and release notes are listed in the structured sources.
