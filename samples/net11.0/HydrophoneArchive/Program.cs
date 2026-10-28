// snippet:complete
using HydrophoneArchive;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
// A controlled provider boundary, not an identity provider or a token issuer.
builder.Services.AddScoped<ArchiveProvider>();
builder.Services.AddScoped<IAccessTokenProvider>(services => services.GetRequiredService<ArchiveProvider>());
await builder.Build().RunAsync();
// endsnippet:complete
