using System.IO.Compression;
using System.Net;
using System.Text;

namespace Playbook.Samples;

// snippet:compression
public static class RequestPolicy
{
    public static HttpContent Compress(string report, string encoding)
    {
        HttpContent source = new StringContent(report, Encoding.UTF8, "text/plain");
        return encoding switch
        {
            "gzip" => new GZipCompressedContent(source, CompressionLevel.Fastest),
            "br" => new BrotliCompressedContent(source, CompressionLevel.Fastest),
            "zstd" => new ZstandardCompressedContent(source),
            _ => DisposeAndReject(source),
        };
    }

    private static HttpContent DisposeAndReject(HttpContent source)
    {
        source.Dispose();
        throw new ArgumentException("Choose gzip, br or zstd.");
    }
}
// endsnippet:compression

// snippet:retirement
public static class ConnectionPolicy
{
    public static SocketsHttpHandler Create(Func<bool> retirementRequested)
    {
        var handler = new SocketsHttpHandler
        {
            UseProxy = false,
            MaxConnectionsPerServer = 1,
            PooledConnectionLifetime = Timeout.InfiniteTimeSpan,
        };
        // Experimental API: isolate the opt-in to the reviewed callback.
#pragma warning disable SYSLIB5008
        handler.ShouldEvictConnection = (context, token) =>
        {
            token.ThrowIfCancellationRequested();
            // Read a cheap, thread-safe cached policy. In a multi-origin policy,
            // inspect context.DnsEndPoint and optional context.RemoteEndPoint.
            // Never retain context beyond this call.
            return Task.FromResult(retirementRequested());
        };
#pragma warning restore SYSLIB5008
        return handler;
    }
}
// endsnippet:retirement

// snippet:capacity
public static class StreamPolicy
{
    public static SocketsHttpHandler Create() => new()
    {
        UseProxy = false,
        InitialHttp2MaxConcurrentStreams = 2,
        EnableMultipleHttp2Connections = false,
    };

    public static async Task SendAsync(HttpClient client, Uri uri, SemaphoreSlim admission, CancellationToken token)
    {
        // No application gate: the initial limit yields to the peer's SETTINGS.
        using var request = new HttpRequestMessage(HttpMethod.Get, uri)
        {
            Version = HttpVersion.Version20,
            VersionPolicy = HttpVersionPolicy.RequestVersionExact,
        };
        using HttpResponseMessage response = await client.SendAsync(request, token);
        response.EnsureSuccessStatusCode();
    }
}
// endsnippet:capacity
