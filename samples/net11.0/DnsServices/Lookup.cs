using System.Net;

namespace Playbook.Samples;

// snippet:lookup
public sealed record ServiceEndpoint(string Target, int Port, int Priority, int Weight, TimeSpan Ttl);
public sealed record ServiceAnswer(string Status, IReadOnlyList<ServiceEndpoint> Records);

public sealed class ServiceLookup : IDisposable
{
    private readonly DnsResolver _resolver;

    public ServiceLookup(IPEndPoint server) => _resolver = new DnsResolver(new DnsResolverOptions
    {
        Servers = [server],
    });

    public async Task<ServiceAnswer> QueryAsync(string name, CancellationToken token)
    {
        DnsResult<SrvRecord> answer = await _resolver.ResolveSrvAsync(name, token);
        return new(answer.ResponseCode.ToString(), answer.Records.Select(record => new ServiceEndpoint(
            record.Target.TrimEnd('.'), record.Port, record.Priority, record.Weight, record.Ttl)).ToArray());
    }

    public void Dispose() => _resolver.Dispose();
}
// endsnippet:lookup
