# Choose Android runtime, device floor and build tools

## Goal

Use this when upgrading an Android application whose supported devices, ABI or
compilation settings were chosen for Mono. CoastalProbe displays the readiness of
a fictional coastal sensor. Its .NET 10 Mono build remains a valid comparison;
the .NET 11 route uses CoreCLR and explicitly trims its ReadyToRun output.

## Make the choices together

1. Set `SupportedOSPlatformVersion` to at least `24.0`. This is the minimum
   device OS, not the API binding version or target SDK behavior level.
2. Replace `android-x86` with a supported deployment choice. This sample selects
   `android-arm64`; `android-x64` is a separate emulator/device choice. Do not
   assume every device supported by the previous binary can run the replacement.
3. Ordinary .NET 11 Android apps use CoreCLR. Remove Mono-specific AOT settings
   such as `RunAOTCompilation` and `AndroidAotMode` when moving to this route.
   `UseMonoRuntime=true` is rejected with NETSDK1242, not a compatibility escape.
4. Keep `PublishTrimmed=true` when authoring `PublishReadyToRun=true`. Otherwise
   the Android workload disables ReadyToRun and composite ReadyToRun with XA0119.
   Test reflection, callbacks and dependency preservation after trimming. An
   untrimmed build is valid when R2R is not required. R2R trades package size for
   startup work; it does not establish a measured performance improvement here.
5. Use JDK 21 or 25 with the current workload. Gradle invoked through JDK 25
   needs Gradle 9.1+. Gradle is not required merely because this is an Android
   project. The measured older build used JDK21.0.8 and Android SDK36.

Android NativeAOT (`PublishAot=true`) is an **experimental alternative**. It does
not use ReadyToRun and needs its own trimming, interop and publication checks.
This recipe does not verify that alternative. MAUI full-R2R profile selection is
separate from this plain Android app; do not add MAUI properties to this project.

## Reproduce the samples

Create the files below in separate directories. Their local build/package files
isolate them from a parent repository; retain the SDK pins. With the matching
Android workload, Android SDK and JDK, run `dotnet build CoastalProbe.csproj -c Release`.
Supply `-p:JavaSdkDirectory=<JDK root>` and `-p:AndroidSdkDirectory=<SDK root>` if
discovery does not select the intended installations. Inspect the merged manifest
and generated Java before deploying. SDK selection alone does not install workloads.

The .NET11 pin is GA; exact reviewed platform/reference assets are Android
37.2.29 GA. That provenance is not a tested GA workload composition.
Obtain a compatible workload before building; do not mix pack directories manually.
There is no provider/account or Blazor rendering prerequisite.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The .NET10 Release Android build and generated Java are authoring evidence.
The .NET11 native build reports NETSDK1147; compilation, APK execution, JNI
ordering and device behavior are **not-tested** under PIN-699. Official immutable
source and actual reference artifacts support the current contracts. No GA,
NativeAOT execution, startup benchmark or additional hardware proof is claimed.

## Older approach

The .NET10 comparison uses Mono and the existing generated Java wrappers. Explicit Java names and deterministic disposal are already valid there. Its current app intentionally targets API36; it adds no catalogue edition or older-target applicability route.

## Migration

1. Recreate the relevant canonical files in an isolated project directory, retaining the SDK pin and local build/package boundaries.
2. Apply the choices described above to the owned application/library; inspect generated manifest, wrappers and packaged ABI rather than relying on project text alone.
3. Build with the matching workload, SDK and JDK; run the focused device check before making a native behavior claim. The unavailable checks below remain excluded.

## Constraints

Requires Android API24+, a matching .NET11 Android workload, Android SDK and JDK21 or25. Gradle through JDK25 requires9.1+. GA SDK selection, stable platform artifacts and historical implementation sources are distinct inputs, not a tested native composition. No external provider or web render mode. NativeAOT is experimental and unexecuted. Device execution and current native compilation remain not-tested under PIN-699; source review does not establish GA behavior.

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
