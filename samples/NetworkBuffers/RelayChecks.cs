using System.Net;
using System.Net.Sockets;

namespace Playbook.Samples;

internal static class Program
{
    public static async Task Main()
    {
        RuntimeIdentity.Assert();
        Require(Socket.OSSupportsIPv4 && Socket.OSSupportsIPv6, "Fixture requires IPv4 and IPv6.");
        IPAddress[] answers = await Dns.GetHostAddressesAsync("localhost");
        Require(answers.All(IPAddress.IsLoopback) && answers.Contains(IPAddress.Loopback) && answers.Contains(IPAddress.IPv6Loopback), "localhost must resolve only to loopback, with both families.");
        Console.WriteLine("DNS localhost=" + string.Join<IPAddress>(",", answers));
        foreach (string scenario in new[] { "both", "v4-only", "v6-only", "neither" })
        {
            using var v4 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            using var v6 = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            v6.DualMode = false;
            v4.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            int port = ((IPEndPoint)v4.LocalEndPoint!).Port;
            v6.Bind(new IPEndPoint(IPAddress.IPv6Loopback, port));
            // Bound non-listening TCP sockets reserve the port and give real refusal paths.
            if (scenario is "both" or "v4-only") v4.Listen(32);
            if (scenario is "both" or "v6-only") v6.Listen(32);
#if NET11_0_OR_GREATER
            foreach (ConnectAlgorithm algorithm in new[] { ConnectAlgorithm.Default, ConnectAlgorithm.Parallel })
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                try
                {
                    using Socket client = await RelayConnector.ConnectAsync("localhost", port, algorithm, timeout.Token);
                    Require(scenario != "neither", "Connected without listener.");
                    await VerifyPeerAsync(client, v4, v6, scenario, timeout.Token);
                    Console.WriteLine($"CONNECT {scenario} {algorithm} selected={client.RemoteEndPoint}");
                }
                catch (SocketException error) when (scenario == "neither") { Console.WriteLine($"FAILURE {algorithm} {error.SocketErrorCode}"); }
                await CheckEventContractAsync(port, algorithm, scenario, v4, v6);
            }
#else
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            try
            {
                using Socket client = await RelayConnector.ConnectAsync("localhost", port, timeout.Token);
                Require(scenario != "neither", "Connected without listener.");
                await VerifyPeerAsync(client, v4, v6, scenario, timeout.Token);
                Console.WriteLine($"CONNECT {scenario} explicit-stagger selected={client.RemoteEndPoint}");
            }
            catch (SocketException error) when (scenario == "neither") { Console.WriteLine($"FAILURE explicit-stagger {error.SocketErrorCode}"); }
            if (scenario == "v4-only")
            {
                using Socket client = await RelayConnector.RaceAsync([IPAddress.IPv6Loopback, IPAddress.Loopback], port, TimeSpan.FromMilliseconds(25), timeout.Token);
                await VerifyPeerAsync(client, v4, v6, scenario, timeout.Token);
                Console.WriteLine("EXPLICIT failed-v6-first => v4; delayed fallback completed");
            }
#endif
        }
        using var canceled = new CancellationTokenSource();
        await canceled.CancelAsync();
        try
        {
#if NET11_0_OR_GREATER
            using Socket ignored = await RelayConnector.ConnectAsync("localhost", 1, ConnectAlgorithm.Parallel, canceled.Token);
#else
            using Socket ignored = await RelayConnector.ConnectAsync("localhost", 1, canceled.Token);
#endif
            throw new InvalidOperationException("Pre-cancellation ignored.");
        }
        catch (OperationCanceledException) { Console.WriteLine("CANCEL pre-dispatch; no socket ownership transferred"); }
#if NET11_0_OR_GREATER
        await CheckPendingCancellationAsync();
        using var invalid = new SocketAsyncEventArgs { RemoteEndPoint = new DnsEndPoint("localhost", 1) };
        try { Socket.ConnectAsync(SocketType.Stream, ProtocolType.Tcp, invalid, (ConnectAlgorithm)42); throw new InvalidOperationException("Invalid algorithm accepted."); }
        catch (ArgumentException) { Console.WriteLine("INVALID algorithm rejected"); }
#else
        using var during = new CancellationTokenSource();
        Task<Socket> waiting = RelayConnector.RaceAsync([IPAddress.IPv6Loopback, IPAddress.Loopback], 1, TimeSpan.FromSeconds(30), during.Token);
        await during.CancelAsync();
        try { using Socket ignored = await waiting; throw new InvalidOperationException("In-flight cancellation ignored."); }
        catch (OperationCanceledException) { Console.WriteLine("CANCEL scheduled attempts drained/disposed"); }
#endif
        Console.WriteLine("PASS RelayDial: actual dual-family listeners/refusals, selected peer and payload, failure/cancellation; fixture sockets disposed.");
    }

    private static async Task VerifyPeerAsync(Socket client, Socket v4, Socket v6, string scenario, CancellationToken token)
    {
        var peer = (IPEndPoint)client.RemoteEndPoint!;
        IPAddress address = peer.Address.IsIPv4MappedToIPv6 ? peer.Address.MapToIPv4() : peer.Address;
        Require(IPAddress.IsLoopback(address), "Nonlocal peer selected.");
        Require(scenario != "v4-only" || address.Equals(IPAddress.Loopback), "Wrong v4 fallback.");
        Require(scenario != "v6-only" || address.Equals(IPAddress.IPv6Loopback), "Wrong v6 fallback.");
        Socket listener = address.AddressFamily == AddressFamily.InterNetwork ? v4 : v6;
        await client.SendAsync(new byte[] { 0x41 }, SocketFlags.None, token);
        while (true)
        {
            using Socket accepted = await listener.AcceptAsync(token);
            // A raced loser can already have reached the accept queue before being closed.
            if (((IPEndPoint)accepted.RemoteEndPoint!).Port != ((IPEndPoint)client.LocalEndPoint!).Port) continue;
            byte[] buffer = new byte[1];
            Require(await accepted.ReceiveAsync(buffer, SocketFlags.None, token) == 1 && buffer[0] == 0x41, "Selected connection cannot transfer payload.");
            break;
        }
    }

