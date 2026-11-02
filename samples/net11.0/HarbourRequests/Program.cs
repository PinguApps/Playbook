// snippet:complete
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.HttpSys;

if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Windows HTTP.sys is required.");
var prefix = Environment.GetEnvironmentVariable("PLAYBOOK_HTTPS_PREFIX")
    ?? throw new InvalidOperationException("Supply the owner-prepared loopback HTTPS prefix.");
var thumbprint = Environment.GetEnvironmentVariable("PLAYBOOK_CERTIFICATE")
    ?? throw new InvalidOperationException("Supply the exact test certificate thumbprint.");
if (!Uri.TryCreate(prefix, UriKind.Absolute, out var endpoint) || endpoint.Scheme != "https"
    || endpoint.Host != "127.0.0.1") throw new ArgumentException("Only loopback HTTPS is permitted.");
foreach (var level in Enum.GetValues<HttpAuthenticationHardeningLevel>())
{
    var builder = WebApplication.CreateBuilder(Array.Empty<string>());
    builder.WebHost.UseHttpSys(options =>
    {
        options.UrlPrefixes.Add(prefix);
        options.Authentication.Schemes = AuthenticationSchemes.None;
        options.Authentication.AllowAnonymous = true;
        options.HttpAuthenticationHardeningLevel = level;
    });
    await using var app = builder.Build();
    Exception? failure = null;
    app.Run(async context =>
    {
        try
        {
            var tls = context.Features.Get<ITlsConnectionFeature>()
                ?? throw new InvalidOperationException("HTTPS TLS feature missing.");
            var available = tls.TryGetChannelBindingBytes(ChannelBindingKind.Endpoint, out var binding);
            if (available != (level != HttpAuthenticationHardeningLevel.Legacy)
                || (available && binding.IsEmpty)) throw new InvalidOperationException("Unexpected endpoint binding.");
            Console.WriteLine($"Harbour binding: {level}, available={available}, bytes={binding.Length}");
            if (level == HttpAuthenticationHardeningLevel.Medium)
            {
                var properties = context.Features.Get<IHttpSysRequestPropertyFeature>()
                    ?? throw new InvalidOperationException("Native request property feature missing.");
                // http.h 10.0.26100.0: property 4, HTTP_REQUEST_PROPERTY_SNI.
                // WCHAR Hostname[256] followed by ULONG Flags: 516 bytes; no qualifier.
                const int sni = 4;
                var empty = properties.TryGetRequestProperty(sni, [], [], out var required);
                Console.WriteLine($"SNI empty: success={empty}, bytesReturned={required}");
                if (empty || required != 0) throw new InvalidOperationException("Unexpected empty SNI result on the qualified host.");
                var small = new byte[515];
                if (properties.TryGetRequestProperty(sni, [], small, out var needed) || needed != 0)
                    throw new InvalidOperationException("Unexpected small SNI result on the qualified host.");
                var buffer = new byte[516];
                Console.WriteLine($"SNI small: bytesReturned={needed}");
                if (!properties.TryGetRequestProperty(sni, [], buffer, out var written) || written != buffer.Length)
                    throw new InvalidOperationException("Exact SNI buffer failed.");
                var name = Encoding.Unicode.GetString(buffer, 0, 512).TrimEnd('\0');
                var flags = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(512));
                if (name.Length != 0 || flags != 2) throw new InvalidOperationException("Expected no SNI for the IP-literal client.");
                Console.WriteLine($"SNI exact: bytesReturned={written}, hostname='{name}', flags={flags}");
                try
                {
                    properties.TryGetRequestProperty(int.MaxValue, [], buffer, out _);
                    throw new InvalidOperationException("Invalid property unexpectedly succeeded.");
                }
                catch (HttpSysException error) { Console.WriteLine($"Native invalid property: {error.NativeErrorCode}"); }
            }
            await context.Response.WriteAsync("Harbour diagnostics checked");
        }
        catch (Exception error) { failure = error; context.Response.StatusCode = 500; }
    });
    using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(20));
    await app.StartAsync(deadline.Token);
    try
    {
        using var handler = new HttpClientHandler { UseProxy = false };
        handler.ServerCertificateCustomValidationCallback = (_, certificate, _, _) =>
            certificate is not null && string.Equals(certificate.Thumbprint, thumbprint, StringComparison.OrdinalIgnoreCase);
        using var client = new HttpClient(handler);
        using var response = await client.GetAsync(endpoint, deadline.Token);
        if (failure is not null) throw new InvalidOperationException("Real request check failed.", failure);
        response.EnsureSuccessStatusCode();
        if (await response.Content.ReadAsStringAsync(deadline.Token) != "Harbour diagnostics checked")
            throw new InvalidOperationException("Response body mismatch.");
    }
    finally { await app.StopAsync(CancellationToken.None); }
}
Console.WriteLine($"PASS: Harbour HTTP.sys checks; {RuntimeInformation.FrameworkDescription}");
// endsnippet:complete
