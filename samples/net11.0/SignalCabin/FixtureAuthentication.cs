// snippet:complete
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SignalCabin;

// Local proof only. A client-provided header is not production authentication.
public sealed class FixtureAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? crew = Request.Headers["X-Test-Crew"];
        if (string.IsNullOrEmpty(crew)) { return Task.FromResult(AuthenticateResult.NoResult()); }
        var identity = new ClaimsIdentity([new Claim("crew", crew)], Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}
// endsnippet:complete
