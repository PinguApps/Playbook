// snippet:complete
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

internal static class RegistrationChecks
{
    private const string Password = "Aquarium!Volunteer72";
    private sealed record Session(bool Authenticated, string? Id);
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public static async Task RunAsync()
    {
        ArtifactIdentity.Verify();
        foreach (var custom in new[] { false, true })
        foreach (var policy in new[] { "email", "phone", "account", "allowed" })
        {
            var directory = Path.Combine(Path.GetTempPath(), "playbook-aquarium-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                var mail = new CapturedMail();
                await using var app = AquariumHost.Create(directory, policy, mail);
                using (var scope = app.Services.CreateScope())
                    await scope.ServiceProvider.GetRequiredService<VolunteerDatabase>().Database.EnsureCreatedAsync();
                await app.StartAsync();
                try
                {
                    var address = new Uri(app.Urls.Single());
                    foreach (var external in new[] { false, true })
                        await CheckRegistration(app.Services, address, mail, policy, custom, external);
                    await CheckGuards(app.Services, address, custom);
                }
                finally { await app.StopAsync(); }
            }
            finally { Directory.Delete(directory, recursive: true); }
        }
        Console.WriteLine("PASS: default and original UI; password/external; three separate requirements and allowed controls; persisted SQLite users, real redirects/cookies/session readback, confirmation and request guards.");
    }

    private static HttpClient Client(Uri address, CookieContainer cookies) => new(new HttpClientHandler
    {
        CheckCertificateRevocationList = true, AllowAutoRedirect = false, CookieContainer = cookies, UseProxy = false
    }) { BaseAddress = address };

