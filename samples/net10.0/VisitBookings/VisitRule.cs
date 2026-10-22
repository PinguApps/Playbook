using System.Collections.Concurrent;
using System.Net.Http.Json;

namespace Playbook.Samples;

// snippet:remote
public sealed class VisitRule(HttpClient client)
{
    public async Task<bool> IsOpenAsync(string code, CancellationToken token) =>
        await client.GetFromJsonAsync<bool>($"/rules/{Uri.EscapeDataString(code)}", token);
}
// endsnippet:remote

public sealed class RuleJournal
{
    private sealed class Entry
    {
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool Held { get; init; }
        private int entered;
        public int Entered => Volatile.Read(ref entered);
        public void RecordEntered() => Interlocked.Increment(ref entered);
        private int completed;
        public int Completed => Volatile.Read(ref completed);
        public void RecordCompleted() => Interlocked.Increment(ref completed);
        private int canceled;
        public int Canceled => Volatile.Read(ref canceled);
        public void RecordCanceled() => Interlocked.Increment(ref canceled);
        private int accepted;
        public int Accepted => Volatile.Read(ref accepted);
        public void RecordAccepted() => Interlocked.Increment(ref accepted);
    }

    private readonly ConcurrentDictionary<string, Entry> entries = new(StringComparer.Ordinal);
    public void Arm(string code) => entries[code] = new Entry { Held = true };
    public void Release(string code) => entries.GetOrAdd(code, _ => new()).Release.TrySetResult();
    public void Accept(string code) => entries.GetOrAdd(code, _ => new()).RecordAccepted();
    public object Snapshot(string code)
    {
        var entry = entries.GetOrAdd(code, _ => new());
        return new { entry.Entered, entry.Completed, entry.Canceled, entry.Accepted };
    }

    public async Task<bool> CheckAsync(string code, CancellationToken token)
    {
        var entry = entries.GetOrAdd(code, _ => new());
        entry.RecordEntered();
        try
        {
            if (entry.Held) await entry.Release.Task.WaitAsync(token);
            else await Task.Delay(100, token);
            entry.RecordCompleted();
            return !code.StartsWith("closed", StringComparison.Ordinal);
        }
        catch (OperationCanceledException)
        {
            entry.RecordCanceled();
            throw;
        }
    }
}
