// snippet:complete
using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

namespace AstronomyBadges;

public sealed class BadgeAccount : RemoteUserAccount
{
    [JsonPropertyName("badge")] public string? Badge { get; set; }
}

public sealed class BadgePrincipalFactory(IAccessTokenProviderAccessor tokens) : AccountClaimsPrincipalFactory<BadgeAccount>(tokens)
{
    public override async ValueTask<ClaimsPrincipal> CreateUserAsync(BadgeAccount account, RemoteAuthenticationUserOptions options)
    {
        var principal = await base.CreateUserAsync(account, options);
        if (principal.Identity is ClaimsIdentity { IsAuthenticated: true } identity && account.Badge is { Length: > 0 } badge)
        {
            identity.AddClaim(new Claim("club:badge", badge));
        }
        return principal;
    }
}
// endsnippet:complete