    private static string Token(string html)
    {
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Require(match.Success, "Rendered form lacks antiforgery token: " + html[..Math.Min(500, html.Length)]);
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private static async Task<HttpResponseMessage> Post(HttpClient client, string path, string token, params (string Key, string Value)[] fields)
        => await client.PostAsync(new Uri(path, UriKind.Relative), new FormUrlEncodedContent(fields.Select(field => new KeyValuePair<string, string>(field.Key, field.Value))
            .Append(new KeyValuePair<string, string>("__RequestVerificationToken", token))));

    private static bool HasCookie(HttpResponseMessage response, string name) => response.Headers.TryGetValues("Set-Cookie", out var values)
        && values.Any(value => value.StartsWith(name + "=", StringComparison.Ordinal) && !value.StartsWith(name + "=;", StringComparison.Ordinal));

    private static async Task CheckRegistration(IServiceProvider services, Uri address, CapturedMail mail, string policy, bool custom, bool external)
    {
        var cookies = new CookieContainer();
        using var client = Client(address, cookies);
        var prefix = custom ? "/Volunteer" : "/Identity/Account";
        var email = $"{(custom ? "original" : "packaged")}-{(external ? "external" : "password")}-{policy}@example.invalid";
        var form = await client.GetAsync(new Uri(prefix + "/Register?returnUrl=%2Fsession", UriKind.Relative));
        Require(form.StatusCode == HttpStatusCode.OK, "Registration form: " + await form.Content.ReadAsStringAsync());
        var html = await form.Content.ReadAsStringAsync();
        var token = Token(html);
        HttpResponseMessage outcome;
        if (external)
        {
            // Follow the actual rendered provider form, including its own action and antiforgery token.
            var providerForm = Regex.Matches(html, "(?s)<form\\b[^>]*>.*?</form>").Single(match =>
                match.Value.Contains("name=\"provider\"", StringComparison.Ordinal) &&
                match.Value.Contains("value=\"LocalAquarium\"", StringComparison.Ordinal)).Value;
            var action = WebUtility.HtmlDecode(Regex.Match(providerForm, "<form\\b[^>]*\\baction=\"([^\"]+)\"").Groups[1].Value);
            Require(action.StartsWith(prefix + "/ExternalLogin?", StringComparison.Ordinal) &&
                action.Contains("returnUrl=%2Fsession", StringComparison.OrdinalIgnoreCase), "Wrong rendered external challenge action");
            using var challenge = await Post(client, action, Token(providerForm), ("provider", "LocalAquarium"));
            Require(challenge.StatusCode == HttpStatusCode.Redirect, "External challenge failed");
            using var authority = await client.GetAsync(challenge.Headers.Location);
            Require(authority.StatusCode == HttpStatusCode.Redirect, "Local authority failed");
            using var callback = await client.GetAsync(authority.Headers.Location);
            Require(callback.StatusCode == HttpStatusCode.Redirect && HasCookie(callback, "Identity.External"), "Missing real temporary external cookie");
            Require(!HasCookie(callback, ".AspNetCore.Identity.Application"), "Provider callback issued application cookie");
            Require(!(await client.GetFromJsonAsync<Session>("/session"))!.Authenticated, "External cookie authenticated application request");
            using var confirmation = await client.GetAsync(callback.Headers.Location);
            Require(confirmation.StatusCode == HttpStatusCode.OK, "External registration form failed: " + await confirmation.Content.ReadAsStringAsync());
            token = Token(await confirmation.Content.ReadAsStringAsync());
            outcome = await Post(client, prefix + "/ExternalLogin?handler=Confirmation&returnUrl=%2Fsession", token,
                (custom ? "Email" : "Input.Email", email));
        }
        else
        {
            outcome = await Post(client, prefix + "/Register?returnUrl=%2Fsession", token,
                (custom ? "Email" : "Input.Email", email), (custom ? "Password" : "Input.Password", Password), ("Input.ConfirmPassword", Password));
        }
        using (outcome)
        {
            Require(outcome.StatusCode == HttpStatusCode.Redirect, "Registration failed: " + await outcome.Content.ReadAsStringAsync());
            // Measure the actual serviced old UI, rather than assuming that every old baseline was fixed.
            var oldDefaultBypass = Environment.Version.Major == 10 && !custom && policy is "email" or "phone";
            var allowed = policy == "allowed" || oldDefaultBypass;
            var location = outcome.Headers.Location!.OriginalString;
            Require(allowed ? location == "/session" : location.StartsWith("/Identity/Account/RegisterConfirmation?", StringComparison.Ordinal), "Unexpected redirect: " + location);
            Require(HasCookie(outcome, ".AspNetCore.Identity.Application") == allowed, "Wrong application-cookie issuance");
            var session = (await client.GetFromJsonAsync<Session>("/session"))!;
            Require(session.Authenticated == allowed, "Wrong authenticated request readback");
            string id;
            using (var scope = services.CreateScope())
            {
                // Fresh DbContext: observe committed rows, not the registration handler's tracked entity.
                var db = scope.ServiceProvider.GetRequiredService<VolunteerDatabase>();
                var user = await db.Users.AsNoTracking().SingleAsync(user => user.Email == email);
                id = user.Id;
                Require(!user.EmailConfirmed && !user.PhoneNumberConfirmed && user.PhoneNumber is null, "Unexpected confirmation state");
                Require((user.PasswordHash is not null) == !external, "Wrong password persistence");
                Require(await db.UserLogins.CountAsync(login => login.UserId == id) == (external ? 1 : 0), "Wrong external login persistence");
                Require(!allowed || session.Id == id, "Session belongs to wrong volunteer");
            }
            Require(mail.Links.ContainsKey(email), "Confirmation transport was not invoked");
            if (oldDefaultBypass)
                await CheckLaterLogin(services, address, prefix, email, id, external, expected: false);
            if (!allowed)
            {
                await CheckLaterLogin(services, address, prefix, email, id, external, expected: false);
                using var pending = await client.GetAsync(outcome.Headers.Location);
                Require(pending.StatusCode == HttpStatusCode.OK, "Confirmation page did not render");
                Require(!(await client.GetFromJsonAsync<Session>("/session"))!.Authenticated, "Confirmation page granted a session");
                // Captured link exercises actual default UI token decoding and UserManager confirmation, without sending mail.
                using var confirmed = await client.GetAsync(new Uri(mail.Links[email]));
                Require(confirmed.StatusCode == HttpStatusCode.OK, "Email confirmation route failed");
                if (policy == "phone")
                    await CheckLaterLogin(services, address, prefix, email, id, external, expected: false);
                using (var scope = services.CreateScope())
                {
                    var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                    var user = (await users.FindByIdAsync(id))!;
                    Require(user.EmailConfirmed, "Email token did not persist confirmation");
                    if (policy == "phone")
                    {
                        // Local token check is separate from delivery: no SMS or public test endpoint.
                        var code = await users.GenerateChangePhoneNumberTokenAsync(user, "+447700900123");
                        Require((await users.ChangePhoneNumberAsync(user, "+447700900123", code)).Succeeded, "Phone token failed");
                    }
                }
                Require(!(await client.GetFromJsonAsync<Session>("/session"))!.Authenticated, "Confirmation itself issued a session");
                await CheckLaterLogin(services, address, prefix, email, id, external, expected: true);
            }
            Console.WriteLine($"CHECK ui={(custom ? "original" : "packaged")} path={(external ? "external" : "password")} policy={policy} redirect={location.Split('?')[0]} appCookie={allowed} session={session.Authenticated} storedUnconfirmed=true externalLogin={external} oldDefaultBypass={oldDefaultBypass}");
        }
    }

    private static async Task CheckLaterLogin(IServiceProvider services, Uri address, string prefix, string email, string id, bool external, bool expected)
    {
        using var client = Client(address, new CookieContainer());
        using var form = await client.GetAsync(new Uri("/Identity/Account/Login?returnUrl=%2Fsession", UriKind.Relative));
        var token = Token(await form.Content.ReadAsStringAsync());
        if (external)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<VolunteerDatabase>();
            var subject = await db.UserLogins.Where(login => login.UserId == id).Select(login => login.ProviderKey).SingleAsync();
            var authority = services.GetRequiredService<LocalAuthority>();
            authority.Subject = subject;
            try
            {
                using var challenge = await Post(client, prefix + "/ExternalLogin?returnUrl=%2Fsession", token, ("provider", "LocalAquarium"));
                using var provider = await client.GetAsync(challenge.Headers.Location);
                using var callback = await client.GetAsync(provider.Headers.Location);
                using var login = await client.GetAsync(callback.Headers.Location);
                Require(!expected || login.StatusCode == HttpStatusCode.Redirect && login.Headers.Location!.OriginalString == "/session", "Confirmed external sign-in failed");
                Require(HasCookie(login, ".AspNetCore.Identity.Application") == expected, "External later sign-in cookie mismatch");
            }
            finally { authority.Subject = null; }
        }
        else
        {
            using var login = await Post(client, "/Identity/Account/Login?returnUrl=%2Fsession", token, ("Input.Email", email), ("Input.Password", Password));
            Require(!expected || login.StatusCode == HttpStatusCode.Redirect && login.Headers.Location!.OriginalString == "/session", "Confirmed password sign-in failed");
            Require(HasCookie(login, ".AspNetCore.Identity.Application") == expected, "Password later sign-in cookie mismatch");
        }
        var session = (await client.GetFromJsonAsync<Session>("/session"))!;
        Require(session.Authenticated == expected && (!expected || session.Id == id), "Later sign-in session mismatch");
        Console.WriteLine($"LATER path={(external ? "external" : "password")} confirmed={expected} session={session.Authenticated}");
    }

