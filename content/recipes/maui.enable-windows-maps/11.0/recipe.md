# Enable the supported Windows map and pin surface

## Goal

Register an Azure Maps service key for the Windows MAUI map and standard pin surface; RC2 camera navigation is unsupported.

## Constraints

The original Quay Survey app maps fictional survey stations. .NET11 uses SDK11.0.100, C#15 and MAUI Controls/Maps11.0.0. .NET10 uses C#14 and Controls/Maps10.0.110; SDK10.0.403 for QuayAtlas and the actual installed native SDK10.0.402 for QuayWindows. These are preparation baselines, not GA verification.

Windows requires the WinUI MapControl from the package-selected Windows App SDK and an installed WebView2 runtime. The control uses Azure Maps through its internal WebView2; source support is not proof of rendering or service compatibility. No Blazor renderer applies.

## Older approach

The exact MAUI10.0.110 packages already contain the Windows handler and the same UseMauiMaps/ConfigureEssentials/UseMapServiceToken setup. The previous sample is a real native map/pin application with a deliberately invalid placeholder, compiled with SDK10.0.402. Its MoveToRegion command passes MapSpan directly to the Windows handler. Keep this valid older approach when it meets your application's target; do not infer absence in10.0.110 from the earlier10.0.0 release source. The conditional target10 route requires these exact package assets. Compilation establishes API compatibility, not map rendering or service access.

## Newer approach

{{snippet:samples-net11-0-quaywindows-mauiprogram-cs}}

Register UseMauiMaps and ConfigureEssentials(options => options.UseMapServiceToken(...)) before Build. The original sample uses PLAYBOOK_PLACEHOLDER_NOT_A_CREDENTIAL. It compiles the exact registration without obtaining or sending a key. Before actual service use, obtain explicit authorization for a separate Azure Maps resource and supply its credential through the application's approved configuration path. Never commit a real key, log it or present a distributed desktop key as secret storage. Review Azure Maps authentication, restrictions, rotation and billing with the account owner; do not assume an arbitrary identity token is accepted by this key property.

The page requests street tiles and one standard pin, with scroll/zoom enabled and traffic disabled. Source-reviewed handler paths include Street/Satellite/Hybrid, traffic, independent scroll/zoom controls and standard pin click events. The pinned RC2 control sends MoveToRegionRequest but its Windows handler accepts MapSpan; the command mapper forwards the argument unchanged. Region navigation is therefore an unsupported route for this exact source baseline, despite broader release-note wording. The sample deliberately avoids relying on it. Reconcile this mismatch against future artifacts before claiming Windows camera movement. Windows also lacks built-in user location, overlay shapes, labels/info windows, background map clicks, clustering, custom pin images and Android JSON styling. No sample silently depends on those unsupported capabilities.

The exact RC3 native Windows MAUI route fails NETSDK1147 (maui-tizen) before compilation. QuayContracts separately compiles this very MauiProgram.cs against the actual Windows package assets, including placeholder registration. That library compilation does not establish a complete native build, map rendering or Azure Maps access. Under PIN-699 those unavailable native actions are **not-tested**, not a mandatory pending Human gate. PIN-700 and PIN-935 were cancelled by Pingu on 7 November 2026. Verification must remain local and incur no cost: do not provision Azure Maps resources, obtain service credentials or make service/tile requests for this work. Live map rendering and service checks are **not-tested** and excluded by this decision, not pending Human work or publication prerequisites. Local compilation and source/artifact evidence remain valid within their recorded scope.

## Migration

1. Select the canonical project for the platform and retain its SDK, language and package pins.
2. Register maps once in app construction, before Build, and apply the supported page behavior shown below.
3. Keep platform-specific styling and capabilities behind the stated platform boundary; never infer Windows overlay support from shared public members.
4. Build the actual native application on an available matching toolchain. Treat contract compilation, official source review and real service execution as separate results.

## Canonical previous and current samples

### samples/net10.0/QuayWindows/Directory.Build.props

{{snippet:samples-net10-0-quaywindows-directory-build-props}}

### samples/net10.0/QuayWindows/Directory.Build.targets

{{snippet:samples-net10-0-quaywindows-directory-build-targets}}

### samples/net10.0/QuayWindows/Directory.Packages.props

{{snippet:samples-net10-0-quaywindows-directory-packages-props}}

### samples/net10.0/QuayWindows/global.json

