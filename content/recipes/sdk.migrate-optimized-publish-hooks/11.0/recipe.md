# Preserve generated files during optimized native publishing

## Goal

Estuary's native cargo checker reads a generated berth manifest beside its executable.
A hook that runs after `Build` used to create that file. Preserve the file when
Native AOT publishing takes a shorter path through the build targets.

## Requirements and actual artifacts

This edition serves target11 only. The net10 comparison is retained here, without
adding a ninth historical foundation or advertising a down-level SDK11 route.

| Layer | Older comparison | Current example |
| --- | --- | --- |
| SDK | 10.0.402 | 11.0.100-rc.2.26504.105 |
| Target / reference and runtime assets | net10.0 / 10.0.13 | net11.0 / 11.0.0-rc.2.26504.105 |
| Compiler / language | 5.9.0-1.26505.111 / C#14 | 5.12.0-1.26504.105 / C#15 |
| Native compiler packages | Microsoft.DotNet.ILCompiler and runtime.win-x64.Microsoft.DotNet.ILCompiler 10.0.13 | Same package IDs, 11.0.0-rc.2.26504.105 |
| Native linker / libraries | MSVC link14.51.36260.0; MSVC14.51.36231 library directory; Windows SDK10.0.26100.0 x64 UM/UCRT | Same measured host toolchain |

Use a complete SDK and its matching Native AOT package closure, plus the supported
Windows x64 native tools. Exact compiler/ref/runtime/Publish.targets hashes are
recorded separately from these prerequisites. There is no external provider,
optional workload, UI or rendering dependency. These are RC2 observations, not
actual-GA, arbitrary SDK/plugin compatibility or performance qualification.

## Older approach

Save this project as `samples/net10.0/EstuaryNative/EstuaryNative.csproj`, and the
program below as `Program.cs` in that directory. Select SDK10.0.402 using
`global.json` with `rollForward: "disable"`.

{{snippet:older}}

{{snippet:program}}

The default legacy hook runs after `Build`. The later inclusion target adds an
existing generated file to the publish list. Missing output deliberately remains
observable: the native program returns23, rather than manufacturing a manifest.
This is a sidecar used at execution time, not a generated assembly or embedded resource.

## Newer approach

Save this current project at `samples/net11.0/EstuaryNative/EstuaryNative.csproj`.
Keep the stated relative link to the older program. Select the exact RC2 SDK in
that directory's `global.json`, again with roll-forward disabled.

{{snippet:newer}}

With `PublishAot=true`, the measured SDK11 defaults `UseOptimizedPublish=true`.
Its publish path invokes compilation without the full `Build` target, so an
`AfterTargets="Build"` hook need not run. An unchanged legacy project can publish a
real native executable which fails because its manifest is absent.

The migrated hook runs before `ComputeFilesToPublish`. It writes to the intermediate
directory, then the dependent inclusion target adds that file before the SDK
collects publish output. Keep the generation and consumption ordering explicit.
Use public target hooks for application customization. Do not copy or replace
private `_Publish*` targets or an SDK-internal extension list.

## Migration

1. Identify what each custom build hook creates and which later stage consumes it.
   Use a fresh intermediate directory: an old generated file can conceal a skipped hook.
2. For this execution-time sidecar, move generation from `AfterTargets="Build"` to
   `BeforeTargets="ComputeFilesToPublish"`, retaining the explicit dependency before
   inclusion. A source generator or resource needed by compilation needs an earlier
   compilation/resource hook; moving every hook here is unsafe.
3. Restore the exact package locks and publish with the absolute linker path and
   all three native library directories supplied as the shown project properties.
   `IlcUseEnvironmentalTools=true` avoids a toolchain setup script changing PATH.
4. Execute the published native program with `copper-lanterns`. Expect
   `ESTUARY_NATIVE: copper-lanterns; AOT=True`; confirm the PE has no CLR header.
   Wrong cargo returns24; a missing sidecar returns23.
5. Inspect a binlog using the matching SDK's reader. The SDK11 legacy default/true
   controls omit full Build and the legacy hook; `UseOptimizedPublish=false`
   restores them. The migrated publish hook runs on the measured SDK10 and11 paths.
6. Repeat publication with `--no-build -p:EstuaryCargo=amber-buoys`. The publish hook
   must refresh the sidecar and the existing native executable must consume it.
   Repeat again without a change. `--no-build` requires previously compiled inputs;
   it does not promise a fresh application build or compilation-time regeneration.
7. If a third-party hook cannot yet be migrated, `-p:UseOptimizedPublish=false` is
   a temporary full-Build alternative on SDK11. Track the actual hook fix and
   recheck it before restoring optimized publishing. This does not make missing
   inputs safe under `--no-build`.

## Constraints

This recipe places an execution-time sidecar before publish-file collection. It
does not prescribe a hook for generated source, embedded resources or arbitrary
third-party plugins. NoBuild needs an existing compiled application; false is a
temporary full-Build alternative, rather than a general compatibility guarantee.
Only the measured Windows x64 native toolchain is qualified.

## Evidence

`eng/Test-EstuaryWindows.ps1 -Record` performs the real paired restores, ILC/link
publishes, native PE inspection, execution and matching binlog replay. Separate
fresh directories cover old/current default, explicit false and explicit true,
then the migrated default, changed sidecar under NoBuild, identical repeat and
missing/wrong sidecar failures. Logs and raw binlogs stay in ignored artifacts;
sample evidence records the exact inputs, commands, dependency closures and outcomes.
No throughput/startup improvement, other RID/OS, general plugin compatibility,
cross-compilation substitute or GA support is claimed. No recurring native CI
matrix is added. The eight foundations and PIN-959/PIN-963/Human gates remain open.

## Sources

- [Pinned SDK publish target implementation](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/sdk/src/Tasks/Microsoft.NET.Build.Tasks/targets/Microsoft.NET.Publish.targets)
- [Extend an MSBuild process with BeforeTargets/AfterTargets](https://learn.microsoft.com/en-us/visualstudio/msbuild/how-to-extend-the-visual-studio-build-process?view=visualstudio)
- [Native AOT prerequisites](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
