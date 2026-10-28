// snippet:complete
#pragma warning disable CA5398 // Deliberate incompatible TLS versions in a loopback-only diagnostic fixture.
using System.Collections.Concurrent;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

using RSA key = RSA.Create(2048);
var request = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
using X509Certificate2 generated = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddHours(1));
using X509Certificate2 certificate = X509CertificateLoader.LoadPkcs12(generated.Export(X509ContentType.Pfx), null);
var observations = new ConcurrentQueue<(long HelloBytes, Exception? Failure)>();
var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();

// snippet:observe
builder.WebHost.ConfigureKestrel(server => server.Listen(IPAddress.Loopback, 0, listener =>
{
    listener.Protocols = HttpProtocols.Http1;
    listener.Use(next => async connection =>
    {
        try { await next(connection); }
        finally
        {
            var handshake = connection.Features.Get<ITlsHandshakeFeature>();
            observations.Enqueue(((long)(connection.Items["lantern.hello-size"] ?? 0L), handshake?.Exception));
        }
    });
    listener.UseTlsClientHelloListener((connection, bytes) =>
    {
        // Read synchronously. Copy into owned storage if bytes must outlive this callback.
        connection.Items["lantern.hello-size"] = (long)bytes.Length;
    }, TimeSpan.FromSeconds(3));
    listener.UseHttps(https =>
    {
        https.ServerCertificate = certificate;
        https.SslProtocols = SslProtocols.Tls12;
        https.HandshakeTimeout = TimeSpan.FromSeconds(3);
    });
}));
// endsnippet:observe
await using var app = builder.Build();
app.MapGet("/lantern", () => "Lantern station ready");
await app.StartAsync();
var address = new Uri(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single());
using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(15));
using (var client = new TcpClient())
{
    await client.ConnectAsync(IPAddress.Loopback, address.Port, deadline.Token);
    using var tls = new SslStream(client.GetStream(), false, (_, remote, _, _) => remote?.GetCertHashString() == certificate.Thumbprint);
    await tls.AuthenticateAsClientAsync(new SslClientAuthenticationOptions { TargetHost = "localhost", EnabledSslProtocols = SslProtocols.Tls12 }, deadline.Token);
    await tls.WriteAsync("GET /lantern HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n\r\n"u8.ToArray(), deadline.Token);
    using var reader = new StreamReader(tls);
    string response = await reader.ReadToEndAsync(deadline.Token);
    if (!response.Contains("Lantern station ready", StringComparison.Ordinal)) { throw new Exception("TLS request failed."); }
}
using (var client = new TcpClient())
{
    await client.ConnectAsync(IPAddress.Loopback, address.Port, deadline.Token);
    using var tls = new SslStream(client.GetStream(), false, (_, _, _, _) => true);
    try
    {
        await tls.AuthenticateAsClientAsync(new SslClientAuthenticationOptions { TargetHost = "localhost", EnabledSslProtocols = SslProtocols.Tls13 }, deadline.Token);
        throw new Exception("Incompatible TLS unexpectedly succeeded.");
    }
    catch (AuthenticationException) { }
}
await app.StopAsync(deadline.Token);
var seen = observations.ToArray();
if (seen.Length != 2 || seen.Any(item => item.HelloBytes <= 0) || seen.Count(item => item.Failure is not null) != 1)
{
    throw new Exception($"Expected two ClientHellos and exactly one handshake failure: {string.Join(';', seen)}");
}
Console.WriteLine($"PASS {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}: two ClientHellos; HTTPS response; failed TLS exception={seen.Single(item => item.Failure is not null).Failure!.GetType().Name}");
// endsnippet:complete
