// snippet:complete
using Microsoft.AspNetCore.Components;

namespace Planetarium;

// This bounded WASM host uses one browser thread, with no workers/threading.
// Fail on a different thread; this is not a general cross-thread scheduler.
internal sealed class BrowserThreadDispatcher : Dispatcher
{
    private readonly int thread = Environment.CurrentManagedThreadId;
    public override bool CheckAccess() => Environment.CurrentManagedThreadId == thread;
    public override Task InvokeAsync(Action workItem) { AssertAccess(); workItem(); return Task.CompletedTask; }
    public override Task InvokeAsync(Func<Task> workItem) { AssertAccess(); return workItem(); }
    public override Task<T> InvokeAsync<T>(Func<T> workItem) { AssertAccess(); return Task.FromResult(workItem()); }
    public override Task<T> InvokeAsync<T>(Func<Task<T>> workItem) { AssertAccess(); return workItem(); }
}
// endsnippet:complete
