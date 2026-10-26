// snippet:complete
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BeaconKeys;

public static class Proof
{
    public static async Task RunAsync(WebApplication app)
    {
        using SoftwareKey key = new();
        MemoryKeyStore store = app.Services.GetRequiredService<MemoryKeyStore>();
        IdentityUser owner = store.Keeper;
        await store.AddOrUpdatePasskeyAsync(owner, key.Stored(), default);
        using HttpClientHandler handler = new() { AllowAutoRedirect = false, CheckCertificateRevocationList = true, CookieContainer = new CookieContainer() };
        using HttpClient client = new(handler) { BaseAddress = new Uri(app.Urls.Single()) };
        string origin = client.BaseAddress.GetLeftPart(UriPartial.Authority);
        client.DefaultRequestHeaders.Add("Origin", origin);
        IOptions<IdentityOptions> options = app.Services.GetRequiredService<IOptions<IdentityOptions>>();

        // The same verified credential reaches each real Identity policy branch.
        options.Value.SignIn.RequireConfirmedAccount = true;
        await Attempt("Complete the required account confirmation", signedIn: false);
        owner.EmailConfirmed = true;
        owner.LockoutEnd = DateTimeOffset.UtcNow.AddHours(1);
        await Attempt("Sign-in is temporarily locked", signedIn: false);
        owner.LockoutEnd = null;
        options.Value.SignIn.RequireConfirmedPhoneNumber = true;
        await Attempt("Complete the required account confirmation", signedIn: false);
        owner.PhoneNumberConfirmed = true;
        options.Value.SignIn.RequireConfirmedEmail = true;
        owner.EmailConfirmed = false;
        await Attempt("Complete the required account confirmation", signedIn: false);
        owner.EmailConfirmed = true;

        // A wrong origin must fail before any successful sign-in, even for a confirmed account.
        string invalidOptions = await client.GetStringAsync(new Uri("/options", UriKind.Relative));
        using HttpResponseMessage invalid = await client.PostAsync(new Uri("/signin", UriKind.Relative), new StringContent(key.Assert(invalidOptions, owner.Id, "https://wrong.invalid"), Encoding.UTF8, "application/json"));
        Check((await invalid.Content.ReadAsStringAsync()).Contains("could not sign you in", StringComparison.Ordinal), "Wrong-origin assertion was not rejected.");
        Check(!HasApplicationCookie(invalid), "Invalid assertion issued a sign-in cookie.");
        await Attempt("You are signed in", signedIn: true);

        using IServiceScope scope = app.Services.CreateScope();
        UserManager<IdentityUser> users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        using SoftwareKey unknownKey = new();
        UserPasskeyInfo unknown = unknownKey.Stored();
        string rename = await PasskeyLabels.SaveAsync(users, owner, unknown);
        Check(rename.StartsWith("/keys/rename/", StringComparison.Ordinal), "Unknown authenticator did not offer rename.");
        using HttpResponseMessage page = await client.GetAsync(new Uri(rename, UriKind.Relative));
        string html = await page.Content.ReadAsStringAsync();
        Check(page.IsSuccessStatusCode && html.Contains("A name you will recognise", StringComparison.Ordinal), "Rename form missing.");
        string token = WebUtility.HtmlDecode(Regex.Match(html, "name=\"__RequestVerificationToken\" value=\"([^\"]+)\"").Groups[1].Value);
        Check(token.Length > 0, "Missing antiforgery token.");
        using HttpResponseMessage missingToken = await client.PostAsync(new Uri(rename, UriKind.Relative), new FormUrlEncodedContent(new Dictionary<string, string> { ["name"] = "Unsafe rename" }));
        Check(missingToken.StatusCode == HttpStatusCode.BadRequest, "Rename accepted no antiforgery token.");
        using HttpResponseMessage renamed = await client.PostAsync(new Uri(rename, UriKind.Relative), new FormUrlEncodedContent(new Dictionary<string, string> { ["name"] = "  Stores spare  ", ["__RequestVerificationToken"] = token }));
        Check(renamed.StatusCode == HttpStatusCode.Redirect, "Rename did not complete.");
        Check((await users.GetPasskeyAsync(owner, unknown.CredentialId))?.Name == "Stores spare", "Rename was not saved.");
        Check(!await PasskeyLabels.RenameAsync(users, new IdentityUser("stranger"), unknown.CredentialId, "Stolen"), "Cross-owner rename accepted.");
        Check(!await PasskeyLabels.RenameAsync(users, owner, unknown.CredentialId, "  "), "Blank name accepted.");
        await CheckNames(users, owner);
        Console.WriteLine($"PASS BeaconKeys {RuntimeInformation.FrameworkDescription}: real signed assertions, account/email/phone/lockout outcomes, no denial cookie, success cookie, rendered custom UI, persisted rename and owner/antiforgery checks. Software authenticator and memory store only; no physical-device, attestation or GA proof.");

        async Task Attempt(string expected, bool signedIn)
        {
            string request = await client.GetStringAsync(new Uri("/options", UriKind.Relative));
            using HttpResponseMessage response = await client.PostAsync(new Uri("/signin", UriKind.Relative), new StringContent(key.Assert(request, owner.Id, origin), Encoding.UTF8, "application/json"));
            string body = await response.Content.ReadAsStringAsync();
            Check(response.IsSuccessStatusCode && body.Contains(expected, StringComparison.Ordinal), $"Expected '{expected}', received {response.StatusCode}: {body}");
            Check(HasApplicationCookie(response) == signedIn, "Unexpected application-cookie issuance.");
        }
    }

    private static bool HasApplicationCookie(HttpResponseMessage response) => response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? values) && values.Any(value => value.StartsWith(".AspNetCore.Identity.Application=", StringComparison.Ordinal));
    private static void Check(bool condition, string error) { if (!condition) { throw new InvalidOperationException(error); } }

    private static async Task CheckNames(UserManager<IdentityUser> users, IdentityUser owner)
    {
        using SoftwareKey extra = new();
        UserPasskeyInfo manual = extra.Stored();
        Check((await PasskeyLabels.SaveAsync(users, owner, manual)).StartsWith("/keys/rename/", StringComparison.Ordinal), "Older flow did not ask for a name.");
        manual.Name = "Keeper's chosen label";
        Check(await PasskeyLabels.SaveAsync(users, owner, manual) == "/keys", "Existing manual label was lost.");
    }
}
// endsnippet:complete
