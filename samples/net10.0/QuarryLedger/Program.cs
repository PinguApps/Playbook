// snippet:host
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Playbook.Samples;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<LedgerState>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
// endsnippet:host

// Test controls are deliberately absent outside Development. Never use these as real login endpoints.
if (app.Environment.IsDevelopment())
{
    app.MapPost("/control/signin/{name}/{permission}", async (HttpContext context, string name, string permission) =>
    {
        if (name is not ("mara" or "ivo") || permission is not ("reader" or "editor")) return Results.BadRequest();
        var identity = new ClaimsIdentity([
            new(ClaimTypes.NameIdentifier, name), new(ClaimTypes.Name, name), new("ledger-permission", permission)
        ], CookieAuthenticationDefaults.AuthenticationScheme);
        await context.SignInAsync(new ClaimsPrincipal(identity));
        return Results.NoContent();
    });
    app.MapPost("/control/revise", (LedgerState state) => state.Revise());
    app.MapGet("/control/state", (LedgerState state) => new { state.Revision, state.Builds, state.Lifecycles });
}
// snippet:run
app.MapRazorComponents<App>();
app.Run();
// endsnippet:run
