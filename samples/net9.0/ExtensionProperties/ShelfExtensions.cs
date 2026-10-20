namespace Playbook.Samples;

// snippet:property
public static class ShelfExtensions
{
    public static bool HasPending<T>(this IReadOnlyCollection<T> shelves)
    {
        ArgumentNullException.ThrowIfNull(shelves);
        return shelves.Count != 0;
    }
}

public static class ShelfWorkflow
{
    public static bool HasPending(IReadOnlyCollection<string> shelves) => shelves.HasPending();
}
// endsnippet:property
