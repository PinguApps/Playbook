# Choose when to generate compiled models and queries

## Goal

Choose which pipeline step generates a model and which precompiles a fixed query. Fern's seed-packet inventory prepares a model with the CLI before publication and precompiles its query at publish. Both release comparisons describe the same model and query.

**Experimental:** Microsoft documents MSBuild integration, precompilation and NativeAOT as experimental and unsuitable for production. These instructions follow that documentation. Automatic GA model-build generation fails on the recorded clean route. The bounded CLI model plus Tasks publish-query alternative below is executed successfully.

## Older approach

EF10 enables its integration with EFOptimizeContext=true, alongside the two stage selectors. Keeping the flag on this release is valid. It enables integration; it does not make arbitrary LINQ queries precompilable.

{{snippet:n10-ferngeneration-csproj}}

{{snippet:n10-program-cs}}

## Newer approach

EF11 removes EFOptimizeContext. The stage selectors work without it. Remove the property entirely: setting it to false still leaves a nonempty property rejected by the pinned task. Reference Design directly alongside Tasks because EF11 Tasks no longer supplies Design transitively.

{{snippet:n11-ferngeneration-csproj}}

{{snippet:n11-program-cs}}

## Migration

The clean SDK11.0.100/Tasks11.0.0 build with EFScaffoldModelStage=build fails:
generation tries to load FernGeneration.dll before that output exists. A bootstrap
build followed by an incremental build can skip compilation and creates no model
manifest. That is not successful model generation. Automatic model-build success
is withdrawn for this route.

Use explicit CLI model generation during build preparation instead:

1. Restore matching Sqlite, Tasks and directly referenced Design11.0.0 packages.
   Install dotnet-ef11.0.0 in a local tool directory.
2. Keep EFScaffoldModelStage=none and EFPrecompileQueriesStage=publish.
   Build with `-p:EFPrecompileQueriesStage=none` to produce the design-time assembly.
3. Run the local dotnet-ef executable with `dbcontext optimize --configuration Release
   --no-build --output-dir CompiledModel --namespace FernGeneration.CompiledModel`.
   Inspect its four generated C# model files and assembly discovery attribute.
4. Publish Release. The normal compiler compiles the model files; Tasks generates
   query interceptors listed in obj/Release/net11.0/FernGeneration.EFGeneratedSources.Publish.txt.
   Verify every manifest entry exists and inspect the interceptor source.
5. Execute bin/Release/net11.0/win-x64/publish/FernGeneration.dll with
   `--require-model`. It refuses a noncompiled model and verifies the stored packet.
   Rerun optimize whenever the model changes; do not retain stale generated files.

`eng/Test-FernGeneration.ps1` executes this exact bounded sequence. CLI model
files are explicitly inspected rather than described as a Tasks Build manifest.
The test's setup compilation is not its generation proof: real generated C#,
Tasks Publish manifest and execution using the compiled model supply that proof.

## Constraints

The stage selectors accept build, publish or none and default to publish. Model
at publish with query precompilation at build is invalid. EF11 rejects any nonempty
EFOptimizeContext, including false; EF10 retains that enabling property. EF11 Tasks
does not transitively supply Design. The direct GA package graph and local CLI
version are required; exact dependencies appear in the sample lock/receipt.

## Evidence

The canonical EF10 comparison retains ordinary compilation with both generation
stages disabled. Its receipt is not generated-artifact proof. The EF11 route above
executes actual GA compiled-model/query artifacts on Windows x64 with bundled
SQLite. The original automatic route's failure remains visible. No NativeAOT,
other-provider, arbitrary-query or other-platform qualification is claimed.
MSBuild/precompilation remain experimental and unsuitable for production.

The resolved generation exception is removed; other preview exceptions, the eight
foundations and PIN-967's global stable-release gate remain unchanged.

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
