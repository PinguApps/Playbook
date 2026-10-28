// snippet:complete
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace AstronomyBadges;

public sealed class BadgeAuthentication : RemoteAuthenticationService<RemoteAuthenticationState, BadgeAccount, OidcProviderOptions>
{
#pragma warning disable CS0618 // Deliberate .NET 10 constructor migration fixture.
    public BadgeAuthentication(
        IJSRuntime js,
        IOptionsSnapshot<RemoteAuthenticationOptions<OidcProviderOptions>> options,
        NavigationManager navigation,
        AccountClaimsPrincipalFactory<BadgeAccount> accounts)
        : base(js, options, navigation, accounts) { }
#pragma warning restore CS0618
}
// endsnippet:complete
