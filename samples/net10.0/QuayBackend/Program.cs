// snippet:complete
using Microsoft.AspNetCore.Components.WebView.Maui;
// .NET 10 keeps its default static-content no-store behaviour.
// There is no equivalent first-party per-resource cache callback or backend contract set.
// Retain platform-specific integration where already maintained; do not invent an API shim.
var view = new BlazorWebView { HostPage = "wwwroot/index.html" };
if (view.HostPage != "wwwroot/index.html") throw new InvalidOperationException("Host content path lost.");
Console.WriteLine("PASS: .NET 10 BlazorWebView host configuration; no cache override or new backend interfaces claimed.");
// endsnippet:complete
