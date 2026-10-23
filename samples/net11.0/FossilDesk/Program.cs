// snippet:all
using FossilDesk;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Desk>("#app");
var host = builder.Build();
WorkerProbe.Runtime = host.Services.GetRequiredService<Microsoft.JSInterop.IJSRuntime>();
await host.RunAsync();
// endsnippet:all
