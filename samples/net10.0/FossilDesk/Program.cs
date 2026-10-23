// snippet:all
using FossilDesk;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Desk>("#app");
await builder.Build().RunAsync();
// endsnippet:all
