# Cache selected local BlazorWebView assets

## Goal

Choose cache headers for versioned bundled assets while retaining no-store for everything else.

## Constraints

Current examples pin SDK11.0.100, C#15 and MAUI11.0.0. Older comparisons pin SDK10.0.403, C#14 and MAUI10.0.110. These pins identify the recorded GA/.NET10 artifacts. They do not establish native execution or minimum supported versions. Keep independent SDK, native platform/workload and package-asset requirements separate. No provider, account or web-server rendering mode is implied.

The fictional Quay Ledger application bundles a versioned harbour map. Only that immutable public asset is eligible for reuse. Keep documents, personalized data and authentication responses out of this policy.

## Older approach

.NET 10's BlazorWebView retains its default no-cache, max-age=0, must-revalidate, no-store static-content response. There is no equivalent first-party per-resource cache callback on that baseline. Keeping the default is valid; do not replace it with an invented historical API or cache sensitive content to imitate the new option.

## Newer approach

Assign StaticContentCacheControlProvider before navigation on the BlazorWebView that hosts the app. The canonical consumer assigns and exercises the actual packaged property. The callback accepts only the exact local HTTPS origin, versioned path and MIME type; it rejects query strings and every other resource with no-store. The local origin in this example is https://0.0.0.1; match the actual host configuration rather than accepting any origin or user-controlled path. Bump the file name when changing its bytes. Cache-Control private is not encryption or access control.

The callback selects response headers for local static content handled by BlazorWebView. It is not an HTTP response cache, service worker, remote fetch interceptor, browser profile purge or authenticated-data store. Requests not served as local static assets do not acquire this policy. Android uses a bounded per-WebView in-memory response cache because intercepted WebResourceResponse objects do not enter Chromium HTTP cache. Non-GET requests, Range, Authorization and request no-store disable that cache; request no-cache/max-age=0/Pragma no-cache force refresh. Response no-store/no-cache or missing/non-positive max-age is ineligible; Apple custom-scheme handling and Windows request handling retain platform-specific limits. Do not promise POST interception, remote-request caching, guaranteed cache hits or eviction behavior. Engines remain free to expire or evict entries. Host documents and framework scripts stay on the default/no-store route unless deliberately reviewed.

Return null, empty or whitespace to retain the framework default. Malformed URIs, callback exceptions and CR/LF header values fall back to that default. The callback may run on background request threads; use immutable decisions and avoid UI state. The sample's explicit no-store fallback is intentionally stricter and simpler than allowing arbitrary user-supplied header strings.

{{snippet:samples-net11-0-quaybackend-program-cs}}

## Migration

1. Inspect the actual target, SDK, MAUI package assets and selected backend before changing the application. Keep the pinned older example for comparison.
2. Apply the demonstrated current configuration at the public seam above. Pin dependencies and restore a reviewed lock file; retain the stated no-store, subscription, disposal and template-option boundaries where applicable.
3. Build the selected canonical consumer and run its focused checks. Treat unavailable native compilation/interaction and declared contract doubles exactly as described below; do not report them as rendered behavior.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

The real neutral net10 and net11 package consumers compile and run. The newer consumer tests the callback allow-list, not native response headers, disk persistence or reload flicker. Native .NET 11 WebView interaction is explicitly not-tested because its required MAUI SDK pack is unavailable on this Windows authoring machine. Apple/Android/device paths are also not-tested under PIN-699. No native cache-hit claim or Human approval is fabricated.

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
- [Pinned WebKitWebViewClient.cs](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/BlazorWebView/src/Maui/Android/WebKitWebViewClient.cs)
- [Pinned StaticContentResponseCache.cs](https://raw.githubusercontent.com/dotnet/maui/d32b79928709cf53b671336dabd0caef5a679fc2/src/BlazorWebView/src/Maui/StaticContentResponseCache.cs)
