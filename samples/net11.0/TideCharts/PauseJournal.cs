// snippet:journal
using System.Collections.Concurrent;
using System.Text.Json;
namespace Playbook.Samples;

// Local evidence instrumentation; optional file output is set by the verification fixture.
public sealed class PauseJournal(IConfiguration configuration)
{
    private readonly object gate = new();
    private readonly ConcurrentQueue<object> events = new();
    public object[] Snapshot() => events.ToArray();
    public void Write(string name, string id = "", bool? accepted = null)
    {
        lock (gate)
        {
            var item = new { name, id, accepted, at = DateTimeOffset.UtcNow, process = Environment.ProcessId };
            events.Enqueue(item);
            if (configuration["JournalFile"] is { } file)
                File.AppendAllText(file, JsonSerializer.Serialize(item) + Environment.NewLine);
        }
    }
}
// endsnippet:journal
