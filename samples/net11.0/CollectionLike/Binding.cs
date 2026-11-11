namespace Playbook.Samples;

// snippet:binding
public static class CollectionBinding
{
    private static int Reserve<T>(HashSet<T> values) => values.Count;
    public static int EmptyLabels() => Reserve<string>([with(comparer: StringComparer.OrdinalIgnoreCase)]);
    public static List<int> EmptyQueue() => [with(capacity: default)];
    public static HashSet<string> EmptySet() => [with(comparer: default)];
}
// endsnippet:binding
