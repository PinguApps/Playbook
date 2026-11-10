# Choose when an owned JNI library loads

## Goal

Use this only when a library's JNI initialization must wait for application setup.
The normal startup preload remains useful. Exemption transfers responsibility
for calling the load operation before the first JNI invocation to the app.

CoastalProbe's optional props file describes an owned ARM64 library named
`libcoastalprobe.so`. First build that real library with its matching NDK/ABI and
place it at the declared path; no binary is supplied or verified by this sample.
Import `NativeLoading.props` in the app project only after that prerequisite is
met. Keep the library packaged with `AndroidNativeLibrary`; exempt its filename
using `AndroidNativeLibraryNoJniPreload`. This does not exclude it from the APK.

After the library's prerequisite setup finishes, call `ProbeLoader.Load()` before
calling its native methods. `LoadLibrary` takes `coastalprobe`, without `lib` or
`.so`. Handle initialization failure through the app's startup error path; do
not continue invoking JNI after a failed load. Coordinate concurrent callers so
none can race ahead of initialization.

`AndroidIgnoreAllJniPreload=true` is the broader alternative when every owned JNI
library requires explicit ordering. Prefer the named exemption when only one
does. Neither setting suppresses mandatory framework-library loads. Managed
assembly preload settings are separate and do not control JNI library loading.

The previous sample uses ordinary startup preloading and needs no exemption.
This is a deliberate configuration change rather than a claim that every API
shown was introduced in .NET11. Rebuild and inspect the APK, then observe JNI
initialization and first use on the real Android device. A property evaluation
or an unloaded binary cannot establish native load order.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The optional native library, its packaging and JNI execution are **not-tested**
under PIN-699: the matching native toolchain/device is unavailable. The ordinary
.NET10 app build excludes this optional props file. The .NET11 build fails
NETSDK1147. Immutable build-item/property sources support the configuration
contracts; no native execution, GA or NativeAOT proof is claimed. No provider,
account or Blazor rendering mode is required.

## Older approach

The .NET10 comparison uses Mono and the existing generated Java wrappers. Explicit Java names and deterministic disposal are already valid there. Its current app intentionally targets API36; it adds no catalogue edition or older-target applicability route.

## Migration

1. Recreate the relevant canonical files in an isolated project directory, retaining the SDK pin and local build/package boundaries.
2. Apply the choices described above to the owned application/library; inspect generated manifest, wrappers and packaged ABI rather than relying on project text alone.
3. Build with the matching workload, SDK and JDK; run the focused device check before making a native behavior claim. The unavailable checks below remain excluded.

## Constraints

Requires Android API24+, a matching .NET11 Android workload, Android SDK and JDK21 or25. Gradle through JDK25 requires9.1+. RC3 SDK and reviewed RC2 platform assets are distinct inputs, not a tested composition. No external provider or web render mode. NativeAOT is experimental and unexecuted. Device execution and current native compilation remain not-tested under PIN-699; source review does not establish GA behavior.

## Newer approach

The complete current files are canonical inputs; retain their isolation files. The older files below remain the .NET10 comparison.

### samples/net10.0/CoastalProbe/CoastalProbe.csproj

{{snippet:net10-coastalprobe-csproj}}

### samples/net10.0/CoastalProbe/MainActivity.cs

{{snippet:net10-mainactivity-cs}}

### samples/net10.0/CoastalProbe/ProbePeer.cs

{{snippet:net10-probepeer-cs}}

### samples/net10.0/CoastalProbe/Properties/AndroidManifest.xml

{{snippet:net10-properties-androidmanifest-xml}}

### samples/net10.0/CoastalProbe/global.json

{{snippet:net10-global-json}}

### samples/net11.0/CoastalProbe/CoastalProbe.csproj

{{snippet:net11-coastalprobe-csproj}}

### samples/net11.0/CoastalProbe/MainActivity.cs

{{snippet:net11-mainactivity-cs}}

### samples/net11.0/CoastalProbe/ProbePeer.cs

{{snippet:net11-probepeer-cs}}

### samples/net11.0/CoastalProbe/Properties/AndroidManifest.xml

{{snippet:net11-properties-androidmanifest-xml}}

### samples/net11.0/CoastalProbe/global.json

{{snippet:net11-global-json}}

### samples/net11.0/CoastalProbe/NativeLoading.props

{{snippet:net11-nativeloading-props}}

### samples/net11.0/CoastalProbe/ProbeLoader.cs

{{snippet:net11-probeloader-cs}}

### samples/net10.0/CoastalProbe/Bindings/ProbeBindings.csproj

{{snippet:net10-bindings-probebindings-csproj}}

### samples/net10.0/CoastalProbe/Directory.Build.props

{{snippet:net10-directory-build-props}}

### samples/net10.0/CoastalProbe/Directory.Build.targets

{{snippet:net10-directory-build-targets}}

### samples/net10.0/CoastalProbe/Directory.Packages.props

{{snippet:net10-directory-packages-props}}

### samples/net11.0/CoastalProbe/Bindings/ProbeBindings.csproj

{{snippet:net11-bindings-probebindings-csproj}}

### samples/net11.0/CoastalProbe/Directory.Build.props

{{snippet:net11-directory-build-props}}

### samples/net11.0/CoastalProbe/Directory.Build.targets

{{snippet:net11-directory-build-targets}}

### samples/net11.0/CoastalProbe/Directory.Packages.props

{{snippet:net11-directory-packages-props}}
