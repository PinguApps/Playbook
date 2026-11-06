# Explicitly save new captures to the device gallery

## Goal

A reef operator may keep a station photograph only in the survey, or explicitly request an additional gallery copy. The switch is opt-in and defaults to false.

## Older approach

The .NET10 comparison copies the live result into application storage. The disabled switch makes its lack of a built-in gallery option visible. A production .NET10 app can implement platform gallery integration separately; the sample does not invent a down-level SaveToGallery route.

## Newer approach

{{snippet:samples-net11-0-reefsurvey-surveypage-cs}}

Pass MediaPickerOptions.SaveToGallery on CapturePhotoAsync or CaptureVideoAsync. The default false preserves private capture behavior. Picking existing photos/videos is selection, not a new capture: this option has no effect there. A FileResult is still a file handle, not a promise of durable application storage; copy through OpenReadAsync as shown.

Android API29+ uses MediaStore without additional gallery write permission. Below29 declare WRITE_EXTERNAL_STORAGE capped at28; the API requests the needed permission. Declare CAMERA and camera-intent visibility as the Android manifest shows. iOS and Mac Catalyst need NSCameraUsageDescription for camera access and NSPhotoLibraryAddUsageDescription for gallery addition; PhotosAddOnly permission is requested. Video capture also needs the applicable microphone usage description. Permission refusal or save failure can throw: surface the error without reporting a successful gallery copy. Windows and Tizen ignore SaveToGallery. Do not treat a successful capture there as gallery persistence. Apple permission setup is documentation-only here; no canonical Apple app or execution is claimed.

Gallery saving and Android replay are distinct lifecycles. Do not automatically save a recovered result again or claim that recovering a file proves a gallery write completed. Retain the operator's consent and reconcile interrupted gallery work separately if the product needs that guarantee.

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

