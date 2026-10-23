// snippet:ticker
using Microsoft.Extensions.Hosting;
using Microsoft.JSInterop;

namespace BeaconWatch;

public sealed class BeaconTicker(IJSRuntime js) : IHostedService, IDisposable
{
    private readonly CancellationTokenSource _stop = new();
    private Task? _loop;
    private int _count;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await RecordAsync("started");
        // RunAsync yields at the timer, so startup and rendering remain responsive.
        _loop = RunAsync(_stop.Token);
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(250));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                // One awaited iteration at a time; pass the token to real I/O too.
                ++_count;
                await RecordAsync("tick");
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await RecordAsync("cancelled");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await RecordAsync("stop-requested");
        await _stop.CancelAsync();
        if (_loop is not null) await _loop.WaitAsync(cancellationToken);
        await RecordAsync("stopped");
    }

    private async Task RecordAsync(string kind)
    {
        try { await js.InvokeVoidAsync("beacon.record", kind, _count); }
        catch (JSException error)
        {
            await Console.Error.WriteLineAsync(error.ToString());
            throw; // Broken instrumentation must fail visibly, never report success.
        }
    }

    public void Dispose() => _stop.Dispose();
}
// endsnippet:ticker
