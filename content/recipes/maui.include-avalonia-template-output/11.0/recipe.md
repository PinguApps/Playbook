# Generate a MAUI project with an Avalonia desktop head

## Goal

Use the first-party MAUI template integration to add an Avalonia desktop target with pinned third-party prerequisites.

## Constraints

Current examples pin SDK11.0.100-rc.2.26504.105, C#15 and MAUI11.0.0-rc.2.26505.5. Older comparisons pin SDK10.0.402, C#14 and MAUI10.0.110. These pins are actual RC2/.NET10 evidence, not GA certification or minimum-version promises. Keep independent SDK, native platform/workload and package-asset requirements separate. No provider, account or web-server rendering mode is implied.

Use the first-party MAUI project template when the desired starting point includes an Avalonia desktop head. Avalonia implements the desktop rendering integration; Microsoft recognizing a template option does not make Avalonia a first-party MAUI platform implementation.

## Older approach

The .NET 10 baseline has no equivalent first-party MAUI project-template Avalonia option. Its original Quay Ledger C# app uses the supported Windows MAUI backend. Existing independently maintained Avalonia integrations remain valid, but this recipe neither invents an older template switch nor teaches a standalone Avalonia framework.

## Newer approach

Select SDK11.0.100-rc.2.26504.105 and install Microsoft.Maui.Templates.net11::11.0.0-rc.2.26505.5. Generate with `dotnet new maui --name QuayDesktop --with-avalonia --avaloniaControlsMauiVersion 11.0.0-rc.2.26287.618 --no-restore`. The version parameter is hidden in CLI help but is accepted by the tested template. The generated project adds a neutral net11.0 desktop target, Avalonia.Controls.Maui, conditional Avalonia.Controls.Maui.Desktop and AvaloniaControlsMauiGenerateBootstrap=true. Other heads use Avalonia embedding; a neutral desktop target is not a Windows/iOS/Android native platform TFM.

For a separate desktop head use the first-party `maui-multiproject --avalonia` option; its desktop project is optional and separate. The single-project `--with-avalonia` option is ignored when XAML sample content is selected. Preserve resource XAML even when C# UI is chosen. Do not apply the template sample-content pages to the unsupported Avalonia combination.

Override the template's floating preview.6.* Avalonia default with the exact matching package pair above. Also freeze Controls to11.0.0-rc.2.26505.5 and Logging.Debug to11.0.0-rc.2.26504.105 before restoring generated heads. The reviewed Avalonia RC2 packages target net11.0 and depend on this exact MAUI RC2 family, Avalonia12.1.4 and SkiaSharp4.151.0. These are actual package prerequisites, not versions inferred from the MAUI workload. Keep transitive resolution in a lock file. Do not offer a .NET10 route without independently compatible assets and evidence.

The wholly original QuayAvalonia consumer below deliberately isolates the neutral desktop package seam from unavailable native SDK resolution. It references Controls, Resizetizer and the two Avalonia packages explicitly, supplies its own C# MAUI screen, enables the packaged bootstrap generator and converts a raw MauiAsset. It is a package consumer, not an unchanged generated template head or a universal MAUI workload replacement. Keep UseMaui/SingleProject and matched workloads when building the ordinary generated native heads.

{{snippet:samples-net11-0-quayavalonia-mauiprogram-cs}}

## Migration

1. Inspect the actual target, SDK, MAUI package assets and selected backend before changing the application. Keep the pinned older example for comparison.
2. Apply the demonstrated current configuration at the public seam above. Pin dependencies and restore a reviewed lock file; retain the stated no-store, subscription, disposal and template-option boundaries where applicable.
3. Build the selected canonical consumer and run its focused checks. Treat unavailable native compilation/interaction and declared contract doubles exactly as described below; do not report them as rendered behavior.

## Evidence

Real template generation and exact option/dependency inspection passed. The original neutral desktop package consumer compiles, with the actual bootstrap and raw-asset conversion targets. No desktop window, native embedding, visual behavior or target-platform matrix was executed for it. Generated native heads retain the unavailable SDK11 MAUI boundary; compilation/interaction there is not-tested under PIN-699. No standalone third-party feature or production support guarantee is added.

## Canonical sources

### samples/net10.0/QuayCSharp/Directory.Build.props

{{snippet:samples-net10-0-quaycsharp-directory-build-props}}

### samples/net10.0/QuayCSharp/Directory.Build.targets

{{snippet:samples-net10-0-quaycsharp-directory-build-targets}}

