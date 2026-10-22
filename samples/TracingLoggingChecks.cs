using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
#if NET11_0_OR_GREATER
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.Tracing;
#endif

namespace Playbook.Samples;

public enum BerthCode { North, South }

internal static class Program
{
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public static void Main()
    {
        List<string> records = [];
#if NET11_0_OR_GREATER
        // snippet:factory
        var services = new ServiceCollection();
        TraceSetup.Register(services, activity => records.Add(activity.OperationName));
        using ServiceProvider provider = services.BuildServiceProvider();
        ActivitySourceFactory factory = provider.GetRequiredService<ActivitySourceFactory>();
        using ActivitySource source = factory.Create("Reef.Dispatch");
        // endsnippet:factory
        using var globalSource = new ActivitySource("Reef.Dispatch");
        using (Activity? global = globalSource.StartActivity("Ship")) Require(global is null, "Local rule admitted global source.");
#else
        using ActivityListener listener = TraceSetup.Listen(activity => records.Add(activity.OperationName));
        using var source = new ActivitySource("Reef.Dispatch");
#endif
        using (Activity? ship = source.StartActivity("Ship")) Require(ship?.Recorded == true, "Ship not sampled.");
        using (Activity? heartbeat = source.StartActivity("Heartbeat")) Require(heartbeat is null, "Heartbeat not excluded.");
        using var other = new ActivitySource("Reef.Other");
        using (Activity? ignored = other.StartActivity("Ship")) Require(ignored is null, "Unmatched source recorded.");
        Require(records.Count == 1 && records[0] == "Ship", "Wrong stopped activity records.");
#if NET11_0_OR_GREATER
        CheckRefresh();
        CheckRawRefresh();
#endif
        var logger = new RecordingLogger();
        DispatchLog.Assigned(logger, BerthCode.South, 9);
        Require(logger.Records.Count == 1 && logger.Records[0].Id == 71 && logger.Records[0].Message == "Assigned berth South for 9 crates", "Generated record mismatch.");
        Require(logger.Fields["Berth"] is BerthCode.South && logger.Fields["Count"] is 9, "Structured fields lost.");
        logger.Enabled = false;
        DispatchLog.Assigned(logger, BerthCode.North, 2);
        int descriptions = 0;
        DispatchLog.Describe(logger, () => { descriptions++; return "sensitive workload"; });
        Require(logger.Records.Count == 1 && descriptions == 0, "Disabled logging did work.");
        logger.Enabled = true;
        DispatchLog.Describe(logger, () => { descriptions++; return "9 crates"; });
        Require(descriptions == 1 && logger.Records.Count == 2, "Enabled debug record missing.");
        Console.WriteLine($"PASS {Environment.Version}: source/operation selection, sampled stopped record, structured generated event 71, disabled argument guard.");
    }

#if NET11_0_OR_GREATER
    private static void CheckRefresh()
    {
        using var configuration = (ConfigurationRoot)new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["EnabledLocalTracing:Reef.Reload"] = "false" }).Build();
        var services = new ServiceCollection();
        int stopped = 0;
        services.AddTracing(builder => builder.AddConfiguration(configuration).AddListener("reload", listener =>
        {
            listener.Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData;
            listener.ActivityStopped = _ => stopped++;
        }));
        using ServiceProvider provider = services.BuildServiceProvider();
        using ActivitySource source = provider.GetRequiredService<ActivitySourceFactory>().Create("Reef.Reload");
        using (Activity? disabled = source.StartActivity("Inspect")) Require(disabled is null, "Initial rule ignored.");
        configuration["EnabledLocalTracing:Reef.Reload"] = "true";
        configuration.Reload();
        using (Activity? enabled = source.StartActivity("Inspect")) Require(enabled is not null, "Existing source not refreshed.");
        configuration["EnabledLocalTracing:Reef.Reload"] = "false";
        configuration.Reload();
        using (Activity? disabled = source.StartActivity("Inspect")) Require(disabled is null, "Disable refresh failed.");
        Require(stopped == 1, "Wrong reload record count.");
        Console.WriteLine("PASS tracing: existing factory source follows false/true/false configuration reload; global scope excluded.");
    }

    private static void CheckRawRefresh()
    {
        bool enabled = false;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => enabled && source.Name == "Reef.Manual",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);
        using var source = new ActivitySource("Reef.Manual");
        Require(!source.HasListeners(), "Unexpected raw subscription.");
        enabled = true;
        listener.RefreshSources();
        Require(source.HasListeners(), "RefreshSources did not revisit existing source.");
        enabled = false;
        listener.RefreshSources();
        Require(!source.HasListeners(), "RefreshSources did not detach existing source.");
    }
#endif

    private sealed class RecordingLogger : ILogger
    {
        public bool Enabled { get; set; } = true;
        public List<(int Id, string Message)> Records { get; } = [];
        public Dictionary<string, object?> Fields { get; } = [];
        public bool IsEnabled(LogLevel logLevel) => Enabled;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Records.Add((eventId.Id, formatter(state, exception)));
            if (state is IEnumerable<KeyValuePair<string, object?>> fields)
                foreach (var field in fields) Fields[field.Key] = field.Value;
        }
    }
}
