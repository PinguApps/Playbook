namespace Playbook.Samples;

// snippet:state
public sealed class LedgerState
{
    private int _revision = 1;
    private int _builds;
    private int _lifecycles;
    public int Revision => Volatile.Read(ref _revision);
    public int Builds => Volatile.Read(ref _builds);
    public int Lifecycles => Volatile.Read(ref _lifecycles);
    public int Revise() => Interlocked.Increment(ref _revision);
    public void EnterLifecycle() => Interlocked.Increment(ref _lifecycles);
    public async Task<string> BuildAsync(string shelf)
    {
        // Stand-in for an expensive local aggregation; no external service.
        await Task.Delay(150);
        return $"{shelf}:survey-{Interlocked.Increment(ref _builds)}";
    }
}
// endsnippet:state
