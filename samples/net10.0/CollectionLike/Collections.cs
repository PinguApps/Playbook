namespace Playbook.Samples;

// snippet:construction
public static class CollectionWorkflow
{
    public static List<string> Queue() => new(12) { "shelf-c", "shelf-a", "shelf-c" };
    public static HashSet<string> Labels() => new(StringComparer.OrdinalIgnoreCase) { "bay-q", "BAY-Q", "bay-r" };
    public static CrateLabels BuiltLabels() => CrateLabelBuilder.Create(StringComparer.OrdinalIgnoreCase, ["bay-q", "BAY-Q", "bay-r"]);

    private static int with(int slot) => slot * 3;
    public static List<int> EscapedCall() => [@with(7)];
}
// endsnippet:construction

// snippet:indexer
public static class LinkedShelfAccess
{
    public static T ReadAt<T>(this LinkedList<T> chain, int slot) => NodeAt(chain, slot).Value;
    public static void WriteAt<T>(this LinkedList<T> chain, int slot, T value) => NodeAt(chain, slot).Value = value;

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
    public static string Read(LinkedList<string> chain, int slot) => chain.ReadAt(slot);
    public static void Write(LinkedList<string> chain, int slot, string label) => chain.WriteAt(slot, label);
}
// endsnippet:indexer
