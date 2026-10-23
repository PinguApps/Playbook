// snippet:lifetime
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.JSInterop;

namespace BeaconWatch;

public sealed class OwnedBrowserLifetime(WebAssemblyHost host)
{
    private Task? _shutdown;

    // Called by a button outside the rendered root, while this document is alive.
    [JSInvokable]
    public Task ShutdownAsync() => _shutdown ??= StopAndDisposeAsync();

    private async Task StopAndDisposeAsync()
    {
        // .NET 10 also needs an owner for the registered services' StopAsync.
        foreach (var service in host.Services.GetServices<IHostedService>())
            await service.StopAsync(CancellationToken.None);
        await host.DisposeAsync();
    }
}
// endsnippet:lifetime
