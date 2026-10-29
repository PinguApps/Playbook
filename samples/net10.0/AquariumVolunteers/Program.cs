// snippet:complete
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

await RegistrationChecks.RunAsync();

internal sealed class VolunteerDatabase(DbContextOptions<VolunteerDatabase> options) : IdentityDbContext<IdentityUser>(options);

internal sealed class CapturedMail : IEmailSender<IdentityUser>
{
    public Dictionary<string, string> Links { get; } = new(StringComparer.Ordinal);
    public Task SendConfirmationLinkAsync(IdentityUser user, string email, string confirmationLink)
    {
        Links.Add(email, WebUtility.HtmlDecode(confirmationLink));
        return Task.CompletedTask;
    }
    public Task SendPasswordResetLinkAsync(IdentityUser user, string email, string resetLink) => throw new NotSupportedException();
    public Task SendPasswordResetCodeAsync(IdentityUser user, string email, string resetCode) => throw new NotSupportedException();
}

internal static class AquariumHost
{
    public static WebApplication Create(string directory, string policy, CapturedMail mail)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = directory,
            ApplicationName = typeof(AquariumHost).Assembly.GetName().Name,
            EnvironmentName = "Development"
        });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
        builder.Services.AddDbContext<VolunteerDatabase>(options => options.UseSqlite(
            $"Data Source={Path.Combine(directory, "volunteers.db")};Pooling=False"));
        builder.Services.AddDefaultIdentity<IdentityUser>(options =>
        {
            options.SignIn.RequireConfirmedEmail = policy == "email";
            options.SignIn.RequireConfirmedPhoneNumber = policy == "phone";
            options.SignIn.RequireConfirmedAccount = policy == "account";
        }).AddEntityFrameworkStores<VolunteerDatabase>();
        builder.Services.AddSingleton<IEmailSender<IdentityUser>>(mail);
        builder.Services.AddSingleton<LocalAuthority>();
        builder.Services.AddAuthentication().AddScheme<LocalProviderOptions, LocalProviderHandler>(
            "LocalAquarium", "Local aquarium test provider", options =>
            {
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.CallbackPath = "/signin-local-aquarium";
                // HTTP loopback fixture only. Production remote providers retain secure correlation cookies and HTTPS.
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.None;
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.Events = new RemoteAuthenticationEvents { OnRemoteFailure = context =>
                {
                    context.HandleResponse(); context.Response.StatusCode = 400;
                    return Task.CompletedTask;
                } };
            });
        builder.Services.AddRazorPages();
        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapRazorPages();
        app.MapGet("/", () => "Seabrook Aquarium volunteers");
        // Read back the application principal on a NEW request. An external cookie is insufficient.
        app.MapGet("/session", (HttpContext context) => Results.Json(new
        {
            authenticated = context.User.Identity?.IsAuthenticated == true,
            id = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        }));
        app.MapGet("/fixture/authority", (HttpContext context) =>
        {
            var state = Uri.EscapeDataString(context.Request.Query["state"].ToString());
            var code = Uri.EscapeDataString(context.Request.Query["code"].ToString());
            return Results.Redirect($"/signin-local-aquarium?state={state}&code={code}");
        });
        return app;
    }
}
// endsnippet:complete
