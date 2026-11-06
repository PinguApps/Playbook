using System.Net;
using System.Collections.Concurrent;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("This fixture uses the current Windows sign-in.");
using RSA key = RSA.Create(2048);
CertificateRequest certificateRequest = new("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
SubjectAlternativeNameBuilder names = new();
names.AddDnsName("localhost");
certificateRequest.CertificateExtensions.Add(names.Build());
certificateRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
certificateRequest.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new("1.3.6.1.5.5.7.3.1") }, true));
using X509Certificate2 generated = certificateRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddHours(1));
using X509Certificate2 certificate = X509CertificateLoader.LoadPkcs12(generated.Export(X509ContentType.Pfx), null);
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0, listen =>
{
    listen.Protocols = HttpProtocols.Http1AndHttp2;
    listen.UseHttps(certificate);
}));
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
builder.Services.AddAuthorization();
await using WebApplication app = builder.Build();
ConcurrentQueue<string> trace = new();
app.Use(async (context, next) =>
{
    await next(context);
    trace.Enqueue($"{context.Request.Protocol}:{context.Response.StatusCode}");
});
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/survey", (HttpContext context) => $"gannet:{context.Request.Protocol}:{context.User.Identity?.IsAuthenticated}:{context.User.Identity?.AuthenticationType}").RequireAuthorization();
app.MapGet("/public", () => "gannet:public");
app.MapGet("/denied", () => "must not be reached").RequireAuthorization(policy => policy.RequireAssertion(_ => false));
await app.StartAsync();
try
{
    string address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single().Replace("127.0.0.1", "localhost", StringComparison.Ordinal);
    foreach ((string name, Version version, HttpVersionPolicy policy, bool credentials, string path) in new[]
    {
        ("manual", HttpVersion.Version11, HttpVersionPolicy.RequestVersionExact, true, "/survey"),
        ("automatic", HttpVersion.Version20, HttpVersionPolicy.RequestVersionOrLower, true, "/survey"),
        ("strict", HttpVersion.Version20, HttpVersionPolicy.RequestVersionExact, true, "/survey"),
        ("anonymous", HttpVersion.Version20, HttpVersionPolicy.RequestVersionOrLower, false, "/survey"),
        ("public", HttpVersion.Version20, HttpVersionPolicy.RequestVersionExact, false, "/public"),
        ("denied", HttpVersion.Version11, HttpVersionPolicy.RequestVersionExact, true, "/denied")
    })
    {
        trace.Clear();
        X509ChainPolicy trust = new() { TrustMode = X509ChainTrustMode.CustomRootTrust, RevocationMode = X509RevocationMode.NoCheck, DisableCertificateDownloads = true };
        trust.CustomTrustStore.Add(certificate);
        using SocketsHttpHandler handler = new()
        {
            UseProxy = false,
            Credentials = credentials ? CredentialCache.DefaultNetworkCredentials : null,
            SslOptions = new SslClientAuthenticationOptions { CertificateChainPolicy = trust }
        };
        using HttpClient client = new(handler) { Timeout = TimeSpan.FromSeconds(15) };
        using HttpRequestMessage request = (Environment.Version.Major >= 11 && name == "automatic") || (Environment.Version.Major == 10 && name == "manual")
            ? SurveyRequest.Create(new Uri(address + path))
            : new(HttpMethod.Get, address + path) { Version = version, VersionPolicy = policy };
            using HttpResponseMessage response = await client.SendAsync(request);
            string body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"{name}: {(int)response.StatusCode} HTTP/{response.Version} {body}");
            Console.WriteLine($"protocol-trace: {string.Join(",", trace)}");
            bool automaticSupported = Environment.Version.Major >= 11;
            HttpStatusCode expected = name switch
            {
                "manual" or "public" => HttpStatusCode.OK,
                "automatic" when automaticSupported => HttpStatusCode.OK,
                "denied" => HttpStatusCode.Forbidden,
                _ => HttpStatusCode.Unauthorized
            };
            if (response.StatusCode != expected) throw new InvalidOperationException($"{name}: unexpected status.");
            if ((name is "manual" || name == "automatic" && automaticSupported) && (response.Version != HttpVersion.Version11 || !body.Contains(":True:", StringComparison.Ordinal))) throw new InvalidOperationException("Authenticated HTTP/1.1 response required.");
            if (name is "strict" or "public" && response.Version != HttpVersion.Version20) throw new InvalidOperationException("Strict HTTP/2 was downgraded.");
            if (name == "automatic" && automaticSupported && (!trace.Contains("HTTP/2:401") || !trace.Contains("HTTP/1.1:200"))) throw new InvalidOperationException("Actual HTTP/2 challenge and HTTP/1.1 retry required.");
    }
    // Name validation stays enabled even for this private test certificate.
    X509ChainPolicy negativeTrust = new() { TrustMode = X509ChainTrustMode.CustomRootTrust, RevocationMode = X509RevocationMode.NoCheck, DisableCertificateDownloads = true };
    negativeTrust.CustomTrustStore.Add(certificate);
    using SocketsHttpHandler negativeHandler = new() { UseProxy = false, SslOptions = new() { CertificateChainPolicy = negativeTrust } };
    using HttpClient negativeClient = new(negativeHandler) { Timeout = TimeSpan.FromSeconds(15) };
    try { using var wrongHost = await negativeClient.GetAsync(new Uri(address.Replace("localhost", "127.0.0.1", StringComparison.Ordinal) + "/public")); throw new InvalidOperationException("Wrong certificate name accepted."); }
    catch (HttpRequestException exception) when (exception.HttpRequestError == HttpRequestError.SecureConnectionError) { Console.WriteLine("wrong-name: TLS rejected"); }
    Console.WriteLine(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
}
finally { await app.StopAsync(); }
