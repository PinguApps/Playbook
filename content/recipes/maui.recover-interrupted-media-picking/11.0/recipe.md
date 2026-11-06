# Recover an Android media workflow after process recreation

## Goal

A reef survey stores one photograph per station. Persist the station before launching the camera. Only one pending workflow is allowed, so a later capture cannot steal its identity.

## Older approach

The .NET10 comparison retains the station and asks the operator to retake an interrupted photograph. It can store a normal in-process result but has no built-in recovered-result API. Retaking is a valid deliberate recovery policy; this example does not pretend it recovers the lost task or file.

## Newer approach

{{snippet:samples-net11-0-reefsurvey-surveypage-cs}}

The .NET11 Android recovery button first reads the non-consuming snapshot. If empty, it waits with a cancellable ten-second token. Canceling this wait removes the listener; it does not discard the operation. Retry after startup/resume when AndroidX replay arrives. Recovery is Android-only static MediaPicker API, not IMediaPicker and not a portable service method.

Persist destination identity before capture. Copy each recovered file through OpenReadAsync into app-owned storage, then clear the recovered ID. The station hash supplies an idempotent destination if the app dies after copying but before clearing. Do not clear on failed copy. The narrow example accepts only CapturePhoto, whose operation yields one photo; multi-file picking requires separate per-file destination identities and is outside this workflow. orphaned and other operation kinds remain untouched for explicit user reconciliation. Get and Wait do not consume results. Clear acknowledges a published result; DiscardPendingMediaPickerOperationAsync abandons an unreplayed pending operation and can lose a late result. It does not cancel an active in-process picker. The sample asks before discarding. Published orphans need a separate explicit keep/discard decision by ID, not an automatic assignment to the current station.

Do not infer success from file existence or erase state merely because the first snapshot is empty. AndroidX activity-result replay is the recovery signal. Serialize startup/resume inspection with capture and disable duplicate actions in a production UI. This teaching page exposes a manual recovery button so replay inspection is intentional.

## Constraints

SDK/runtime11, C#15, Controls11.0.0-rc.2.26505.5 and matching MAUI workload are intended current pins; older comparison uses SDK10.0.402, C#14 and Controls10.0.110. Native controls, no web rendering mode, no external provider or account. Stable APIs, no experimental opt-in. No documented older-target package route.

## Evidence

Actual RC2 package XML and immutable implementation reviewed; the current native build and all native interactions are not-tested under PIN-699. The .NET10 Android Release app compiled with the existing system SDK10.0.402 and MAUI10 toolchain, with RunAOTCompilation=false. This does not establish AOT success. SDK11 build failed NETSDK1147 before compilation. Packed MCP retrieval proves content delivery only. No GA proof or new Human gate.

## Migration

1. Keep the previous application behavior as the comparison baseline.
2. Select the matching .NET11 workload and explicit platform options shown below.
3. Preserve workflow ownership and test native permission/lifecycle/contrast behavior on the chosen platform before relying on it. Those interactions are not-tested here.

## Canonical samples

### samples/net10.0/ReefSurvey/Directory.Build.props

{{snippet:samples-net10-0-reefsurvey-directory-build-props}}

### samples/net10.0/ReefSurvey/Directory.Build.targets

{{snippet:samples-net10-0-reefsurvey-directory-build-targets}}

### samples/net10.0/ReefSurvey/Directory.Packages.props

{{snippet:samples-net10-0-reefsurvey-directory-packages-props}}

### samples/net10.0/ReefSurvey/global.json

{{snippet:samples-net10-0-reefsurvey-global-json}}

### samples/net10.0/ReefSurvey/MauiProgram.cs

{{snippet:samples-net10-0-reefsurvey-mauiprogram-cs}}

### samples/net10.0/ReefSurvey/Platforms/Android/AndroidManifest.xml

{{snippet:samples-net10-0-reefsurvey-platforms-android-androidmanifest-xml}}

### samples/net10.0/ReefSurvey/Platforms/Android/MainActivity.cs

{{snippet:samples-net10-0-reefsurvey-platforms-android-mainactivity-cs}}

### samples/net10.0/ReefSurvey/Platforms/Android/MainApplication.cs

{{snippet:samples-net10-0-reefsurvey-platforms-android-mainapplication-cs}}

### samples/net10.0/ReefSurvey/ReefSurvey.csproj

{{snippet:samples-net10-0-reefsurvey-reefsurvey-csproj}}

### samples/net10.0/ReefSurvey/SurveyChrome.cs

{{snippet:samples-net10-0-reefsurvey-surveychrome-cs}}

### samples/net10.0/ReefSurvey/SurveyPage.cs

{{snippet:samples-net10-0-reefsurvey-surveypage-cs}}

### samples/net11.0/ReefSurvey/Directory.Build.props

{{snippet:samples-net11-0-reefsurvey-directory-build-props}}

### samples/net11.0/ReefSurvey/Directory.Build.targets

{{snippet:samples-net11-0-reefsurvey-directory-build-targets}}

### samples/net11.0/ReefSurvey/Directory.Packages.props

{{snippet:samples-net11-0-reefsurvey-directory-packages-props}}

### samples/net11.0/ReefSurvey/global.json

{{snippet:samples-net11-0-reefsurvey-global-json}}

### samples/net11.0/ReefSurvey/MauiProgram.cs

{{snippet:samples-net11-0-reefsurvey-mauiprogram-cs}}

### samples/net11.0/ReefSurvey/Platforms/Android/AndroidManifest.xml

{{snippet:samples-net11-0-reefsurvey-platforms-android-androidmanifest-xml}}

### samples/net11.0/ReefSurvey/Platforms/Android/MainActivity.cs

{{snippet:samples-net11-0-reefsurvey-platforms-android-mainactivity-cs}}

### samples/net11.0/ReefSurvey/Platforms/Android/MainApplication.cs

{{snippet:samples-net11-0-reefsurvey-platforms-android-mainapplication-cs}}

### samples/net11.0/ReefSurvey/ReefSurvey.csproj

{{snippet:samples-net11-0-reefsurvey-reefsurvey-csproj}}

### samples/net11.0/ReefSurvey/SurveyChrome.cs

{{snippet:samples-net11-0-reefsurvey-surveychrome-cs}}

### samples/net11.0/ReefSurvey/SurveyPage.cs

{{snippet:samples-net11-0-reefsurvey-surveypage-cs}}

