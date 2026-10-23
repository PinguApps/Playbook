namespace Playbook.Samples;

// snippet:identity
// Deliberately a reference type without value equality: each refresh returns new objects.
public sealed class Consignment(int id, string label)
{
    public int Id { get; } = id;
    public string Label { get; } = label;
    public static IEqualityComparer<Consignment> ById { get; } =
        EqualityComparer<Consignment>.Create((a, b) => a?.Id == b?.Id, row => row.Id.GetHashCode());
}
// endsnippet:identity
