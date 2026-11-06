# Generate app artwork for the selected platform and theme

## Goal

Tern Assets uses its own bird silhouette for app identity and launch artwork. It selects one app icon per target and keeps theme and sampling choices explicit.

## Older approach

The .NET10 comparison uses one SVG icon and one light splash with the normal image pipeline. This is a valid baseline. Custom native Android resources can supply additional themed artwork, and preprocessed images can preserve intended sampling; these remain separate authored assets rather than the new single-project metadata. There is no claim that the newer metadata or MAUI .icon integration exists in that release.

## Newer approach

{{snippet:samples-net11-0-ternassets-ternassets-csproj}}

MonochromeFile supplies a dedicated Android adaptive-icon silhouette for launchers supporting themed icons. It is not a replacement foreground image on every OS. DarkFile and DarkColor provide separate launch artwork and background. Android emits night-qualified resources; Apple dark launch asset catalogs require a supported minimum OS14+ and matching workload. Unsupported platforms do not acquire equivalent theme behavior merely because metadata is present.

ResizeQuality chooses build-time sampling: Auto preserves the default, Best requests higher-quality resampling and Fastest uses nearest-neighbour without mipmaps. Choose Fastest for deliberately crisp block artwork; it does not improve every photograph. The setting applies to image-based icons, splash and images, including SVG rasterization. SVG sources are referenced as .png by the running app. Icon Composer bundles bypass this rasterization path.

The current project conditions select the .icon directory only for iOS or Mac Catalyst and the image icon only for other targets. Never retain an unconditional second MauiIcon beside an Apple bundle. Keep icon.json and the complete Assets directory together; the included simple silhouette bundle follows the official Apple workload fixture's layer-file contract. The root icon.json contains {"groups":[{"layers":[{"image-name":"tern.svg","name":"Tern silhouette"}]}]}; Assets/tern.svg is the canonical SVG shown below. Open and review it in Icon Composer before production use. It has not been compiled or visually checked with actool. Do not apply image tint/resize metadata to it. Remove conflicting AppIcon overrides or match the bundle directory's basename.

## Migration

1. Build the older Android sample to establish the baseline image outputs. Replace the original silhouette only with your own artwork; preserve Android resource naming rules.
2. Use a matching .NET11 native workload and the pinned Resizetizer package closure for the current sample. Keep exactly one platform-conditioned icon item.
3. For a real Apple app, include the complete tern.icon directory in an iOS/Mac Catalyst host, with the matching Apple workload and its required Xcode. MAUI integration requires11 RC2; underlying Apple .icon support appeared earlier and does not establish MAUI support on an older target. This Android sample's conditions illustrate that host integration; it is not a runnable Apple application.
4. Inspect generated monochrome and night-qualified Android resources and raster dimensions, then inspect launcher/light/dark startup on an actual device. For Apple, compile with actool and inspect the resulting app catalog. Those current native checks are excluded here under PIN-699, not reported as successful.

## Constraints

Current intended pins: SDK11 RC3, C#15, MAUI Controls/Resizetizer11 RC2 and matching native workload. Apple requires .icon-capable Apple tooling (26.5.11720-net11-p6 or later; use the exact workload-compatible Xcode), plus normal host configuration. The comparison uses SDK10.0.402, C#14 and MAUI10.0.110. Native build assets, no provider/account or web rendering mode. No experimental API opt-in; no GA or visual quality guarantee from RC source review.

## Evidence

The actual .NET10 Android Release app compiled and generated its baseline assets with AOT disabled. Current .NET11 build failed NETSDK1147. Android device/emulator and Apple/Xcode are unavailable: current native generation, themed launcher/startup, sampling appearance, Apple compilation and bundle appearance are explicitly not-tested under PIN-699. Source/artifact inspection establishes metadata contracts only. Packed MCP retrieval establishes offline delivery, not native output quality.

## Canonical inputs


### samples/net10.0/TernAssets/Directory.Build.props

{{snippet:samples-net10-0-ternassets-directory-build-props}}

### samples/net10.0/TernAssets/Directory.Build.targets

{{snippet:samples-net10-0-ternassets-directory-build-targets}}

