// snippet:boundary
using Microsoft.Extensions.Hosting;
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
        try
        {
            await host.RunAsync(cancellationToken);
            return 0;
        }
        catch (Exception failure)
        {
            // An explicit process boundary: log, then preserve a failure exit.
            ReportFailure(logger, failure);
            return 17;
        }
    }
}
// endsnippet:boundary
