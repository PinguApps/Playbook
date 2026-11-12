# Preserve Java entry points when migrating an owned peer

## Goal

Use this when rebuilding a library with Java-callable managed types. CoastalProbe
owns `ProbeBindings`, whose peer exports a reading count. The app references that
library so wrapper generation must discover peers across an assembly boundary.
This is an owned managed peer library, not a replacement for binding a third-party JAR.

## Audit the boundary

- Select the trimmable type map explicitly for the current application. It emits
  managed mapping assemblies and Java wrappers from a static scan; trimming can
  remove unused entries. Do not depend on runtime reflection to discover arbitrary
  new peer types, especially for NativeAOT. Keep peers statically reachable and
  verify the exported members in generated Java and linked output.
- Derive owned peers from `Java.Lang.Object` rather than implementing only a raw
  `IJavaObject.Handle` contract. Custom peer implementations must satisfy the
  selected runtime's Java.Interop peer lifetime and registration contracts;
  retaining an interface implementation is not proof that generated mapping works.
- Preserve Java names consumed by existing callers. The sample gives the peer an
  explicit `Register` name. Historical Preview5 notes describe Crc64 defaults
  with LowercaseCrc64 compatibility; the pinned current property reference only
  documents LowercaseCrc64. This sample states that compatibility choice explicitly
  instead of inferring a default from release prose. Inspect all generated names
  before upgrading an externally consumed library.
- `[Java.Interop.Export("readingCount")]` exposes an ordinary public instance
  callback. The older wrapper contains that exact Java method. Do not extrapolate
  this simple signature to every Export/ExportField, generic or custom-peer shape.
- Replace overrides of `JavaFinalize` with deliberate lifetime handling.
  The actual GA reference marks it obsolete. `Dispose(bool)` is suitable for
  explicit managed cleanup; dispose managed objects only when `disposing` is true
  and call the base implementation. A C# finalizer is for separately owned
  unmanaged resources, not a place to access arbitrary managed state. Existing
  deterministic disposal was already valid on .NET10 and remains valid here.

Build the app and its referenced library with the pinned SDK and compatible native
workload. Inspect generated Java for `org.coastal.probe.ProbePeer.readingCount`,
mapping assemblies, constructor accessibility and actual trim output. Invoke the
callback from Java on a device to prove dispatch and lifecycle; compilation alone
does not establish it. This recipe requires no account or web rendering mode.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The .NET10 build uses real Android36.1.118 packs. Current GA reference metadata
and source are inspected independently; the GA-pinned .NET11 build fails
NETSDK1147. Its typemap generation and both editions' device callbacks/lifetimes
are **not-tested** under PIN-699. Experimental Android NativeAOT is not executed.
No older-target route or GA verification is claimed.

## Older approach

The .NET10 comparison uses Mono and the existing generated Java wrappers. Explicit Java names and deterministic disposal are already valid there. Its current app intentionally targets API36; it adds no catalogue edition or older-target applicability route.

## Migration

### Rebuild source bindings that create JNI delegates

Direct source calls to `Android.Runtime.JNINativeWrapper.CreateDelegate`
produce CS0619 in the GA reference contract. Remove hand-authored delegate
wrappers and regenerate bindings using the current .NET for Android binding
generator. Keep Java names and signatures stable, and inspect the generated JNI
callbacks. The canonical `ProbePeer` uses `Java.Interop.Export` rather than
calling `CreateDelegate` itself.

The public implementation remains for already compiled bindings. That binary
compatibility does not permit a new source call, and suppressing an ordinary
obsolete warning does not repair this error-level obsolete contract. Rebuilding
with the current generator is the migration; runtime callback and device checks
remain separately not-tested under PIN-699.

{{snippet:rejected-callback}}

The original API description below is passed to the actual GA generator; its generated callbacks compile against the GA reference pack.

{{snippet:callback-api}}

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
