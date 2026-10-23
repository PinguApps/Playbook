// snippet:program
using LoomActivity;
using Microsoft.AspNetCore.Components.Infrastructure;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
// .NET 10 exposes these public static methods; they are not extension methods.
ComponentsMetricsServiceCollectionExtensions.AddComponentsMetrics(builder.Services);
ComponentsMetricsServiceCollectionExtensions.AddComponentsTracing(builder.Services);
using var telemetry = new BrowserTelemetry("10.0");
builder.RootComponents.Add<App>("#app");
await builder.Build().RunAsync();
// endsnippet:program
