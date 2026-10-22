using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Tracing;
using Microsoft.Extensions.Logging;

namespace Playbook.Samples;

public static class TraceSetup
{
    // snippet:tracing
    public static void Register(IServiceCollection services, Action<Activity> stopped)
    {
        services.AddTracing(tracing => tracing
            .AddListener("dispatch", listener =>
            {
                listener.Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded;
                listener.ActivityStopped = stopped;
            })
            .EnableTracing("Reef.Dispatch", listenerName: "dispatch", scopes: ActivitySourceScopes.Local)
            .DisableTracing("Reef.Dispatch", "Heartbeat", "dispatch", ActivitySourceScopes.Local));
    }
    // endsnippet:tracing
}

// snippet:logging
public static partial class DispatchLog
{
    [LoggerMessage(71, LogLevel.Information, "Assigned berth {Berth} for {Count} crates")]
    public static partial void Assigned<TBerth>(ILogger logger, TBerth berth, int count)
        where TBerth : struct, Enum;

    [LoggerMessage(73, LogLevel.Debug, "Cargo manifest {Manifest}")]
    private static partial void Manifest(ILogger logger, string manifest);

    public static void Describe(ILogger logger, Func<string> describe)
    {
        // A generated IsEnabled guard cannot prevent evaluation of call arguments.
        if (logger.IsEnabled(LogLevel.Debug))
            Manifest(logger, describe());
    }
}
// endsnippet:logging

#if UNSUPPORTED_REF_STRUCT
public static partial class RejectedLog
{
    [LoggerMessage(72, LogLevel.Debug, "Value {Value}")]
    public static partial void Write<T>(ILogger logger, T value) where T : allows ref struct;
}
#endif

#if EXPENSIVE_LOG_ARGUMENT
public static class RejectedArgument
{
    public static void Write(ILogger logger, Func<string> describe) =>
        logger.LogDebug("Cargo manifest {Manifest}", describe());
}
#endif
