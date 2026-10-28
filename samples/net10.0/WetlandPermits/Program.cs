// snippet:complete
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using WetlandPermits;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.Services.AddOidcAuthentication(options =>
{
    options.ProviderOptions.Authority = "http://localhost:5980";
    options.ProviderOptions.ClientId = "wetland-permits";
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Add("profile");
    options.AuthenticationPaths.LogOutSucceededPath = "permits";
});
await builder.Build().RunAsync();
// endsnippet:complete
