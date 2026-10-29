// snippet:complete
using System.Diagnostics;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Quic;

[assembly: SupportedOSPlatform("windows")]

if (!QuicConnection.IsSupported || !QuicListener.IsSupported)
    throw new PlatformNotSupportedException("This reproduction requires Windows with working MsQuic and TLS 1.3.");
using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(45));
using var key = RSA.Create(2048);
var request = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
var names = new SubjectAlternativeNameBuilder();
names.AddDnsName("localhost"); names.AddIpAddress(IPAddress.Loopback);
request.CertificateExtensions.Add(names.Build());
using var generated = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddHours(1));
using var certificate = X509CertificateLoader.LoadPkcs12(generated.Export(X509ContentType.Pfx), null);
var builder = WebApplication.CreateBuilder();
builder.Logging.ClearProviders();
// These are per-connection transport settings, not a request/user quota.
builder.Services.Configure<QuicTransportOptions>(options =>
{
    options.MaxBidirectionalStreamCount = 2;
    options.MaxUnidirectionalStreamCount = 3; // HTTP/3 control and QPACK streams need credit.
    options.MaxReadBufferSize = 16 * 1024;
    options.MaxWriteBufferSize = 16 * 1024;
});
builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0, listen =>
{
    listen.Protocols = HttpProtocols.Http3;
    listen.UseHttps(certificate);
}));
await using var app = builder.Build();
var twoEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
var allowRead = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
var connections = new HashSet<string>();
int entered = 0, written = 0;
const int total = 16 * 1024 * 1024;
app.MapGet("/observations/hold", async context =>
{
    lock (connections) connections.Add(context.Connection.Id);
    if (Interlocked.Increment(ref entered) == 2) twoEntered.TrySetResult();
    await release.Task.WaitAsync(context.RequestAborted);
    await context.Response.WriteAsync("estuary", context.RequestAborted);
});
app.MapGet("/observations/export", async context =>
{
    byte[] chunk = new byte[16 * 1024];
    context.Response.ContentLength = total;
    for (int offset = 0; offset < total; offset += chunk.Length)
    {
        await context.Response.Body.WriteAsync(chunk, context.RequestAborted);
        Interlocked.Add(ref written, chunk.Length);
    }
});
app.MapPost("/observations/import", async context =>
{
    await allowRead.Task.WaitAsync(context.RequestAborted);
    using var sink = new MemoryStream();
    await context.Request.Body.CopyToAsync(sink, context.RequestAborted);
    Require(sink.Length == total, "Incomplete upload");
    await context.Response.WriteAsync(sink.Length.ToString(), context.RequestAborted);
});
await app.StartAsync(deadline.Token);
var address = new Uri(app.Urls.Single());
using var handler = new SocketsHttpHandler
{
    UseProxy = false, EnableMultipleHttp3Connections = false,
    SslOptions = new SslClientAuthenticationOptions
    {
        // Fixture-local certificate pin; do not copy an allow-all validator into an app.
        RemoteCertificateValidationCallback = (_, peer, _, _) => peer?.GetCertHashString() == certificate.GetCertHashString()
    }
};
using var client = new HttpClient(handler)
{
    BaseAddress = address, DefaultRequestVersion = HttpVersion.Version30,
    DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact, Timeout = TimeSpan.FromSeconds(40)
};
Task<string> first = client.GetStringAsync(new Uri(address, "/observations/hold"), deadline.Token);
Task<string> second = client.GetStringAsync(new Uri(address, "/observations/hold"), deadline.Token);
Task ready = await Task.WhenAny(twoEntered.Task, first, second).WaitAsync(deadline.Token);
await ready;
await twoEntered.Task.WaitAsync(deadline.Token);
Task<string> third = client.GetStringAsync(new Uri(address, "/observations/hold"), deadline.Token);
await Task.Delay(300, deadline.Token);
Require(entered == 2 && !third.IsCompleted, "Third HTTP/3 request bypassed stream credit");
release.SetResult();
Require((await Task.WhenAll(first, second, third)).All(value => value == "estuary"), "Request did not resume");
Require(entered == 3 && connections.Count == 1, "Expected one connection, three completed requests");
Console.WriteLine("PASS HTTP/3: two concurrent requests; third queued then resumed on the same connection.");

