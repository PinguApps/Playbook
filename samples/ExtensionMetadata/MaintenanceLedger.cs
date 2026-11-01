// A bounded service with real asynchronous completion/cancellation, not a validation mock.
namespace Playbook.Samples;

internal sealed class MaintenanceLedger
{
    public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Decision { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public MaintenanceLedger(bool ready = true) { if (ready) Decision.SetResult(); }
    public void Check()
    {
        if (!Decision.Task.IsCompletedSuccessfully) throw new InvalidOperationException("Use asynchronous validation while the ledger is pending.");
    }
    public async Task CheckAsync(CancellationToken token)
    {
        Entered.TrySetResult();
        await Decision.Task.WaitAsync(token);
    }
}
