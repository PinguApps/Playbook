// snippet:complete
namespace SeedExchange;

// Operator-owned configuration, never populated from Host, Referer or the caller.
public sealed class ExchangeOrigins
{
    private readonly HashSet<string> trusted;

    public ExchangeOrigins(string[] origins)
    {
        if (origins.Length == 0 || origins.Any(origin =>
            !Uri.TryCreate(origin, UriKind.Absolute, out Uri? uri) ||
            (uri.Scheme != "https" && !(uri.Scheme == "http" && uri.IsLoopback)) ||
            origin.Contains('*', StringComparison.Ordinal) ||
            origin != uri.GetLeftPart(UriPartial.Authority) || uri.UserInfo.Length != 0))
        {
            throw new InvalidOperationException("Configure exact HTTPS origins (HTTP only for loopback); no paths, wildcards or trailing slash.");
        }
        trusted = new(origins, StringComparer.Ordinal);
    }

    public bool Allows(HttpRequest request) =>
        request.Headers.Origin.Count == 1 && trusted.Contains(request.Headers.Origin.ToString());
}
// endsnippet:complete
