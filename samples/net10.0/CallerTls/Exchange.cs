// snippet:stream-tls
using System.Net.Security;

namespace Samples;

public static class DispatchChannel
{
    public static async Task<byte[]> SendAsync(Stream transport, SslClientAuthenticationOptions options, byte[] parcel, CancellationToken cancellationToken)
    {
        // Application supplies the stream; SslStream drives the TLS I/O itself.
        using SslStream secure = new(transport, leaveInnerStreamOpen: true);
        await secure.AuthenticateAsClientAsync(options, cancellationToken);
        await secure.WriteAsync(parcel, cancellationToken);
        byte[] reply = new byte[parcel.Length];
        await secure.ReadExactlyAsync(reply, cancellationToken);
        return reply;
    }
}
// endsnippet:stream-tls
