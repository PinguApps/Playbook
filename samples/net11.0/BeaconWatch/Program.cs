// snippet:program
using BeaconWatch;
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
// The .NET 11 host starts registered IHostedService instances. No manual StartAsync.
await host.RunAsync();
// RunAsync has no public cancellation overload and normally never completes.
// endsnippet:program
