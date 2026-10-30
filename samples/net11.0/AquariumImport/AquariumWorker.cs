// snippet:worker
using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;

namespace Playbook.Samples;

public sealed class ImportFailures
{
    private readonly ConcurrentQueue<Exception> failures = new();
    public void Record(Exception failure) => failures.Enqueue(failure);
    public Exception[] Snapshot() => failures.ToArray();
}

public class AquariumWorker(ImportFailures failures, Func<CancellationToken, Task> import) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try { await import(stoppingToken); }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { throw; }
        catch (Exception failure)
        {
            // Needed for the .NET 10 boundary; keep the original exception.
            failures.Record(failure);
            throw;
        }
    }
}
// endsnippet:worker
