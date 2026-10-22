// snippet:quic-order
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
        // .NET 10 has no public stream-priority setting. Choose when to submit work.
        await notice.WriteAsync("dispatch:urgent"u8.ToArray(), completeWrites: true, cancellationToken);
        await archive.WriteAsync("dispatch:archive"u8.ToArray(), completeWrites: true, cancellationToken);
    }
}
// endsnippet:quic-order
