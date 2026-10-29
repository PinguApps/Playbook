// snippet:complete
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

// Disposable controlled provider, NOT an OAuth/OIDC implementation or third-party interoperability proof.
internal sealed class LocalAuthority
{
    private readonly ConcurrentDictionary<string, string> codes = new();
    // Test driver selects an already-linked provider identity for a later real challenge.
    public string? Subject { get; set; }
    public string Issue()
    {
        var code = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        codes[code] = Subject ?? Guid.NewGuid().ToString("N");
        return code;
    }
    public string? Redeem(string code) => codes.TryRemove(code, out var subject) ? subject : null;
}

internal sealed class LocalProviderOptions : RemoteAuthenticationOptions;

internal sealed class LocalProviderHandler(
    IOptionsMonitor<LocalProviderOptions> options, ILoggerFactory logger, UrlEncoder encoder,
    IDataProtectionProvider protection, LocalAuthority authority)
    : RemoteAuthenticationHandler<LocalProviderOptions>(options, logger, encoder)
{
    private readonly PropertiesDataFormat stateFormat = new(protection.CreateProtector("Aquarium.local-provider.state.v1"));

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        GenerateCorrelationId(properties);
        var state = Uri.EscapeDataString(stateFormat.Protect(properties));
        Response.Redirect($"/fixture/authority?state={state}&code={authority.Issue()}");
        return Task.CompletedTask;
    }

    protected override Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
    {
        var properties = stateFormat.Unprotect(Request.Query["state"].ToString());
        if (properties is null || !ValidateCorrelationId(properties))
            return Task.FromResult(HandleRequestResult.Fail("Invalid local provider state/correlation."));
        var subject = authority.Redeem(Request.Query["code"].ToString());
        if (subject is null) return Task.FromResult(HandleRequestResult.Fail("Invalid or reused local provider code."));
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, subject),
            new Claim(ClaimTypes.Email, $"visitor-{subject}@example.invalid")
        ], Scheme.Name);
        return Task.FromResult(HandleRequestResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), properties, Scheme.Name)));
    }
}
// endsnippet:complete
