// snippet:program
using BeaconWatch;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Dashboard>("#app");
builder.Services.AddHostedService<BeaconTicker>();
var host = builder.Build();
var lifetime = new OwnedBrowserLifetime(host);
using var reference = DotNetObjectReference.Create(lifetime);
try { await host.Services.GetRequiredService<IJSRuntime>().InvokeVoidAsync("beacon.attach", reference); }
catch (JSException error)
{
    await Console.Error.WriteLineAsync(error.ToString());
    throw;
}
// .NET 10 registers services but its WASM host does not start them.
// The unowned query is a negative verification control, not the normal route.
if (new Uri(host.Services.GetRequiredService<NavigationManager>().Uri).Query != "?unowned=1")
{
    foreach (var service in host.Services.GetServices<IHostedService>())
        await service.StartAsync(CancellationToken.None);
}
await host.RunAsync();
// RunAsync has no public cancellation overload and normally never completes.
// endsnippet:program
