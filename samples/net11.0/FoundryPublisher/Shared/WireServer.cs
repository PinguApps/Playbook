using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Foundry.Staging;

// An original disposable HTTP/1.1 fixture implementing the pinned staging wire
// contract. It is not a NuGet hosted provider and never forwards requests.
internal sealed class WireServer : IAsyncDisposable
{
    private readonly TcpListener listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource stop = new();
    private readonly List<Task> clients = [];
    private readonly Task accepting;
    public ConcurrentQueue<Receipt> Uploads { get; } = new();
    public TaskCompletionSource Arrived { get; private set; } = NewSignal();
    public string Origin { get; }
    public string Endpoint => Origin + "/foundry/batches/?lane=bronze";
    public int Status { get; set; } = 201;
    public bool Hold { get; set; }

    public WireServer()
    {
        listener.Start();
        Origin = $"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}";
        accepting = Accept();
    }

    public void ResetSignal() => Arrived = NewSignal();
    private static TaskCompletionSource NewSignal() => new(TaskCreationOptions.RunContinuationsAsynchronously);

    private async Task Accept()
    {
        try
        {
            while (!stop.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(stop.Token);
                clients.Add(Serve(client));
            }
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
    }

    private async Task Serve(TcpClient client)
    {
        using (client)
        {
            try
            {
                var stream = client.GetStream();
                string first = await Line(stream);
                var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (string line = await Line(stream); line.Length > 0; line = await Line(stream))
                {
                    int colon = line.IndexOf(':', StringComparison.Ordinal);
                    headers.Add(line[..colon], line[(colon + 1)..].Trim());
                }
                if (headers.TryGetValue("Expect", out var expect) && expect == "100-continue")
                    await stream.WriteAsync("HTTP/1.1 100 Continue\r\n\r\n"u8.ToArray(), stop.Token);
                using var body = new MemoryStream();
                if (headers.TryGetValue("Transfer-Encoding", out var encoding) && encoding == "chunked")
                {
                    while (true)
                    {
                        int length = Convert.ToInt32((await Line(stream)).Split(';')[0], 16);
                        if (length == 0) { while ((await Line(stream)).Length > 0) { } break; }
                        await Copy(stream, body, length);
                        if ((await Line(stream)).Length != 0) throw new InvalidDataException("Chunk terminator");
                    }
                }
                else if (headers.TryGetValue("Content-Length", out var size)) await Copy(stream, body, int.Parse(size));
                string[] request = first.Split(' ');
                if (request[0] == "GET")
                {
                    string type = request[1].Contains("missing", StringComparison.Ordinal) ? "PackageStaging/9.0.0" : "PackageStaging/1.0.0";
                    var resources = new[]
                    {
                        new Dictionary<string, string> { ["@id"] = request[1].Contains("alien", StringComparison.Ordinal) ? Endpoint.Replace("127.0.0.1", "localhost", StringComparison.Ordinal) : Endpoint, ["@type"] = type },
                        new Dictionary<string, string> { ["@id"] = Origin + "/published", ["@type"] = "PackagePublish/2.0.0" }
                    };
                    await Respond(stream, 200, JsonSerializer.SerializeToUtf8Bytes(new { version = "3.0.0", resources }));
                }
                else if (request[0] == "PUT" && request[1] is "/foundry/batches/package?lane=bronze" or "/foundry/batches/symbols?lane=bronze" or "/published/")
                {
                    if (!headers.TryGetValue("Content-Type", out var type) || !type.StartsWith("multipart/form-data;", StringComparison.Ordinal))
                    {
                        await Respond(stream, 400, []);
                        return;
                    }
                    Uploads.Enqueue(new(request[0], request[1], headers, body.ToArray()));
                    Arrived.TrySetResult();
                    if (Hold) await Task.Delay(Timeout.Infinite, stop.Token);
                    await Respond(stream, Status, []);
                }
                else await Respond(stream, 404, []);
            }
            catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
            catch (IOException) { /* The client may disconnect after timeout/cancellation. */ }
        }
    }

    private async Task<string> Line(NetworkStream stream)
    {
        using var bytes = new MemoryStream();
        byte[] one = new byte[1];
        while (true)
        {
            if (await stream.ReadAsync(one, stop.Token) != 1) throw new IOException("Unexpected EOF");
            if (one[0] == '\n') return Encoding.ASCII.GetString(bytes.ToArray()).TrimEnd('\r');
            bytes.WriteByte(one[0]);
            if (bytes.Length > 65536) throw new InvalidDataException("Header too large");
        }
    }

    private async Task Copy(NetworkStream stream, Stream body, int count)
    {
        byte[] buffer = new byte[Math.Min(count, 16384)];
        while (count > 0)
        {
            int read = await stream.ReadAsync(buffer.AsMemory(0, Math.Min(buffer.Length, count)), stop.Token);
            if (read == 0) throw new IOException("Unexpected EOF");
            await body.WriteAsync(buffer.AsMemory(0, read), stop.Token);
            count -= read;
        }
    }

    private async Task Respond(NetworkStream stream, int status, byte[] body)
    {
        byte[] header = Encoding.ASCII.GetBytes($"HTTP/1.1 {status} Fixture\r\nContent-Type: application/json\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n");
        await stream.WriteAsync(header, stop.Token);
        await stream.WriteAsync(body, stop.Token);
    }

    public async ValueTask DisposeAsync()
    {
        await stop.CancelAsync();
        listener.Stop();
        await accepting;
        await Task.WhenAll(clients);
        listener.Dispose();
        stop.Dispose();
    }
}

internal sealed record Receipt(string Method, string Route, Dictionary<string, string> Headers, ReadOnlyMemory<byte> Body);
