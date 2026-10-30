# Inspect null-coalescing conversions in C# and VB

## Goal

A fictional monitoring station accepts optional readings. Its compiler host needs to distinguish the conversion applied to a present reading from the fallback expression. Obtain `ICoalesceOperation` for C# `??` or VB's two-argument `If`; inspect its common conversion first, then use the extension belonging to the operation's language when language-specific flags matter. This is compiler-host guidance, not new VB syntax.

## Older approach

The actual Roslyn5.9.0 packages lack both language-specific `GetValueConversion` methods. `ICoalesceOperation.ValueConversion` remains a valid public `CommonConversion`: it reports numeric, reference and identity categories but does not expose VB's `IsWidening` or C#'s complete language-specific `Conversion` contract. Do not cast private operation implementations, reflect into compiler internals or pretend a common conversion contains every language-specific detail.

Create separate `ReadingConversions` and `ReadingConversionsVB` directories. Save each block under the indicated filename. Each project is a complete console application. Use SDK10.0.402, actual runtime/reference10.0.13, C#14 and VB16.9. Pin the SDK in each project's `global.json` with `sdk.version` set to `10.0.402` and `sdk.rollForward` to `disable`. The C# host deliberately analyses both languages; the second executable is an actual VB caller.

`ReadingConversions/ReadingConversions.csproj`:

{{snippet:project-10-readingconversions}}

`ReadingConversions/Directory.Packages.props`:

{{snippet:packages-10-readingconversions}}

`ReadingConversions/NuGet.Config`:

{{snippet:feeds-10-readingconversions}}

`ReadingConversions/Readings.cs`:

{{snippet:source-10-readingconversions}}

`ReadingConversionsVB/ReadingConversionsVB.vbproj`:

{{snippet:project-10-readingconversionsvb}}

`ReadingConversionsVB/Directory.Packages.props`:

{{snippet:packages-10-readingconversionsvb}}

`ReadingConversionsVB/NuGet.Config`:

{{snippet:feeds-10-readingconversionsvb}}

`ReadingConversionsVB/Readings.vb`:

{{snippet:source-10-readingconversionsvb}}

The `CURRENT` branches are intentionally excluded by these older projects. All three actual operation categories pass in both older executables. Deliberate direct calls to the missing language APIs fail with CS0117 against5.9.0; merely choosing an SDK with a larger version does not add an API to an older referenced package.

## Newer approach

Use SDK11.0.100-rc.2.26504.105 with its matching11 RC2 runtime/reference pack for the measured route. Pin that exact SDK in `global.json` as above. The declared C#15/VB16.9 modes require no `preview` switch. Restore Microsoft.CodeAnalysis.CSharp and VisualBasic **5.12.0-1.26504.105**, which resolve Common at the same version and Analyzers5.12.0-1.26465.4. These prerelease packages are available on Microsoft's `dotnet-tools` feed; the exact source mapping below keeps CodeAnalysis packages on that feed. NuGet.org's observed5.9.0 ceiling is not an alternative source for this pin.

Use separate directories from the older comparison, keeping the same filenames. The public signatures are `CSharpExtensions.GetValueConversion(ICoalesceOperation) -> Microsoft.CodeAnalysis.CSharp.Conversion` and `VisualBasicExtensions.GetValueConversion(ICoalesceOperation) -> Microsoft.CodeAnalysis.VisualBasic.Conversion`. Fully qualify their declaring namespaces: similarly named extension classes also exist in the common namespace.

`ReadingConversions/ReadingConversions.csproj`:

{{snippet:project-11-readingconversions}}

`ReadingConversions/Directory.Packages.props`:

{{snippet:packages-11-readingconversions}}

`ReadingConversions/NuGet.Config`:

{{snippet:feeds-11-readingconversions}}

`ReadingConversions/Readings.cs`:

{{snippet:source-11-readingconversions}}

`ReadingConversionsVB/ReadingConversionsVB.vbproj`:

{{snippet:project-11-readingconversionsvb}}

`ReadingConversionsVB/Directory.Packages.props`:

{{snippet:packages-11-readingconversionsvb}}

`ReadingConversionsVB/NuGet.Config`:

{{snippet:feeds-11-readingconversionsvb}}

`ReadingConversionsVB/Readings.vb`:

{{snippet:source-11-readingconversionsvb}}

The integer-nullable/long-fallback case reports numeric conversion for the non-null integer value. The language-specific results additionally report C# implicit or VB widening conversion. String/object operands report a reference conversion; nullable-integer/integer reports identity. `ValueConversion` concerns the present value, not a second conversion of the fallback. Both extensions throw `ArgumentNullException` for null and `ArgumentException` for an operation created by the other language. Branch on `Language` before dispatch in a mixed-language host. VB `If(value, fallback)` is a binary missing-value expression; its three-argument conditional form is a different operation.

## Migration

1. Start from a semantic model for a successfully bound source tree. Resolve references and check diagnostics before trusting operation classifications. The example uses the console host's actual trusted platform assemblies to analyse small in-memory declarations; a production workspace should use its project's real reference closure.
2. Obtain the public `ICoalesceOperation`. Keep `ValueConversion` when its common flags answer the question; do not replace a working common-only inspector unnecessarily.
3. Pin both language packages to the same qualified version when using language-specific flags. Keep the project, package declarations and NuGet source mapping together.
4. Dispatch according to the operation's `Language`; reject an unsupported language explicitly. Do not pass an operation from C# into the VB extension or vice versa.
5. From each project directory run `dotnet restore`, `dotnet build -c Release` and `dotnet run -c Release --no-build` using that installation's absolute executable. The generated lock file should resolve exactly the declared closure; retain it for later `dotnet restore --locked-mode` checks. No network is needed to analyse declarations after restore.
6. Expect both C# and VB classification PASS lines, all three VB-native PASS lines, and the newer wrong-language/null-rejection PASS. An unexpected diagnostic, changed category or absent exception fails the sample.

## Constraints

The served edition targets11 only;10 remains an executed comparison, preserving the exact eight foundations. `introducedIn:11.0` identifies this qualified release-family route, not a claim about the first nightly build or a new VB language version. Actual net11 hosts select package `lib/net10.0` assets. Their labels and the netstandard assets do not alone establish an older-target route; none is advertised here. No provider, rendering mode, workload, native toolchain, reflection, private casts or experimental suppression is needed. These public APIs were exercised on Windows x64; no recurring OS matrix is introduced.

The package's repository metadata identifies dotnet/dotnet VMR820230c386db5b0106c5430b3e5aee92a7826bf5. The audit's Roslyn-main5.13 source is a separate snapshot, not these package bytes. RC2 package evidence is not actual GA proof. PIN-959 qualifying coverage and PIN-961 final language reconciliation remain open.

## Evidence

`eng/Test-CompilerInspections.ps1 -Record` builds and executes all four real C#/VB consumers with their actual5.9/5.12 package closures. Saved sample records retain commands, resolved dependencies, SDK inventory, input hashes and observed classification/failure outputs. The helper also compiles deliberate missing-API probes against5.9. The ordinary tool is built/packed once and that exact installed artifact is inspected with real stdio list/get from an unrelated directory, checking this ID/title/goal, complete canonical Markdown, target11/edition11, applicability and current evidence; target10 returns `no_applicable_edition`. Unusable proxies establish proxy-independent retrieval, not network isolation. No source-only or mock result is counted as execution.
