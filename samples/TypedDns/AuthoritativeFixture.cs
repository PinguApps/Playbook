using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Playbook.Samples;

// Test-only wire fixture. Answers only local synthetic requests; this is not a DNS server library.
internal sealed class AuthoritativeFixture : IAsyncDisposable
{
    private readonly UdpClient _socket;
    private readonly CancellationTokenSource _stop;
    private readonly Task _serving;
    private readonly string _station;
    public IPEndPoint Endpoint => (IPEndPoint)_socket.Client.LocalEndPoint!;
    public TaskCompletionSource Dropped { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public AuthoritativeFixture(IPAddress address, string station, CancellationToken token)
    {
        _socket = new UdpClient(new IPEndPoint(address, OperatingSystem.IsWindows() ? 53 : 0));
        _stop = CancellationTokenSource.CreateLinkedTokenSource(token);
        _station = station;
        _serving = ServeAsync(_stop.Token);
    }

    private async Task ServeAsync(CancellationToken token)
    {
        while (true)
        {
            UdpReceiveResult request = await _socket.ReceiveAsync(token);
            byte[] query = request.Buffer;
            int position = 12;
            var labels = new List<string>();
            while (query[position] != 0)
            {
                int length = query[position++];
                labels.Add(Encoding.ASCII.GetString(query, position, length));
                position += length;
            }
            position++;
            ushort type = BinaryPrimitives.ReadUInt16BigEndian(query.AsSpan(position));
            position += 4;
            string prefix = labels[0];
            if (prefix == "drop") { Dropped.TrySetResult(); continue; }
            bool negative = prefix is "absent" or "empty" or "nosoa";
            using var response = new MemoryStream();
            response.Write(query.AsSpan(0, 2));
            Write16(response, prefix == "absent" ? 0x8503 : 0x8500); // QR/AA/RD, no recursive service.
            Write16(response, 1);
            Write16(response, negative ? 0 : type is 1 or 28 ? 2 : 1);
            Write16(response, negative && prefix != "nosoa" ? 1 : 0);
            Write16(response, 0);
            response.Write(query.AsSpan(12, position - 12));
            if (negative && prefix != "nosoa")
            {
                using var soa = new MemoryStream();
                WriteName(soa, "ns.reedbank.test"); WriteName(soa, "hostmaster.reedbank.test");
                foreach (uint value in new uint[] { 41, 3600, 600, 86400, 45 }) Write32(soa, value);
                WriteRecord(response, 6, 120, soa.ToArray());
            }
            else if (!negative)
            {
                int count = type is 1 or 28 ? 2 : 1;
                for (int i = 0; i < count; i++)
                {
                    using var data = new MemoryStream();
                    switch (type)
                    {
                        case 1: data.Write(IPAddress.Parse(i == 0 ? "192.0.2.23" : "192.0.2.24").GetAddressBytes()); break;
                        case 28: data.Write(IPAddress.Parse(i == 0 ? "2001:db8::23" : "2001:db8::24").GetAddressBytes()); break;
                        case 15: Write16(data, 17); WriteName(data, "mail.reedbank.test"); break;
                        case 16:
                            foreach (string value in new[] { "station=" + _station, "batch=41" })
                            { byte[] bytes = Encoding.ASCII.GetBytes(value); data.WriteByte((byte)bytes.Length); data.Write(bytes); }
                            break;
                        case 5: WriteName(data, "archive.reedbank.test"); break;
                        case 12: WriteName(data, "sensor.reedbank.test"); break;
                        case 2: WriteName(data, "ns.reedbank.test"); break;
                        default: throw new InvalidOperationException("Unexpected DNS query type " + type);
                    }
                    WriteRecord(response, type, (uint)(90 + type), data.ToArray());
                }
            }
            await _socket.SendAsync(response.ToArray(), request.RemoteEndPoint, token);
        }
    }

    private static void WriteRecord(Stream stream, ushort type, uint ttl, byte[] data)
    {
        Write16(stream, 0xc00c); Write16(stream, type); Write16(stream, 1);
        Write32(stream, ttl); Write16(stream, checked((ushort)data.Length)); stream.Write(data);
    }
    private static void WriteName(Stream stream, string name)
    {
        foreach (string label in name.Split('.')) { stream.WriteByte((byte)label.Length); stream.Write(Encoding.ASCII.GetBytes(label)); }
        stream.WriteByte(0);
    }
    private static void Write16(Stream stream, int value)
    {
        Span<byte> bytes = stackalloc byte[2]; BinaryPrimitives.WriteUInt16BigEndian(bytes, checked((ushort)value)); stream.Write(bytes);
    }
    private static void Write32(Stream stream, uint value)
    {
        Span<byte> bytes = stackalloc byte[4]; BinaryPrimitives.WriteUInt32BigEndian(bytes, value); stream.Write(bytes);
    }
    public async ValueTask DisposeAsync()
    {
        await _stop.CancelAsync();
        try { await _serving; } catch (OperationCanceledException) when (_stop.IsCancellationRequested) { }
        finally { _socket.Dispose(); _stop.Dispose(); }
    }
}
