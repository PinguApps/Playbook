// snippet:package-inspector
using System.Net;
using DnsClient;
using DnsClient.Protocol;

namespace Playbook.Samples;

public sealed class ZoneInspector : IDisposable
{
    private readonly LookupClient _resolver;
    public ZoneInspector(IPEndPoint server) => _resolver = new(new LookupClientOptions(server)
    {
        UseCache = false, ThrowDnsErrors = false, Retries = 0, Timeout = TimeSpan.FromSeconds(3),
    });

    public async Task<ZoneAnswer> QueryAsync(string name, RecordKind kind, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        QueryType type = kind switch
        {
            RecordKind.A => QueryType.A, RecordKind.Aaaa => QueryType.AAAA, RecordKind.Mx => QueryType.MX,
            RecordKind.Txt => QueryType.TXT, RecordKind.CName => QueryType.CNAME, RecordKind.ReverseName => QueryType.PTR,
            RecordKind.Ns => QueryType.NS, _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
        IDnsQueryResponse result;
        try { result = await _resolver.QueryAsync(name, type, QueryClass.IN, token); }
        catch (DnsResponseException) when (token.IsCancellationRequested) { throw new OperationCanceledException(token); }
        var records = new List<ZoneRecord>();
        foreach (DnsResourceRecord record in result.Answers)
        {
            string[]? values = record switch
            {
                ARecord r when kind == RecordKind.A => [r.Address.ToString()],
                AaaaRecord r when kind == RecordKind.Aaaa => [r.Address.ToString()],
                MxRecord r when kind == RecordKind.Mx => [r.Exchange.Value.TrimEnd('.'), r.Preference.ToString(System.Globalization.CultureInfo.InvariantCulture)],
                TxtRecord r when kind == RecordKind.Txt => r.Text.ToArray(),
                CNameRecord r when kind == RecordKind.CName => [r.CanonicalName.Value.TrimEnd('.')],
                PtrRecord r when kind == RecordKind.ReverseName => [r.PtrDomainName.Value.TrimEnd('.')],
                NsRecord r when kind == RecordKind.Ns => [r.NSDName.Value.TrimEnd('.')],
                _ => null,
            };
            if (values is not null) records.Add(new(values, TimeSpan.FromSeconds(record.InitialTimeToLive)));
        }
        string status = result.Header.ResponseCode switch
        {
            DnsHeaderResponseCode.NoError => "NoError", DnsHeaderResponseCode.NotExistentDomain => "NxDomain",
            _ => result.Header.ResponseCode.ToString(),
        };
        SoaRecord? soa = result.Authorities.OfType<SoaRecord>().FirstOrDefault();
        TimeSpan negative = records.Count == 0 && soa is not null
            ? TimeSpan.FromSeconds(Math.Min(soa.InitialTimeToLive, soa.Minimum)) : TimeSpan.Zero;
        return new(status, records, negative);
    }

    public void Dispose() { } // DnsClient owns no disposable resolver handle.
}
// endsnippet:package-inspector
