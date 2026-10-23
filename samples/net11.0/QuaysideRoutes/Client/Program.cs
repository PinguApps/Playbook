// snippet:main
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Hosting;
using QuaysideRoutes;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.AddBlazorClientServiceDefaults();
builder.Services.AddHttpClient("quays", client => client.BaseAddress = new Uri("http://quays"));
await builder.Build().RunAsync();
// endsnippet:main
