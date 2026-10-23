// snippet:program
using LoomActivity;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
// .NET 11 registers component metrics/tracing when metrics are supported/enabled.
using var telemetry = new BrowserTelemetry("11.0");
builder.RootComponents.Add<App>("#app");
await builder.Build().RunAsync();
// endsnippet:program
