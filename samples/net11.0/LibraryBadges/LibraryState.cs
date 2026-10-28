using System.Collections.Concurrent;

namespace Playbook.Samples;

// snippet:state
public sealed class LibraryState
{
    public ConcurrentDictionary<string, int> Counts { get; } = new();
    public int Enter(string kind) => Counts.AddOrUpdate(kind, 1, (_, count) => count + 1);
}
// endsnippet:state
