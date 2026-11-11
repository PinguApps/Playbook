// snippet:complete
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

BoundaryChecks.Run();
using var rsa = RSA.Create(2048);
using var wrongRsa = RSA.Create(2048);
var key = new RsaSecurityKey(rsa) { KeyId = "observatory-local" };
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Logging.ClearProviders();
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
var guarded = !args.Contains("--observe-builtin", StringComparer.Ordinal);
var authentication = builder.Services.AddAuthentication("session").AddCookie("session");
foreach (var source in new[] { "options", "properties", "additional", "redirect", "absent" })
authentication.AddOpenIdConnect(source, options => {
    options.SignInScheme = "session";
    options.ClientId = "observatory";
    options.Configuration = new OpenIdConnectConfiguration {
        Issuer = "https://identity.observatory.example", AuthorizationEndpoint = "https://identity.observatory.example/authorize",
        SigningKeys = { key }
    };
    options.RequireHttpsMetadata = false;
    options.ResponseType = "id_token";
    options.ResponseMode = "form_post";
    options.CallbackPath = "/signin-" + source;
    if (source == "options") options.MaxAge = TimeSpan.FromSeconds(60);
    if (source == "additional") options.AdditionalAuthorizationParameters["max_age"] = "60";

    options.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(5);
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.None;
    options.NonceCookie.SecurePolicy = CookieSecurePolicy.None;
        {
        options.Events.OnRedirectToIdentityProvider = context => {
            var requested = context.HttpContext.Request.Query["source"].ToString();
            if (requested == "redirect") context.ProtocolMessage.MaxAge = "60";
            return guarded ? Freshness.Capture(context) : Task.CompletedTask;
        };
        if (guarded) options.Events.OnTokenValidated = Freshness.Validate;
    }
    options.Events.OnTicketReceived = async context => {
        context.HandleResponse(); await context.Response.WriteAsync("accepted");
    };
    options.Events.OnRemoteFailure = async context => {
        context.HandleResponse(); context.Response.StatusCode = 400;
        await context.Response.WriteAsync("rejected:" + context.Failure?.Message);
    };
});
await using var app = builder.Build();
app.UseAuthentication();
app.MapGet("/challenge", async context => {
    var properties = new OpenIdConnectChallengeProperties();
    if (context.Request.Query["source"] == "properties") properties.MaxAge = TimeSpan.FromSeconds(60);
    await context.ChallengeAsync(context.Request.Query["source"].ToString(), properties);
});
await app.StartAsync();
using var transport = new HttpClientHandler { CheckCertificateRevocationList = true, AllowAutoRedirect = false, CookieContainer = new() };
using var client = new HttpClient(transport) { BaseAddress = new Uri(app.Urls.Single()) };

foreach (var source in new[] { "options", "properties", "additional", "redirect", "absent" })
foreach(var scenario in new[] { "valid", "missing", "invalid", "string", "fractional", "negative", "range", "stale", "future", "wrong-signature", "wrong-nonce", "tampered-state" }) {
    var challenge = await client.GetAsync(new Uri("/challenge?source=" + source, UriKind.Relative));
    var query = QueryHelpers.ParseQuery(challenge.Headers.Location!.Query);
    if ((source != "absent") != query.ContainsKey("max_age")) throw new InvalidOperationException("Effective max_age missing or unexpected");
    var now = DateTimeOffset.UtcNow;
    var payload = new JwtPayload {
        ["iss"] = "https://identity.observatory.example", ["aud"] = "observatory", ["sub"] = "observer-1",
        ["iat"] = now.ToUnixTimeSeconds(), ["exp"] = now.AddMinutes(5).ToUnixTimeSeconds(),
        ["nonce"] = query["nonce"].ToString()
    };
    if (scenario != "missing") payload["auth_time"] = scenario switch {
        "invalid" => "not-a-date", "string" => now.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture),
        "fractional" => 1.5, "negative" => -1L, "range" => long.MaxValue, "stale" => (object)now.AddHours(-1).ToUnixTimeSeconds(),
        "future" => now.AddHours(1).ToUnixTimeSeconds(), _ => now.ToUnixTimeSeconds()
    };
    if (scenario == "wrong-nonce") payload["nonce"] = "different";
    var token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
        new JwtHeader(new SigningCredentials(scenario == "wrong-signature" ? new RsaSecurityKey(wrongRsa) { KeyId = key.KeyId } : key, SecurityAlgorithms.RsaSha256)), payload));
    var callback = await client.PostAsync(new Uri("/signin-" + source, UriKind.Relative), new FormUrlEncodedContent(new Dictionary<string,string> {
        ["id_token"] = token, ["state"] = scenario == "tampered-state" ? "broken" : query["state"].ToString()
    }));
    var result = await callback.Content.ReadAsStringAsync();
    Console.WriteLine($"OBSERVED guarded={guarded};source={source};scenario={scenario};status={(int)callback.StatusCode};result={result}");
    if (scenario == "valid" && result != "accepted") throw new InvalidOperationException("Valid signed token failed");
    var protocolFailure = scenario is "wrong-signature" or "wrong-nonce" or "tampered-state";
    var denied = protocolFailure || (guarded && source != "absent" && scenario != "valid");
    if ((result == "accepted") == denied) throw new InvalidOperationException("Unexpected authentication outcome: " + scenario);
}
await app.StopAsync();

// endsnippet:complete


