// snippet:complete
namespace RestorationBench;
// One local status channel; it does not own or roll back an upload.
public sealed class StatusFeed
{
    private readonly object _gate = new();
    private readonly Dictionary<int, Func<string, Task>> _listeners = new();
    private int _next, _cancellations, _callbacks;
    public CancellationTokenRegistration Subscribe(Func<string, Task> callback, CancellationToken token)
    {
        int id;
        lock (_gate) { id = ++_next; _listeners.Add(id, callback); }
        return token.Register(() =>
        {
            lock (_gate) { if (_listeners.Remove(id)) _cancellations++; }
        });
    }
    public async Task PublishAsync(string status)
    {
        Func<string, Task>[] listeners;
        lock (_gate) { listeners = _listeners.Values.ToArray(); }
        foreach (var listener in listeners)
        {
            await listener(status);
            lock (_gate) { _callbacks++; }
        }
    }
    public object Snapshot()
    {
        lock (_gate) return new { active = _listeners.Count, subscriptions = _next, cancellations = _cancellations, callbacks = _callbacks };
    }
}
// endsnippet:complete
