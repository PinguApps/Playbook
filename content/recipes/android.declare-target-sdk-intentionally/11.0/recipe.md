# Set Android target SDK independently of device support

## Goal

Use this when an existing manifest contains only part of `<uses-sdk>`. Minimum
SDK controls which devices may install an app; target SDK selects Android's
compatibility behavior. Raising one does not make them interchangeable.

The older workload could use the minimum SDK as the effective target when a
partial `<uses-sdk>` omitted `android:targetSdkVersion`. The new workload writes
`TargetSdkVersion` instead. An unchanged partial manifest can therefore change
runtime behavior after rebuilding. This is a migration trigger, not a reason to
keep an obsolete store target indefinitely.

CoastalProbe states both values deliberately: .NET10 uses minimum24/target36;
.NET11 uses minimum24/target37 with API37.2 bindings. Keep the manifest's explicit
target aligned with the project setting. Alternatively omit the whole uses-sdk
element and let the workload generate it from the project, then inspect the
merged result. Never infer target SDK from the TFM or minimum alone.

Build `CoastalProbe.csproj -c Release` using the pinned SDK and matching Android
workload, JDK and Android SDK. Confirm the generated manifest has the intended
`minSdkVersion` and `targetSdkVersion`. Then exercise permissions, background
work and other target-gated behavior on supported Android versions before rollout.
Those tests depend on the actual app; a manifest assertion cannot establish them.

## Evidence

The .NET10 Release build produces minimum24/target36 and an ARM64 APK. The .NET11
native build reports NETSDK1147 and its target37 generation is **not-tested**.
Device behavior for both samples is not-tested under PIN-699. RC2 source/artifact
review supports the changed partial-manifest rule; no GA proof is claimed.
This is native Android guidance, with no provider or web rendering requirement.
The .NET10 example is comparison-only, not another catalogue edition.

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
