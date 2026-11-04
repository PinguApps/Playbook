using System.Collections.Concurrent;

namespace ReedReservations;

public sealed class Availability
{
    private sealed class Check
    {
        public readonly TaskCompletionSource Gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int Entered, Completed, Canceled, Accepted;
    }

    private readonly ConcurrentDictionary<string, Check> checks = new();
    private Check Get(string code) => checks.GetOrAdd(code, _ => new Check());
    public void Arm(string code) => checks[code] = new Check();
    public void Release(string code) => Get(code).Gate.TrySetResult();
    public object Snapshot(string code)
    {
        var check = Get(code);
        return new { entered = Volatile.Read(ref check.Entered), completed = Volatile.Read(ref check.Completed),
            canceled = Volatile.Read(ref check.Canceled), accepted = Volatile.Read(ref check.Accepted) };
    }
    public void Accept(string code) => Interlocked.Increment(ref Get(code).Accepted);

    public async Task<bool> IsAvailableAsync(string code, CancellationToken cancellationToken)
    {
        var check = Get(code);
        Interlocked.Increment(ref check.Entered);
        try
        {
            await check.Gate.Task.WaitAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            Interlocked.Increment(ref check.Completed);
            return !code.StartsWith("closed", StringComparison.Ordinal);
        }
        catch (OperationCanceledException)
        {
            Interlocked.Increment(ref check.Canceled);
            throw;
        }
    }
}