### samples/net10.0/TernAssets/Directory.Packages.props

{{snippet:samples-net10-0-ternassets-directory-packages-props}}

### samples/net10.0/TernAssets/global.json

{{snippet:samples-net10-0-ternassets-global-json}}

### samples/net10.0/TernAssets/MauiProgram.cs

{{snippet:samples-net10-0-ternassets-mauiprogram-cs}}

### samples/net10.0/TernAssets/Platforms/Android/AndroidManifest.xml

{{snippet:samples-net10-0-ternassets-platforms-android-androidmanifest-xml}}

### samples/net10.0/TernAssets/Platforms/Android/MainActivity.cs

{{snippet:samples-net10-0-ternassets-platforms-android-mainactivity-cs}}

### samples/net10.0/TernAssets/Platforms/Android/MainApplication.cs

{{snippet:samples-net10-0-ternassets-platforms-android-mainapplication-cs}}

### samples/net10.0/TernAssets/Resources/AppIcon/mono.svg

{{snippet:samples-net10-0-ternassets-resources-appicon-mono-svg}}

### samples/net10.0/TernAssets/Resources/AppIcon/tern.svg

{{snippet:samples-net10-0-ternassets-resources-appicon-tern-svg}}

### samples/net10.0/TernAssets/Resources/Images/marker.svg

{{snippet:samples-net10-0-ternassets-resources-images-marker-svg}}

### samples/net10.0/TernAssets/Resources/Splash/day.svg

{{snippet:samples-net10-0-ternassets-resources-splash-day-svg}}

### samples/net10.0/TernAssets/Resources/Splash/night.svg

{{snippet:samples-net10-0-ternassets-resources-splash-night-svg}}

### samples/net10.0/TernAssets/TernAssets.csproj

{{snippet:samples-net10-0-ternassets-ternassets-csproj}}

### samples/net11.0/TernAssets/Directory.Build.props

{{snippet:samples-net11-0-ternassets-directory-build-props}}

### samples/net11.0/TernAssets/Directory.Build.targets

{{snippet:samples-net11-0-ternassets-directory-build-targets}}

### samples/net11.0/TernAssets/Directory.Packages.props

{{snippet:samples-net11-0-ternassets-directory-packages-props}}

### samples/net11.0/TernAssets/global.json

{{snippet:samples-net11-0-ternassets-global-json}}

### samples/net11.0/TernAssets/MauiProgram.cs

{{snippet:samples-net11-0-ternassets-mauiprogram-cs}}

### samples/net11.0/TernAssets/Platforms/Android/AndroidManifest.xml

{{snippet:samples-net11-0-ternassets-platforms-android-androidmanifest-xml}}

### samples/net11.0/TernAssets/Platforms/Android/MainActivity.cs

{{snippet:samples-net11-0-ternassets-platforms-android-mainactivity-cs}}

### samples/net11.0/TernAssets/Platforms/Android/MainApplication.cs

{{snippet:samples-net11-0-ternassets-platforms-android-mainapplication-cs}}

### samples/net11.0/TernAssets/Resources/AppIcon/mono.svg

{{snippet:samples-net11-0-ternassets-resources-appicon-mono-svg}}

### samples/net11.0/TernAssets/Resources/AppIcon/tern.icon/Assets/tern.svg

{{snippet:samples-net11-0-ternassets-resources-appicon-tern-icon-assets-tern-svg}}

### samples/net11.0/TernAssets/Resources/AppIcon/tern.svg

{{snippet:samples-net11-0-ternassets-resources-appicon-tern-svg}}

### samples/net11.0/TernAssets/Resources/Images/marker.svg

{{snippet:samples-net11-0-ternassets-resources-images-marker-svg}}

### samples/net11.0/TernAssets/Resources/Splash/day.svg

{{snippet:samples-net11-0-ternassets-resources-splash-day-svg}}

### samples/net11.0/TernAssets/Resources/Splash/night.svg

{{snippet:samples-net11-0-ternassets-resources-splash-night-svg}}

### samples/net11.0/TernAssets/TernAssets.csproj

{{snippet:samples-net11-0-ternassets-ternassets-csproj}}
