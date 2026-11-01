#region relay-connector
using System.Net;
using System.Net.Sockets;

namespace Playbook.Samples;

public static class RelayConnector
{
    public static async Task<Socket> ConnectAsync(string host, int port, CancellationToken cancellationToken)
    {
        IPAddress[] addresses = await Dns.GetHostAddressesAsync(host, cancellationToken);
        return await RaceAsync(addresses, port, TimeSpan.FromMilliseconds(250), cancellationToken);
    }

    // Caller policy: preserve resolver order and stagger attempts. Not full RFC 8305.
    public static async Task<Socket> RaceAsync(IPAddress[] addresses, int port, TimeSpan stagger,
        CancellationToken cancellationToken)
    {
        if (addresses.Length == 0) throw new ArgumentException("At least one address is required.", nameof(addresses));
        ArgumentOutOfRangeException.ThrowIfLessThan(stagger, TimeSpan.Zero);
        cancellationToken.ThrowIfCancellationRequested();
        using var attemptsCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task<Socket>[] attempts = addresses.Select((address, index) => AttemptAsync(address, index)).ToArray();
        var pending = attempts.ToList();
        Socket? winner = null;
        try
        {
            while (pending.Count > 0)
            {
                Task<Socket> finished = await Task.WhenAny(pending);
                pending.Remove(finished);
                try
                {
                    Socket connected = await finished;
                    cancellationToken.ThrowIfCancellationRequested();
                    winner = connected;
                    return winner; // Ownership transfers only after losers have been drained below.
                }
                catch (SocketException) when (pending.Count > 0) { }
            }
            throw new SocketException((int)SocketError.HostUnreachable);
        }
        finally
        {
            await attemptsCancellation.CancelAsync();
            try { await Task.WhenAll(attempts); }
            catch (Exception error) when (error is SocketException or OperationCanceledException) { }
            foreach (Task<Socket> attempt in attempts)
            {
                if (!attempt.IsCompletedSuccessfully) continue;
                Socket connected = await attempt;
                if (!ReferenceEquals(connected, winner)) connected.Dispose();
            }
        }

        async Task<Socket> AttemptAsync(IPAddress address, int index)
        {
            await Task.Delay(stagger * index, attemptsCancellation.Token);
            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await socket.ConnectAsync(address, port, attemptsCancellation.Token);
                return socket;
            }
            catch { socket.Dispose(); throw; }
        }
    }
}
#endregion
