// snippet:complete
using System.Net;
using System.Net.Http;
using Foundation;
namespace TernProxy;
public static class Route
{
    // One client per fixed destination/proxy policy; caller owns its lifetime.
    public static HttpClient Create(Uri destination, Uri proxy, NetworkCredential proxyCredential)
    {
        if (destination.Scheme != Uri.UriSchemeHttps) throw new ArgumentException("Tern uploads require HTTPS.", nameof(destination));
        if (proxy.Scheme != Uri.UriSchemeHttp) throw new ArgumentException("Use a plain HTTP CONNECT proxy.", nameof(proxy));
        var handler = new NSUrlSessionHandler
        {
            UseProxy = true,
            Proxy = new WebProxy(proxy) { Credentials = proxyCredential, BypassProxyOnLocal = false, BypassList = [] },
            AllowAutoRedirect = false
        };
        return new HttpClient(handler) { BaseAddress = destination };
    }
}
// endsnippet:complete
