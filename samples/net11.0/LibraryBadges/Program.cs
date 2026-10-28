// snippet:host
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Playbook.Samples;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<LibraryState>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
// endsnippet:host

// Development-only fictional identities, not an application login system.
if (app.Environment.IsDevelopment())
{
    app.MapPost("/control/signin/{name}", async (HttpContext context, string name) =>
    {
        if (name is not ("ada" or "ben")) return Results.BadRequest();
        var identity = new ClaimsIdentity([new(ClaimTypes.NameIdentifier, name), new(ClaimTypes.Name, name)],
            CookieAuthenticationDefaults.AuthenticationScheme);
        await context.SignInAsync(new ClaimsPrincipal(identity));
        return Results.NoContent();
    });
    app.MapGet("/control/counts", (LibraryState state) => state.Counts);
}
// snippet:run
app.MapRazorComponents<App>();
Console.WriteLine($"Sample runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
app.Run();
// endsnippet:run
