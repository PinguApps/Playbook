# Explore a dense set of survey markers

## Goal

Cluster related markers, show custom icons and inspect map interaction; JSON map styling applies only to Android.

## Constraints

The original Quay Survey app maps fictional survey stations. .NET11 uses SDK11.0.100-rc.3.26522.148, C#15 and MAUI Controls/Maps11.0.0-rc.2.26505.5. .NET10 uses C#14 and Controls/Maps10.0.110; SDK10.0.403 for QuayAtlas and the actual installed native SDK10.0.402 for QuayWindows. These are preparation baselines, not GA verification.

The canonical iOS app uses Apple MapKit; native compilation and interaction are **not-tested** because Mac/Xcode is unavailable. Android requires its own matched target/workload, Google Play Services and an authorized Google Maps SDK key. The guarded MapStyle JSON is Android-only; it has no effect on iOS, Mac Catalyst or Windows. No Android build or service execution is claimed. Do not request user location until the app has declared the platform usage descriptions and obtained location permission. The sample leaves IsShowingUser false.

## Older approach

.NET10 uses MoveToRegion, ordinary labelled pins and a Circle. Removing/readding the circle is a valid way to control visibility. It supplies neither the new declarative Region property nor built-in clustering/custom icon APIs. Keep this simple route when the marker count is small; no invented older clustering equivalent is claimed.

## Newer approach

{{snippet:samples-net11-0-quayatlas-mauiprogram-cs}}

Region initializes the camera from MapSpan.FromLocations. Circle.IsVisible toggles the boundary without changing collection membership; ZIndex puts that overlay above lower ordered shapes. CircleClicked updates the status rather than assuming a background map click identifies an overlay. Visibility defaults to true and ZIndex to zero; the sample explicitly selects true and 2.

The 24 stations share one ClusteringIdentifier; IsClusteringEnabled defaults false and is enabled deliberately. MarkerClicked selects a station; InfoWindowClicked closes its information window. ClusterClicked leaves Handled false to retain the platform's default zoom into the cluster. A real marker asset is bundled as survey.png. MapLongClicked reports coordinates; UserLocationChanged is subscribed but cannot fire from a location feature the app has not enabled. Fit surveys calls the animated MoveToRegion overload. Span computation does not avoid all cartographic edge cases; use coordinates appropriate to your domain.

Windows is an explicitly unsupported route for these overlay, clustering, custom icon and info-window behaviors. Its handler does not render circles/polylines/polygons, clustering or Android style JSON. Use maui.enable-windows-maps for its narrower surface. This recipe declares no verified older-target applicability route.

## Migration

1. Select the canonical project for the platform and retain its SDK, language and package pins.
2. Register maps once in app construction, before Build, and apply the supported page behavior shown below.
3. Keep platform-specific styling and capabilities behind the stated platform boundary; never infer Windows overlay support from shared public members.
4. Build the actual native application on an available matching toolchain. Treat contract compilation, official source review and real service execution as separate results.

## Canonical previous and current samples

### samples/net10.0/QuayAtlas/Directory.Build.props

{{snippet:samples-net10-0-quayatlas-directory-build-props}}

### samples/net10.0/QuayAtlas/Directory.Build.targets

{{snippet:samples-net10-0-quayatlas-directory-build-targets}}

### samples/net10.0/QuayAtlas/Directory.Packages.props

{{snippet:samples-net10-0-quayatlas-directory-packages-props}}

### samples/net10.0/QuayAtlas/global.json

{{snippet:samples-net10-0-quayatlas-global-json}}

### samples/net10.0/QuayAtlas/MauiProgram.cs

{{snippet:samples-net10-0-quayatlas-mauiprogram-cs}}

### samples/net10.0/QuayAtlas/Platforms/iOS/AppDelegate.cs

{{snippet:samples-net10-0-quayatlas-platforms-ios-appdelegate-cs}}

### samples/net10.0/QuayAtlas/Platforms/iOS/Info.plist

