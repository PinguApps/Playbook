// snippet:complete
using Microsoft.JSInterop;

namespace Planetarium;

// Forward to the real browser runtime through its public IJSRuntime surface.
// The base renderer uses the ordinary interop path with the runtime's own
// DotNetObjectReference converters, rather than requiring its internal WASM options.
internal sealed class RendererServices(IServiceProvider services) : IServiceProvider
{
    private readonly IJSRuntime runtime = new ForwardingRuntime(services.GetRequiredService<IJSRuntime>());
    public object? GetService(Type serviceType) => serviceType == typeof(IJSRuntime) ? runtime : services.GetService(serviceType);

    private sealed class ForwardingRuntime(IJSRuntime browser) : IJSRuntime
    {
        public async ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args)
        {
            try { return await browser.InvokeAsync<T>(identifier, args); }
            catch (JSException error) { throw new InvalidOperationException("Browser interop failed.", error); }
        }

        public async ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            try { return await browser.InvokeAsync<T>(identifier, cancellationToken, args); }
            catch (JSException error) { throw new InvalidOperationException("Browser interop failed.", error); }
        }
    }
}
// endsnippet:complete
