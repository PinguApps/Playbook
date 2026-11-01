#region relay-connector
using System.Net;
using System.Net.Sockets;

namespace Playbook.Samples;

public static class RelayConnector
{
    public static async Task<Socket> ConnectAsync(string host, int port, ConnectAlgorithm algorithm,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var operation = new SocketAsyncEventArgs { RemoteEndPoint = new DnsEndPoint(host, port) };
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        operation.Completed += (_, _) => completion.TrySetResult();
        // False means synchronous completion: Completed will not be raised.
        bool pending = Socket.ConnectAsync(SocketType.Stream, ProtocolType.Tcp, operation, algorithm);
        if (!pending) completion.TrySetResult();
        using CancellationTokenRegistration registration = cancellationToken.Register(
            () => Socket.CancelConnectAsync(operation));
        await completion.Task; // Drain completion before disposing event args, even on cancellation.
        if (cancellationToken.IsCancellationRequested)
        {
            operation.ConnectSocket?.Dispose(); // Cancellation may race a successful connection.
            cancellationToken.ThrowIfCancellationRequested();
        }
        if (operation.SocketError != SocketError.Success)
        {
            operation.ConnectSocket?.Dispose();
            throw new SocketException((int)operation.SocketError);
        }
        return operation.ConnectSocket ?? throw new InvalidOperationException("No connected socket returned.");
    }
}
#endregion
