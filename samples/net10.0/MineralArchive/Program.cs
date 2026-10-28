// snippet:complete
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MineralArchive;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
await builder.Build().RunAsync();
// endsnippet:complete
