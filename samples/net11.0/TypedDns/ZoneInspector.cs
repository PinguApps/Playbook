using System.Net;
using System.Net.Sockets;

namespace Playbook.Samples;

// snippet:typed-inspector
public sealed class ZoneInspector : IDisposable
{
    private readonly DnsResolver _resolver;
    public ZoneInspector(IPEndPoint server) => _resolver = new(new DnsResolverOptions { Servers = [server] });

    public async Task<ZoneAnswer> QueryAsync(string name, RecordKind kind, CancellationToken token) => kind switch
    {
        RecordKind.A => Project(await _resolver.ResolveAddressesAsync(name, AddressFamily.InterNetwork, token), r => [r.Address.ToString()], r => r.Ttl),
        RecordKind.Aaaa => Project(await _resolver.ResolveAddressesAsync(name, AddressFamily.InterNetworkV6, token), r => [r.Address.ToString()], r => r.Ttl),
        RecordKind.Mx => Project(await _resolver.ResolveMxAsync(name, token), r => [r.Exchange.TrimEnd('.'), r.Preference.ToString(System.Globalization.CultureInfo.InvariantCulture)], r => r.Ttl),
        RecordKind.Txt => Project(await _resolver.ResolveTxtAsync(name, token), r => r.Values.ToArray(), r => r.Ttl),
        RecordKind.CName => Project(await _resolver.ResolveCNameAsync(name, token), r => [r.CanonicalName.TrimEnd('.')], r => r.Ttl),
        RecordKind.ReverseName => Project(await _resolver.ResolvePtrAsync(name, token), r => [r.Name.TrimEnd('.')], r => r.Ttl),
        RecordKind.Ns => Project(await _resolver.ResolveNsAsync(name, token), r => [r.Name.TrimEnd('.')], r => r.Ttl),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    private static ZoneAnswer Project<T>(DnsResult<T> result, Func<T, string[]> values, Func<T, TimeSpan> ttl) =>
        new(result.ResponseCode.ToString(), result.Records.Select(r => new ZoneRecord(values(r), ttl(r))).ToArray(), result.NegativeCacheTtl);

    public void Dispose() => _resolver.Dispose();
}
// endsnippet:typed-inspector
