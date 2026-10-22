using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Playbook.Samples;

public static class TraceSetup
{
    // snippet:tracing
    public static ActivityListener Listen(Action<Activity> stopped)
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Reef.Dispatch",
            Sample = (ref ActivityCreationOptions<ActivityContext> options) =>
                options.Name == "Heartbeat" ? ActivitySamplingResult.None : ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = stopped
        };
        ActivitySource.AddActivityListener(listener);
        return listener; // Caller owns and disposes the listener.
    }
    // endsnippet:tracing
}

// snippet:logging
public static partial class DispatchLog
{
    [LoggerMessage(71, LogLevel.Information, "Assigned berth {Berth} for {Count} crates")]
    public static partial void Assigned(ILogger logger, BerthCode berth, int count);

    [LoggerMessage(73, LogLevel.Debug, "Cargo manifest {Manifest}")]
    private static partial void Manifest(ILogger logger, string manifest);

    public static void Describe(ILogger logger, Func<string> describe)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            Manifest(logger, describe());
    }
}
// endsnippet:logging
