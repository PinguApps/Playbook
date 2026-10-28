// snippet:complete
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace AstronomyBadges;

// The same supported five-argument path is already available on .NET 10.
public sealed class SupportedBadgeAuthentication(
    IJSRuntime js,
    IOptionsSnapshot<RemoteAuthenticationOptions<OidcProviderOptions>> options,
    NavigationManager navigation,
    AccountClaimsPrincipalFactory<BadgeAccount> accounts,
    ILogger<RemoteAuthenticationService<RemoteAuthenticationState, BadgeAccount, OidcProviderOptions>> logger)
    : RemoteAuthenticationService<RemoteAuthenticationState, BadgeAccount, OidcProviderOptions>(js, options, navigation, accounts, logger);
// endsnippet:complete
