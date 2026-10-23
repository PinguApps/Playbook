// snippet:host
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Connections;
using Playbook.Samples;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Lockhouse.Session";
        options.SlidingExpiration = false;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddRazorComponents().AddInteractiveServerComponents()
    .AddHubOptions(options => options.MaximumReceiveMessageSize = 64 * 1024);
var app = builder.Build();
if (!app.Environment.IsDevelopment())
    throw new InvalidOperationException("Fictional sign-in is Development-only; supply real authentication in an application.");
app.Use(async (context, next) =>
{
    if (context.Connection.RemoteIpAddress is not { } ip || !IPAddress.IsLoopback(ip))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }
    await next();
});
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapStaticAssets();

// Local fixture only: no password, provider, database or production sign-in claim.
app.MapGet("/sign-in", async (HttpContext context) =>
{
    var expires = DateTimeOffset.UtcNow.AddSeconds(25);
    var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "Mira at Westmere lock")],
        CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignInAsync(new ClaimsPrincipal(identity), new AuthenticationProperties
    {
        IsPersistent = false, AllowRefresh = false, ExpiresUtc = expires
    });
    return Results.Redirect("/");
});
app.MapGet("/ticket", async (HttpContext context) =>
{
    var ticket = await context.AuthenticateAsync();
    return Results.Json(new { authenticated = ticket.Succeeded, expires = ticket.Properties?.ExpiresUtc });
});

void ConfigurePolicy(HttpConnectionDispatcherOptions connection)
{
    connection.Transports = HttpTransportType.WebSockets;
    connection.CloseOnAuthenticationExpiration = builder.Configuration.GetValue("CloseOnExpiry", true);
    connection.ApplicationMaxBufferSize = 8 * 1024;
    connection.TransportMaxBufferSize = 16 * 1024;
}

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode(options => options.ConfigureConnection = ConfigurePolicy)
    .RequireAuthorization();

// Read the actual mapped connection metadata, not a second copy of the policy.
app.MapGet("/policy", (IEnumerable<EndpointDataSource> sources) => sources.SelectMany(s => s.Endpoints)
    .OfType<RouteEndpoint>()
    .Where(e => e.RoutePattern.RawText?.Contains("_blazor", StringComparison.Ordinal) == true)
    .Select(e => new { route = e.RoutePattern.RawText, options = e.Metadata.GetMetadata<HttpConnectionDispatcherOptions>() })
    .Where(e => e.options is not null)
    .Select(e => new { e.route, transports = e.options!.Transports.ToString(),
        e.options.CloseOnAuthenticationExpiration, e.options.ApplicationMaxBufferSize, e.options.TransportMaxBufferSize }));
app.Run();
// endsnippet:host
