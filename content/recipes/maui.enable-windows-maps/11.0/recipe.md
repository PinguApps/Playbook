# Enable the supported Windows map and pin surface

## Goal

Register an Azure Maps service key for the Windows MAUI map and standard pin surface; RC2 camera navigation is unsupported.

## Constraints

The original Quay Survey app maps fictional survey stations. .NET11 uses SDK11.0.100-rc.3.26522.148, C#15 and MAUI Controls/Maps11.0.0-rc.2.26505.5. .NET10 uses C#14 and Controls/Maps10.0.110; SDK10.0.403 for QuayAtlas and the actual installed native SDK10.0.402 for QuayWindows. These are preparation baselines, not GA verification.

Windows requires the WinUI MapControl from the package-selected Windows App SDK and an installed WebView2 runtime. The control uses Azure Maps through its internal WebView2; source support is not proof of rendering or service compatibility. No Blazor renderer applies.

## Older approach

MAUI10 has no first-party Windows Map handler equivalent. The canonical previous sample builds a Windows page explaining that limit. It does not call a nonexistent map service. A web or third-party integration can be valid after its own provider, accessibility and security assessment; none is asserted compatible by this recipe.

## Newer approach

{{snippet:samples-net11-0-quaywindows-mauiprogram-cs}}

Register UseMauiMaps and ConfigureEssentials(options => options.UseMapServiceToken(...)) before Build. The original sample uses PLAYBOOK_PLACEHOLDER_NOT_A_CREDENTIAL. It compiles the exact registration without obtaining or sending a key. Before actual service use, obtain explicit authorization for a separate Azure Maps resource and supply its credential through the application's approved configuration path. Never commit a real key, log it or present a distributed desktop key as secret storage. Review Azure Maps authentication, restrictions, rotation and billing with the account owner; do not assume an arbitrary identity token is accepted by this key property.

The page requests street tiles and one standard pin, with scroll/zoom enabled and traffic disabled. Source-reviewed handler paths include Street/Satellite/Hybrid, traffic, independent scroll/zoom controls and standard pin click events. The pinned RC2 control sends MoveToRegionRequest but its Windows handler accepts MapSpan; the command mapper forwards the argument unchanged. Region navigation is therefore an unsupported route for this exact source baseline, despite broader release-note wording. The sample deliberately avoids relying on it. Reconcile this mismatch against future artifacts before claiming Windows camera movement. Windows also lacks built-in user location, overlay shapes, labels/info windows, background map clicks, clustering, custom pin images and Android JSON styling. No sample silently depends on those unsupported capabilities.

The exact RC3 native Windows MAUI route fails NETSDK1147 (maui-tizen) before compilation. QuayContracts separately compiles this very MauiProgram.cs against the actual Windows package assets, including placeholder registration. That library compilation does not establish a complete native build, map rendering or Azure Maps access. Under PIN-699 those unavailable native actions are **not-tested**, not a mandatory pending Human gate. PIN-935 remains open and has not run; PIN-700 remains unchanged. No resources were provisioned and no credentials acquired.

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
