# Call a typed .NET action from local HybridWebView content

## Goal

Register a generated HybridWebView invocation target and JSON context for trimmed JavaScript-to-.NET dispatch; Apple NativeAOT publishing remains experimental.

## Constraints

The fictional Marsh Survey app records a bird count from bundled HTML. .NET10 uses SDK10.0.403, C#14 and Controls10.0.110; .NET11 uses SDK11.0.100, C#15 and Controls11.0.0. These are pinned preparation baselines, within the recorded verification scope execution evidence. No account, provider or web rendering mode is required.

## Generated dispatch and JSON

The .NET10 one-argument registration is a valid reflection-based route for an app that retains the required code and metadata. It is not a full-trimming or NativeAOT guarantee. On .NET11 use the two-argument generic overload with the concrete SurveyActions type and generated SurveyJsonContext. Keep Microsoft.Maui.Core.HybridWebViewSourceGen from Controls.Build.Tasks enabled: DisableMauiAnalyzers must not be true and _MauiHybridWebViewInterceptorsSupport must not be false. The imported targets add Microsoft.Maui.Controls.Generated to InterceptorsPreviewNamespaces. Reaching the unintercepted overload throws; suppressing warnings does not repair missing interception.

JsonSerializable covers both request and receipt. Add every new payload type before exposing another method. This concerns calls from JavaScript to .NET, not EvaluateJavaScriptAsync. The framework serves _framework/hybridwebview.js; bundle only the original HTML below. Expose a deliberately small action object, keep navigation restricted to trusted local content, and validate real domain inputs before performing privileged actions.

For a matched Apple toolchain, build Release first, then publish the net11.0-ios27.0 project for ios-arm64 with PublishAot=true. Apple NativeAOT is **experimental**; full trimming and actual native publication were not run here. Inspect emitted interceptor code and trim/AOT diagnostics and exercise the button on the actual published app before claiming that configuration works. Expected receipt: Reedbank, 5 birds. Do not claim a NativeAOT result from Debug or interpreter execution.

## Older approach

Use the net10.0 canonical project below; its public API route remains valid within the stated limits.

## Newer approach

Use the net11.0 canonical project below with its matched platform packages.

{{snippet:samples-net11-0-marshbridge-mauiprogram-cs}}

## Migration

1. Select the matched SDK, workload and package pins from the canonical project.
2. Apply the registration from the current sample to the custom control or invocation target.
3. Keep the older project when its behavior is sufficient; do not suppress trim diagnostics.
4. Build Release and run the described native checks before relying on platform behavior.

## Canonical previous and current samples

### samples/net10.0/MarshBridge/Directory.Build.props

{{snippet:samples-net10-0-marshbridge-directory-build-props}}

### samples/net10.0/MarshBridge/Directory.Build.targets

{{snippet:samples-net10-0-marshbridge-directory-build-targets}}

### samples/net10.0/MarshBridge/Directory.Packages.props

{{snippet:samples-net10-0-marshbridge-directory-packages-props}}

### samples/net10.0/MarshBridge/global.json

{{snippet:samples-net10-0-marshbridge-global-json}}

### samples/net10.0/MarshBridge/MarshBridge.csproj

{{snippet:samples-net10-0-marshbridge-marshbridge-csproj}}

### samples/net10.0/MarshBridge/MauiProgram.cs

{{snippet:samples-net10-0-marshbridge-mauiprogram-cs}}

### samples/net10.0/MarshBridge/Platforms/iOS/AppDelegate.cs

{{snippet:samples-net10-0-marshbridge-platforms-ios-appdelegate-cs}}

### samples/net10.0/MarshBridge/Platforms/iOS/Info.plist

{{snippet:samples-net10-0-marshbridge-platforms-ios-info-plist}}

### samples/net10.0/MarshBridge/Platforms/iOS/Program.cs

{{snippet:samples-net10-0-marshbridge-platforms-ios-program-cs}}

### samples/net10.0/MarshBridge/Platforms/iOS/SceneDelegate.cs

