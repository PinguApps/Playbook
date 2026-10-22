// snippet:quic-priority
using System.Net.Quic;
using System.Runtime.Versioning;

namespace Samples;

[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("osx")]
public static class DispatchPriority
{
    public static async Task SendAsync(QuicStream notice, QuicStream archive, CancellationToken cancellationToken)
    {
        if (notice.Priority != QuicStream.DefaultPriority || archive.Priority != QuicStream.DefaultPriority)
        {
            throw new InvalidOperationException("Expected default priority 127.");
        }
        notice.Priority = 240;
        archive.Priority = 16;
        // Both are eligible to send; priority concerns this connection's outgoing scheduler.
        await Task.WhenAll(
            notice.WriteAsync("dispatch:urgent"u8.ToArray(), completeWrites: true, cancellationToken).AsTask(),
            archive.WriteAsync("dispatch:archive"u8.ToArray(), completeWrites: true, cancellationToken).AsTask());
    }
}
// endsnippet:quic-priority
