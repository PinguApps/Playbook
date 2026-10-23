// snippet:shutdown
namespace Playbook.Samples;

// The host calls StoppingAsync before stopping hosted services, including its web server.
public sealed class MaintenanceShutdown(MaintenanceRegistry registry, PauseJournal journal,
    IConfiguration configuration) : IHostedLifecycleService
{
    private int stopping;
    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref stopping, 1) != 0) return;
        journal.Write("host-stopping");
        var circuits = registry.BeginDrain();
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        budget.CancelAfter(TimeSpan.FromSeconds(12));
        try
        {
            // Verification of the request boundary; this doesn't cancel host shutdown.
            if (configuration.GetValue("Verification", false))
            {
                using var cancelled = new CancellationTokenSource();
                await cancelled.CancelAsync();
                foreach (var circuit in circuits) await circuit.RequestAsync(cancelled.Token);
            }
            foreach (var circuit in circuits)
            {
                var accepted = await circuit.RequestAsync(budget.Token);
                journal.Write("acceptance", accepted: accepted);
            }
            // Acceptance isn't completion. Observe every captured circuit's actual closure.
            await Task.WhenAll(circuits.Select(circuit => circuit.Closed.Task)).WaitAsync(budget.Token);
            journal.Write("drain-observed");
        }
        catch (OperationCanceledException)
        {
            journal.Write("drain-budget-expired");
            // Shutdown proceeds; a deferred/unresponsive client may lose its unsaved state.
        }
    }
    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        journal.Write("host-stopped");
        return Task.CompletedTask;
    }
}
// endsnippet:shutdown