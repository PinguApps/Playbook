// snippet:complete
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AstronomyBadges;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Services.AddOidcAuthentication<RemoteAuthenticationState, BadgeAccount>(options =>
{
    options.ProviderOptions.Authority = "https://identity.example.invalid";
    options.ProviderOptions.ClientId = "astronomy-badges";
    options.UserOptions.NameClaim = "name";
});
builder.Services.AddScoped<BadgePrincipalFactory>();
builder.Services.AddScoped<AccountClaimsPrincipalFactory<BadgeAccount>>(sp => sp.GetRequiredService<BadgePrincipalFactory>());
builder.Services.AddScoped<BadgeAuthentication>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<BadgeAuthentication>());
builder.Services.AddScoped<IRemoteAuthenticationService<RemoteAuthenticationState>>(sp => sp.GetRequiredService<BadgeAuthentication>());
builder.Services.AddScoped<IAccessTokenProvider>(sp => sp.GetRequiredService<BadgeAuthentication>());
await builder.Build().RunAsync();
// endsnippet:complete