{{snippet:samples-net10-0-quaywindows-global-json}}

### samples/net10.0/QuayWindows/MauiProgram.cs

{{snippet:samples-net10-0-quaywindows-mauiprogram-cs}}

### samples/net10.0/QuayWindows/Platforms/Windows/App.xaml

{{snippet:samples-net10-0-quaywindows-platforms-windows-app-xaml}}

### samples/net10.0/QuayWindows/Platforms/Windows/App.xaml.cs

{{snippet:samples-net10-0-quaywindows-platforms-windows-app-xaml-cs}}

### samples/net10.0/QuayWindows/QuayWindows.csproj

{{snippet:samples-net10-0-quaywindows-quaywindows-csproj}}

### samples/net11.0/QuayWindows/Directory.Build.props

{{snippet:samples-net11-0-quaywindows-directory-build-props}}

### samples/net11.0/QuayWindows/Directory.Build.targets

{{snippet:samples-net11-0-quaywindows-directory-build-targets}}

### samples/net11.0/QuayWindows/Directory.Packages.props

{{snippet:samples-net11-0-quaywindows-directory-packages-props}}

### samples/net11.0/QuayWindows/global.json

{{snippet:samples-net11-0-quaywindows-global-json}}

### samples/net11.0/QuayWindows/MauiProgram.cs

{{snippet:samples-net11-0-quaywindows-mauiprogram-cs}}

### samples/net11.0/QuayWindows/Platforms/Windows/App.xaml

{{snippet:samples-net11-0-quaywindows-platforms-windows-app-xaml}}

### samples/net11.0/QuayWindows/Platforms/Windows/App.xaml.cs

{{snippet:samples-net11-0-quaywindows-platforms-windows-app-xaml-cs}}

### samples/net11.0/QuayWindows/QuayWindows.csproj

{{snippet:samples-net11-0-quaywindows-quaywindows-csproj}}

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

eng/Test-QuayMaps.ps1 checks exact official source hashes, artifact repository commits and actual assembly metadata for present/absent APIs, plus downloaded Maps package assets in samples/QuayMaps/sources.json. QuayContracts links the canonical MauiProgram.cs files and compiles both releases against real Windows dependencies; it does not execute them. The actual .NET10 Windows map/pin app and placeholder registration compile with installed SDK10.0.402; it was not launched against the service. iOS native builds and interaction are not-tested (Mac/Xcode unavailable); .NET11 Windows MAUI build fails NETSDK1147 with SDK11 RC3. Rendering and service calls have not run. Evidence records retain input freshness; no GA or map execution proof is claimed. Packed-tool list/get verifies offline delivery only.

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


### Exact previous-release artifact references

- [Map.cs in MAUI10.0.110](https://raw.githubusercontent.com/dotnet/maui/6c379bf1dc24461a985a093be3fd8a3e9bc6fc00/src/Controls/Maps/src/Map.cs)
- [Pin.cs in MAUI10.0.110](https://raw.githubusercontent.com/dotnet/maui/6c379bf1dc24461a985a093be3fd8a3e9bc6fc00/src/Controls/Maps/src/Pin.cs)
- [MapElement.cs in MAUI10.0.110](https://raw.githubusercontent.com/dotnet/maui/6c379bf1dc24461a985a093be3fd8a3e9bc6fc00/src/Controls/Maps/src/MapElement.cs)
- [Circle.cs in MAUI10.0.110](https://raw.githubusercontent.com/dotnet/maui/6c379bf1dc24461a985a093be3fd8a3e9bc6fc00/src/Controls/Maps/src/Circle.cs)
- [MapSpan.cs in MAUI10.0.110](https://raw.githubusercontent.com/dotnet/maui/6c379bf1dc24461a985a093be3fd8a3e9bc6fc00/src/Core/maps/src/Primitives/MapSpan.cs)
- [MapHandler.Windows.cs in MAUI10.0.110](https://raw.githubusercontent.com/dotnet/maui/6c379bf1dc24461a985a093be3fd8a3e9bc6fc00/src/Core/maps/src/Handlers/Map/MapHandler.Windows.cs)
- [AppHostBuilderExtensions.cs in MAUI10.0.110](https://raw.githubusercontent.com/dotnet/maui/6c379bf1dc24461a985a093be3fd8a3e9bc6fc00/src/Controls/Maps/src/AppHostBuilderExtensions.cs)
