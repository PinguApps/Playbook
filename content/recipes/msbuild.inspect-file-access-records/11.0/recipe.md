# Read experimental build file-access records correctly

## Goal

Observe real file accesses in a disposable Windows x64 build, preserving build context, OS errors, enumeration patterns and opened attributes. This is an **experimental observation plugin**, not a successful-build cache implementation. The reporting host is command-line Visual Studio x64 `MSBuild.exe`, not `dotnet msbuild` or the IDE.

## Older approach

The original Cwm Seed Bank decoder preserves paths, operations, requested access, errors and requested flags. It remains useful when richer fields are unavailable:

{{snippet:older}}

## Newer approach

Keep enumeration patterns and opened-object attributes separate from requested flags. A missing pattern is absent, not `*`. An unsuccessful access is not proof that contents were read or written.

{{snippet:newer}}

When saving **either** decoder as `AccessDecoder.cs`, prepend `using Microsoft.Build.Experimental.FileAccess;` and then `namespace CwmSeedBank.Build;`. Those canonical file-header lines are outside the extracted decoder regions and are required by the complete fixture below. Keep the older file under `samples/net10.0/BuildAccessObserver/` and the newer file under `samples/net11.0/BuildAccessObserver/`.

The eleven-argument constructor remains present; its new fields default to null and zero. The thirteen-argument overload appends enumeration pattern and opened attributes. Constructor fixtures establish defaults only; real callbacks establish capture. Do not parse private wire packets or log process arguments.

Use `Microsoft.Build.ProjectCache` for plugin, context and result types. Do not also import obsolete `Microsoft.Build.Experimental.ProjectCache` aliases. File-access types retain the separate `Microsoft.Build.Experimental.FileAccess` namespace.

## Complete observer

Save this as `samples/net11.0/BuildAccessObserver/Observer.cs` beside the newer decoder. The SDK11 library compiles it against SDK-local managed assemblies. That compilation alone does not deploy a plugin into the Framework reporting host.

{{snippet:observer}}

The key hashes length-prefixed project path, ordered global properties and targets without logging property values. Counts and lengths keep property values containing newlines or equals signs unambiguous. Capture is limited to the disposable input root and 10,000 records per context. Finalization drains only its own context. The plugin always returns a cache miss and never publishes reusable cache state.

On this host, active cancellation does **not** call `HandleProjectFinishedAsync`. End-build drains remaining observations as **abandoned**. A killed host may omit end-build too; an empty stream or missing receipt never establishes success.

## Host composition and reproduction

Measured Windows host: Visual Studio Community x64 MSBuild **18.11.0.47511**, product **18.11.0-1.26475.11+3cde5c8ec22ccb6ca106fe1554412cace0979413**, running on .NET Framework. Matching Microsoft.Build/Framework assemblies, BuildXL managed instrumentation and `Bin/x64` native assets are separate from the SDK compiler/runtime. The harness records selected file versions and SHA256 hashes. Library compilation uses SDK **11.0.100**, C#15; the older comparison retains SDK **10.0.403**, C#14.

The real reporting plugin is compiled by SDK11 as a separate **net48** consumer against installed Framework4.8 references and the matching Visual Studio assemblies. Visual Studio's `Bin/Roslyn/csc.exe` compiles the small probe/IO/cancellation companions. No installation, PATH or persistent environment change is required.

The item-discovered composition requires **both `/graph` and `/reportFileAccesses`** on this host. Reporting alone runs the fixture without activating the observer. Graph without reporting activates the plugin but emits no access records. These measured controls do not generalize to every programmatic descriptor route. Use `/nr:false` to disable node reuse.

Save these complete companion files under `samples/net11.0/BuildAccessObserver/Host/`:

`BuildAccessObserverHost.csproj` links the canonical decoder and observer. Its actual net48 DLL is loaded by the real reporting host; the net11 library is not loaded into Framework:

{{snippet:host-project}}

`IsExternalInit.cs` supports records on Framework without changing the runtime contract:

{{snippet:host-shim}}

`global.json` pins the compiler SDK for this separate consumer:

{{snippet:host-global}}

