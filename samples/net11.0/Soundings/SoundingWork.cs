// snippet:complete
using System.Collections.Concurrent;

namespace Soundings;

public sealed class SoundingWork
{
    private readonly ConcurrentDictionary<string, Operation> _operations = new(StringComparer.Ordinal);

    public Operation Get(string id) => _operations.GetOrAdd(id, _ => new Operation());

    public async Task<string> CalculateAsync(string id, CancellationToken cancellationToken)
    {
        Operation operation = Get(id);
        operation.Started.TrySetResult();
        try
        {
            // A held calculation makes cancellation observable without timing races.
            // Real I/O and bounded calculation loops must also observe this token.
            await operation.Release.Task.WaitAsync(cancellationToken);
            operation.Finished.TrySetResult("completed");
            return $"sounding:{id}";
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            operation.Finished.TrySetResult("canceled");
            throw;
        }
    }

    public sealed class Operation
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<string> Finished { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
// endsnippet:complete
