using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace Playbook.Samples;

// snippet:provider
public sealed record LanternTicket(int Index, string Label);

public sealed class QueueJournal
{
    public const int Total = 200;
    private readonly ConcurrentDictionary<string, Run> _runs = new();

    public void Hold(string key) => _runs.GetOrAdd(key, _ => new()).Hold();
    public void Release(string key) => _runs.GetOrAdd(key, _ => new()).Release();
    public object Read(string key) => _runs.GetOrAdd(key, _ => new()).Read();

    public async ValueTask<ItemsProviderResult<LanternTicket>> LoadAsync(string key, ItemsProviderRequest request)
    {
        Run run = _runs.GetOrAdd(key, _ => new());
        Task ready = run.Start(request.StartIndex, request.Count);
        try
        {
            await ready.WaitAsync(request.CancellationToken);
            await Task.Delay(120, request.CancellationToken);
            int count = Math.Min(request.Count, Math.Max(0, Total - request.StartIndex));
            LanternTicket[] rows = Enumerable.Range(request.StartIndex, count)
                .Select(index => new LanternTicket(index, $"Lantern collection {index:D3}")).ToArray();
            run.Finish(request.StartIndex, count, false);
            return new(rows, Total);
        }
        catch (OperationCanceledException)
        {
            run.Finish(request.StartIndex, 0, true);
            throw;
        }
    }

    private sealed class Run
    {
        private readonly object _sync = new();
        private TaskCompletionSource? _gate;
        private readonly List<object> _events = [];
        private int _pending;

        public void Hold() { lock (_sync) { _gate ??= new(TaskCreationOptions.RunContinuationsAsynchronously); } }
        public void Release() { lock (_sync) { _gate?.TrySetResult(); _gate = null; } }
        public Task Start(int start, int count)
        {
            lock (_sync)
            {
                _pending++;
                _events.Add(new { phase = "started", start, count });
                return _gate?.Task ?? Task.CompletedTask;
            }
        }
        public void Finish(int start, int count, bool canceled)
        {
            lock (_sync)
            {
                _pending--;
                _events.Add(new { phase = canceled ? "canceled" : "completed", start, count });
            }
        }
        public object Read() { lock (_sync) { return new { pending = _pending, held = _gate is not null, events = _events.ToArray() }; } }
    }
}
// endsnippet:provider
