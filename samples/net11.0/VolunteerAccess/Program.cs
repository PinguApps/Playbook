// snippet:complete
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

var state = Path.Combine(Path.GetTempPath(), "volunteer-access-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(state);
try
{
    ArtifactIdentity.Verify();
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        Args = args,
        ContentRootPath = AppContext.BaseDirectory,
        EnvironmentName = "Production"
    });
    builder.Logging.ClearProviders();
    builder.WebHost.UseUrls("http://127.0.0.1:0");
    builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
    builder.Services.AddDbContext<VolunteerDatabase>(options => options.UseSqlite(
        $"Data Source={Path.Combine(state, "volunteers.db")};Pooling=False"));
    builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<VolunteerDatabase>();
    builder.Services.AddRazorPages();
    await using var app = builder.Build();
    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapRazorPages();
    app.MapGet("/session", (HttpContext context) => context.User.Identity?.IsAuthenticated == true ? "signed-in" : "anonymous");
    using (var scope = app.Services.CreateScope())
    {
        await scope.ServiceProvider.GetRequiredService<VolunteerDatabase>().Database.EnsureCreatedAsync();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        foreach (var confirmed in new[] { false, true })
        {
            var email = confirmed ? "ready@bracken.example" : "pending@bracken.example";
            var created = await users.CreateAsync(new IdentityUser { UserName = email, Email = email, EmailConfirmed = confirmed }, "Bracken.Rota42!");
            Check(created.Succeeded, "fixture account created");
        }
    }
    await app.StartAsync();
    try
    {
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        Console.WriteLine("VOLUNTEER-URL " + address + "/Identity/Account/Login");
        if (args.Contains("--serve", StringComparer.Ordinal)) await app.WaitForShutdownAsync();
        else
        {
            using var handler = new HttpClientHandler { CookieContainer = new CookieContainer(), AllowAutoRedirect = false, CheckCertificateRevocationList = true };
            using var client = new HttpClient(handler) { BaseAddress = new Uri(address), Timeout = TimeSpan.FromSeconds(15) };
            var page = await client.GetStringAsync(new Uri("/Identity/Account/Login", UriKind.Relative));
            Check(page.Contains("Bracken Rescue volunteers", StringComparison.Ordinal), "custom published layout");
            Check(page.Contains("volunteer-panel", StringComparison.Ordinal), "custom markup");
            foreach (var asset in new[] { "/css/volunteers.css", "/Identity/lib/bootstrap/dist/css/bootstrap.min.css" })
            {
                var css = await client.GetStringAsync(new Uri(asset, UriKind.Relative));
                Check(css.Length > 100, "published CSS " + asset);
                Console.WriteLine($"ASSET {asset} bytes={System.Text.Encoding.UTF8.GetByteCount(css)}");
            }
            var token = WebUtility.HtmlDecode(Regex.Match(page, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
            Check(token.Length > 20, "real antiforgery token");
            using var invalid = await client.PostAsync(new Uri("/Identity/Account/Login", UriKind.Relative), new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token, ["Input.Email"] = "not-an-email", ["Input.Password"] = ""
            }));
            var errors = await invalid.Content.ReadAsStringAsync();
            Check(invalid.StatusCode == HttpStatusCode.OK && errors.Contains("valid e-mail", StringComparison.Ordinal) &&
                errors.Contains("Password is required", StringComparison.Ordinal), "published server validation");
            using var missingToken = await client.PostAsync(new Uri("/Identity/Account/Login", UriKind.Relative), new FormUrlEncodedContent(new Dictionary<string, string>()));
            Check(missingToken.StatusCode == HttpStatusCode.BadRequest, "antiforgery retained");
            Check(await client.GetStringAsync(new Uri("/session", UriKind.Relative)) == "anonymous", "invalid form cannot issue session");
            foreach (var confirmed in new[] { false, true })
            {
                using var posted = await client.PostAsync(new Uri("/Identity/Account/Login?returnUrl=https://untrusted.invalid", UriKind.Relative),
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["__RequestVerificationToken"] = token,
                        ["Input.Email"] = confirmed ? "ready@bracken.example" : "pending@bracken.example",
                        ["Input.Password"] = "Bracken.Rota42!"
                    }));
                Check(posted.StatusCode == (confirmed ? HttpStatusCode.Redirect : HttpStatusCode.OK), "confirmation policy retained");
                if (confirmed) Check(posted.Headers.Location?.OriginalString == "/session", "external return URL rejected");
                Check(await client.GetStringAsync(new Uri("/session", UriKind.Relative)) == (confirmed ? "signed-in" : "anonymous"), "fresh session principal");
            }
            Console.WriteLine($"PASS VolunteerAccess published layout/assets/validation/antiforgery; runtime={Environment.Version}");
        }
    }
    finally { await app.StopAsync(); }
}
finally { Directory.Delete(state, true); }

static void Check(bool condition, string claim)
{
    if (!condition) throw new InvalidOperationException(claim);
}
internal sealed class VolunteerDatabase(DbContextOptions<VolunteerDatabase> options) : IdentityDbContext<IdentityUser>(options);
// endsnippet:complete
