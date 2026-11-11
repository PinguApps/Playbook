// snippet:complete
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

static class Freshness
{
    public const string StateKey = "observatory.effective-max-age";
    public static Task Capture(RedirectContext context) {
        // Run AFTER application redirect customizations, BEFORE state is protected.
        var value = context.ProtocolMessage.MaxAge;
        context.Properties.Items.Remove(StateKey);
        if (value is not null) {
            if (!TryMaxAge(value, out _)) throw new InvalidOperationException("max_age must be invariant nonnegative integral seconds");
            context.Properties.Items[StateKey] = value;
        }
        return Task.CompletedTask;
    }
    public static Task Validate(TokenValidatedContext context) {
        if (context.Properties!.Items.TryGetValue(StateKey, out var maxAge)) {
            var encoded = (object)context.SecurityToken switch {
                JsonWebToken token => token.EncodedToken,
                JwtSecurityToken token => token.RawData,
                _ => throw new InvalidOperationException("Unsupported validated token representation")
            };
            // Inspect the signed payload, not claims from UserInfo or a mapped/stringified principal.
            using var payload = JsonDocument.Parse(Base64UrlEncoder.DecodeBytes(encoded.Split('.')[1]));
            if (!IsValid(payload.RootElement, maxAge, DateTimeOffset.UtcNow,
                context.Options.TokenValidationParameters.ClockSkew)) context.Fail("Requested authentication freshness was not established");
        }
        return Task.CompletedTask;
    }
    public static bool TryMaxAge(string? value, out long seconds) =>
        long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out seconds) && seconds >= 0;
    public static bool IsValid(JsonElement payload, string? maxAge, DateTimeOffset now, TimeSpan skew) {
        if (!TryMaxAge(maxAge, out var age) || !payload.TryGetProperty("auth_time", out var claim) ||
            claim.ValueKind != JsonValueKind.Number || !claim.TryGetInt64(out var authenticated) ||
            authenticated < 0 || authenticated > DateTimeOffset.MaxValue.ToUnixTimeSeconds()) return false;
        var elapsed = (decimal)(now - DateTimeOffset.UnixEpoch).Ticks / TimeSpan.TicksPerSecond - authenticated;
        var allowance = (decimal)skew.Ticks / TimeSpan.TicksPerSecond;
        return elapsed >= -allowance && elapsed <= age + allowance;
    }
}
// endsnippet:complete