#if NET11_0_OR_GREATER
    private static async Task CheckEventContractAsync(int port, ConnectAlgorithm algorithm, string scenario, Socket v4, Socket v6)
    {
        using var operation = new SocketAsyncEventArgs { RemoteEndPoint = new DnsEndPoint("localhost", port) };
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int events = 0;
        operation.Completed += (_, _) => { Interlocked.Increment(ref events); completion.TrySetResult(); };
        bool pending = Socket.ConnectAsync(SocketType.Stream, ProtocolType.Tcp, operation, algorithm);
        if (!pending) completion.TrySetResult();
        await completion.Task.WaitAsync(TimeSpan.FromSeconds(10));
        using Socket? socket = operation.ConnectSocket;
        Require(events == (pending ? 1 : 0), "Completed event/return contract disagrees.");
        Require((scenario == "neither") == (operation.SocketError != SocketError.Success), "Raw operation result disagrees.");
        if (socket is not null && operation.SocketError == SocketError.Success)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await VerifyPeerAsync(socket, v4, v6, scenario, timeout.Token);
        }
        Console.WriteLine($"EVENT {algorithm} pending={pending} completedEvents={events} error={operation.SocketError}; returned socket disposed");
    }

    private static async Task CheckPendingCancellationAsync()
    {
        // A loopback listener with a saturated accept queue holds a real TCP connect pending.
        using var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        listener.Listen(1);
        int port = ((IPEndPoint)listener.LocalEndPoint!).Port;
        var fillers = new List<Socket>();
        var connects = new List<Task>();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        try
        {
            for (int i = 0; i < 16; i++)
            {
                var filler = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                fillers.Add(filler);
                connects.Add(filler.ConnectAsync(IPAddress.Loopback, port, timeout.Token).AsTask());
            }
            foreach (ConnectAlgorithm algorithm in new[] { ConnectAlgorithm.Default, ConnectAlgorithm.Parallel })
            {
                using var cancellation = new CancellationTokenSource();
                Task<Socket> operation = RelayConnector.ConnectAsync("localhost", port, algorithm, cancellation.Token);
                Require(!operation.IsCompleted, "Cancellation fixture did not hold connection pending.");
                await cancellation.CancelAsync();
                try { using Socket ignored = await operation.WaitAsync(TimeSpan.FromSeconds(10)); throw new InvalidOperationException("Cancellation ignored."); }
                catch (OperationCanceledException) { Console.WriteLine($"CANCEL {algorithm} in-flight TCP; completion drained, socket disposed"); }
            }
        }
        finally
        {
            await timeout.CancelAsync();
            foreach (Socket socket in fillers) socket.Dispose();
            try { await Task.WhenAll(connects); } catch (Exception error) when (error is SocketException or OperationCanceledException or ObjectDisposedException) { }
        }
    }
#endif
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
