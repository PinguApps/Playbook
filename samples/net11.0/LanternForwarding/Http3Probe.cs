// snippet:complete
using System.Net;
using System.Net.Quic;
using System.Net.Security;
using System.Runtime.Versioning;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace LanternForwarding;

// Small wire fixture: no client header normalization can hide malformed input.
[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
internal static class Http3Probe
{
    public static async Task<bool> SendAsync(int port, X509Certificate2 certificate, (string Name, string Value)[] headers, CancellationToken token)
    {
        if (!QuicConnection.IsSupported) { throw new PlatformNotSupportedException("Real HTTP/3 requires MsQuic support."); }
        await using var connection = await QuicConnection.ConnectAsync(new QuicClientConnectionOptions
        {
            RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, port),
            DefaultCloseErrorCode = 0x100,
            DefaultStreamErrorCode = 0x10c,
            MaxInboundUnidirectionalStreams = 8,
            ClientAuthenticationOptions = new SslClientAuthenticationOptions
            {
                TargetHost = "localhost",
                ApplicationProtocols = [SslApplicationProtocol.Http3],
                RemoteCertificateValidationCallback = (_, remote, _, _) => remote?.GetCertHashString() == certificate.Thumbprint
            }
        }, token);
        await using var control = await connection.OpenOutboundStreamAsync(QuicStreamType.Unidirectional, token);
        await control.WriteAsync(new byte[] { 0, 4, 0 }, token); // Control stream, empty SETTINGS.
        await using var stream = await connection.OpenOutboundStreamAsync(QuicStreamType.Bidirectional, token);
        var block = new List<byte> { 0, 0, 0xd1, 0xd7, 0x51 }; // QPACK prefix, GET, https, literal :path.
        AddString(block, "/delivery");
        block.Add(0x50); AddString(block, "lantern"); // :authority
        foreach (var header in headers)
        {
            int length = Encoding.ASCII.GetByteCount(header.Name);
            block.Add((byte)(0x20 | Math.Min(length, 7)));
            if (length >= 7) { block.Add((byte)(length - 7)); }
            block.AddRange(Encoding.ASCII.GetBytes(header.Name));
            AddString(block, header.Value);
        }
        var frame = new List<byte> { 1 };
        AddVarInt(frame, block.Count); frame.AddRange(block);
        await stream.WriteAsync(frame.ToArray(), token);
        stream.CompleteWrites();
        try
        {
            while (true)
            {
                int type = await ReadVarIntAsync(stream, token);
                int length = await ReadVarIntAsync(stream, token);
                byte[] payload = new byte[length];
                await stream.ReadExactlyAsync(payload, token);
                if (type == 0 && Encoding.ASCII.GetString(payload) == "OK") { return false; }
            }
        }
        catch (QuicException exception) when (exception.QuicError == QuicError.StreamAborted && exception.ApplicationErrorCode == 0x10e)
        {
            return true; // H3_MESSAGE_ERROR, not a client-side validation failure.
        }
    }

    private static void AddString(List<byte> target, string value)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(value);
        if (bytes.Length >= 127) { throw new ArgumentException("Fixture strings must be short."); }
        target.Add((byte)bytes.Length); target.AddRange(bytes);
    }
    private static void AddVarInt(List<byte> target, int value)
    {
        if (value < 64) { target.Add((byte)value); }
        else if (value < 16384) { target.Add((byte)(0x40 | (value >> 8))); target.Add((byte)value); }
        else { throw new ArgumentOutOfRangeException(nameof(value)); }
    }
    private static async Task<int> ReadVarIntAsync(Stream stream, CancellationToken token)
    {
        byte[] first = new byte[1];
        await stream.ReadExactlyAsync(first, token);
        int length = 1 << (first[0] >> 6);
        int value = first[0] & 63;
        for (int i = 1; i < length; i++) { await stream.ReadExactlyAsync(first, token); value = checked((value << 8) | first[0]); }
        return value;
    }
}
// endsnippet:complete