    private static async Task CheckGuards(IServiceProvider services, Uri address, bool custom)
    {
        using var client = Client(address, new CookieContainer());
        var prefix = custom ? "/Volunteer" : "/Identity/Account";
        using var missingToken = await client.PostAsync(new Uri(prefix + "/Register", UriKind.Relative), new FormUrlEncodedContent(new Dictionary<string, string>()));
        Require(missingToken.StatusCode == HttpStatusCode.BadRequest, "Missing antiforgery token accepted");
        using var missingExternalToken = await client.PostAsync(new Uri(prefix + "/ExternalLogin?handler=Confirmation", UriKind.Relative), new FormUrlEncodedContent(new Dictionary<string, string>()));
        Require(missingExternalToken.StatusCode == HttpStatusCode.BadRequest, "External registration ignored antiforgery");
        using var form = await client.GetAsync(new Uri(prefix + "/Register", UriKind.Relative));
        var token = Token(await form.Content.ReadAsStringAsync());
        using var noExternal = await Post(client, prefix + "/ExternalLogin?handler=Confirmation", token, (custom ? "Email" : "Input.Email", "missing@example.invalid"));
        Require(custom ? noExternal.StatusCode == HttpStatusCode.BadRequest : noExternal.StatusCode == HttpStatusCode.Redirect, "Missing external state was accepted");
        using var invalidState = await client.GetAsync(new Uri("/signin-local-aquarium?state=tampered&code=invalid", UriKind.Relative));
        Require(invalidState.StatusCode == HttpStatusCode.BadRequest, "Tampered provider state accepted");
        if (custom)
        {
            using var hostileReturn = await Post(client, prefix + "/Register?returnUrl=https%3A%2F%2Fhostile.example.invalid", token,
                ("Email", "redirect@example.invalid"), ("Password", Password));
            Require(hostileReturn.StatusCode == HttpStatusCode.BadRequest, "Nonlocal return URL accepted");
        }
        using var scope = services.CreateScope();
        Require(!await scope.ServiceProvider.GetRequiredService<VolunteerDatabase>().Users.AnyAsync(user => user.Email == "missing@example.invalid" || user.Email == "redirect@example.invalid"), "Guard failure created user");
        Require(!(await client.GetFromJsonAsync<Session>("/session"))!.Authenticated, "Guard failure granted a session");
        Console.WriteLine($"GUARDS ui={(custom ? "original" : "packaged")} antiforgery=password+external missingExternalState=denied tamperedProviderState=denied nonlocalReturn={(custom ? "denied-before-write" : "not-exercised")} ");
    }
}
// endsnippet:complete
