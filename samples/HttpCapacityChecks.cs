using System.IO.Compression;
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
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        CancellationToken token = timeout.Token;
        string payload = string.Concat(Enumerable.Repeat("harbour sensor 47: temperature=18;\n", 200));
        string[] encodings = ["gzip", "br"];
#if NET11_0_OR_GREATER
        encodings = [.. encodings, "zstd"];
#endif
        foreach (string encoding in encodings)
        {
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            Task receiving = ReceiveCompressedAsync(listener, payload, encoding, token);
            using var handler = new SocketsHttpHandler { UseProxy = false };
            using var client = new HttpClient(handler);
            using HttpContent body = RequestPolicy.Compress(payload, encoding);
            using HttpResponseMessage response = await client.PostAsync(Address(listener), body, token);
            response.EnsureSuccessStatusCode();
            await receiving;
        }

        await CheckRetirementAsync(token);
        await Http2Capacity.CheckAsync(token);
        Console.WriteLine($"PASS {Environment.Version}: real HTTP compression {string.Join('/', encodings)}, decoded original payload and Content-Encoding; pooled reuse then retirement; controlled HTTP/2 initial and peer stream limits.");
    }

    private static Uri Address(TcpListener listener) => new($"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}/telemetry");

    private static async Task ReceiveCompressedAsync(TcpListener listener, string expected, string encoding, CancellationToken token)
    {
        using TcpClient connection = await listener.AcceptTcpClientAsync(token);
        await using NetworkStream stream = connection.GetStream();
        string headers = await ReadHeadersAsync(stream, token);
        Require(headers.Contains($"Content-Encoding: {encoding}\r\n", StringComparison.OrdinalIgnoreCase), "Missing wire Content-Encoding.");
        Require(headers.Contains("Content-Type: text/plain; charset=utf-8", StringComparison.OrdinalIgnoreCase), "Original content type lost.");
        using var bytes = new MemoryStream();
        if (headers.Contains("Transfer-Encoding: chunked", StringComparison.OrdinalIgnoreCase))
        {
            while (true)
            {
                int size = Convert.ToInt32((await ReadLineAsync(stream, token)).Split(';')[0], 16);
                if (size == 0) { await ReadLineAsync(stream, token); break; }
                byte[] chunk = new byte[size];
                await stream.ReadExactlyAsync(chunk, token);
                bytes.Write(chunk);
                Require((await ReadLineAsync(stream, token)).Length == 0, "Invalid chunk terminator.");
            }
        }
        else
        {
            string lengthLine = headers.Split("\r\n").Single(line => line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase));
            byte[] content = new byte[int.Parse(lengthLine.Split(':')[1], System.Globalization.CultureInfo.InvariantCulture)];
            await stream.ReadExactlyAsync(content, token);
            bytes.Write(content);
        }
        Require(bytes.Length < Encoding.UTF8.GetByteCount(expected), "Fixture payload was not compressed.");
        bytes.Position = 0;
        using Stream decoder = encoding switch
        {
            "gzip" => new GZipStream(bytes, CompressionMode.Decompress),
            "br" => new BrotliStream(bytes, CompressionMode.Decompress),
#if NET11_0_OR_GREATER
            "zstd" => new ZstandardStream(bytes, CompressionMode.Decompress),
#endif
            _ => throw new InvalidOperationException("Unsupported fixture encoding."),
        };
        using var reader = new StreamReader(decoder);
        Require(await reader.ReadToEndAsync(token) == expected, "Decoded original payload mismatch.");
        await stream.WriteAsync(Encoding.ASCII.GetBytes("HTTP/1.1 204 No Content\r\nConnection: close\r\n\r\n"), token);
    }

    private static async Task CheckRetirementAsync(CancellationToken token)
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        using var serving = CancellationTokenSource.CreateLinkedTokenSource(token);
        Task server = ServeConnectionsAsync(listener, serving.Token);
        int retire = 0;
        using var handler = ConnectionPolicy.Create(() => Volatile.Read(ref retire) == 1);
        using var client = new HttpClient(handler);
        string first = await client.GetStringAsync(Address(listener), token);
        Require(await client.GetStringAsync(Address(listener), token) == first, "Healthy connection not reused.");
#if NET11_0_OR_GREATER
        Volatile.Write(ref retire, 1);
#else
        await Task.Delay(350, token);
#endif
        string next;
        do { next = await client.GetStringAsync(Address(listener), token); if (next == first) await Task.Delay(10, token); } while (next == first);
        Volatile.Write(ref retire, 0);
        Require(next != first, "Connection retirement did not open a new transport.");
        client.Dispose();
        await serving.CancelAsync();
        try { await server; } catch (OperationCanceledException) when (serving.IsCancellationRequested) { }
    }

    private static async Task ServeConnectionsAsync(TcpListener listener, CancellationToken token)
    {
        var connections = new List<Task>();
        int id = 0;
        try
        {
            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(token);
                connections.Add(ServeOneAsync(client, (++id).ToString(System.Globalization.CultureInfo.InvariantCulture), token));
            }
        }
        finally
        {
            try { await Task.WhenAll(connections); } catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        }
    }

    private static async Task ServeOneAsync(TcpClient client, string id, CancellationToken token)
    {
        using (client)
        {
            await using NetworkStream stream = client.GetStream();
            while (await ReadHeadersAsync(stream, token) is { Length: > 0 })
            {
                await stream.WriteAsync(Encoding.ASCII.GetBytes($"HTTP/1.1 200 OK\r\nContent-Length: {id.Length}\r\n\r\n{id}"), token);
            }
        }
    }

    private static async Task<string> ReadHeadersAsync(Stream stream, CancellationToken token)
    {
        var result = new StringBuilder();
        while (true)
        {
            string line = await ReadLineAsync(stream, token);
            if (line.Length == 0) return result.Length == 0 ? "" : result.Append("\r\n").ToString();
            result.Append(line).Append("\r\n");
            if (result.Length > 16_384) throw new IOException("Fixture header limit exceeded.");
        }
    }

    private static async Task<string> ReadLineAsync(Stream stream, CancellationToken token)
    {
        var bytes = new List<byte>();
        byte[] one = new byte[1];
        while (await stream.ReadAsync(one, token) != 0)
        {
            bytes.Add(one[0]);
            if (bytes.Count >= 2 && bytes[^2] == 13 && bytes[^1] == 10) return Encoding.ASCII.GetString(bytes.ToArray(), 0, bytes.Count - 2);
            if (bytes.Count > 16_384) throw new IOException("Fixture line limit exceeded.");
        }
        if (bytes.Count != 0) throw new IOException("Truncated fixture line.");
        return "";
    }
}
