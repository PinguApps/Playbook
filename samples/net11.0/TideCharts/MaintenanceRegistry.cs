// snippet:registry
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Playbook.Samples;

public sealed class MaintenanceRegistry
{
    private readonly object gate = new();
    private readonly Dictionary<string, WorksheetCircuit> connected = new();
    private bool draining;
    public WorksheetCircuit[] BeginDrain()
    {
        lock (gate) { draining = true; return connected.Values.ToArray(); }
    }
    public void Add(string id, WorksheetCircuit circuit)
    {
        lock (gate)
        {
            if (draining) throw new InvalidOperationException("Maintenance has started. Retry later.");
            connected[id] = circuit;
        }
    }
    public void Remove(string id) { lock (gate) { connected.Remove(id); } }
}

public sealed class WorksheetCircuit(MaintenanceRegistry registry, PauseJournal journal) : CircuitHandler
{
    private Circuit? current;
    public TaskCompletionSource Closed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        current = circuit;
        registry.Add(circuit.Id, this);
        journal.Write("up", circuit.Id);
        return Task.CompletedTask;
    }
    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        registry.Remove(circuit.Id);
        journal.Write("down", circuit.Id);
        return Task.CompletedTask;
    }
    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        registry.Remove(circuit.Id);
        journal.Write("closed", circuit.Id);
        Closed.TrySetResult();
        return Task.CompletedTask;
    }
    public async Task<bool> RequestAsync(CancellationToken token)
    {
        if (current is not { } circuit) return false;
        var accepted = await circuit.RequestCircuitPauseAsync(token);
        journal.Write(token.IsCancellationRequested ? "cancelled-request" : "pause-request", circuit.Id, accepted);
        return accepted;
    }
}
// endsnippet:registry