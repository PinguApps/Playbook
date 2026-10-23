// snippet:all
using FossilWorker;
using Microsoft.JSInterop;

namespace FossilDesk;

// Diagnostic entry points used only by the browser verification below.
// These deliberately exercise cancellation that production ownership avoids.
public static class WorkerProbe
{
    public static IJSRuntime Runtime { get; set; } = null!;
    private static CancellationTokenSource? _creation;

    [JSInvokable]
    public static async Task<string> CreationCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        _creation = cancellation;
        try
        {
            await using var unexpected = await WebWorkerClient.CreateAsync(Runtime,
                timeoutMs: 10000, cancellationToken: cancellation.Token);
            return "created-before-cancel";
        }
        catch (OperationCanceledException) { return "creation-await-cancelled"; }
        finally { _creation = null; }
    }

    [JSInvokable]
    public static void CancelCreation() => _creation?.Cancel();

    [JSInvokable]
    public static async Task<string> CreationTimeout()
    {
        try
        {
            await using var unexpected = await WebWorkerClient.CreateAsync(Runtime, timeoutMs: 1);
            return "created-before-timeout";
        }
        catch (InvalidOperationException error) { return error.Message; }
    }
}
// endsnippet:all
