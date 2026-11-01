// snippet:cairn-reports
using System.Net;
using System.IO.Compression;
using ZstdSharp;

namespace Playbook.Samples;

// ZstdSharp.Port 0.8.6, managed lib/net9.0 asset on this .NET 10 consumer.
internal static class CairnReports
{
    public static HttpClient Create() => new(new ZstdResponseHandler
    {
        InnerHandler = new SocketsHttpHandler
        {
            UseProxy = false,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None
        }
    });

    private sealed class ZstdResponseHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            foreach (string encoding in new[] { "zstd", "gzip", "deflate", "br" })
                if (!request.Headers.AcceptEncoding.Any(h => h.Value.Equals(encoding, StringComparison.OrdinalIgnoreCase)))
                    request.Headers.AcceptEncoding.ParseAdd(encoding);
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            string[] encodings = response.Content.Headers.ContentEncoding.ToArray();
            string? outer = encodings.LastOrDefault()?.ToLowerInvariant();
            // Decode exactly one outer layer, like the automatic .NET 11 handler.
            if (outer is "zstd" or "gzip" or "deflate" or "br")
            {
                try
                {
                    HttpContent original = response.Content;
                    Stream raw = await original.ReadAsStreamAsync(cancellationToken);
                    Stream decoder = outer switch
                    {
                        "zstd" => new DecompressionStream(raw),
                        "gzip" => new GZipStream(raw, CompressionMode.Decompress),
                        "deflate" => new ZLibStream(raw, CompressionMode.Decompress),
                        _ => new BrotliStream(raw, CompressionMode.Decompress)
                    };
                    var decoded = new OwnedContent(decoder, original);
                    foreach (var header in original.Headers)
                        if (!header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) && !header.Key.Equals("Content-Encoding", StringComparison.OrdinalIgnoreCase))
                            decoded.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    foreach (string encoding in encodings[..^1]) decoded.Headers.ContentEncoding.Add(encoding);
                    response.Content = decoded;
                }
                catch { response.Dispose(); throw; }
            }
            return response;
        }
    }

    // Disposing the returned content releases both the decoder and original HTTP content.
    private sealed class OwnedContent : StreamContent
    {
        private readonly Stream decoder;
        private readonly HttpContent original;
        public OwnedContent(Stream decoder, HttpContent original) : base(decoder)
        {
            this.decoder = decoder;
            this.original = original;
        }
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken) =>
            decoder.CopyToAsync(stream, cancellationToken);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing) { decoder.Dispose(); original.Dispose(); }
        }
    }
}
// endsnippet:cairn-reports
