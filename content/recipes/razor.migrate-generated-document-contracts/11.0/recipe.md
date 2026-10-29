# Keep generated catalogue output intact when rebuilding a Razor tool

## Goal

A fictional catalogue indexer keeps generated C# text, diagnostics and source mappings so its host can display compiler feedback next to an exhibit template. Rebuild the module with the new compiler dependency set without silently dropping that data. This is an advanced, version-coupled SDK compiler integration, not runtime Razor view compilation.

The actual older SDK is 10.0.402 and the current SDK is 11.0.100-rc.2.26504.105. Their compiler product versions are 10.4.0-preview.26505.111 and 11.1.0-preview.26504.105; their matching Roslyn products are 5.9.0-1.26505.111 and 5.12.0-1.26504.105. Exact file versions, hashes and full identities are in the execution records. These SDK-bundled tool versions are separate from the app runtime/target release. No independent compiler package or native asset is substituted.

## Migration

1. Inventory the existing host's compiler, Razor utility and Roslyn references. Keep one coherent SDK dependency set. The complete projects below resolve their references from the selected SDK's MSBuild paths; do not mix in Razor.Language 6.0 or a different Roslyn package. Build with the declared SDK and requalify deliberately when it changes.
2. Preserve a valid older host integration that already supplies generated output. On the inspected SDK 10.0.402, `RazorCodeDocument`'s generated-output reader is internal. There is no supported public reader equivalent to teach here. The old module therefore accepts its host's `RazorCSharpDocument`; it does not pretend to extract one publicly. In the current SDK, the public `GetCSharpDocument` enables the new `Compile` method.
3. Preserve `Text`, diagnostics, both mapping data and line pragmas when constructing a new document. The supplied mapping array is the compiler's order by generated span; the constructor also exposes original-span order. Use `Text.ToString()` on the older document, or the current `GeneratedCode` convenience property. Do not discard invalid-input diagnostics merely because generated C# exists.
4. Review `CodeRenderingContext` construction. The current `reportDiagnostics` optional argument defaults to true. False skips importing diagnostics from the supplied intermediate document tree; it does **not** suppress a later explicit `AddDiagnostic`. The diagnostic-only example demonstrates both counts with a real compiler diagnostic: true gives one imported/two after explicit addition, false gives zero imported/one after addition. Its empty writer does not implement a replacement renderer. Keep reporting enabled normally; skip the import only when another host path owns that feedback.
5. Choose `isDeclarationDocument` and `isStubDocument` deliberately when constructing the current document. Both default to false. They are labels, not a transformation that creates declaration-only or stub C# from existing text. Use the actual compiler's intended generation path when you need different output; do not flip flags to manufacture it.
6. Recompile dependent constructor callers. Old five-argument source calls compile against the new optional defaults. A previously compiled call still binds the old five-parameter constructor signature and fails against this new tooling set with `MissingMethodException`. Optional arguments do not repair that ABI. Keep the old dependency closure with old binaries or rebuild/requalify the host and its extensions together.
7. Exercise valid and invalid templates through the real compiler, inspect output/diagnostics and retain the mapping/line-pragma data. The verification harness checks the actual pipeline and a previous compiled consumer. A successful library compilation alone cannot establish those behaviours.

## Constraints

Both canonical modules are complete library projects for a custom host. Build them with the selected SDK using `dotnet build CatalogueCompiler.csproj -c Release`; call the current `CatalogueTool.Compile` with the host's source and read its result. The older `Copy` requires an output object from the host's existing integration. No new public .NET 10 reader or older-target backport is advertised. The SDK's compiler DLL distribution is version coupled and supplies no promise of a stable external-host ABI.

The verification harness alone observes the older internal reader by reflection so that the original compiler's actual output can be checked. This private probe is not part of either canonical module and is not an implementation route for applications. If that private detail changes, verification must fail or be requalified; do not recommend reflection as a supported replacement reader.

Mappings are preserved exactly, with original/generated spans inside the actual buffers. On the inspected pipeline a generated span may begin at the emitted `Write(...)` wrapper rather than the source expression itself. A consumer must respect the compiler mapping contract; copying coordinates does not repair them or promise a character-for-character correspondence between substrings. This tool does not execute generated C#, provide arbitrary IDE hosts, emulate design-time compilation or establish downstream rendering correctness.

The measured route is managed Windows x64 with matching .NET 10/11 runtime families, C#14/15 and no NuGet package, native toolchain, optional workload, provider or experimental suppression. Current evidence is RC2; actual GA reconciliation and comprehensive coverage remain open. The exact SDK pin is an advanced tooling-closure requirement here, not a universal requirement for normal Razor applications.

## Older approach

Keep the existing host integration supplying a generated document; reconstruct it with all diagnostic/mapping data and read `Text`. The selected SDK has no public generated-output reader. This constructor consumer is usable with host-supplied output, not a fabricated end-to-end public reader.

### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### .NET 10: `CatalogueCompiler.csproj`

{{snippet:n10-cataloguecompiler-csproj}}

### .NET 10: `CatalogueTool.cs`

{{snippet:n10-cataloguetool-cs}}

## Newer approach

The current public reader permits an end-to-end compiler module. Preserve data when copying; choose diagnostic reporting and flags explicitly only when the host intends them.

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### .NET 11: `CatalogueCompiler.csproj`

{{snippet:n11-cataloguecompiler-csproj}}

### .NET 11: `CatalogueTool.cs`

{{snippet:n11-cataloguetool-cs}}

## Evidence

`eng/Record-CatalogueCompiler.ps1` compiles both actual dependency sets and executes the original valid `<h2>Catalogue: @DateTime.Now.Year</h2>` and invalid `<h2>@(` inputs. It asserts preserved generated text, expected diagnostics, mapping/line-pragma arrays and bounded spans; current diagnostic reporting/flags and rebuilt source defaults; and a previous compiled constructor consumer's actual binding failure. The older private reader appears only in this verification code. Records retain exact commands, inputs, SDK/compiler/Roslyn hashes, runtime and outputs. Installed MCP inspection separately verifies complete offline delivery and canonical snippet equality. No generated application execution, arbitrary host compatibility or actual GA proof is inferred.