{{snippet:samples-net10-0-marshbridge-platforms-ios-scenedelegate-cs}}

### samples/net10.0/MarshBridge/Resources/AppIcon/appicon.svg

{{snippet:samples-net10-0-marshbridge-resources-appicon-appicon-svg}}

### samples/net10.0/MarshBridge/Resources/Raw/wwwroot/index.html

{{snippet:samples-net10-0-marshbridge-resources-raw-wwwroot-index-html}}

### samples/net11.0/MarshBridge/Directory.Build.props

{{snippet:samples-net11-0-marshbridge-directory-build-props}}

### samples/net11.0/MarshBridge/Directory.Build.targets

{{snippet:samples-net11-0-marshbridge-directory-build-targets}}

### samples/net11.0/MarshBridge/Directory.Packages.props

{{snippet:samples-net11-0-marshbridge-directory-packages-props}}

### samples/net11.0/MarshBridge/global.json

{{snippet:samples-net11-0-marshbridge-global-json}}

### samples/net11.0/MarshBridge/MarshBridge.csproj

{{snippet:samples-net11-0-marshbridge-marshbridge-csproj}}

### samples/net11.0/MarshBridge/MauiProgram.cs

{{snippet:samples-net11-0-marshbridge-mauiprogram-cs}}

### samples/net11.0/MarshBridge/Platforms/iOS/AppDelegate.cs

{{snippet:samples-net11-0-marshbridge-platforms-ios-appdelegate-cs}}

### samples/net11.0/MarshBridge/Platforms/iOS/Info.plist

{{snippet:samples-net11-0-marshbridge-platforms-ios-info-plist}}

### samples/net11.0/MarshBridge/Platforms/iOS/Program.cs

{{snippet:samples-net11-0-marshbridge-platforms-ios-program-cs}}

### samples/net11.0/MarshBridge/Platforms/iOS/SceneDelegate.cs

{{snippet:samples-net11-0-marshbridge-platforms-ios-scenedelegate-cs}}

### samples/net11.0/MarshBridge/Resources/AppIcon/appicon.svg

{{snippet:samples-net11-0-marshbridge-resources-appicon-appicon-svg}}

### samples/net11.0/MarshBridge/Resources/Raw/wwwroot/index.html

{{snippet:samples-net11-0-marshbridge-resources-raw-wwwroot-index-html}}

## Generated-dispatch consumer

The workload-free MarshDispatch consumer uses the actual neutral MAUI assemblies and packaged generator. It publishes and executes on Windows x64 with NativeAOT, asserting generated request deserialization and response serialization. This does not exercise the iOS WebView, its local content transport, or Apple publishing. The pinned GA ILCompiler emits NU1903 for GHSA-p3jp-9v83-76w9; this historical authoring baseline is not a production toolchain recommendation.

{{snippet:marsh-dispatch}}

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Official source hashes are retained in samples/MarshInterop/sources.json and checked by eng/Test-MarshInterop.ps1. Evidence records distinguish source review from compilation. iOS compilation, trimming, NativeAOT publication and WebView interaction excluded: Mac/Xcode unavailable. No native execution or GA verification is claimed. Installed-package list/get verifies offline content delivery only. No verified older-target route is declared for this edition.

## Official references

- [HybridWebView.cs official source](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Core/HybridWebView/HybridWebView.cs)
- [HybridWebViewInvokeTargetGenerator.cs official source](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Core/src/HybridWebViewSourceGen/HybridWebViewInvokeTargetGenerator.cs)
- [Microsoft.Maui.Controls.targets official source](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Controls/src/Build.Tasks/nuget/buildTransitive/netstandard2.0/Microsoft.Maui.Controls.targets)
- [HybridWebView.cs official source](https://raw.githubusercontent.com/dotnet/maui/9d881e94b1c56692e40a16262897677a3bb46fcb/src/Controls/src/Core/HybridWebView/HybridWebView.cs)
- [HybridWebViewHandler.cs official source](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/Core/src/Handlers/HybridWebView/HybridWebViewHandler.cs)
