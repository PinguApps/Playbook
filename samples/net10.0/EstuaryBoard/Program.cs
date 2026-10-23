// snippet:program
using EstuaryBoard;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Dashboard>("#app");
// .NET 10 loads JSON but requires an explicit environment provider.
string initial = builder.Configuration["Board:Endpoint"] ?? "missing";
if (Environment.GetEnvironmentVariable("BOARD_SKIP_PROVIDER") != "1")
{
    builder.Configuration.AddEnvironmentVariables();
}
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["Board:Caption"] = "Operator override"
});
// Negative migration control: retaining an old registration moves its values last.
if (Environment.GetEnvironmentVariable("BOARD_DUPLICATE") == "1")
{
    builder.Configuration.AddEnvironmentVariables();
}
var root = (IConfigurationRoot)builder.Configuration;
builder.Services.AddSingleton(new ConfigurationTrace(initial,
    root.Providers.Select(provider => new ProviderTrace(provider.GetType().Name,
        provider.TryGet("Board:Endpoint", out string? endpoint) ? endpoint : null,
        provider.TryGet("Board:Caption", out string? caption) ? caption : null)).ToArray(),
    root.Providers.Count(provider => provider is EnvironmentVariablesConfigurationProvider)));
await builder.Build().RunAsync();
// endsnippet:program
