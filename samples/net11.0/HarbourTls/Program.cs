// snippet:complete
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.DirectTls;
var builder = WebApplication.CreateBuilder(args);
string certificatePath = builder.Configuration["CertificatePath"] ?? throw new InvalidOperationException("Supply CertificatePath.");
string password = builder.Configuration["CertificatePassword"] ?? throw new InvalidOperationException("Supply CertificatePassword through local configuration, not source.");
using var certificate = X509CertificateLoader.LoadPkcs12FromFile(certificatePath, password);
// Documentation-based configuration. The pinned RC1 package lacks DirectTlsEndpoint.
#pragma warning disable ASPNETCORE_DIRECTTLS_001 // Explicit experimental transport adoption.
builder.WebHost.UseKestrel();
builder.WebHost.UseDirectTls();
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Loopback, 5443, listen => { listen.Protocols = HttpProtocols.Http1; listen.UseHttps(certificate); });
    var direct = new DirectTlsEndpoint(IPAddress.Loopback, 5444);
    direct.Options.ServerCertificate = certificate;
    options.Listen(direct, listen => listen.Protocols = HttpProtocols.Http1);
});
#pragma warning restore ASPNETCORE_DIRECTTLS_001
var app = builder.Build();
app.MapGet("/catalogue", () => "Harbour archive");
await app.RunAsync();
// endsnippet:complete