### samples/net10.0/QuayCSharp/Directory.Packages.props

{{snippet:samples-net10-0-quaycsharp-directory-packages-props}}

### samples/net10.0/QuayCSharp/global.json

{{snippet:samples-net10-0-quaycsharp-global-json}}

### samples/net10.0/QuayCSharp/MauiProgram.cs

{{snippet:samples-net10-0-quaycsharp-mauiprogram-cs}}

### samples/net10.0/QuayCSharp/Platforms/Windows/App.xaml

{{snippet:samples-net10-0-quaycsharp-platforms-windows-app-xaml}}

### samples/net10.0/QuayCSharp/Platforms/Windows/App.xaml.cs

{{snippet:samples-net10-0-quaycsharp-platforms-windows-app-xaml-cs}}

### samples/net10.0/QuayCSharp/QuayCSharp.csproj

{{snippet:samples-net10-0-quaycsharp-quaycsharp-csproj}}

### samples/net10.0/QuayCSharp/Resources/AppIcon/quay.svg

{{snippet:samples-net10-0-quaycsharp-resources-appicon-quay-svg}}

### samples/net10.0/QuayCSharp/Resources/Styles/Quay.xaml

{{snippet:samples-net10-0-quaycsharp-resources-styles-quay-xaml}}

### samples/net11.0/QuayAvalonia/Directory.Build.props

{{snippet:samples-net11-0-quayavalonia-directory-build-props}}

### samples/net11.0/QuayAvalonia/Directory.Build.targets

{{snippet:samples-net11-0-quayavalonia-directory-build-targets}}

### samples/net11.0/QuayAvalonia/Directory.Packages.props

{{snippet:samples-net11-0-quayavalonia-directory-packages-props}}

### samples/net11.0/QuayAvalonia/global.json

{{snippet:samples-net11-0-quayavalonia-global-json}}

### samples/net11.0/QuayAvalonia/MauiProgram.cs

{{snippet:samples-net11-0-quayavalonia-mauiprogram-cs}}

### samples/net11.0/QuayAvalonia/QuayAvalonia.csproj

{{snippet:samples-net11-0-quayavalonia-quayavalonia-csproj}}

### samples/net11.0/QuayAvalonia/Resources/Raw/quay.txt

{{snippet:samples-net11-0-quayavalonia-resources-raw-quay-txt}}

## Official references

- [Pinned IAlertManager.cs](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/Platform/AlertManager/IAlertManager.cs)
- [Pinned Microsoft.Maui.Resizetizer.After.targets](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/SingleProject/Resizetizer/src/nuget/buildTransitive/Microsoft.Maui.Resizetizer.After.targets)
- [Pinned MauiBlazorWebViewBuilderExtensions.cs](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/BlazorWebView/src/Maui/MauiBlazorWebViewBuilderExtensions.cs)
- [Pinned IGesturePlatformManagerFactory.cs](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/Platform/GestureManager/IGesturePlatformManagerFactory.cs)
- [Pinned ScreenshotDispatch.cs](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Core/src/ScreenshotDispatch.cs)
- [Pinned Microsoft.Maui.Controls.SingleProject.targets](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Build.Tasks/nuget/buildTransitive/netstandard2.0/Microsoft.Maui.Controls.SingleProject.targets)
- [Pinned BlazorWebView.cs](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/BlazorWebView/src/Maui/BlazorWebView.cs)
- [Pinned BlazorWebViewStaticContentRequest.cs](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/BlazorWebView/src/Maui/BlazorWebViewStaticContentRequest.cs)
- [Pinned StaticContentCacheControl.cs](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/BlazorWebView/src/Maui/StaticContentCacheControl.cs)
- [Pinned StaticContentProvider.cs](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/BlazorWebView/src/Maui/Windows/StaticContentProvider.cs)
- [Pinned template.json](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Templates/src/templates/maui-mobile/.template.config/template.json)
- [Pinned template.json](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Templates/src/templates/maui-multiproject/.template.config/template.json)
- [Pinned IGesturePlatformManager.cs](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/Platform/GestureManager/IGesturePlatformManager.cs)
- [Pinned IAlertManagerSubscription.cs](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/Platform/AlertManager/IAlertManagerSubscription.cs)
- [Pinned BlazorWebView.cs](https://raw.githubusercontent.com/dotnet/maui/9d881e94b1c56692e40a16262897677a3bb46fcb/src/BlazorWebView/src/Maui/BlazorWebView.cs)
