// snippet:lifetime
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

namespace BeaconWatch;

public sealed class OwnedBrowserLifetime(WebAssemblyHost host)
{
    private Task? _shutdown;

    // Called by a button outside the rendered root, while this document is alive.
    [JSInvokable]
    public Task ShutdownAsync() => _shutdown ??= host.DisposeAsync().AsTask();
}
// endsnippet:lifetime
