// snippet:complete
namespace ReedArchive;

public sealed class CollectionContext(string name) : IDisposable
{
    public string Name { get; } = name;
    public int Count { get; private set; }
    private readonly Dictionary<Func<Task>, Action> listeners = [];
    internal void Subscribe(Func<Task> listener, Action dispose) => listeners.Add(listener, dispose);
    internal void Unsubscribe(Func<Task> listener) => listeners.Remove(listener);
    public int SubscriptionsCreated { get; set; }
    public int DisposeCalls { get; set; }
    public int ActiveSubscribers { get; set; }

    // Call on the owning renderer's dispatcher; this sample is not thread safe.
    public async Task AddAsync()
    {
        Count++;
        foreach (var listener in listeners.Keys.ToArray())
            await listener();
    }

    // Scope teardown is a fallback when renderer disposal does not unsubscribe.
    public void Dispose()
    {
        foreach (var dispose in listeners.Values.ToArray()) dispose();
    }
}

public sealed class ArchiveState : IDisposable
{
    public CollectionContext Letters { get; } = new("letters");
    public CollectionContext Maps { get; } = new("maps");
    public void Dispose() { Letters.Dispose(); Maps.Dispose(); }
}

public interface IArchiveSubscriber
{
    Task RefreshArchiveAsync();
}
// endsnippet:complete
