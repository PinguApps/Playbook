// snippet:complete
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using LanternForwarding;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

using RSA key = RSA.Create(2048);
var certificateRequest = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
using X509Certificate2 generated = certificateRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddHours(1));
using X509Certificate2 certificate = X509CertificateLoader.LoadPkcs12(generated.Export(X509ContentType.Pfx), null);
var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.WebHost.ConfigureKestrel(server =>
{
    server.Listen(IPAddress.Loopback, 0, listener => listener.Protocols = HttpProtocols.Http1);
    server.Listen(IPAddress.Loopback, 0, listener => listener.Protocols = HttpProtocols.Http2);
    server.Listen(IPAddress.Loopback, 0, listener => { listener.Protocols = HttpProtocols.Http3; listener.UseHttps(certificate); });
});
int accepted = 0;
await using var app = builder.Build();
app.MapMethods("/delivery", ["GET", "POST"], async context =>
{
    Interlocked.Increment(ref accepted);
    if (context.Request.Headers.ContainsKey("x-lantern-station"))
    {
        string[] localFields = ["proxy-authorization", "proxy-authenticate", "trailer", "x-lantern-internal"];
        Require(!localFields.Any(context.Request.Headers.ContainsKey), "Proxy credentials or connection-local fields reached the origin.");
    }
    await context.Request.Body.CopyToAsync(Stream.Null);
    context.Response.ContentLength = 2;
    await context.Response.WriteAsync("OK");
});
await app.StartAsync();
var addresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Select(value => new Uri(value)).ToArray();
using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(25));
// Both headers used to be accepted with keep-alive. Servicing may backport connection closure.
string ambiguous = "POST /delivery HTTP/1.1\r\nHost: lantern\r\nContent-Length: 42\r\nTransfer-Encoding: chunked\r\n\r\n2\r\nOK\r\n0\r\n\r\n";
string dual = await SendHttp1Async(ambiguous);
Require(dual.Contains("200 OK", StringComparison.Ordinal), "CL+TE request must be processed, not described as rejected.");
Require(dual.Contains("Connection: close", StringComparison.OrdinalIgnoreCase), "Serviced runtime must close CL+TE connection.");
// snippet:framing
string corrected = "POST /delivery HTTP/1.1\r\nHost: lantern\r\nContent-Length: 2\r\n\r\nOK";
// A known body length is sent once; a streaming HTTP/1.1 writer can instead use only chunked framing.
// endsnippet:framing
string pipeline = corrected + "GET /delivery HTTP/1.1\r\nHost: lantern\r\nConnection: close\r\n\r\n";
Require((await SendHttp1Async(pipeline)).Split("200 OK").Length == 3, "Corrected H1 framing must permit two requests on one connection.");

foreach (var header in new[] { ("connection", "close"), ("proxy-connection", "keep-alive"), ("keep-alive", "timeout=5"), ("transfer-encoding", "chunked"), ("upgrade", "h2c"), ("te", "gzip") })
{
    bool reset = await SendHttp2Async([header]);
    Require(reset, $"Malformed H2 {header.Item1} must be rejected.");
    var clean = ForwardHeaders([header, ("x-lantern-station", "north"), ("te", "trailers"),
        ("connection", "x-lantern-internal"), ("x-lantern-internal", "local"),
        ("proxy-authorization", "Basic Zml4dHVyZTpsb2NhbA=="), ("proxy-authenticate", "Basic"), ("trailer", "x-checksum")], 2).ToArray();
    Require(!await SendHttp2Async(clean), $"Corrected H2 {header.Item1} must receive a response.");
    if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
    {
        bool h3Rejected = await Http3Probe.SendAsync(addresses[2].Port, certificate, [header], deadline.Token);
        Require(h3Rejected, $"Malformed H3 {header.Item1} must fail with H3_MESSAGE_ERROR.");
        Require(!await Http3Probe.SendAsync(addresses[2].Port, certificate, clean, deadline.Token), "Corrected H3 must receive OK.");
    }
    else { throw new PlatformNotSupportedException("HTTP/3 fixture requires a supported QUIC OS."); }
}
Require(accepted == 15, $"Expected 15 accepted H1/H2 requests, got {accepted}.");
await app.StopAsync(deadline.Token);
Console.WriteLine(string.Join(", ", Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Where(module => module.ModuleName.Contains("msquic", StringComparison.OrdinalIgnoreCase)).Select(module => $"{module.ModuleName} {module.FileVersionInfo.FileVersion}")));
Console.WriteLine($"PASS {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}: H1 CL+TE processed then closed; corrected pipeline accepted; six malformed H2 requests rejected and six corrected requests accepted. H3 malformed/corrected QPACK requests verified.");

