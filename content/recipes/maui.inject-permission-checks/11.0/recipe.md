# Own permission checks and foreground location updates

## Goal

Tern Field asks before collecting foreground location fixes. It reports notification authorization independently and preserves the meaning of altitude measurements.

## Older approach

The .NET10 sample calls Permissions directly. It filters accepted fixes in application code using CalculateDistance in kilometres, converting35 metres to0.035 kilometres. That filter reduces application processing, not native sensor work. A small application-owned permission adapter remains a valid way to substitute checks on this release. The .NET10 common PostNotifications route is Android-only; an iOS application needs its own UserNotifications authorization integration rather than assuming the common permission works there.

## Newer approach

{{snippet:samples-net11-0-ternfield-fieldsession-cs}}

Pass IPermissions to the session at composition time; the default constructor captures Permissions.Current. Current is a getter, and SetCurrent is internal. Registering an IPermissions service in your own container does not replace the process-wide static implementation. Pass a replacement explicitly to the session; do not use reflection to change MAUI globals. The service is borrowed; the session does not dispose it.

Check before requesting. Start from a user action on the UI thread, explain purpose and Android rationale in the surrounding UI, and preserve denied/restricted outcomes. Do not loop permission prompts. Notification denial does not prevent this foreground location workflow. A grant does not schedule a notification, register remote notifications or guarantee display. On iOS, the current implementation maps authorized, provisional and supported ephemeral authorization to Granted; that value does not promise unrestricted alert delivery.

MinimumDistance is in metres; zero disables distance filtering and negative values are invalid. Android forwards it to native location updates, Apple uses DistanceFilter, and Windows uses MovementThreshold. It is a request to the platform rather than a guarantee of exact spacing or background operation. MinimumTime and requested accuracy still matter. Stop on page disappearance or listener failure and detach both event handlers; this sample owns the process's single foreground listener.

Android API34+ can prefer an available mean-sea-level altitude and matching vertical accuracy, reporting Geoid. Otherwise retain the supplied reference system. Altitude and vertical accuracy are nullable metres. Do not compare geoid and ellipsoid heights without a deliberate conversion, infer sea-level altitude from a non-null number, or treat accuracy as altitude. The sample keeps all three values together rather than stripping metadata.

## Migration

1. Build the canonical Android project with its pinned SDK/workload and declared coarse/fine location and notification permissions. Android notification authorization is version-dependent.
2. Construct the current session with Permissions.Current or an application-owned IPermissions implementation. Wire start/stop to the supplied page lifecycle.
3. For an iOS host, declare NSLocationWhenInUseUsageDescription with an honest purpose string in Info.plist, provide normal iOS application bootstrap and use the matching Apple workload/Xcode. No Android manifest applies there. The canonical app here targets Android; iOS integration has not been compiled.
4. Choose distance/time/accuracy deliberately. Check denial and granted behavior on the target device before relying on sensor output. No permission prompt or location fix was executed here.

## Constraints

SDK/runtime11, C#15 and MAUI11 RC2 assets plus matching native workload are required for the current APIs. The .NET10 Android comparison uses SDK10.0.402, C#14 and Controls10.0.110. No older-target support for IPermissions or MinimumDistance is claimed. Native foreground UI only; no background location, notification scheduling, external provider or web rendering mode. APIs are stable contracts reviewed at RC2, not GA-tested behavior.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The actual .NET10 Android Release application compiled with RunAOTCompilation=false; no AOT success is claimed. .NET11 build failed NETSDK1147 before compilation. Android devices/emulators and Apple/Xcode are unavailable: permission prompts, iOS authorization and actual location updates are explicitly not-tested under PIN-699. Immutable permission/location implementation review supports the described ownership and metadata semantics; it is not sensor execution.

## Canonical inputs


### samples/net10.0/TernField/Directory.Build.props

{{snippet:samples-net10-0-ternfield-directory-build-props}}

### samples/net10.0/TernField/Directory.Build.targets

{{snippet:samples-net10-0-ternfield-directory-build-targets}}

### samples/net10.0/TernField/Directory.Packages.props

{{snippet:samples-net10-0-ternfield-directory-packages-props}}

### samples/net10.0/TernField/FieldSession.cs

{{snippet:samples-net10-0-ternfield-fieldsession-cs}}

### samples/net10.0/TernField/global.json

{{snippet:samples-net10-0-ternfield-global-json}}

### samples/net10.0/TernField/MauiProgram.cs

{{snippet:samples-net10-0-ternfield-mauiprogram-cs}}

### samples/net10.0/TernField/Platforms/Android/AndroidManifest.xml

{{snippet:samples-net10-0-ternfield-platforms-android-androidmanifest-xml}}

### samples/net10.0/TernField/Platforms/Android/MainActivity.cs

{{snippet:samples-net10-0-ternfield-platforms-android-mainactivity-cs}}

### samples/net10.0/TernField/Platforms/Android/MainApplication.cs

{{snippet:samples-net10-0-ternfield-platforms-android-mainapplication-cs}}

### samples/net10.0/TernField/TernField.csproj

{{snippet:samples-net10-0-ternfield-ternfield-csproj}}

### samples/net11.0/TernField/Directory.Build.props

{{snippet:samples-net11-0-ternfield-directory-build-props}}

### samples/net11.0/TernField/Directory.Build.targets

{{snippet:samples-net11-0-ternfield-directory-build-targets}}

### samples/net11.0/TernField/Directory.Packages.props

{{snippet:samples-net11-0-ternfield-directory-packages-props}}

### samples/net11.0/TernField/FieldSession.cs

{{snippet:samples-net11-0-ternfield-fieldsession-cs}}

### samples/net11.0/TernField/global.json

{{snippet:samples-net11-0-ternfield-global-json}}

### samples/net11.0/TernField/MauiProgram.cs

{{snippet:samples-net11-0-ternfield-mauiprogram-cs}}

### samples/net11.0/TernField/Platforms/Android/AndroidManifest.xml

{{snippet:samples-net11-0-ternfield-platforms-android-androidmanifest-xml}}

### samples/net11.0/TernField/Platforms/Android/MainActivity.cs

{{snippet:samples-net11-0-ternfield-platforms-android-mainactivity-cs}}

### samples/net11.0/TernField/Platforms/Android/MainApplication.cs

{{snippet:samples-net11-0-ternfield-platforms-android-mainapplication-cs}}

### samples/net11.0/TernField/TernField.csproj

{{snippet:samples-net11-0-ternfield-ternfield-csproj}}
