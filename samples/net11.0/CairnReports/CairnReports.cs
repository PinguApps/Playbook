// snippet:cairn-reports
using System.Net;

namespace Playbook.Samples;

internal static class CairnReports
{
    public static HttpClient Create() => new(new SocketsHttpHandler
    {
        UseProxy = false,
        AllowAutoRedirect = false,
        AutomaticDecompression = DecompressionMethods.Zstandard | DecompressionMethods.GZip |
            DecompressionMethods.Deflate | DecompressionMethods.Brotli
    });
}
// endsnippet:cairn-reports
