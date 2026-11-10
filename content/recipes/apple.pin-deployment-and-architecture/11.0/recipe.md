# Publish Apple apps for deliberate device and runtime targets

## Goal

Choose Apple minimum OS versions, architectures and runtime options before SDK evaluation when a publish must work beyond the build host.

## Newer approach

Tern's field catalogue has an explicit Mac Catalyst arm64 distribution target.
Set the target framework, supported OS floor and RID in Directory.Build.props,
before SDK target selection, rather than relying on the build host architecture.
The example chooses OS17 as product policy; it does not claim this is the minimum
for every Apple workload. The platform version in the TFM selects reference APIs;
SupportedOSPlatformVersion selects the deployment floor. They solve different jobs.

For an Intel-only artifact select maccatalyst-x64. For a universal distribution,
clear RuntimeIdentifier and set RuntimeIdentifiers to maccatalyst-x64;maccatalyst-arm64
early (or as global build properties). Do not leave both singular and plural values
set. A host-default build is useful for local development but does not establish
support for both architectures. iOS devices use ios-arm64; simulator RIDs do not
produce a device distribution. A Mac Catalyst universal app is not a universal iOS app.

The RC2 Apple source selects CoreCLR by default on current iOS/tvOS/Mac Catalyst;
UseMonoRuntime=true is rejected. Remove inherited Mono-only interpreter settings
when moving this workload forward. NativeAOT remains an explicit PublishAot=true
choice. It disallows runtime code generation: use generated serialization and
bindings, and qualify dependencies for trimming/AOT before selecting it.

CoreCLR's dynamic-code availability is not a promise that every device permits
JIT compilation. RuntimePolicy reports RuntimeFeature.IsDynamicCodeSupported so
the catalogue can select an appropriate generated path. Do not infer it from
Debug/Release alone. DynamicCodeSupport=false is an authored restriction; it cannot
make a dependency that generates code compatible automatically.

Registrar and assembly preparation are runtime-dependent build decisions. Preserve
workload defaults until a dependency requires a change; inspect evaluated Registrar,
EnableAssemblyILStripping, trimming and dynamic-registration settings together.
Removing the dynamic registrar requires trimming plus a static/managed-static
registrar. Do not force a dynamic registrar into NativeAOT or disable assembly
preparation merely to hide a publish failure. Retest Objective-C registration and
reflection-dependent code after changing these settings.

{{snippet:net11-ternpublish-csproj}}

## Older approach

The .NET10 comparison uses the older Apple workload's valid runtime defaults with
the same explicit floor and RID. Older Mono/interpreter configurations are valid
for that runtime; copying them unchanged into the current CoreCLR workload is not
a migration strategy. The source comparison does not add a .NET10 catalogue edition.

## Migration

1. Choose the actual device family and minimum supported OS; move the floor and
   RID selection early in evaluation. Preserve independent iOS and Catalyst floors.
2. Remove inherited Mono-only switches when selecting the current workload.
3. Build the canonical app, then publish Release with the same global target/RID
   inputs. Provision/sign only through your separately authorized distribution flow.
4. On an available Mac, inspect the published app's Mach-O architectures with
   lipo -archs, its Info.plist deployment target, runtimeconfig/trimmer output and
   registrar diagnostics. Check both universal slices, not just the host slice.
5. Exercise startup, generated serialization and Objective-C registration under
   the chosen runtime. These native outputs/behaviors were not produced here.

## Canonical samples

### net10-directory-build-props

{{snippet:net10-directory-build-props}}

### net10-info-plist

{{snippet:net10-info-plist}}

### net10-program-cs

{{snippet:net10-program-cs}}

### net10-runtimepolicy-cs

{{snippet:net10-runtimepolicy-cs}}

### net10-ternpublish-csproj

{{snippet:net10-ternpublish-csproj}}

### net11-directory-build-props

{{snippet:net11-directory-build-props}}

### net11-info-plist

{{snippet:net11-info-plist}}

### net11-program-cs

{{snippet:net11-program-cs}}

### net11-runtimepolicy-cs

{{snippet:net11-runtimepolicy-cs}}

### net11-ternpublish-csproj

{{snippet:net11-ternpublish-csproj}}

## Constraints

The selected runtime, native toolchain and route policy must match applicability. No signing, trust changes or service provisioning are part of these examples.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The canonical net11 Catalyst C# compiles against the pinned RC2 Mac Catalyst reference assembly via eng/Test-AppleRoutes.ps1 -CompileReference. This is reference compilation, not native workload compilation, publish output or execution.


SDK10.0.402/C#14 and SDK11.0.100/C#15 are intended sample pins. No preview language syntax or experimental API opt-in is used. The .NET10 examples compile on Windows; no native app execution is claimed. Exact Apple RC2 sources at a75b11fe9cdbd66a6ddb8bfea120d006c1ec1e8d are checked by eng/Test-AppleRoutes.ps1. Current workload compilation, native publishing/signing, controlled Apple endpoint behavior and runtime/registrar execution are explicitly not-tested under PIN-699 because Apple runtime/Xcode is unavailable. Source review is not compilation or execution. No pending Human proof, package publication or GA certification is claimed. Packed-tool list/get verifies offline guidance delivery only. See applicability for exact source and asset identities.
