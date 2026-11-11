# Connect a backend through public MAUI contracts

## Goal

Integrate backend services, handler registration and neutral-target folder selection; this contract demonstration excludes rendering and real-view capture.

## Constraints

Current examples pin SDK11.0.100, C#15 and MAUI11.0.0. Older comparisons pin SDK10.0.403, C#14 and MAUI10.0.110. These pins are actual GA/.NET10 evidence, within the recorded verification scope certification or minimum-version promises. Keep independent SDK, native platform/workload and package-asset requirements separate. No provider, account or web-server rendering mode is implied.

The fictional Quay backend demonstrates supported integration contracts without implementing a new native renderer. The contract-only scope excludes rendering, dialogs, pixels and real-view capture. The screenshot result is an explicit test double whose pixel APIs throw.

## Older approach

.NET 10 supports its built-in platform handlers and existing platform-specific integrations. It does not expose this same neutral-backend contract set. Keep a maintained platform-specific implementation when necessary; do not backport the new interfaces by reflection or pretend that a user-defined shim is a first-party older API. The older consumer retains ordinary BlazorWebView host configuration.

## Newer approach

Register the screenshot implementation as IScreenshot and implement IViewScreenshot on that same instance. MAUI's neutral ViewExtensions.CaptureAsync resolves IScreenshot from the handler's MauiContext, checks IsCaptureSupported, then checks IViewScreenshot. An IViewScreenshot-only service registration is insufficient. Missing handler, platform view, service, capability or view interface yields null. The canonical check invokes the real MAUI extension with a contract token and verifies the handler-free null route. Its successful result proves dispatch, not captured pixels.

A backend-owned scope can give each window its own IAlertManager; this demonstration creates the scope explicitly. A real backend must connect that scope to its window lifetime rather than assume MAUI creates it automatically. The manager owns its own subscriptions. Subscribe must be idempotent and Unsubscribe paired; the demonstration counts those obligations but deliberately throws for requests requiring dialog rendering. Alternatively implement IAlertManagerSubscription for the default manager: it is not resolved when a custom IAlertManager replaces that manager. The public argument classes remain usable despite their Internals namespace. Obsolete page-busy notifications have no replacement; do not add new uses.

Register IGesturePlatformManagerFactory application-wide, returning a fresh disposable IGesturePlatformManager for every handler connection. The framework owns its lifetime and disposes it on disconnect/change. Never share one disposable manager across connections. The consumer verifies independent instances and isolated disposal; it does not claim to have exercised native pointer recognizers.

Call AddMauiBlazorWebView before UsePlatformHandler and apply the custom registration after every downstream Blazor configuration. Last registration wins; another later AddMauiBlazorWebView can restore the default handler. The consumer checks two competing custom registrations and a later default registration separately. The overload taking a factory receives the handler factory's service provider, not arbitrary root app services. Our registered handler intentionally refuses CreatePlatformView because there is no renderer.

For neutral SingleProject builds, contribute MauiPlatformSpecificFolder with BackendIdentity=quay and select MauiActiveBackend=quay. The actual packaged Before/SingleProject targets normalize this metadata and keep only the active backend's folder; the supporting Selection.proj tests active and inactive selection without compiling a native platform. Recognized TargetPlatformIdentifiers remain a separate activation route. Do not rely on malformed/empty activation metadata to include platform files. The complete raw-asset metadata and actual package conversion example are in QuayAvalonia: MauiAsset uses an explicit LogicalName, keeping application resources independent of platform source folders. Image/font/icon processing still needs the matched Resizetizer and backend integration. Recognized OnPlatform GTK/macOS/WPF names select backend branches; they do not install or implement those backends.

{{snippet:samples-net11-0-quaybackend-program-cs}}

## Migration

1. Inspect the actual target, SDK, MAUI package assets and selected backend before changing the application. Keep the pinned older example for comparison.
2. Apply the demonstrated current configuration at the public seam above. Pin dependencies and restore a reviewed lock file; retain the stated no-store, subscription, disposal and template-option boundaries where applicable.
3. Build the selected canonical consumer and run its focused checks. Treat unavailable native compilation/interaction and declared contract doubles exactly as described below; do not report them as rendered behavior.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The actual MAUI11 neutral assemblies compile and execute service, registration and screenshot-dispatch contracts. SingleProject selection uses the actual package targets, with both active and inactive results checked. These are contract checks with a declared screenshot double, not native rendering or device evidence. The Avalonia raw-asset consumer compiles separately. No external service, credentials, account or Human approval is required for this agreed scope. No third-party backend package is published or provisioned.

## Canonical sources

### samples/net10.0/QuayBackend/Directory.Build.props

{{snippet:samples-net10-0-quaybackend-directory-build-props}}

### samples/net10.0/QuayBackend/Directory.Build.targets

{{snippet:samples-net10-0-quaybackend-directory-build-targets}}

### samples/net10.0/QuayBackend/Directory.Packages.props

{{snippet:samples-net10-0-quaybackend-directory-packages-props}}

### samples/net10.0/QuayBackend/global.json

{{snippet:samples-net10-0-quaybackend-global-json}}

### samples/net10.0/QuayBackend/Program.cs

{{snippet:samples-net10-0-quaybackend-program-cs}}

### samples/net10.0/QuayBackend/QuayBackend.csproj

{{snippet:samples-net10-0-quaybackend-quaybackend-csproj}}

### samples/net11.0/QuayBackend/BackendContracts.cs

{{snippet:samples-net11-0-quaybackend-backendcontracts-cs}}

### samples/net11.0/QuayBackend/BackendSelection/Platforms/Other/OtherMarker.cs

{{snippet:samples-net11-0-quaybackend-backendselection-platforms-other-othermarker-cs}}

### samples/net11.0/QuayBackend/BackendSelection/Platforms/Quay/QuayMarker.cs

{{snippet:samples-net11-0-quaybackend-backendselection-platforms-quay-quaymarker-cs}}

### samples/net11.0/QuayBackend/BackendSelection/Selection.proj

{{snippet:samples-net11-0-quaybackend-backendselection-selection-proj}}

### samples/net11.0/QuayBackend/Directory.Build.props

{{snippet:samples-net11-0-quaybackend-directory-build-props}}

### samples/net11.0/QuayBackend/Directory.Build.targets

{{snippet:samples-net11-0-quaybackend-directory-build-targets}}

### samples/net11.0/QuayBackend/Directory.Packages.props

{{snippet:samples-net11-0-quaybackend-directory-packages-props}}

### samples/net11.0/QuayBackend/global.json

{{snippet:samples-net11-0-quaybackend-global-json}}

### samples/net11.0/QuayBackend/Program.cs

{{snippet:samples-net11-0-quaybackend-program-cs}}

### samples/net11.0/QuayBackend/QuayBackend.csproj

{{snippet:samples-net11-0-quaybackend-quaybackend-csproj}}

### Raw asset package consumer

{{snippet:quay-asset-project}}

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
