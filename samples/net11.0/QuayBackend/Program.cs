// snippet:complete
using Microsoft.AspNetCore.Components.WebView.Maui;

// This workload-free consumer checks the policy callback contract, not WebView caching.
var view = new BlazorWebView { StaticContentCacheControlProvider = QuayCachePolicy.Select };
foreach (var (path, mime, expected) in new[]
{
    ("https://0.0.0.1/assets/quay-map-v1.svg", "image/svg+xml", "private, max-age=86400"),
    ("https://0.0.0.1/index.html", "text/html", "no-store"),
    ("https://0.0.0.1/assets/quay-map-v1.svg?account=7", "image/svg+xml", "no-store"),
    ("https://example.invalid/assets/quay-map-v1.svg", "image/svg+xml", "no-store"),
    ("https://0.0.0.1/assets/quay-map-v1.svg", "application/json", "no-store")
})
{
    var actual = view.StaticContentCacheControlProvider(new(new Uri(path), mime));
    if (actual != expected) throw new InvalidOperationException($"Unexpected policy for {path}: {actual}");
}
Console.WriteLine("PASS: local versioned map allow-list; HTML, query, foreign origin and wrong MIME remain no-store. No native WebView executed.");
await BackendContracts.VerifyAsync();

internal static class QuayCachePolicy
{
    public static string Select(BlazorWebViewStaticContentRequest request)
    {
        var uri = request.Uri;
        return uri.Scheme == "https" && uri.Host == "0.0.0.1" && uri.IsDefaultPort
            && uri.Query.Length == 0 && uri.Fragment.Length == 0
            && uri.AbsolutePath == "/assets/quay-map-v1.svg" && request.ContentType == "image/svg+xml"
                ? "private, max-age=86400" : "no-store";
    }
}
// endsnippet:complete
