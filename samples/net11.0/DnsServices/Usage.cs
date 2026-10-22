using System.Net;

namespace Playbook.Samples;

public static class Usage
{
    // snippet:lookup-use
    public static async Task PrintAsync(IPEndPoint resolverEndpoint, CancellationToken cancellationToken)
    {
        using var lookup = new ServiceLookup(resolverEndpoint);
        ServiceAnswer answer = await lookup.QueryAsync("_readings._tcp.harbour.test", cancellationToken);
        if (answer.Status == "NoError" && answer.Records.Count > 0)
        {
            foreach (ServiceEndpoint record in answer.Records)
                Console.WriteLine($"{record.Target}:{record.Port} priority={record.Priority} weight={record.Weight} ttl={record.Ttl}");
        }
    }
    // endsnippet:lookup-use
}
