# Declare application predictive-back behaviour

## Goal

Choose Android predictive-back participation at application scope when migrating manifest configuration to an Application attribute.

## Newer approach

Skua saves its observation before leaving the activity. It uses normal platform back
navigation to return to the launcher; it adds no custom back interception.
The new application attribute expresses the same choice as the older manifest XML.
True opts into supported platform predictive-back behaviour; false opts out where
the OS supports that opt-out. The setting does not add a navigation stack or guarantee
an animation for every route. Activity-specific declarations can override app scope.

Predictive back starts with Android13/API33. Check OS and target-SDK behaviour:
newer targets change platform defaults and can remove legacy callback paths.
Use supported navigation callbacks for custom interception. Do not adopt a deprecated
OnBackPressed override merely to use this declaration. The flag alone does not prove
gesture progress, cancellation, or a correct custom transition.

### net11-0-skuaback-directory-build-props

{{snippet:net11-0-skuaback-directory-build-props}}

### net11-0-skuaback-directory-build-targets

{{snippet:net11-0-skuaback-directory-build-targets}}

### net11-0-skuaback-directory-packages-props

{{snippet:net11-0-skuaback-directory-packages-props}}

### net11-0-skuaback-global-json

{{snippet:net11-0-skuaback-global-json}}

### net11-0-skuaback-mainactivity-cs

{{snippet:net11-0-skuaback-mainactivity-cs}}

### net11-0-skuaback-properties-androidmanifest-xml

{{snippet:net11-0-skuaback-properties-androidmanifest-xml}}

### net11-0-skuaback-skuaback-csproj

{{snippet:net11-0-skuaback-skuaback-csproj}}

## Older approach

The .NET10 canonical comparison below solves the same observation-notebook goal.
It remains valid for its declared older toolchain. The .NET11 edition supplies no
older-target route and leaves the eight curated .NET10 foundations unchanged.

### net10-0-skuaback-directory-build-props

{{snippet:net10-0-skuaback-directory-build-props}}

### net10-0-skuaback-directory-build-targets

{{snippet:net10-0-skuaback-directory-build-targets}}

### net10-0-skuaback-directory-packages-props

{{snippet:net10-0-skuaback-directory-packages-props}}

### net10-0-skuaback-global-json

{{snippet:net10-0-skuaback-global-json}}

### net10-0-skuaback-mainactivity-cs

{{snippet:net10-0-skuaback-mainactivity-cs}}

### net10-0-skuaback-properties-androidmanifest-xml

{{snippet:net10-0-skuaback-properties-androidmanifest-xml}}

### net10-0-skuaback-skuaback-csproj

{{snippet:net10-0-skuaback-skuaback-csproj}}

## Migration

1. Inspect the actual target framework, workload and OS minimum before changing startup.
2. Compare the canonical declarations below with the existing application configuration.
3. Move only the shown configuration/callback responsibilities; preserve unrelated permissions and launch handling.
4. Keep one owner per launch action and acknowledge each completion exactly once.
5. Build with the declared workload, then test cold launch, warm launch, unknown actions and ordinary back/foreground behaviour on a supported device.

## Constraints

Use the explicit runtime, SDK, package assets, platforms and requirements returned
with this recipe. These samples target .NET11 with a .NET10 comparison. The ordinary
Playbook host requires no native workload. No external provider, account, render
mode or resource provisioning is involved. APIs shown are stable contracts;
the evidence uses RC artifacts and does not certify final GA behaviour.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The .NET10 samples compile on Windows. Current native builds fail NETSDK1147;
source/artifact review is separate from native compilation. Current canonical C# files also pass direct compilation against pinned real reference assemblies; that check excludes native build targets, manifest processing, packaging and execution. Android device and
Apple Xcode execution are not-tested under PIN-699. No Human execution or GA proof
is claimed. Exact source hashes, artifact versions and sample input freshness live
in the canonical verification records. Packed stdio inspection checks offline
discovery, Markdown and applicability; it does not verify native behaviour.

## References

- [Pinned official lifecycle contract](https://raw.githubusercontent.com/dotnet/android/801f9ecbab5118b13d21cb52409e507ad4b2feb7/src/Xamarin.Android.NamingCustomAttributes/Android.App/ApplicationAttribute.cs)
- [Android application manifest](https://developer.android.com/guide/topics/manifest/application-element#enableOnBackInvokedCallback)
- [Android predictive back navigation](https://developer.android.com/guide/navigation/custom-back/predictive-back-gesture)
