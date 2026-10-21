using Microsoft.Extensions.Primitives;

namespace Playbook.Samples;

internal static class ReloadChecks
{
    public static async Task RunAsync()
    {
        using ReloadSignal signal = new();
        TaskCompletionSource entered = NewSignal();
        TaskCompletionSource release = NewSignal();
        TaskCompletionSource finished = NewSignal();
        int calls = 0;
        int failures = 0;
        async Task RefreshAsync()
        {
            int call = Interlocked.Increment(ref calls);
            if (call == 1)
            {
                entered.SetResult();
                await release.Task;
            }
            if (call == 2)
            {
                await Task.Yield();
                throw new InvalidOperationException("The berth catalogue is unavailable.");
            }
            if (call == 3) { finished.SetResult(); }
        }

        await using ReloadSubscription subscription = new(signal.Token, RefreshAsync, _ => Interlocked.Increment(ref failures));
        signal.Fire();
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        signal.Fire();
        signal.Fire();
        Check(calls == 1, "Reloads must not overlap.");
        release.SetResult();
        await WaitAsync(() => Volatile.Read(ref failures) == 1);
        Check(calls == 2, "Intermediate changes must coalesce.");
        await WaitAsync(() => signal.Registrations > 0);
        signal.Fire();
        await finished.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Check(failures == 1, "The callback must report its asynchronous failure and rearm.");

#if NET11_0_OR_GREATER
        using ReloadSignal disposingSignal = new();
        TaskCompletionSource disposeEntered = NewSignal();
        TaskCompletionSource disposeRelease = NewSignal();
        TaskCompletionSource disposeFinished = NewSignal();
        IDisposable registration = ChangeToken.OnChange(disposingSignal.Token, async () =>
        {
            disposeEntered.SetResult();
            await disposeRelease.Task;
            disposeFinished.SetResult();
        });
        disposingSignal.Fire();
        await disposeEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        registration.Dispose();
        Check(!disposeFinished.Task.IsCompleted, "Disposal must not pretend to join running work.");
        disposeRelease.SetResult();
        await disposeFinished.Task.WaitAsync(TimeSpan.FromSeconds(5));

        using ReloadSignal throwingSignal = new();
        using IDisposable throwing = ChangeToken.OnChange(throwingSignal.Token,
            (Func<Task>)(() => throw new InvalidOperationException("Synchronous delegate failure")));
        try { throwingSignal.Fire(); throw new Exception("Expected synchronous propagation."); }
        catch (AggregateException error) when (error.InnerException is InvalidOperationException) { }
#endif
    }

    private static TaskCompletionSource NewSignal() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static void Check(bool condition, string message) { if (!condition) { throw new Exception(message); } }
    private static async Task WaitAsync(Func<bool> condition)
    {
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(5));
        while (!condition()) { await Task.Delay(10, timeout.Token); }
    }

    private sealed class ReloadSignal : IDisposable
    {
        private CancellationTokenSource _source = new();
        public int Registrations => _token?.Registrations ?? 0;
        private CountedToken? _token;
        public IChangeToken Token() => _token = new CountedToken(_source.Token);
        public void Fire()
        {
            CancellationTokenSource old = _source;
            _source = new();
            old.Cancel();
            old.Dispose();
        }
        public void Dispose() => _source.Dispose();
    }

    private sealed class CountedToken(CancellationToken token) : IChangeToken
    {
        private int _registrations;
        public int Registrations => Volatile.Read(ref _registrations);
        public bool HasChanged => token.IsCancellationRequested;
        public bool ActiveChangeCallbacks => true;
        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state)
        {
            Interlocked.Increment(ref _registrations);
            return token.Register(callback, state);
        }
    }
}
