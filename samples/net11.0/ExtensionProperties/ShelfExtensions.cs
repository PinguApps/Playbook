namespace Playbook.Samples;

// snippet:property
public static class ShelfExtensions
{
    extension<T>(IReadOnlyCollection<T> shelves)
    {
        public bool HasPending
        {
            get
            {
                ArgumentNullException.ThrowIfNull(shelves);
                return shelves.Count != 0;
            }
        }
    }
}

public static class ShelfWorkflow
{
    public static bool HasPending(IReadOnlyCollection<string> shelves) => shelves.HasPending;
}
// endsnippet:property
