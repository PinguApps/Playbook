// snippet:complete
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace AstronomyBadges;

public sealed class BadgeAuthentication : RemoteAuthenticationService<RemoteAuthenticationState, BadgeAccount, OidcProviderOptions>
{
    public BadgeAuthentication(
        IJSRuntime js,
        IOptionsSnapshot<RemoteAuthenticationOptions<OidcProviderOptions>> options,
        NavigationManager navigation,
        AccountClaimsPrincipalFactory<BadgeAccount> accounts,
        ILogger<RemoteAuthenticationService<RemoteAuthenticationState, BadgeAccount, OidcProviderOptions>> logger)
        : base(js, options, navigation, accounts, logger) { }
}
// endsnippet:complete