{{snippet:samples-net10-0-quayatlas-platforms-ios-info-plist}}

### samples/net10.0/QuayAtlas/Platforms/iOS/Program.cs

{{snippet:samples-net10-0-quayatlas-platforms-ios-program-cs}}

### samples/net10.0/QuayAtlas/Platforms/iOS/SceneDelegate.cs

{{snippet:samples-net10-0-quayatlas-platforms-ios-scenedelegate-cs}}

### samples/net10.0/QuayAtlas/QuayAtlas.csproj

{{snippet:samples-net10-0-quayatlas-quayatlas-csproj}}

### samples/net11.0/QuayAtlas/Directory.Build.props

{{snippet:samples-net11-0-quayatlas-directory-build-props}}

### samples/net11.0/QuayAtlas/Directory.Build.targets

{{snippet:samples-net11-0-quayatlas-directory-build-targets}}

### samples/net11.0/QuayAtlas/Directory.Packages.props

{{snippet:samples-net11-0-quayatlas-directory-packages-props}}

### samples/net11.0/QuayAtlas/global.json

{{snippet:samples-net11-0-quayatlas-global-json}}

### samples/net11.0/QuayAtlas/MauiProgram.cs

{{snippet:samples-net11-0-quayatlas-mauiprogram-cs}}

### samples/net11.0/QuayAtlas/Platforms/iOS/AppDelegate.cs

{{snippet:samples-net11-0-quayatlas-platforms-ios-appdelegate-cs}}

### samples/net11.0/QuayAtlas/Platforms/iOS/Info.plist

{{snippet:samples-net11-0-quayatlas-platforms-ios-info-plist}}

### samples/net11.0/QuayAtlas/Platforms/iOS/Program.cs

{{snippet:samples-net11-0-quayatlas-platforms-ios-program-cs}}

### samples/net11.0/QuayAtlas/Platforms/iOS/SceneDelegate.cs

{{snippet:samples-net11-0-quayatlas-platforms-ios-scenedelegate-cs}}

### samples/net11.0/QuayAtlas/QuayAtlas.csproj

{{snippet:samples-net11-0-quayatlas-quayatlas-csproj}}

## Evidence

eng/Test-QuayMaps.ps1 checks exact official source hashes and downloaded Maps package assets in samples/QuayMaps/sources.json. QuayContracts links the canonical MauiProgram.cs files and compiles both releases against real Windows dependencies; it does not execute them. The .NET10 Windows fallback app compiles with installed SDK10.0.402. iOS native builds and interaction are not-tested (Mac/Xcode unavailable); .NET11 Windows MAUI build fails NETSDK1147 with SDK11 RC3. Rendering and service calls have not run. Evidence records retain input freshness; no GA or map execution proof is claimed. Packed-tool list/get verifies offline delivery only.

## Official references

- [Map.cs pinned official source](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/Maps/src/Map.cs)
- [Pin.cs pinned official source](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/Maps/src/Pin.cs)
- [MapElement.cs pinned official source](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/Maps/src/MapElement.cs)
- [Circle.cs pinned official source](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/Maps/src/Circle.cs)
- [Map.Impl.cs pinned official source](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/Maps/src/HandlerImpl/Map.Impl.cs)
- [MapHandler.Windows.cs pinned official source](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Core/maps/src/Handlers/Map/MapHandler.Windows.cs)
- [AppHostBuilderExtensions.cs pinned official source](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/Maps/src/AppHostBuilderExtensions.cs)
- [MapSpan.cs pinned official source](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Core/maps/src/Primitives/MapSpan.cs)
- [Azure Maps authentication and credential security](https://learn.microsoft.com/azure/azure-maps/azure-maps-authentication)


### samples/net10.0/QuayAtlas/Resources/Images/survey.svg

{{snippet:samples-net10-0-quayatlas-resources-images-survey-svg}}


### samples/net11.0/QuayAtlas/Resources/Images/survey.svg

{{snippet:samples-net11-0-quayatlas-resources-images-survey-svg}}
