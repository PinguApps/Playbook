using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Playbook.Samples;

internal static class Wire
{
    public static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public static async Task<byte[]> ReadAsync(Stream stream, int count, CancellationToken token)
    {
        byte[] bytes = new byte[count];
        await stream.ReadExactlyAsync(bytes, token);
        return bytes;
    }

    public static async Task<string> HeadersAsync(Stream stream, CancellationToken token)
    {
        List<byte> bytes = [];
        while (bytes.Count < 16384)
        {
            bytes.Add((await ReadAsync(stream, 1, token))[0]);
            if (bytes.Count >= 4 && Encoding.ASCII.GetString(bytes.ToArray()).EndsWith("\r\n\r\n", StringComparison.Ordinal))
                return Encoding.ASCII.GetString(bytes.ToArray());
        }
        throw new InvalidDataException("Fixture header limit.");
    }
}

// One real HTTP exchange, isolated loopback port, bounded lifetime and deterministic cleanup.
internal sealed class HttpFixture : IAsyncDisposable
{
    private readonly TcpListener listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource lifetime = new(TimeSpan.FromSeconds(15));
    public TaskCompletionSource<string> Request { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public Uri Address { get; }
    private readonly Task server;

    public HttpFixture(byte[] body, string? encoding = null, bool stallBody = false)
    {
        listener.Start();
        Address = new Uri($"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}/cairn/report");
        server = ServeAsync(body, encoding, stallBody);
    }

    private async Task ServeAsync(byte[] body, string? encoding, bool stallBody)
    {
        CancellationToken token = lifetime.Token;
        using TcpClient client = await listener.AcceptTcpClientAsync(token);
        using NetworkStream stream = client.GetStream();
        Request.TrySetResult(await Wire.HeadersAsync(stream, token));
        string headers = $"HTTP/1.1 200 OK\r\nContent-Length: {body.Length}\r\nContent-Type: application/octet-stream\r\nX-Cairn: survey-7\r\nConnection: close\r\n";
        if (encoding is not null) headers += $"Content-Encoding: {encoding}\r\n";
        await stream.WriteAsync(Encoding.ASCII.GetBytes(headers + "\r\n"), token);
        if (stallBody)
        {
            // Wait for peer closure rather than an arbitrary timing delay.
            Wire.Check(await stream.ReadAsync(new byte[1], token) == 0, "Cancelled HTTP peer did not close.");
        }
        else await stream.WriteAsync(body, token);
    }

    public async ValueTask DisposeAsync()
    {
        await lifetime.CancelAsync();
        listener.Dispose();
        try { await server; }
        catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
        finally { lifetime.Dispose(); }
    }
}
