// snippet:complete
using Microsoft.AspNetCore.Components.WebAssembly.Infrastructure;
using Microsoft.JSInterop;

namespace MineralArchive;

// Historical adapter fixture only: never called by the supported app.
// It mutates browser history first; the notification itself does NOT navigate.
public static class LegacyAdapter
{
    public static async Task OpenAsync(IJSRuntime js, string absoluteUri)
    {
        await js.InvokeVoidAsync("legacyArchive.push", absoluteUri);
#pragma warning disable CS0618 // Deliberate removed-overload compile comparison only.
        JSInteropMethods.NotifyLocationChanged(absoluteUri, false);
#pragma warning restore CS0618
    }
}
// endsnippet:complete
