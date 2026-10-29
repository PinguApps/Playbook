// snippet:complete
using System.Security.Claims;
using System.Text.Encodings.Web;
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
app.MapHub<QuarryHub>("/quarry", options => options.CloseOnAuthenticationExpiration = true).RequireAuthorization();
Console.WriteLine("Sample runtime: " + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
if (args.Contains("--serve", StringComparer.Ordinal)) { await app.RunAsync(); return; }
app.Urls.Add("http://127.0.0.1:0");
await app.StartAsync();
try
{
    var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    await CheckConnection(address + "/quarry");
    Console.WriteLine("PASS: older stop/start obtains updated claims over a real hub connection; connection identifier changes.");
}
finally { await app.StopAsync(); await app.DisposeAsync(); }

static async Task CheckConnection(string address)
{
    string ticket = "mara:visitor";
    // .NET 10 has no in-place authentication refresh. Reconnect with the new ticket.
    await using var connection = new HubConnectionBuilder()
        .WithUrl(address, options => options.AccessTokenProvider = () => Task.FromResult<string?>(ticket))
        .Build();
    await connection.StartAsync();
    var original = await connection.InvokeAsync<QuarryIdentity>("Read");
    ticket = "mara:surveyor";
    Require((await connection.InvokeAsync<QuarryIdentity>("Read")).Clearance == "visitor", "Existing principal unexpectedly changed");
    await connection.StopAsync();
    await connection.StartAsync();
    var updated = await connection.InvokeAsync<QuarryIdentity>("Read");
    Require(updated.Clearance == "surveyor" && updated.User == original.User && updated.Connection != original.Connection, "Reconnect failed");
    await connection.StopAsync();
}
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }

namespace CopperRidgeHub
{
public sealed record QuarryIdentity(string Connection, string? User, string? Clearance);
public sealed class QuarryHub : Hub
{
    public QuarryIdentity Read() => new(Context.ConnectionId, Context.UserIdentifier, Context.User?.FindFirst("clearance")?.Value);
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
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, parts[0]), new Claim("clearance", parts[1]) }, Scheme.Name));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(user, new AuthenticationProperties { ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) }, Scheme.Name)));
    }
}
}
// endsnippet:complete
