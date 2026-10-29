// snippet:complete
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using CopperRidgeHub;

var builder = WebApplication.CreateBuilder(args);
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
    options.OnAuthenticationRefresh = context => Task.FromResult(context.NewUser.FindFirst("clearance")?.Value != "suspended");
}).RequireAuthorization();
// endsnippet:refresh
// A local negotiate redirect is a client constraint check, not Azure Service proof.
app.MapPost("/gateway/negotiate", (HttpContext context) => Results.Json(new { url = $"http://{context.Request.Host}/quarry", accessToken = "mara:visitor" }));
app.MapPost("/gateway/refresh", async (HttpContext context) =>
{
    using var client = new HttpClient();
    using var request = new HttpRequestMessage(HttpMethod.Post, $"http://{context.Request.Host}/quarry/refresh?id={Uri.EscapeDataString(context.Request.Query["id"].ToString())}");
    request.Headers.TryAddWithoutValidation("Authorization", context.Request.Headers.Authorization.ToString());
    using var response = await client.SendAsync(request);
    if (!response.IsSuccessStatusCode) return Results.StatusCode((int)response.StatusCode);
    var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
    return Results.Json(new { tokenLifetimeSeconds = body.GetProperty("tokenLifetimeSeconds").GetDouble(), accessToken = context.Request.Headers.Authorization.ToString()[7..] });
});
Console.WriteLine("Sample runtime: " + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
if (args.Contains("--serve", StringComparer.Ordinal)) { await app.RunAsync(); return; }
app.Urls.Add("http://127.0.0.1:0");
await app.StartAsync();
try
{
    var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    await CheckConnection(address + "/quarry");
    await CheckConnection(address + "/gateway");
    Console.WriteLine("PASS: direct and redirected real .NET hub refresh; accepted claims, rejected policy/user switch, events, stable connection and user routing.");
}
finally { await app.StopAsync(); await app.DisposeAsync(); }

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
    foreach (var rejected in new[] { "mara:suspended", "other:surveyor" })
    {
        ticket = rejected;
        try { await connection.RefreshAuthenticationAsync(); throw new InvalidOperationException("Expected rejection"); }
        catch (HttpRequestException error) when (error.StatusCode == System.Net.HttpStatusCode.Forbidden) { }
        Require((await connection.InvokeAsync<QuarryIdentity>("Read")) == updated, "Rejected identity leaked");
    }
    // Invocation ordering ensures the server's preceding refresh notification arrived.
    Require(refreshed == 1 && failed == 2 && hubEvents == 1, "Refresh events missing");
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
        else token = Request.Query["access_token"].ToString();
        var parts = token.Split(':');
        if (parts.Length != 2) return Task.FromResult(AuthenticateResult.NoResult());
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, parts[0]), new Claim("clearance", parts[1]) }, Scheme.Name));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(user, new AuthenticationProperties { ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) }, Scheme.Name)));
    }
}
}
// endsnippet:complete
