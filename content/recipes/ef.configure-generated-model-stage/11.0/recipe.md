# Choose when to generate compiled models and queries

## Goal

Choose which pipeline step generates a model and which precompiles a fixed query. Fern's seed-packet inventory places model scaffolding at build and query precompilation at publish. Both release comparisons describe the same model and query.

**Experimental:** Microsoft documents MSBuild integration, precompilation and NativeAOT as experimental and unsuitable for production. These instructions follow that documentation. Actual RC generation failed; generated-artifact proof is pending. There is no promise that GA fixes these failures.

## Older approach

EF10 enables its integration with EFOptimizeContext=true, alongside the two stage selectors. Keeping the flag on this release is valid. It enables integration; it does not make arbitrary LINQ queries precompilable.

{{snippet:n10-ferngeneration-csproj}}

{{snippet:n10-program-cs}}

## Newer approach

EF11 removes EFOptimizeContext. The stage selectors work without it. Remove the property entirely: setting it to false still leaves a nonempty property rejected by the pinned task. Reference Design directly alongside Tasks because EF11 Tasks no longer supplies Design transitively.

{{snippet:n11-ferngeneration-csproj}}

{{snippet:n11-program-cs}}

## Migration

1. Inventory the existing generated files and select stages deliberately. Each selector accepts build, publish or none and defaults to publish. PublishAot also activates publication generation by default.
2. Remove EFOptimizeContext for EF11. Set EFScaffoldModelStage for model code and EFPrecompileQueriesStage for query code. This sample selects build/publish. Do not select model generation at publish with query precompilation at build: query code needs its model first.
3. Keep the context self-contained with IDesignTimeDbContextFactory. Select the publish RuntimeIdentifier; this example uses win-x64. Enable the generated interceptor namespace in the compiler.
4. Restore in the sample directory, then run dotnet build -c Release and dotnet publish -c Release. Inspect obj/Release/net11.0 and RID subdirectories for EFGeneratedSources.Build.txt and EFGeneratedSources.Publish.txt manifests and every referenced model/query source. Confirm that model code appears at build and query interceptors at publish. Ordinary compilation with generation disabled does not satisfy this check.
5. Compile the generated output and execute the published app to verify its seed-packet result. Qualify PublishAot separately using the platform compiler and supported query shapes. These generation, generated-consumer and NativeAOT checks remain pending here.

## Constraints

Reference Tasks in every participating project; it is not transitive by default. The context must configure itself: selecting a different startup project would create an inverse build dependency. The project must compile before model generation, so partial-method customization of that model does not fit this integration. MSBuild emits NativeAOT-oriented model code; use the documented dotnet-ef CLI alternative for a conventional compiled model without NativeAOT.

The query is fixed and deliberately small. Runtime-composed queries and different providers need separate qualification. No universal precompilation, performance gain or production readiness is established. Console source compilation needs no browser, renderer or optional workload. Native publication has additional platform/toolchain requirements.

## Evidence

SDK10.0.403/EF10.0.13 and SDK11 RC3/EF11 RC2 sources compile with both stage selectors explicitly overridden to none in verification.json. This is ordinary compilation only. Clean EF11 RC2 and isolated RC3 generation attempts failed because the output DLL did not yet exist. A later RC2 publish probe emitted query interceptors but failed generated-source compilation; that is not successful generation or execution evidence. No generated model/query or NativeAOT route is certified.

Pingu authorized documentation-based delivery on 4 November 2026 after official-source confirmation. Pending proof remains visible and retains the existing stable-release packaging gate. Refresh actual build/publish artifacts and exact dependency/toolchain evidence on a working release before upgrading these claims.

## Complete project configuration

Keep each release in a separate directory with its SDK pin, direct package versions and restored canonical lock.

### net10.0 Directory.Build.props

{{snippet:n10-directory-build-props}}

### net10.0 Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### net10.0 Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### net10.0 global.json

{{snippet:n10-global-json}}

### net11.0 Directory.Build.props

{{snippet:n11-directory-build-props}}

### net11.0 Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### net11.0 Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### net11.0 global.json

{{snippet:n11-global-json}}

## References

- [Official MSBuild integration](https://learn.microsoft.com/en-us/ef/core/cli/msbuild)
- [Official EF11 breaking changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-11.0/breaking-changes)
