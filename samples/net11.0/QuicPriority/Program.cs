// snippet:quic-connection
using System.Diagnostics;
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Samples;

if (!(OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS()) ||
    !QuicListener.IsSupported || !QuicConnection.IsSupported)
{
    throw new PlatformNotSupportedException("Both QUIC support probes must pass; do not report a skip as evidence.");
}
using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(20));
using X509Certificate2 certificate = TransportCertificate.Create();
SslApplicationProtocol protocol = new("dispatch-priority");
await using QuicListener listener = await QuicListener.ListenAsync(new()
{
    ListenEndPoint = new IPEndPoint(IPAddress.Loopback, 0),
    ApplicationProtocols = [protocol],
    ConnectionOptionsCallback = (_, _, _) => ValueTask.FromResult(new QuicServerConnectionOptions
    {
        DefaultStreamErrorCode = 13,
        DefaultCloseErrorCode = 14,
        MaxInboundBidirectionalStreams = 2,
        ServerAuthenticationOptions = new() { ServerCertificate = certificate, ApplicationProtocols = [protocol] }
    })
}, timeout.Token);
Task<QuicConnection> accept = listener.AcceptConnectionAsync(timeout.Token).AsTask();
SslClientAuthenticationOptions authentication = TransportCertificate.Client(certificate);
authentication.ApplicationProtocols = [protocol];
await using QuicConnection client = await QuicConnection.ConnectAsync(new()
{
    RemoteEndPoint = listener.LocalEndPoint,
    DefaultStreamErrorCode = 13,
    DefaultCloseErrorCode = 14,
    ClientAuthenticationOptions = authentication
}, timeout.Token);
await using QuicConnection server = await accept;
await using QuicStream notice = await client.OpenOutboundStreamAsync(QuicStreamType.Bidirectional, timeout.Token);
await using QuicStream archive = await client.OpenOutboundStreamAsync(QuicStreamType.Bidirectional, timeout.Token);
Task serverWork = Task.Run(async () =>
{
    Task[] exchanges = new Task[2];
    for (int i = 0; i < exchanges.Length; i++)
    {
        QuicStream stream = await server.AcceptInboundStreamAsync(timeout.Token);
        exchanges[i] = EchoAsync(stream, timeout.Token);
    }
    await Task.WhenAll(exchanges);
});
await DispatchPriority.SendAsync(notice, archive, timeout.Token);
string[] replies = await Task.WhenAll(ReadAsync(notice, timeout.Token), ReadAsync(archive, timeout.Token));
await serverWork;
if (replies[0] != "dispatch:urgent" || replies[1] != "dispatch:archive") { throw new InvalidOperationException("QUIC payload mismatch."); }
#if NET11_0_OR_GREATER
if (notice.Priority != 240 || archive.Priority != 16) { throw new InvalidOperationException("Priority configuration lost."); }
notice.Priority = 0;
archive.Priority = 255;
#endif
string native = Process.GetCurrentProcess().Modules.Cast<ProcessModule>()
    .Where(m => m.ModuleName.Contains("msquic", StringComparison.OrdinalIgnoreCase))
    .Select(m => $"{m.ModuleName} {m.FileVersionInfo.FileVersion}").Single();
Console.WriteLine($"PASS: QUIC real TLS 1.3 connection, two bidirectional stream echoes; {native}; {RuntimeInformation.FrameworkDescription}; no wire-order or bandwidth guarantee tested.");

static async Task EchoAsync(QuicStream stream, CancellationToken cancellationToken)
{
    await using (stream)
    {
        string message = await ReadAsync(stream, cancellationToken);
        await stream.WriteAsync(Encoding.UTF8.GetBytes(message), completeWrites: true, cancellationToken);
    }
}

static async Task<string> ReadAsync(QuicStream stream, CancellationToken cancellationToken)
{
    using MemoryStream buffer = new();
    await stream.CopyToAsync(buffer, cancellationToken);
    return Encoding.UTF8.GetString(buffer.ToArray());
}
// endsnippet:quic-connection
