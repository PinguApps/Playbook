using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Playbook.Samples;

internal static class Http2Capacity
{
    public static async Task CheckAsync(CancellationToken token)
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        Task server = ServeAsync(listener, token);
        using var handler = StreamPolicy.Create();
#if NET11_0_OR_GREATER
        try { handler.InitialHttp2MaxConcurrentStreams = 0; throw new InvalidOperationException("Zero initial stream limit accepted."); }
        catch (ArgumentOutOfRangeException) { }
#endif
        using var client = new HttpClient(handler);
        using var admission = new SemaphoreSlim(2);
        var address = new Uri($"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}/readings");
        Task[] requests = Enumerable.Range(0, 6).Select(_ => StreamPolicy.SendAsync(client, address, admission, token)).ToArray();
        await Task.WhenAll(requests);
        await server;
    }

    private static async Task ServeAsync(TcpListener listener, CancellationToken token)
    {
        using TcpClient connection = await listener.AcceptTcpClientAsync(token);
        await using NetworkStream stream = connection.GetStream();
        byte[] preface = new byte[24];
        await stream.ReadExactlyAsync(preface, token);
        if (Encoding.ASCII.GetString(preface) != "PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n") throw new IOException("Not an HTTP/2 peer.");
        using var reads = CancellationTokenSource.CreateLinkedTokenSource(token);
        Task<Frame> pending = ReadAsync(stream, reads.Token);
        var active = new List<int>();
        int received = 0;
        async Task ReceiveHeadersAsync()
        {
            while (true)
            {
                Frame frame = await pending;
                pending = ReadAsync(stream, reads.Token);
                if (frame.Type == 1) { active.Add(frame.Stream); received++; return; }
            }
        }
        async Task AssertNoMoreHeadersAsync()
        {
            Task pause = Task.Delay(200, token);
            while (await Task.WhenAny(pending, pause) == pending)
            {
                Frame frame = await pending;
                pending = ReadAsync(stream, reads.Token);
                if (frame.Type == 1) throw new IOException("Client exceeded the configured active-stream limit.");
            }
            token.ThrowIfCancellationRequested();
        }
        await ReceiveHeadersAsync();
        await ReceiveHeadersAsync();
        // Withhold SETTINGS until the initial two requests are observed.
        await AssertNoMoreHeadersAsync();
        byte[] settings = new byte[6];
        BinaryPrimitives.WriteUInt16BigEndian(settings, 3); // SETTINGS_MAX_CONCURRENT_STREAMS
        BinaryPrimitives.WriteUInt32BigEndian(settings.AsSpan(2), 3);
        await WriteAsync(stream, 4, 0, 0, settings, token);
        await WriteAsync(stream, 4, 1, 0, [], token); // ACK client settings
#if NET11_0_OR_GREATER
        await ReceiveHeadersAsync(); // Peer limit 3 replaces configured initial limit 2.
#endif
        await AssertNoMoreHeadersAsync();
        foreach (int id in active) await WriteAsync(stream, 1, 5, id, [0x88], token); // :status 200, END_HEADERS|END_STREAM
        active.Clear();
        while (received < 6)
        {
            await ReceiveHeadersAsync();
            await WriteAsync(stream, 1, 5, active[^1], [0x88], token);
            active.Clear();
        }
        // Finish the outstanding read before disposing its stream.
        await reads.CancelAsync();
        try { await pending; } catch (OperationCanceledException) when (reads.IsCancellationRequested) { }
        Console.WriteLine("PASS HTTP/2: two initial requests before SETTINGS; peer limit three respected; all six completed.");
    }

    private sealed record Frame(byte Type, int Stream);

    private static async Task<Frame> ReadAsync(Stream stream, CancellationToken token)
    {
        byte[] header = new byte[9];
        await stream.ReadExactlyAsync(header, token);
        int length = (header[0] << 16) | (header[1] << 8) | header[2];
        if (length > 65_536) throw new IOException("Fixture frame too large.");
        await stream.ReadExactlyAsync(new byte[length], token);
        return new Frame(header[3], BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(5)) & int.MaxValue);
    }

    private static async Task WriteAsync(Stream stream, byte type, byte flags, int id, byte[] payload, CancellationToken token)
    {
        byte[] frame = new byte[9 + payload.Length];
        frame[0] = (byte)(payload.Length >> 16);
        frame[1] = (byte)(payload.Length >> 8);
        frame[2] = (byte)payload.Length;
        frame[3] = type;
        frame[4] = flags;
        BinaryPrimitives.WriteInt32BigEndian(frame.AsSpan(5), id);
        payload.CopyTo(frame, 9);
        await stream.WriteAsync(frame, token);
    }
}
