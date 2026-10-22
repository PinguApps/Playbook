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
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        CancellationToken token = timeout.Token;
        using var server = new TcpListener(IPAddress.Loopback, 0);
        server.Start();
        int port = ((IPEndPoint)server.LocalEndpoint).Port;
        var received = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var respond = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Task serving = ServeAsync(server, received, respond, token);
        using var observer = new HttpObservation();
        using var handler = new SocketsHttpHandler { UseProxy = false };
        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
        Task<string> request = client.GetStringAsync(new Uri($"http://127.0.0.1:{port}/manifest"), token);
        await received.Task.WaitAsync(token);
#if NET11_0_OR_GREATER
        Require(observer.Read("http.client.active_requests") == 0, "Observable callback ran before poll.");
        observer.Poll();
        Require(observer.InstrumentTypes.Values.All(type => type.StartsWith("ObservableUpDownCounter", StringComparison.Ordinal)), "Expected observable counters.");
#else
        Require(observer.InstrumentTypes.Values.All(type => type.StartsWith("UpDownCounter", StringComparison.Ordinal)), "Expected event counters.");
#endif
        Require(observer.InstrumentTypes.Count == 2, "Missing actual HTTP instruments.");
        Require(observer.Read("http.client.active_requests") == 1 && observer.Read("http.client.open_connections") == 1, "In-flight request/connection observation missing.");
        respond.SetResult();
        Require(await request == "manifest-ready", "Real response mismatch.");
        await WaitForAsync(() =>
        {
#if NET11_0_OR_GREATER
            observer.Poll();
#endif
            return observer.Read("http.client.active_requests") == 0;
        }, token);
        Require(observer.Read("http.client.open_connections") == 1, "Keep-alive connection disappeared before disposal.");
#if NET11_0_OR_GREATER
        observer.Poll();
        long first = observer.Read("http.client.open_connections");
        observer.Poll();
        Require(observer.Read("http.client.open_connections") == first, "Repeated polls accumulated snapshots.");
#endif
        client.Dispose();
        handler.Dispose();
        await serving;
        await WaitForAsync(() =>
        {
#if NET11_0_OR_GREATER
            observer.Poll();
#endif
            return observer.Read("http.client.open_connections") == 0;
        }, token);
        Console.WriteLine($"PASS {Environment.Version}: real loopback HTTP request; active/open=1 while held, active=0 after response, open=0 after disposal; {string.Join(", ", observer.InstrumentTypes.Select(p => p.Key + "=" + p.Value))}.");
    }

    private static async Task ServeAsync(TcpListener server, TaskCompletionSource received, TaskCompletionSource respond, CancellationToken token)
    {
        using TcpClient connection = await server.AcceptTcpClientAsync(token);
        await using NetworkStream stream = connection.GetStream();
        byte[] buffer = new byte[4096];
        int length = 0;
        while (true)
        {
            int count = await stream.ReadAsync(buffer.AsMemory(length), token);
            if (count == 0) throw new IOException("Client closed before headers.");
            length += count;
            if (Encoding.ASCII.GetString(buffer, 0, length).Contains("\r\n\r\n", StringComparison.Ordinal)) break;
            if (length == buffer.Length) throw new IOException("Request headers exceed fixture limit.");
        }
        received.SetResult();
        await respond.Task.WaitAsync(token);
        byte[] response = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Length: 14\r\nConnection: keep-alive\r\n\r\nmanifest-ready");
        await stream.WriteAsync(response, token);
        while (await stream.ReadAsync(buffer, token) != 0) { }
    }

    private static async Task WaitForAsync(Func<bool> predicate, CancellationToken token)
    {
        while (!predicate()) await Task.Delay(10, token);
    }
}
