# Preserve required embedded framework headers and modules

## Goal

Keep framework headers and modules only when the distributed Apple app needs them, before final code signing.

## Newer approach

Oystercatcher distributes a survey integration whose consumer explicitly requires framework headers and module declarations to remain in the app bundle. Set StripFrameworkHeaders=false in that app's project only for this documented packaging requirement. It preserves Headers, PrivateHeaders and Modules in embedded frameworks before signing. The default is true; retain it for normal app distribution.

This switch does not preserve debug symbols, disable binary symbol stripping or configure mergeable-library metadata. It does not create a framework, repair a missing module map or make a static framework suitable for embedding. NativeReference selection and the vendor's redistribution licence remain separate concerns. Audit retained private headers and package size before distribution.

{{snippet:net11-oystercatcherframework-csproj}}

## Older approach

The .NET10 comparison leaves the supported older workload's packaging behavior unchanged. When an older workload removes a required directory, use a carefully scoped pre-sign packaging target or obtain a compatible framework package; do not patch a signed bundle afterward. No generic older-workload stripping opt-out is claimed here. The samples are minimal configuration libraries, not fabricated usable native frameworks.

## Migration

1. Confirm the recipient needs these directories at runtime/distribution, rather than only when compiling a binding.
2. Set StripFrameworkHeaders=false on the owning app. Do not disable stripping for all products by default.
3. Publish with an independently authorized signing identity and matching Xcode/workload.
4. Compare Headers, PrivateHeaders and Modules with default stripping, including versioned framework layouts. Check Info.plist and the binary remain intact.
5. Verify signatures only after final packaging. The Windows test exercises the real strip task on owned directory fixtures and compares an untouched preservation control. It does not produce or sign a Mach-O framework or app.

## Canonical samples

### net10-directory-build-props

{{snippet:net10-directory-build-props}}

### net10-marker-cs

{{snippet:net10-marker-cs}}

### net10-oystercatcherframework-csproj

{{snippet:net10-oystercatcherframework-csproj}}

### net11-directory-build-props

{{snippet:net11-directory-build-props}}

### net11-marker-cs

{{snippet:net11-marker-cs}}

### net11-oystercatcherframework-csproj

{{snippet:net11-oystercatcherframework-csproj}}

## Constraints

Applicability lists all runtime, SDK, language, workload, artifact, provider, platform, rendering and stability constraints. These are .NET11 Apple workload features, with stable API labels and RC preparation evidence; neither actual GA nor a complete RC3 Apple composition was tested. No preview language syntax or experimental opt-in is used. .NET10 comparison compilation is separate from current task/reference checks.

## Evidence

Run eng/Test-AppleAuthoring.ps1 for pinned-source/artifact checks, real typed plist and stripping task output, actual binding generation and generated-source/consumer reference compilation. The source manifest records exact SHA-256 identities. SDK11 RC3 runs RC2 Apple tasks and bgen on Windows; that does not establish native linking, signing or runtime support. Source-review receipts retain sample/input freshness; native exclusions apply to the named examples under PIN-699. No Human action is pending. Packed stdio list/get establishes offline guidance delivery only. The eight .NET10 foundations and the comprehensive/GA gates remain unchanged.
