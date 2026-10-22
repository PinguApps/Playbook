using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Playbook.Samples;

internal static class Program
{
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public static async Task Main()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        // Windows DnsQueryEx only supports port 53. Bind only loopback and fail
        // explicitly if unavailable; never change the machine's DNS settings.
        using var server = new UdpClient(new IPEndPoint(IPAddress.Loopback, 53));
        using var serving = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token);
        Task responding = RespondAsync(server, serving.Token);
        var endpoint = new IPEndPoint(IPAddress.Loopback, 53);
#if NET11_0_OR_GREATER
        using var lookup = new ServiceLookup(endpoint);
#else
        var lookup = new ServiceLookup(endpoint);
#endif
        string suffix = Guid.NewGuid().ToString("N") + ".harbour.test";
        ServiceAnswer answer = await lookup.QueryAsync("_readings._tcp." + suffix, timeout.Token);
        Require(answer.Status == "NoError" && answer.Records.Count == 1, "Missing SRV answer.");
        ServiceEndpoint record = answer.Records[0];
        Require(record.Target == "collector.harbour.test" && record.Port == 4719 && record.Priority == 7 && record.Weight == 11, "Original SRV fields changed.");
        Require(record.Ttl > TimeSpan.Zero && record.Ttl <= TimeSpan.FromSeconds(90), "SRV TTL outside advertised bounds.");
        ServiceAnswer missing = await lookup.QueryAsync("_missing._tcp." + suffix, timeout.Token);
        Require(missing.Status == "NxDomain" && missing.Records.Count == 0, "NXDOMAIN not distinguished.");
        ServiceAnswer empty = await lookup.QueryAsync("_empty._tcp." + suffix, timeout.Token);
        Require(empty.Status == "NoError" && empty.Records.Count == 0, "NODATA not distinguished.");
        using var canceled = new CancellationTokenSource();
        await canceled.CancelAsync();
        try { await lookup.QueryAsync("_cancel._tcp." + suffix, canceled.Token); throw new InvalidOperationException("Cancellation ignored."); }
        catch (OperationCanceledException) { }
#if NET11_0_OR_GREATER
        if (OperatingSystem.IsWindows())
        {
            try { using var invalid = new DnsResolver(new DnsResolverOptions { Servers = [new(IPAddress.Loopback, 5353)] }); throw new InvalidOperationException("Windows custom port accepted."); }
            catch (PlatformNotSupportedException) { }
        }
#endif
        await serving.CancelAsync();
        try { await responding; } catch (OperationCanceledException) when (serving.IsCancellationRequested) { }
        Console.WriteLine($"PASS {Environment.Version}: real loopback UDP DNS SRV collector.harbour.test:4719 priority=7 weight=11 TTL={record.Ttl.TotalSeconds}; NXDOMAIN, NODATA, cancellation; no system resolver configuration changed.");
    }

    private static async Task RespondAsync(UdpClient server, CancellationToken token)
    {
        while (true)
        {
            UdpReceiveResult request = await server.ReceiveAsync(token);
            byte[] query = request.Buffer;
            int end = 12;
            var labels = new List<string>();
            while (query[end] != 0)
            {
                int length = query[end++];
                labels.Add(Encoding.ASCII.GetString(query, end, length));
                end += length;
            }
            end += 5; // root label and QTYPE/QCLASS
            string name = string.Join('.', labels);
            bool absent = name.StartsWith("_missing.", StringComparison.Ordinal);
            bool empty = name.StartsWith("_empty.", StringComparison.Ordinal);
            using var response = new MemoryStream();
            response.Write(query.AsSpan(0, 2));
            Write16(response, absent ? 0x8583 : 0x8580); // response, authoritative, RD/RA
            Write16(response, 1);
            Write16(response, absent || empty ? 0 : 1);
            Write16(response, 0);
            Write16(response, 0);
            response.Write(query.AsSpan(12, end - 12));
            if (!absent && !empty)
            {
                Write16(response, 0xc00c); // pointer to question owner
                Write16(response, 33); // SRV
                Write16(response, 1);
                response.Write([0, 0, 0, 90]);
                using var data = new MemoryStream();
                Write16(data, 7);
                Write16(data, 11);
                Write16(data, 4719);
                foreach (string label in new[] { "collector", "harbour", "test" })
                {
                    data.WriteByte((byte)label.Length);
                    data.Write(Encoding.ASCII.GetBytes(label));
                }
                data.WriteByte(0);
                Write16(response, checked((int)data.Length));
                response.Write(data.ToArray());
            }
            await server.SendAsync(response.ToArray(), request.RemoteEndPoint, token);
        }
    }

    private static void Write16(Stream stream, int value)
    {
        Span<byte> bytes = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(bytes, checked((ushort)value));
        stream.Write(bytes);
    }
}
