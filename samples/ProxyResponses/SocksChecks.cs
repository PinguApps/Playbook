using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Playbook.Samples;

internal static class SocksChecks
{
    private const string Host = "relay.cairn.invalid";

    public static async Task RunAsync()
    {
        // This intentionally performs a local negative DNS check; it is not a leak-prevention test.
        try
        {
            await Dns.GetHostAddressesAsync(Host).WaitAsync(TimeSpan.FromSeconds(10));
            throw new InvalidOperationException("Fixture hostname unexpectedly resolves locally.");
        }
        catch (SocketException) { Console.WriteLine($"DNS {Host}: local resolution failed as required"); }

        foreach (bool authenticate in new[] { false, true })
        {
            await using HttpFixture target = new(Encoding.UTF8.GetBytes("cairn relay reached"));
            await using SocksFixture proxy = new(target.Address.Port, authenticate ? "auth" : "success");
            using HttpClient client = CairnRelay.Create(proxy.Port, authenticate ? new NetworkCredential("survey", "fixture-only") : null);
            string body = await client.GetStringAsync(new Uri($"http://{Host}:{target.Address.Port}/cairn/report"));
            Wire.Check(body == "cairn relay reached", "Proxy did not reach actual target.");
            await proxy.Completion;
            Wire.Check(proxy.Host == Host && proxy.TargetPort == target.Address.Port, "Proxy did not receive original hostname/port.");
            Wire.Check((await target.Request.Task).Contains($"Host: {Host}:{target.Address.Port}", StringComparison.OrdinalIgnoreCase), "HTTP hostname was changed.");
            Console.WriteLine($"SOCKS success auth={authenticate}: ATYP=3 hostname={proxy.Host}, actual loopback target reached");
        }

        foreach (string mode in new[] { "refused", "bad-auth", "no-method", "cancel" })
        {
            await using SocksFixture proxy = new(443, mode);
            using HttpClient client = CairnRelay.Create(proxy.Port, mode == "bad-auth" ? new NetworkCredential("survey", "wrong") : null);
            using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(10));
            Task<HttpResponseMessage> request = client.GetAsync(new Uri($"http://{Host}:443/report"), cancellation.Token);
            if (mode == "cancel")
            {
                await proxy.Greeting.Task.WaitAsync(TimeSpan.FromSeconds(5));
                await cancellation.CancelAsync();
            }
            try { using var unexpected = await request; throw new InvalidOperationException($"Accepted {mode}."); }
            catch (OperationCanceledException) when (mode == "cancel" && cancellation.IsCancellationRequested) { }
            catch (HttpRequestException) when (mode != "cancel") { }
            await proxy.Completion;
            Console.WriteLine($"SOCKS {mode}: expected failure, no direct fallback, tunnel closed");
        }

        // The explicit newer alias is not a supported old-runtime proxy scheme.
        if (Environment.Version.Major == 10)
        {
            using var aliasClient = new HttpClient(new SocketsHttpHandler { Proxy = new WebProxy("socks5h://127.0.0.1:1"), UseProxy = true });
            try { using var unexpected = await aliasClient.GetAsync(new Uri($"http://{Host}/")); throw new InvalidOperationException("Old runtime accepted socks5h."); }
            catch (NotSupportedException) { Console.WriteLine("SOCKS old socks5h alias rejected; canonical socks5 route above works"); }
        }
    }

    private sealed class SocksFixture : IAsyncDisposable
    {
        private readonly TcpListener listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource lifetime = new(TimeSpan.FromSeconds(15));
        public TaskCompletionSource Greeting { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task Completion { get; }
        public int Port => ((IPEndPoint)listener.LocalEndpoint).Port;
        public string? Host { get; private set; }
        public int TargetPort { get; private set; }

        public SocksFixture(int targetPort, string mode)
        {
            listener.Start();
            Completion = ServeAsync(targetPort, mode);
        }

        private async Task ServeAsync(int targetPort, string mode)
        {
            CancellationToken token = lifetime.Token;
            using TcpClient peer = await listener.AcceptTcpClientAsync(token);
            using NetworkStream stream = peer.GetStream();
            byte[] greeting = await Wire.ReadAsync(stream, 2, token);
            Wire.Check(greeting[0] == 5, "Not SOCKS5.");
            byte[] methods = await Wire.ReadAsync(stream, greeting[1], token);
            Greeting.TrySetResult();
            if (mode == "cancel")
            {
                Wire.Check(await stream.ReadAsync(new byte[1], token) == 0, "Cancelled SOCKS tunnel remained open.");
                return;
            }
            if (mode == "no-method") { await stream.WriteAsync(new byte[] { 5, 255 }, token); return; }
            bool auth = mode is "auth" or "bad-auth";
            Wire.Check(methods.Contains((byte)(auth ? 2 : 0)), "Required authentication method not offered.");
            await stream.WriteAsync(new byte[] { 5, (byte)(auth ? 2 : 0) }, token);
            if (auth)
            {
                byte[] start = await Wire.ReadAsync(stream, 2, token);
                string user = Encoding.UTF8.GetString(await Wire.ReadAsync(stream, start[1], token));
                int length = (await Wire.ReadAsync(stream, 1, token))[0];
                string password = Encoding.UTF8.GetString(await Wire.ReadAsync(stream, length, token));
                Wire.Check(start[0] == 1 && user == "survey", "Wrong auth identity.");
                bool accepted = password == "fixture-only";
                await stream.WriteAsync(new byte[] { 1, (byte)(accepted ? 0 : 1) }, token);
                if (!accepted) return;
            }
            byte[] command = await Wire.ReadAsync(stream, 5, token);
            Wire.Check(command[0] == 5 && command[1] == 1 && command[2] == 0 && command[3] == 3, "Expected hostname CONNECT.");
            Host = Encoding.ASCII.GetString(await Wire.ReadAsync(stream, command[4], token));
            byte[] port = await Wire.ReadAsync(stream, 2, token);
            TargetPort = (port[0] << 8) | port[1];
            Wire.Check(Host == SocksChecks.Host && TargetPort == targetPort, "Unexpected SOCKS destination.");
            if (mode == "refused") { await stream.WriteAsync(new byte[] { 5, 5, 0, 1, 127, 0, 0, 1, 0, 0 }, token); return; }
            // Controlled proxy mapping replaces DNS only at the proxy, then opens a real target socket.
            using TcpClient target = new();
            await target.ConnectAsync(IPAddress.Loopback, TargetPort, token);
            using NetworkStream upstream = target.GetStream();
            await stream.WriteAsync(new byte[] { 5, 0, 0, 1, 127, 0, 0, 1, 0, 0 }, token);
            string headers = await Wire.HeadersAsync(stream, token);
            await upstream.WriteAsync(Encoding.ASCII.GetBytes(headers), token);
            await upstream.CopyToAsync(stream, token);
        }

        public async ValueTask DisposeAsync()
        {
            await lifetime.CancelAsync(); listener.Dispose();
            try { await Completion; }
            catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
            finally { lifetime.Dispose(); }
        }
    }
}
