// snippet:complete
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;

namespace Soundings;

public static class Proof
{
    public static async Task RunAsync(Uri address, bool newServer)
    {
        const bool newClient = false;
        using HttpClient control = new() { BaseAddress = address, Timeout = TimeSpan.FromSeconds(15) };
        await using HubConnection connection = new HubConnectionBuilder()
            .WithUrl(new Uri(address, "/soundings"), options =>
            {
                options.Transports = HttpTransportType.WebSockets;
                options.SkipNegotiation = true;
            }).Build();
        await connection.StartAsync();
        Console.WriteLine($"Client: {RuntimeInformation.FrameworkDescription}; SignalR {typeof(HubConnection).Assembly.GetName().Version}; server11={newServer}");

        string id = Guid.NewGuid().ToString("N");
        using CancellationTokenSource abandoned = new();
        // The final token controls the invocation; it is not a serialized hub argument.
        Task<string> invocation = connection.InvokeAsync<string>("Calculate", id, abandoned.Token);
        Check(await control.GetStringAsync(new Uri(address, $"/proof/{id}/started")) == "started", "Server never entered work.");
        await abandoned.CancelAsync();
        try
        {
            await invocation.WaitAsync(TimeSpan.FromSeconds(10));
            throw new InvalidOperationException("Expected a canceled client task.");
        }
        catch (OperationCanceledException) when (abandoned.IsCancellationRequested)
        {
            Console.WriteLine("Client task canceled.");
        }

        Check(await connection.InvokeAsync<string>("Echo", "still-connected").WaitAsync(TimeSpan.FromSeconds(10)) == "still-connected", "Connection was lost.");
        if (newClient && newServer)
        {
            Check(await control.GetStringAsync(new Uri(address, $"/proof/{id}/finished")) == "canceled", "Server did not observe invocation cancellation.");
            Console.WriteLine("PASS: non-streaming server work canceled; the same connection still answers Echo.");

            string completedId = Guid.NewGuid().ToString("N");
            Task<string> completed = connection.InvokeAsync<string>("Calculate", completedId, CancellationToken.None);
            await control.GetStringAsync(new Uri(address, $"/proof/{completedId}/started"));
            using HttpResponseMessage release = await control.PostAsync(new Uri(address, $"/proof/{completedId}/release"), null);
            release.EnsureSuccessStatusCode();
            Check(await completed.WaitAsync(TimeSpan.FromSeconds(10)) == $"sounding:{completedId}", "Normal work did not complete.");
            Check(await control.GetStringAsync(new Uri(address, $"/proof/{completedId}/finished")) == "completed", "Wrong normal outcome.");
            Console.WriteLine("PASS: an uncanceled invocation completes normally on that connection.");
        }
        else
        {
            // Echo is a same-connection ordering barrier, not a sleep-based assertion.
            Check(await control.GetStringAsync(new Uri(address, $"/proof/{id}/state")) == "running", "Legacy-pair negative control unexpectedly stopped.");
            Console.WriteLine("PASS: a missing new client/server capability cancels only the client wait; server work remains running.");
            // Valid .NET 10 fallback: stop the dedicated connection and propagate its token.
            await connection.StopAsync();
            Check(await control.GetStringAsync(new Uri(address, $"/proof/{id}/finished")) == "canceled", "Disconnect fallback did not cancel server work.");
            Check(connection.State == HubConnectionState.Disconnected, "Fallback unexpectedly retained the connection.");
            Console.WriteLine("PASS: stopping the connection cancels server work through its connection-aborted token.");
        }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) { throw new InvalidOperationException(message); }
    }
}
// endsnippet:complete
