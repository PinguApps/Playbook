// snippet:cairn-relay
using System.Net;

namespace Playbook.Samples;

internal static class CairnRelay
{
    public static HttpClient Create(int proxyPort, NetworkCredential? credentials = null) => new(new SocketsHttpHandler
    {
        Proxy = new WebProxy(new Uri($"socks5h://127.0.0.1:{proxyPort}")) { Credentials = credentials },
        UseProxy = true,
        AllowAutoRedirect = false
    });
}
// endsnippet:cairn-relay