`SeedIo.cs` performs actual successful/failed reads and writes plus enumeration on disposable original inputs:

{{snippet:seed-io}}

`SeedBuild.proj` discovers the plugin and runs the input consumer:

{{snippet:seed-build}}

`ObserverProbe.cs` composes the canonical observer and compares shared fields from both decoders on every live callback. Only the older decoder's namespace is renamed. This does not claim execution of an older native host:

{{snippet:observer-probe}}

`CancelBuild.cs` calls the real MSBuild graph API's `CancelAllSubmissions` only after the IO consumer writes its active marker:

{{snippet:cancel-build}}

Save this harness as `samples/net11.0/BuildAccessObserver/Test-Observer.ps1` and invoke it directly. The checkout's `eng/Test-BuildAccessObserver.ps1` is a thin wrapper. Keep both decoder files at their shown paths. It supplies the shim and compiles the Framework consumers. The cancellation driver's configuration retains the installed host's binding redirects with absolute codebase paths. Disposable output goes under `artifacts/build-access/`.

{{snippet:host-check}}

## Migration

1. Identify the reporting host and matching managed/native assets. Separate SDK library compilation, Framework plugin compilation and native execution evidence.
2. Retain the older decoder when only common fields exist. Decode richer fields only when delivered by the host; do not manufacture observations.
3. Save the complete sources and harness in the shown layout. Override `-MSBuild` or `-FrameworkReferences` only for a separately qualified matching host.
4. Compare reporting-only, capture-disabled and enabled graph builds. Check real success/errors, `*.seed` enumeration, absent patterns and requested/opened attribute differences.
5. Verify first/second projects retain distinct context keys. Do not infer unmatched files were read merely because a directory was enumerated.
6. Exercise failure and active cancellation. Keep abandoned/failed state separate from success. A future cache implementation needs its own validity and publication policy.
7. Refresh affected evidence when sources, compiler, host or instrumentation change. Preserve historical RC records and unrelated native exclusions. PIN-967 owns final candidate readiness.

## Constraints

Use the matching Windows x64 Visual Studio command-line host and complete native instrumentation. The observer is bounded, local and observation-only. Preserve errors/context and never infer successful completion from an empty stream. Cancellation may omit per-project finalization; end-build abandonment is separate from a reusable cache result.

## Evidence and limitations

The complete net11 observer compiles with SDK11.0.100. The real Framework composition captures successful reads/writes, failed IO with errors2/5, a present `*.seed` pattern, absent patterns and opened directory attributes. Separate projects and the same project with distinct delimiter-bearing global properties retain distinct contexts. The latter regression fails with the previous delimiter encoding and passes with length prefixes. Reporting-only and capture-disabled controls pass. A failed build finalizes as failed; active cancellation returns build failure and end-build abandons its partial context. Shared-field decoding and constructor defaults are checked separately.

These results qualify the measured Windows x64 host only. SDK10 remains a decoder compilation baseline, not older native-host execution. No cross-version wire compatibility, production cache, performance improvement, IDE, x86, ARM64 or other-OS reporting is claimed. Experimental status, eight .NET10 foundations and the separate PIN-967 release gate remain unchanged.

## Official references

- [Project-cache design and host restrictions](https://github.com/dotnet/msbuild/blob/main/documentation/specs/project-cache.md)
- [Measured host's plugin contract](https://github.com/dotnet/msbuild/blob/3cde5c8ec22ccb6ca106fe1554412cace0979413/src/Build/BackEnd/Components/ProjectCache/ProjectCachePluginBase.cs)
- [Measured host's build context](https://github.com/dotnet/msbuild/blob/3cde5c8ec22ccb6ca106fe1554412cace0979413/src/Build/BackEnd/Components/ProjectCache/FileAccessContext.cs)
- [Historical current FileAccessData](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/msbuild/src/Build/FileAccess/FileAccessData.cs)
- [Historical previous FileAccessData](https://github.com/dotnet/dotnet/blob/a8dd0577c5c13f43a2f57fe33b4ff11b90f6371e/src/msbuild/src/Build/FileAccess/FileAccessData.cs)
