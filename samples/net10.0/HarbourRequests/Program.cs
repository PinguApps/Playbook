// snippet:complete
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Server.HttpSys;

if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("HTTP.sys requires Windows.");
args = [Environment.GetEnvironmentVariable("PLAYBOOK_HTTPS_PREFIX") ?? "", Environment.GetEnvironmentVariable("PLAYBOOK_CERTIFICATE") ?? ""];
if (args.Length != 2 || !Uri.TryCreate(args[0], UriKind.Absolute, out var endpoint)
    || endpoint.Scheme != "https" || endpoint.Host != "127.0.0.1")
    throw new ArgumentException("Supply the prepared loopback HTTPS prefix and certificate thumbprint.");
var builder = WebApplication.CreateBuilder(Array.Empty<string>());
builder.WebHost.UseHttpSys(options => {
    options.UrlPrefixes.Add(endpoint.AbsoluteUri);
    options.Authentication.Schemes = AuthenticationSchemes.None;
    options.Authentication.AllowAnonymous = true;
});
await using var app = builder.Build();
app.Run(context => context.Response.WriteAsync("Willow Quay HTTP.sys access"));
using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(20));
await app.StartAsync(deadline.Token);
try {
    using var handler = new HttpClientHandler { UseProxy = false };
    // This probe pins one certificate. It does not establish OS/browser trust.
    handler.ServerCertificateCustomValidationCallback = (_, certificate, _, _) =>
        certificate is not null && string.Equals(certificate.Thumbprint, args[1], StringComparison.OrdinalIgnoreCase);
    using var client = new HttpClient(handler);
    var actual = await client.GetStringAsync(endpoint, deadline.Token);
    if (actual != "Willow Quay HTTP.sys access") throw new InvalidOperationException(actual);
    Console.WriteLine($"PASS: HTTP.sys HTTPS loopback with pinned certificate; {RuntimeInformation.FrameworkDescription}");
}
finally { await app.StopAsync(CancellationToken.None); }

// endsnippet:complete
