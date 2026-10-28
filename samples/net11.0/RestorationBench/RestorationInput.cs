// snippet:complete
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
namespace RestorationBench;
public sealed class RestorationInput : InputFile
{
    [Inject] public StatusFeed Feed { get; set; } = default!;
    [Parameter] public EventCallback<string> OnStatus { get; set; }
    private readonly CancellationTokenSource _lifetime = new();
    private CancellationTokenRegistration _subscription;
    private volatile bool _disposed;
    protected override void OnInitialized()
    {
        _subscription = Feed.Subscribe(status => InvokeAsync(async () =>
        {
            // A publisher may already have copied the callback when cancellation starts.
            if (!_disposed) await OnStatus.InvokeAsync(status);
        }), _lifetime.Token);
    }
    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                try { _lifetime.Cancel(); }
                finally { _subscription.Dispose(); _lifetime.Dispose(); }
            }
        }
        finally { base.Dispose(disposing); }
    }
}
// endsnippet:complete
