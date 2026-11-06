# Choose system-bar contrast independently of app theme

## Goal

The reef survey uses a dark navigation header with light system icons while leaving the application's theme alone.

## Older approach

The .NET10 Android comparison sets AppearanceLightStatusBars=false through AndroidX after the native window is created. This remains a valid native integration when the application owns platform behavior. Reapply native settings when necessary for lifecycle or other chrome changes; it is not a portable MAUI property.

## Newer approach

{{snippet:samples-net11-0-reefsurvey-surveychrome-cs}}

Window.StatusBarTheme controls icon foreground. Dark means a dark backdrop with light icons; Light means a light backdrop with dark icons. Default follows the current app theme and preserves existing behavior. Setting this property does not set a background or change Application.UserAppTheme. Android and iOS support these OS status icons; Windows and Mac Catalyst are no-op desktop routes. Do not equate StatusBarTheme with arbitrary icon tint or assume it controls every navigation-bar icon.

The Android-only MauiAndroidSystemBarsUseMauiChrome project opt-in matches supported status/navigation backgrounds to effective NavigationPage, Shell, TabbedPage and modal chrome. Its default is false. It does not select icon appearance. The canonical project enables it, sets NavigationPage.BarBackgroundColor independently, and explicitly selects Dark icons semantics for that dark background. Check edge-to-edge/insets and contrast on the actual OS/navigation mode before claiming legibility. iOS gets the icon property, not this Android background opt-in. No theme mutation is needed.

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

