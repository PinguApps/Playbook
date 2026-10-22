namespace Playbook.Samples;

// Local behavior-check instrumentation; never an application administration API.
// snippet:gate
public sealed class InspectionGate
{
    private TaskCompletionSource _release = NewSource();

    public Task WaitAsync(CancellationToken token) => _release.Task.WaitAsync(token);

    public void Arm() => _release = NewSource();

    public void Release() => _release.TrySetResult();

    private static TaskCompletionSource NewSource() => new(TaskCreationOptions.RunContinuationsAsynchronously);
}
// endsnippet:gate
