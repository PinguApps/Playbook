// snippet:identity
namespace Playbook.Samples;

public sealed class Entry(int id, int lines, bool expanded)
{
    public int Id { get; } = id;
    public int Lines { get; } = lines;
    public bool Expanded { get; set; } = expanded;
}

public sealed class EntryIdentity : IEqualityComparer<Entry>
{
    public static EntryIdentity Instance { get; } = new();

    public bool Equals(Entry? x, Entry? y) => x?.Id == y?.Id;

    public int GetHashCode(Entry obj) => obj.Id.GetHashCode();
}
// endsnippet:identity
