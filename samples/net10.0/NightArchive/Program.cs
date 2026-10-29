// snippet:complete
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
// The fixture owns its certificate and ephemeral key ring; it changes no trust store.
using var key = RSA.Create(2048);
var request = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
using var generated = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddHours(1));
using var certificate = X509CertificateLoader.LoadPkcs12(generated.Export(X509ContentType.Pfx), null);
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Loopback, 0);
    options.Listen(IPAddress.Loopback, 0, endpoint => endpoint.UseHttps(certificate));
});
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddAuthentication("Archive").AddCookie("Archive", options =>
{
    options.Cookie.Name = "NightArchive";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = 401; return Task.CompletedTask; };
    options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
});
builder.Services.AddAuthorization(options => options.AddPolicy("catalogue", policy => policy.RequireRole("curator")));
await using var app = builder.Build();
var missingFeatureRequests = 0;
app.Use(async (context, next) =>
{
    if (!context.Request.IsHttps)
    {
        context.Response.StatusCode = 400;
        return;
    }
    // Historical telemetry only: absence never grants or denies access.
    if (context.Features.Get<ITlsTokenBindingFeature>()?.GetProvidedTokenBindingId() is null)
        missingFeatureRequests++;
    await next(context);
});
app.UseAuthentication();
app.UseAuthorization();
var handled = 0;
app.MapGet("/catalogue", (ClaimsPrincipal user) => { handled++; return Results.Text($"Night archive: {user.Identity!.Name}"); })
    .RequireAuthorization("catalogue");
await app.StartAsync();
var addresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses;
var https = addresses.Single(address => address.StartsWith("https:", StringComparison.Ordinal));
var http = addresses.Single(address => address.StartsWith("http:", StringComparison.Ordinal));
using var handler = new HttpClientHandler
{
    UseCookies = false,
    AllowAutoRedirect = false,
    // A fixture-only exact certificate match; never accept every certificate in a real client.
    ServerCertificateCustomValidationCallback = (_, peer, _, _) => peer?.GetCertHashString() == certificate.GetCertHashString(),
};
using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) };
// Mint protected tickets through the REAL cookie handler. There is no public test-login endpoint.
async Task<string> Cookie(string role)
{
    using var scope = app.Services.CreateScope();
    var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
    context.Request.Scheme = "https";
    context.Response.Body = Stream.Null;
    var principal = new ClaimsPrincipal(new ClaimsIdentity(
        new[] { new Claim(ClaimTypes.Name, "Mira"), new Claim(ClaimTypes.Role, role) }, "Archive"));
    await context.SignInAsync("Archive", principal);
    return context.Response.Headers.SetCookie.Single()!.Split(';')[0];
}
async Task Check(string url, string? cookie, HttpStatusCode expected)
{
    using var message = new HttpRequestMessage(HttpMethod.Get, url + "/catalogue");
    if (cookie is not null) message.Headers.Add("Cookie", cookie);
    using var response = await client.SendAsync(message);
    var body = await response.Content.ReadAsStringAsync();
    if (response.StatusCode != expected || (expected == HttpStatusCode.OK && body != "Night archive: Mira"))
        throw new InvalidOperationException($"Request failed: {response.StatusCode}, {body}");
    Console.WriteLine($"PASS {new Uri(url).Scheme} {(cookie is null ? "anonymous" : "ticket")} => {(int)response.StatusCode}");
}
var curator = await Cookie("curator");
var visitor = await Cookie("visitor");
await Check(https, curator, HttpStatusCode.OK);
await Check(https, visitor, HttpStatusCode.Forbidden);
await Check(https, null, HttpStatusCode.Unauthorized);
await Check(https, "NightArchive=invalid", HttpStatusCode.Unauthorized);
await Check(http, curator, HttpStatusCode.BadRequest);
if (handled != 1) throw new InvalidOperationException("A denied request reached the protected handler.");
if (missingFeatureRequests != 4) throw new InvalidOperationException("Expected real Kestrel feature absence.");
Console.WriteLine($"PASS absent RFC8471 feature on all {missingFeatureRequests} HTTPS requests; policy unchanged.");
Console.WriteLine($"PASS runtime={Environment.Version}; Http.Features={typeof(ITlsConnectionFeature).Assembly.GetName().Version}; handler={handled}");
await app.StopAsync();
// endsnippet:complete
