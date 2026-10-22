using System.Net;
using DnsClient;

namespace Playbook.Samples;

// snippet:lookup
public sealed record ServiceEndpoint(string Target, int Port, int Priority, int Weight, TimeSpan Ttl);
public sealed record ServiceAnswer(string Status, IReadOnlyList<ServiceEndpoint> Records);

public sealed class ServiceLookup
{
    private readonly LookupClient _resolver;

    public ServiceLookup(IPEndPoint server) => _resolver = new LookupClient(new LookupClientOptions(server)
    {
        UseCache = false,
        ThrowDnsErrors = false,
        Retries = 0,
        Timeout = TimeSpan.FromSeconds(3),
    });

    public async Task<ServiceAnswer> QueryAsync(string name, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        IDnsQueryResponse answer;
        try { answer = await _resolver.QueryAsync(name, QueryType.SRV, QueryClass.IN, token); }
        catch (DnsResponseException) when (token.IsCancellationRequested)
        {
            // This package can wrap cancellation as a DNS response exception.
            throw new OperationCanceledException(token);
        }
        // A name error and an empty successful answer are different outcomes.
        string status = answer.Header.ResponseCode switch
        {
            DnsHeaderResponseCode.NoError => "NoError",
            DnsHeaderResponseCode.NotExistentDomain => "NxDomain",
            _ => answer.Header.ResponseCode.ToString(),
        };
        return new(status, answer.Answers.SrvRecords().Select(record => new ServiceEndpoint(
            record.Target.Value.TrimEnd('.'), record.Port, record.Priority, record.Weight,
            TimeSpan.FromSeconds(record.InitialTimeToLive))).ToArray());
    }
}
// endsnippet:lookup
