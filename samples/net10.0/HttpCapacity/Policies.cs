using System.IO.Compression;
using System.Net;
using System.Text;

namespace Playbook.Samples;

// snippet:compression
public static class RequestPolicy
{
    public static HttpContent Compress(string report, string encoding)
    {
        using var buffer = new MemoryStream();
        using (Stream compressor = encoding switch
        {
            "gzip" => new GZipStream(buffer, CompressionLevel.Fastest, leaveOpen: true),
            "br" => new BrotliStream(buffer, CompressionLevel.Fastest, leaveOpen: true),
            _ => throw new ArgumentException("Choose gzip or br.", nameof(encoding)),
        })
        {
            compressor.Write(Encoding.UTF8.GetBytes(report));
        }
        var content = new ByteArrayContent(buffer.ToArray());
        content.Headers.ContentType = new("text/plain") { CharSet = "utf-8" };
        content.Headers.ContentEncoding.Add(encoding);
        return content;
    }
}
// endsnippet:compression

// snippet:retirement
public static class ConnectionPolicy
{
    public static SocketsHttpHandler Create(Func<bool> retirementRequested) => new()
    {
        UseProxy = false, // Direct loopback fixture; choose proxy policy for production.
        MaxConnectionsPerServer = 1,
        // Short authoring-test interval; choose a suitable production lifetime.
        PooledConnectionLifetime = TimeSpan.FromMilliseconds(250),
    };
    // .NET 10 cannot use retirementRequested as a per-connection pool callback.
}
// endsnippet:retirement

// snippet:capacity
public static class StreamPolicy
{
    public static SocketsHttpHandler Create() => new() { UseProxy = false };

    public static async Task SendAsync(HttpClient client, Uri uri, SemaphoreSlim admission, CancellationToken token)
    {
        // Reuse one admission gate across this client's requests. This is an
        // application limit for the entire response, not a pre-SETTINGS setting.
        await admission.WaitAsync(token);
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri)
            {
                Version = HttpVersion.Version20,
                VersionPolicy = HttpVersionPolicy.RequestVersionExact,
            };
            using HttpResponseMessage response = await client.SendAsync(request, token);
            response.EnsureSuccessStatusCode();
        }
        finally { admission.Release(); }
    }
}
// endsnippet:capacity