// snippet:filter
static IEnumerable<(string Name, string Value)> ForwardHeaders(IEnumerable<(string Name, string Value)> incoming, int protocolMajor)
{
    var headers = incoming.ToArray();
    var remove = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "connection", "proxy-connection", "keep-alive", "transfer-encoding", "upgrade", "content-length", "host",
        "proxy-authorization", "proxy-authenticate", "trailer"
    };
    // Also remove fields nominated by an incoming HTTP/1 Connection header.
    foreach (var header in headers.Where(header => header.Name.Equals("connection", StringComparison.OrdinalIgnoreCase)))
    {
        foreach (string token in header.Value.Split(',')) { remove.Add(token.Trim()); }
    }
    foreach (var header in headers)
    {
        if (remove.Contains(header.Name)) { continue; }
        if (header.Name.Equals("te", StringComparison.OrdinalIgnoreCase) && (protocolMajor < 2 || !header.Value.Trim().Equals("trailers", StringComparison.OrdinalIgnoreCase))) { continue; }
        yield return (header.Name.ToLowerInvariant(), header.Value);
    }
    // This filter only removes inbound fields. The outbound writer must generate authority and body framing.
}
// endsnippet:filter

async Task<string> SendHttp1Async(string request)
{
    using var client = new TcpClient();
    await client.ConnectAsync(IPAddress.Loopback, addresses[0].Port, deadline.Token);
    await client.GetStream().WriteAsync(Encoding.ASCII.GetBytes(request), deadline.Token);
    using var reader = new StreamReader(client.GetStream());
    return await reader.ReadToEndAsync(deadline.Token);
}

async Task<bool> SendHttp2Async((string Name, string Value)[] headers)
{
    using var client = new TcpClient();
    await client.ConnectAsync(IPAddress.Loopback, addresses[1].Port, deadline.Token);
    NetworkStream stream = client.GetStream();
    await stream.WriteAsync("PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n"u8.ToArray(), deadline.Token);
    await WriteFrameAsync(stream, 4, 0, 0, []);
    // HPACK static indices: GET, http, custom :path and :authority. Literal fields use no Huffman/dynamic table.
    var block = new List<byte> { 0x82, 0x86, 0x04 };
    AddString(block, "/delivery");
    block.Add(0x01);
    AddString(block, "lantern");
    foreach (var header in headers) { block.Add(0); AddString(block, header.Name); AddString(block, header.Value); }
    await WriteFrameAsync(stream, 1, 5, 1, block.ToArray()); // END_HEADERS | END_STREAM
    while (true)
    {
        byte[] frame = new byte[9];
        try { await stream.ReadExactlyAsync(frame, deadline.Token); }
        catch (IOException exception) when (exception.InnerException is SocketException socket && socket.SocketErrorCode is SocketError.ConnectionAborted or SocketError.ConnectionReset)
        {
            Console.WriteLine($"H2 {headers[0].Name}: transport aborted before response");
            return true;
        }
        int length = (frame[0] << 16) | (frame[1] << 8) | frame[2];
        byte[] payload = new byte[length];
        await stream.ReadExactlyAsync(payload, deadline.Token);
        int id = BinaryPrimitives.ReadInt32BigEndian(frame.AsSpan(5)) & int.MaxValue;
        if (frame[3] == 4 && frame[4] == 0) { await WriteFrameAsync(stream, 4, 1, 0, []); }
        if (id == 1 && frame[3] == 3)
        {
            Require(BinaryPrimitives.ReadInt32BigEndian(payload) == 1, "Expected PROTOCOL_ERROR reset.");
            return true;
        }
        if (id == 1 && frame[3] == 0 && Encoding.ASCII.GetString(payload).Contains("OK", StringComparison.Ordinal)) { return false; }
        if (frame[3] == 7) { Require(BinaryPrimitives.ReadInt32BigEndian(payload.AsSpan(4)) == 1, "Expected PROTOCOL_ERROR GOAWAY."); return true; }
    }
}

async Task WriteFrameAsync(Stream stream, byte type, byte flags, int id, byte[] payload)
{
    byte[] frame = new byte[9 + payload.Length];
    frame[0] = (byte)(payload.Length >> 16); frame[1] = (byte)(payload.Length >> 8); frame[2] = (byte)payload.Length;
    frame[3] = type; frame[4] = flags;
    BinaryPrimitives.WriteInt32BigEndian(frame.AsSpan(5), id);
    payload.CopyTo(frame, 9);
    await stream.WriteAsync(frame, deadline.Token);
}

static void AddString(List<byte> block, string text)
{
    byte[] bytes = Encoding.ASCII.GetBytes(text);
    if (bytes.Length >= 127) { throw new ArgumentException("Fixture uses short literals only."); }
    block.Add((byte)bytes.Length); block.AddRange(bytes);
}
static void Require(bool condition, string message) { if (!condition) { throw new Exception(message); } }
// endsnippet:complete
