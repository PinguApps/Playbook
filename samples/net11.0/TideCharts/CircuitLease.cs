using Microsoft.AspNetCore.Components.Server.Circuits;
namespace Playbook.Samples;

// This disposable resource deliberately isn't persisted. A resumed circuit creates a new one.
public sealed class CircuitLease(PauseJournal journal) : IDisposable
{
    public string Id { get; } = Guid.NewGuid().ToString("N");
    public void Dispose() => journal.Write("resource-disposed", Id);
}