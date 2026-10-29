// snippet:complete
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using CopperRidgeHub;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddAuthentication("LocalTicket").AddScheme<AuthenticationSchemeOptions, LocalTicket>("LocalTicket", null);
builder.Services.AddAuthorization();
builder.Services.AddSignalR();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
// snippet:refresh
app.MapHub<QuarryHub>("/quarry", options =>
{
    options.EnableAuthenticationRefresh = true;
    options.CloseOnAuthenticationExpiration = true;
    options.OnAuthenticationRefresh = context => Task.FromResult(context.NewUser.FindFirst("clearance")?.Value is "visitor" or "surveyor");
}).RequireAuthorization();
// endsnippet:refresh
// A local negotiate redirect is a client constraint check, not Azure Service proof.
app.MapPost("/gateway/negotiate", (HttpContext context) =>
{
    if (!AuthenticationHeaderValue.TryParse(context.Request.Headers.Authorization.ToString(), out var bearer) ||
        !string.Equals(bearer.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(bearer.Parameter)) return Results.Unauthorized();
    return Results.Json(new { url = HubAddress(app), accessToken = bearer.Parameter });
}).RequireAuthorization();
app.MapPost("/gateway/refresh", async (HttpContext context) =>
{
    if (!AuthenticationHeaderValue.TryParse(context.Request.Headers.Authorization.ToString(), out var bearer) ||
        !string.Equals(bearer.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(bearer.Parameter)) return Results.Unauthorized();
    using var client = new HttpClient();
    // Select the trusted local binding; request Host cannot redirect credentials.
    using var request = new HttpRequestMessage(HttpMethod.Post, $"{HubAddress(app)}/refresh?id={Uri.EscapeDataString(context.Request.Query["id"].ToString())}");
    request.Headers.Authorization = bearer;
    using var response = await client.SendAsync(request);
    if (!response.IsSuccessStatusCode) return Results.StatusCode((int)response.StatusCode);
    var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
    return Results.Json(new { tokenLifetimeSeconds = body.GetProperty("tokenLifetimeSeconds").GetDouble(), accessToken = bearer.Parameter });
}).RequireAuthorization();
Console.WriteLine("Sample runtime: " + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
if (args.Contains("--serve", StringComparer.Ordinal)) { await app.RunAsync(); return; }
app.Urls.Add("http://127.0.0.1:0");
await app.StartAsync();
try
{
    var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    await CheckConnection(address + "/quarry");
    await CheckConnection(address + "/gateway");
    using var client = new HttpClient();
    using var missing = await client.PostAsync(new Uri(address + "/gateway/refresh"), null);
    Require(missing.StatusCode == System.Net.HttpStatusCode.Unauthorized, "Missing gateway credentials must return 401");
    using var redirect = new HttpRequestMessage(HttpMethod.Post, address + "/gateway/negotiate?negotiateVersion=1");
    redirect.Headers.Host = "127.0.0.1:1";
    redirect.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "mara:visitor");
    using var negotiated = await client.SendAsync(redirect);
    negotiated.EnsureSuccessStatusCode();
    var negotiation = await negotiated.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
    Require(negotiation.GetProperty("url").GetString() == address + "/quarry", "Host header changed redirect target");
    Console.WriteLine("PASS: gateway requires authentication and ignores untrusted Host when selecting its local target.");
    Console.WriteLine("PASS: direct and redirected real .NET hub refresh; accepted claims, rejected policy/user switch, events, stable connection and user routing.");
}
finally { await app.StopAsync(); await app.DisposeAsync(); }

static string HubAddress(WebApplication application) => application.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single() + "/quarry";

static async Task CheckConnection(string address)
{
    string ticket = "mara:visitor";
    int refreshed = 0, failed = 0, hubEvents = 0;
    // snippet:client
    await using var connection = new HubConnectionBuilder()
        .WithUrl(address, options => options.AccessTokenProvider = () => Task.FromResult<string?>(ticket))
        .WithAuthenticationRefresh(options => options.EnableAutoRefresh = false)
        .Build();
    connection.AuthenticationRefreshed += _ => { refreshed++; return Task.CompletedTask; };
    connection.AuthenticationRefreshFailed += _ => { failed++; return Task.CompletedTask; };
    connection.On("ClearanceChanged", () => hubEvents++);
    await connection.StartAsync();
    var original = await connection.InvokeAsync<QuarryIdentity>("Read");
    ticket = "mara:surveyor";
    var lifetime = await connection.RefreshAuthenticationAsync();
    var updated = await connection.InvokeAsync<QuarryIdentity>("Read");
    // endsnippet:client
    Require(updated.Clearance == "surveyor" && lifetime > TimeSpan.Zero, "Accepted refresh missing");
    Require(updated.Connection == original.Connection && updated.User == original.User, "Refresh changed routing");
    foreach (var rejected in new[] { "mara:suspended", "mara:", "mara:unknown", "other:surveyor" })
    {
        ticket = rejected;
        try { await connection.RefreshAuthenticationAsync(); throw new InvalidOperationException("Expected rejection"); }
        catch (HttpRequestException error) when (error.StatusCode == System.Net.HttpStatusCode.Forbidden) { }
        Require((await connection.InvokeAsync<QuarryIdentity>("Read")) == updated, "Rejected identity leaked");
    }
    // Invocation ordering ensures the server's preceding refresh notification arrived.
    Require(refreshed == 1 && failed == 4 && hubEvents == 1, "Refresh events missing");
    await connection.StopAsync();
}
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }

namespace CopperRidgeHub
{
public sealed record QuarryIdentity(string Connection, string? User, string? Clearance);
public sealed class QuarryHub : Hub
{
    public QuarryIdentity Read() => new(Context.ConnectionId, Context.UserIdentifier, Context.User?.FindFirst("clearance")?.Value);
    public override Task OnAuthenticationRefreshedAsync() => Clients.Caller.SendAsync("ClearanceChanged");
}

// Local deterministic tickets are test data, not production bearer-token validation.
public sealed class LocalTicket(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = Request.Headers.Authorization.ToString();
        if (token.StartsWith("Bearer ", StringComparison.Ordinal)) token = token[7..];
        else if (Request.Path.StartsWithSegments("/quarry", StringComparison.OrdinalIgnoreCase)) token = Request.Query["access_token"].ToString();
        else return Task.FromResult(AuthenticateResult.NoResult());
        var parts = token.Split(':');
        if (parts.Length != 2) return Task.FromResult(AuthenticateResult.NoResult());
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, parts[0]) };
        if (parts[1].Length != 0) claims.Add(new Claim("clearance", parts[1]));
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(user, new AuthenticationProperties { ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) }, Scheme.Name)));
    }
}
}
// endsnippet:complete
