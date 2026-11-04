# Start a MAUI project with C# screens

## Goal

Use the first-party C# UI template option when screens should be authored in code while resource dictionaries remain XAML.

## Constraints

Current examples pin SDK11.0.100-rc.2.26504.105, C#15 and MAUI11.0.0-rc.2.26505.5. Older comparisons pin SDK10.0.402, C#14 and MAUI10.0.110. These pins are actual RC2/.NET10 evidence, not GA certification or minimum-version promises. Keep independent SDK, native platform/workload and package-asset requirements separate. No provider, account or web-server rendering mode is implied.

The fictional Quay Ledger screen counts checked crates using built-in MAUI controls. Both canonical apps implement that same screen in C#; no third-party fluent UI library is involved.

## Older approach

C# UI is already valid on .NET 10: construct the Application, Window and ContentPage explicitly. Its first-party page/content-view C# item templates remain useful. The .NET 11 change is a project-template option, not the invention of C# UI or proof that XAML is obsolete.

## Newer approach

With SDK11 and the matching MAUI template installed, run `dotnet new maui --name QuayLedger --ui csharp --no-restore`. Use Microsoft.Maui.Templates.net11 version 11.0.0-rc.2.26505.5 for the retained RC2 preparation baseline. A prerequisite template install is `dotnet new install Microsoft.Maui.Templates.net11::11.0.0-rc.2.26505.5`; native workload installation is separate. Select a directory containing the matching global.json before these commands.

The option generates App.cs, AppShell.cs and MainPage.cs and removes their screen XAML counterparts. Resources/Styles/Colors.xaml and Styles.xaml remain. C# screens do not imply deleting resource dictionaries, assets or platform startup XAML. XAML remains the default (`--ui xaml`). `--sample-content` is XAML-only: with C# UI the template warns and omits that sample content. Keep the original small crate screen rather than presenting the template's demo as an original worked example.

Before restoring, replace the generated Controls $(MauiVersion) and floating Logging.Debug 11.0.0-* references with reviewed exact versions: Controls11.0.0-rc.2.26505.5 and Logging.Debug11.0.0-rc.2.26504.105. Commit a resolved lock file and pin the SDK. The original canonical samples below already pin their actual dependencies independently from the tool host. They target Windows only; the template's additional Android/Apple targets need their own matched native toolchains.

{{snippet:samples-net11-0-quaycsharp-mauiprogram-cs}}

## Migration

1. Inspect the actual target, SDK, MAUI package assets and selected backend before changing the application. Keep the pinned older example for comparison.
2. Apply the demonstrated current configuration at the public seam above. Pin dependencies and restore a reviewed lock file; retain the stated no-store, subscription, disposal and template-option boundaries where applicable.
3. Build the selected canonical consumer and run its focused checks. Treat unavailable native compilation/interaction and declared contract doubles exactly as described below; do not report them as rendered behavior.

## Evidence

The actual first-party package was installed into an isolated template hive and exercised for C# UI, resource retention and the incompatible sample-content option. The original net10 Windows app compiled and reached Label.Loaded; a managed button event updated its count, then its window closed. That check is not pointer interaction or visual/accessibility approval. The exact net11 Windows sample failed before compilation with NETSDK1147 because the SDK11 installation has no usable MAUI SDK pack. Its native compilation/interaction is explicitly not-tested under PIN-699. The native11 restore never completed, so that sample has no packages.lock.json and its dependency/workload declarations remain empty. RestorePackagesWithLockFile requests a lock on a future successful restore; it is not proof that a resolved lock exists. Do not invent a lock or treat this head as a locked build. Do not turn a template-generation pass into native app proof.

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

### samples/net11.0/QuayCSharp/Directory.Build.props

{{snippet:samples-net11-0-quaycsharp-directory-build-props}}

### samples/net11.0/QuayCSharp/Directory.Build.targets

{{snippet:samples-net11-0-quaycsharp-directory-build-targets}}

### samples/net11.0/QuayCSharp/Directory.Packages.props

{{snippet:samples-net11-0-quaycsharp-directory-packages-props}}

### samples/net11.0/QuayCSharp/global.json

{{snippet:samples-net11-0-quaycsharp-global-json}}

### samples/net11.0/QuayCSharp/MauiProgram.cs

{{snippet:samples-net11-0-quaycsharp-mauiprogram-cs}}

### samples/net11.0/QuayCSharp/Platforms/Windows/App.xaml

{{snippet:samples-net11-0-quaycsharp-platforms-windows-app-xaml}}

### samples/net11.0/QuayCSharp/Platforms/Windows/App.xaml.cs

{{snippet:samples-net11-0-quaycsharp-platforms-windows-app-xaml-cs}}

### samples/net11.0/QuayCSharp/QuayCSharp.csproj

{{snippet:samples-net11-0-quaycsharp-quaycsharp-csproj}}

### samples/net11.0/QuayCSharp/Resources/AppIcon/quay.svg

{{snippet:samples-net11-0-quaycsharp-resources-appicon-quay-svg}}

### samples/net11.0/QuayCSharp/Resources/Styles/Quay.xaml

{{snippet:samples-net11-0-quaycsharp-resources-styles-quay-xaml}}

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
