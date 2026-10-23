// snippet:complete
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ParcelStartup.LegacyClient;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<ParcelCounter>("#app");
await builder.Build().RunAsync();
// endsnippet:complete
