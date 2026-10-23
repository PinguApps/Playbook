// snippet:main
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using QuaysideRoutes;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
await builder.Build().RunAsync();
// endsnippet:main
