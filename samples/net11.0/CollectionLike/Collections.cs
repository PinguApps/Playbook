namespace Playbook.Samples;

// snippet:construction
public static class CollectionWorkflow
{
    public static List<string> Queue() => [with(capacity: 12), "shelf-c", "shelf-a", "shelf-c"];
    public static HashSet<string> Labels() => [with(StringComparer.OrdinalIgnoreCase), "bay-q", "BAY-Q", "bay-r"];
    public static CrateLabels BuiltLabels() => [with(StringComparer.OrdinalIgnoreCase), "bay-q", "BAY-Q", "bay-r"];

    private static int with(int slot) => slot * 3;
    public static List<int> EscapedCall() => [@with(7)];
}
// endsnippet:construction

// snippet:indexer
public static class LinkedShelfAccess
{
    extension<T>(LinkedList<T> chain)
    {
        public T this[int slot]
        {
            get => NodeAt(chain, slot).Value;
            set => NodeAt(chain, slot).Value = value;
        }
    }

    private static LinkedListNode<T> NodeAt<T>(LinkedList<T> chain, int slot)
    {
        ArgumentNullException.ThrowIfNull(chain);
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(slot, chain.Count);
        LinkedListNode<T> node = chain.First!;
        for (int position = 0; position < slot; position++) { node = node.Next!; }
        return node;
    }
}

public static class IndexWorkflow
{
    public static string Read(LinkedList<string> chain, int slot) => chain[slot];
    public static void Write(LinkedList<string> chain, int slot, string label) => chain[slot] = label;
}
// endsnippet:indexer
