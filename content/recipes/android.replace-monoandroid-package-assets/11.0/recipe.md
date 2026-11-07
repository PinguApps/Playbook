# Replace a dependency selected from legacy Android assets

## Goal

Replace selected MonoAndroid compile or runtime assets when migrating an Android application, rather than hiding a compatibility warning.

## Newer approach

Whimbrel's coastal trail library consumes an owned marker package. Version 1.0.0 places a portable fixture assembly under lib/monoandroid10.0; version 2.0.0 places the same API under lib/net10.0-android36.0. This isolates asset selection from API changes. It is not evidence that an arbitrary Xamarin binary is compatible with CoreCLR, JNI or a newer Android runtime.

NU1703 requires a .NETCoreApp Android target with framework major at least 11 and SDK analysis level at least 11.0.100. Either selected compile or selected runtime assets must be MonoAndroid and contain a real non-placeholder asset. An unused folder, an empty _._ asset or a net10 Android consumer alone does not meet those guards. Inspect obj/project.assets.json under the actual target: compile and runtime paths matter separately.

Choose a maintained package with compatible modern Android assets and validate its APIs and native dependencies. The local modern fixture uses a net10 Android asset that a newer compatible consumer may select; its package version is independent of the target version. For an upstream library, prefer its publisher's supported current asset rather than relabelling a DLL yourself.

{{snippet:net11-project}}

{{snippet:net11-trailmap}}

## Older approach

The net10 Android consumer selects the original MonoAndroid fixture and compiles when restored with -p:MarkerVersion=1.0.0. Its committed default and lock retain the modern 2.0.0 end state; eng/Test-WhimbrelGuidance.ps1 explicitly restores, builds and asserts both stages against its generated local feed. NU1703's new target guard does not apply there. That successful managed library build does not establish device compatibility. Preserve a working older product while separately validating a replacement; absence of this diagnostic is not proof of compatibility.

{{snippet:net10-project}}

{{snippet:net10-trailmap}}

## Migration

1. Record the SDK analysis level, target, resolved package version and compile/runtime asset paths before changing the dependency.
2. Generate the local fixtures with eng/Test-WhimbrelGuidance.ps1. Restore the original fixture using -p:MarkerVersion=1.0.0 and that generated local feed; verify lib/monoandroid10.0/TrailMarkers.dll in both asset groups. On an available matched SDK11 Android workload, expect NU1703 with analysis level 11.0.100 or later.
3. Change MarkerVersion to 2.0.0, restore again and verify modern paths. Build and exercise the application's actual package-specific behavior before accepting an upstream replacement.
4. If a short-lived suppression is needed, put NoWarn="NU1703" on only that PackageReference and record its removal condition. Do not add a global NoWarn, downgrade analysis to evade checks, or claim suppression repairs ABI/runtime compatibility.
5. The current SDK11 native workload is unavailable here: current NU1703 emission/suppression, current asset selection/build and device checks are explicitly not-tested. The source guards and older real selection checks are distinct evidence.

## Canonical samples

{{snippet:net10-project}}

{{snippet:net10-trailmap}}

{{snippet:net11-project}}

{{snippet:net11-trailmap}}

## Constraints

Applicability records runtime, SDK, C# version, packages, workloads, provider, platform, rendering and stability conditions. RC2 published composition and RC3 verification compiler are separate baselines, not a supported combined native installation. No experimental feature opt-in is required for these APIs. Comparisons retain the eight .NET10 foundations. Native exclusions do not establish compatibility or GA readiness.

## Evidence

Run eng/Test-WhimbrelGuidance.ps1 for older Android selected compile/runtime assets and actual old/current MAUI engine checks. Older Apple library compilation is recorded separately. Pinned mobile source/reference review uses eng/Inspect-MobileApiGaps.ps1. Native exclusions apply only to the named samples and conditions; no mock is labelled device evidence. Installed stdio list/get proves offline delivery, not native behavior. PIN-959 and PIN-966 remain open for their own qualifying coverage/GA criteria.

## References

- [Official MonoAndroidDeprecation.cs](https://raw.githubusercontent.com/NuGet/NuGet.Client/623454983addd9cad23a4c16a28c8faf458c7546/src/NuGet.Core/NuGet.Commands/RestoreCommand/MonoAndroidDeprecation.cs)