using (var response = await client.GetAsync(new Uri(address, "/observations/export"), HttpCompletionOption.ResponseHeadersRead, deadline.Token))
{
    Require(response.Version == HttpVersion.Version30, "HTTP/3 fallback");
    await Task.Delay(300, deadline.Token);
    int before = Volatile.Read(ref written);
    await Task.Delay(300, deadline.Token);
    Require(before > 0 && before < total && Volatile.Read(ref written) == before, "Slow reader did not backpressure writes");
    Require((await response.Content.ReadAsByteArrayAsync(deadline.Token)).Length == total, "Incomplete export");
    Console.WriteLine($"PASS slow download: writer paused at {before} bytes then completed {total} bytes.");
}
using (var content = new ObservationContent(total))
{
    Task<HttpResponseMessage> upload = client.PostAsync(new Uri(address, "/observations/import"), content, deadline.Token);
    await content.Started.Task.WaitAsync(deadline.Token);
    await Task.Delay(300, deadline.Token);
    int before = content.Written;
    await Task.Delay(300, deadline.Token);
    Require(before > 0 && before < total && content.Written == before && !upload.IsCompleted, "Slow receiver did not backpressure upload");
    allowRead.SetResult();
    using var response = await upload;
    Require(await response.Content.ReadAsStringAsync(deadline.Token) == total.ToString(), "Upload did not resume");
    Console.WriteLine($"PASS slow upload: sender paused at {before} bytes then completed {total} bytes.");
}

// A second real QUIC connection checks inbound unidirectional credit directly.
await using (var connection = await QuicConnection.ConnectAsync(new QuicClientConnectionOptions
{
    RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, address.Port),
    DefaultStreamErrorCode = 0x10c, DefaultCloseErrorCode = 0x100,
    MaxInboundUnidirectionalStreams = 3,
    ClientAuthenticationOptions = new SslClientAuthenticationOptions
    {
        TargetHost = "localhost", ApplicationProtocols = [SslApplicationProtocol.Http3],
        RemoteCertificateValidationCallback = (_, peer, _, _) => peer?.GetCertHashString() == certificate.GetCertHashString()
    }
}, deadline.Token))
{
    await using var control = await connection.OpenOutboundStreamAsync(QuicStreamType.Unidirectional, deadline.Token);
    await control.WriteAsync(new byte[] { 0, 4, 0 }, deadline.Token); // control stream, empty SETTINGS frame
    await using var encoder = await connection.OpenOutboundStreamAsync(QuicStreamType.Unidirectional, deadline.Token);
    await encoder.WriteAsync(new byte[] { 2 }, deadline.Token);
    await using var decoder = await connection.OpenOutboundStreamAsync(QuicStreamType.Unidirectional, deadline.Token);
    await decoder.WriteAsync(new byte[] { 3 }, deadline.Token);
    using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
    try
    {
        await using var extra = await connection.OpenOutboundStreamAsync(QuicStreamType.Unidirectional, timeout.Token);
        throw new Exception("Fourth inbound unidirectional stream bypassed credit");
    }
    catch (OperationCanceledException) when (timeout.IsCancellationRequested)
    {
        Console.WriteLine("PASS fourth inbound unidirectional stream waits for credit; three protocol streams remain open.");
    }
    await connection.CloseAsync(0x100, deadline.Token);
}
await app.StopAsync(deadline.Token);
foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
    if (module.ModuleName.Contains("msquic", StringComparison.OrdinalIgnoreCase))
        Console.WriteLine($"NATIVE {module.FileName} | {module.FileVersionInfo.FileVersion} | {Convert.ToHexStringLower(SHA256.HashData(await File.ReadAllBytesAsync(module.FileName, deadline.Token)))}");
Console.WriteLine($"PASS EstuaryBudgets {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}: transport limits, read/write backpressure; no application quota or exact-memory-bound claim.");

static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }

sealed class ObservationContent(int contentLength) : HttpContent
{
    public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int written;
    public int Written => Volatile.Read(ref written);
    protected override bool TryComputeLength(out long length) { length = contentLength; return true; }
    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        byte[] chunk = new byte[16 * 1024];
        for (int offset = 0; offset < contentLength; offset += chunk.Length)
        {
            await stream.WriteAsync(chunk);
            Interlocked.Add(ref written, chunk.Length);
            Started.TrySetResult();
        }
    }
}
// endsnippet:complete
