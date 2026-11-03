// snippet:complete
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);
string certificatePath = builder.Configuration["CertificatePath"] ?? throw new InvalidOperationException("Supply CertificatePath.");
string password = builder.Configuration["CertificatePassword"] ?? throw new InvalidOperationException("Supply CertificatePassword through local configuration, not source.");
using var certificate = X509CertificateLoader.LoadPkcs12FromFile(certificatePath, password);
builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 5443, listen =>
{
    listen.Protocols = HttpProtocols.Http1;
    listen.UseHttps(certificate);
}));
var app = builder.Build();
app.MapGet("/catalogue", () => "Harbour archive");
await app.RunAsync();
// endsnippet:complete
