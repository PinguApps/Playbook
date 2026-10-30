// snippet:boundary
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Playbook.Samples;

public static class HostBoundary
{
    private static readonly Action<ILogger, Exception?> ReportFailure = LoggerMessage.Define(
        LogLevel.Error, new EventId(71, "AquariumFailure"), "Aquarium importer stopped with a failure.");
    // RunAsync disposes the host even when stopping throws. This method owns it.
    public static async Task<int> RunOwnedAsync(IHost host, ILogger logger,
        CancellationToken cancellationToken = default)
    {
        ImportFailures failures = host.Services.GetRequiredService<ImportFailures>();
        bool stopHost = host.Services.GetRequiredService<IOptions<HostOptions>>().Value.BackgroundServiceExceptionBehavior == BackgroundServiceExceptionBehavior.StopHost;
        Exception? shutdownFailure = null;
        try { await host.RunAsync(cancellationToken); }
        catch (Exception failure) { shutdownFailure = failure; }
        // This boundary owns the process result; a recorded failure must not become success.
        List<Exception> all = stopHost ? failures.Snapshot().ToList() : [];
        if (shutdownFailure is not null) all.Add(shutdownFailure);
        if (all.Count == 0) return 0;
        Exception report = all.Count == 1 ? all[0] : new AggregateException(all);
        ReportFailure(logger, report);
        return 17;
    }
}
// endsnippet:boundary
