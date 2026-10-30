using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Playbook.Samples;

internal static class AquariumChecks
{
    private static async Task<int> Main(string[] args)
    {
        if (args.Length > 0) return args[0] == "demo" ? await AquariumDemo.RunAsync() : await Child(args[0], args[1]);
        Console.WriteLine($"runtime={RuntimeInformation.FrameworkDescription}; os={RuntimeInformation.OSDescription}; arch={RuntimeInformation.ProcessArchitecture}");
        Console.WriteLine($"hosting={typeof(Host).Assembly.FullName}; location={typeof(Host).Assembly.Location}");
        Require(!new[] { typeof(Host), typeof(BackgroundService), typeof(HostOptions), typeof(HostingAbstractionsHostExtensions) }.SelectMany(type => type.GetCustomAttributesData().Concat(type.GetMethods().SelectMany(method => method.GetCustomAttributesData()))).Any(attribute => attribute.AttributeType.FullName == "System.Diagnostics.CodeAnalysis.ExperimentalAttribute"), "Selected hosting APIs unexpectedly experimental.");
        Console.WriteLine("METADATA: selected hosting APIs have no Experimental attribute.");
        foreach (string route in new[] { "runAsync", "run", "explicit", "waitAsync", "wait", "timedStop", "boundary", "demo" })
        foreach (string scenario in route == "demo" ? ["one"] : new[] { "one", "two", "shutdown", "cancel", "ignore" })
        {
            var start = new ProcessStartInfo(Environment.ProcessPath!)
            {
                UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true
            };
            if (string.Equals(Path.GetFileNameWithoutExtension(Environment.ProcessPath), "dotnet", StringComparison.OrdinalIgnoreCase))
                start.ArgumentList.Add(Assembly.GetExecutingAssembly().Location);
            start.ArgumentList.Add(route);
            start.ArgumentList.Add(scenario);
            using var child = Process.Start(start) ?? throw new InvalidOperationException("Child did not start.");
            Task<string> stdout = child.StandardOutput.ReadToEndAsync();
            Task<string> stderr = child.StandardError.ReadToEndAsync();
            try
            {
                using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                await child.WaitForExitAsync(deadline.Token);
                string output = await stdout;
                string errors = await stderr;
                Console.WriteLine($"CHILD route={route}; scenario={scenario}; exit={child.ExitCode}\n{output}");
                Require(errors.Length == 0, $"Unexpected child stderr: {errors}");
                if (route == "demo")
                {
                    Require(child.ExitCode == 17 && output.Contains("DEMO STARTED", StringComparison.Ordinal), "Canonical demo did not preserve failure.");
                    continue;
                }
                bool fault = scenario is "one" or "two" or "shutdown";
                bool propagates = Environment.Version.Major >= 11 || scenario == "shutdown" || route == "boundary";
                int expected = fault && propagates ? 17 : 0;
                Require(child.ExitCode == expected, $"Unexpected exit {route}/{scenario}: {child.ExitCode}, expected {expected}.");
                Require(output.Contains("STARTED", StringComparison.Ordinal) && output.Contains("DISPOSED=1", StringComparison.Ordinal), "Missing startup/disposal proof.");
                Require(output.Contains("IDENTITIES=PASS", StringComparison.Ordinal), "Missing exact exception identity checks.");
                if (scenario == "ignore") Require(output.Contains("IGNORE stillRunning=True", StringComparison.Ordinal), "Ignore shut down automatically.");
            }
            finally
            {
                if (!child.HasExited) { child.Kill(entireProcessTree: true); await child.WaitForExitAsync(); }
            }
        }
        await VerifyStartupFailure();
        Console.WriteLine("PASS: 35 lifecycle children plus canonical demo; host boundaries, original exception identities, separate logs/exits, cancellation, Ignore and disposal; all children reaped.");
        return 0;
    }

    private static async Task<int> Child(string route, string scenario)
    {
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(12));
        var state = new ImportState(scenario);
        HostApplicationBuilder builder = AquariumSetup.CreateBuilder(state.Journal, _ => Task.CompletedTask);
        // Replace the single production registration with disposal-observed instances of the same worker.
        builder.Services.RemoveAll<IHostedService>();
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new FailureLog(state));
        builder.Services.Configure<HostOptions>(options =>
        {
            options.BackgroundServiceExceptionBehavior = scenario == "ignore" ? BackgroundServiceExceptionBehavior.Ignore : BackgroundServiceExceptionBehavior.StopHost;
            options.ShutdownTimeout = TimeSpan.FromSeconds(8);
        });
        builder.Services.AddSingleton(state);
        builder.Services.AddSingleton<IHostedService>(services => new SensorImporter(services.GetRequiredService<ImportState>(), state.First));
        if (scenario == "two") builder.Services.AddSingleton<IHostedService>(services => new SensorImporter(services.GetRequiredService<ImportState>(), state.Second));
        if (scenario == "shutdown") builder.Services.AddSingleton<IHostedService, ShutdownFailure>();
        IHost host = builder.Build();
        IHostApplicationLifetime lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        using var startedRegistration = lifetime.ApplicationStarted.Register(() => state.Started.TrySetResult());
        using var stoppingRegistration = lifetime.ApplicationStopping.Register(() => state.Stopping.TrySetResult());
        Task driver = Task.Run(async () =>
        {
            await state.Started.Task.WaitAsync(deadline.Token);
            Console.WriteLine("STARTED");
            if (scenario == "cancel") { lifetime.StopApplication(); return; }
            state.Release.TrySetResult();
            if (scenario == "ignore")
            {
                await state.Logged.Task.WaitAsync(deadline.Token);
                bool stillRunning = !lifetime.ApplicationStopping.IsCancellationRequested;
                Console.WriteLine($"IGNORE stillRunning={stillRunning}");
                Require(stillRunning, "Ignore unexpectedly stopped host.");
                lifetime.StopApplication();
            }
            await state.Stopping.Task.WaitAsync(deadline.Token);
        });
        Exception? caught = null;
        int exit = 0;
        // Run/RunAsync own disposal; explicit start/stop/wait callers own it.
        bool runOwns = route is "runAsync" or "run" or "boundary";
        try
        {
            if (route == "boundary")
            {
                exit = await HostBoundary.RunOwnedAsync(host, new BoundaryLog(ex => caught = ex), deadline.Token);
            }
            else if (route == "runAsync") await host.RunAsync(deadline.Token);
            // Exercise the advertised synchronous wrappers deliberately in isolated children.
#pragma warning disable CA1849
            else if (route == "run") host.Run();
            else
            {
                await host.StartAsync(deadline.Token);
                Console.WriteLine("StartAsync returned successfully");
                if (route == "waitAsync") await host.WaitForShutdownAsync(deadline.Token);
                else if (route == "wait") host.WaitForShutdown();
#pragma warning restore CA1849
                else
                {
                    await state.Stopping.Task.WaitAsync(deadline.Token);
                    if (route == "timedStop") await host.StopAsync(TimeSpan.FromSeconds(8));
                    else await host.StopAsync(deadline.Token);
                }
            }
        }
        catch (Exception ex) { caught = ex; exit = 17; }
        finally { if (!runOwns) host.Dispose(); }
        await driver;
        Exception[] actual = caught is AggregateException aggregate ? aggregate.Flatten().InnerExceptions.ToArray() : caught is null ? [] : [caught];
        bool propagates = Environment.Version.Major >= 11 || route == "boundary";
        Exception[] expected = scenario switch
        {
            "one" when propagates => [state.First],
            "two" when propagates => [state.First, state.Second],
            "shutdown" when propagates => [state.First, state.StopFailure],
            "shutdown" => [state.StopFailure],
            _ => []
        };
        Require(actual.Length == expected.Length && expected.All(wanted => actual.Any(item => ReferenceEquals(item, wanted))), "Thrown identity/aggregation mismatch.");
        Require(expected.Length <= 1 || caught is AggregateException, "Multiple failures were not aggregated.");
        int expectedLogs = scenario == "cancel" ? 0 : scenario == "two" ? 2 : 1;
        Require(state.Failures.Count == expectedLogs, "Wrong BackgroundService failure log count.");
        Require(state.Failures.All(ex => ReferenceEquals(ex, state.First) || ReferenceEquals(ex, state.Second)), "Logged identity changed.");
        Require(state.Disposed == (scenario == "two" ? 2 : 1), "Wrong service disposal count.");
        Console.WriteLine($"THROWN={caught?.GetType().FullName ?? "none"}; inner={string.Join(',', actual.Select(ex => ex.Message))}; logged={state.Failures.Count}; exitPolicy={exit}; DISPOSED=1; IDENTITIES=PASS");
        return exit;
    }

    private static async Task VerifyStartupFailure()
    {
        var failure = new InvalidOperationException("aquarium-startup");
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { DisableDefaults = true });
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton<IHostedService>(new StartupFailure(failure));
        using IHost host = builder.Build();
        try { await host.StartAsync(); }
        catch (Exception ex) when (ReferenceEquals(ex, failure)) { Console.WriteLine("STARTUP: StartAsync throws original failure on this release."); return; }
        throw new InvalidOperationException("StartAsync failed to throw startup failure.");
    }

    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}

internal sealed class ImportState(string scenario)
{
    public string Scenario { get; } = scenario;
    public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Stopping { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource Logged { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public Exception First { get; } = new SensorImportException("reef-sensor-A");
    public Exception Second { get; } = new SensorImportException("reef-sensor-B");
    public Exception StopFailure { get; } = new InvalidOperationException("aquarium-shutdown");
    public ImportFailures Journal { get; } = new();
    public System.Collections.Concurrent.ConcurrentBag<Exception> Failures { get; } = [];
    private int disposed;
    public int Disposed => disposed;
    public void RecordDisposal() => Interlocked.Increment(ref disposed);
}

internal sealed class SensorImporter(ImportState state, Exception failure) : AquariumWorker(state.Journal, async token =>
{
    await state.Release.Task.WaitAsync(token);
    throw failure;
})
{
    public override void Dispose()
    {
        state.RecordDisposal();
        base.Dispose();
    }
}

internal sealed class SensorImportException : Exception
{
    public SensorImportException() { }
    public SensorImportException(string message) : base(message) { }
    public SensorImportException(string message, Exception innerException) : base(message, innerException) { }
}
internal sealed class ShutdownFailure(ImportState state) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.FromException(state.StopFailure);
}
internal sealed class StartupFailure(Exception failure) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => Task.FromException(failure);
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
internal sealed class FailureLog(ImportState state) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new BoundaryLog(ex =>
    {
        // Event 9 is the host's BackgroundServiceFaulted diagnostic, not a mock host.
        if (ex is SensorImportException)
        {
            state.Failures.Add(ex);
            state.Logged.TrySetResult();
        }
    }, 9);
    public void Dispose() { }
}
internal sealed class BoundaryLog(Action<Exception> observe, int? selectedEvent = null) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (exception is not null && (selectedEvent is null || eventId.Id == selectedEvent))
        {
            observe(exception);
            Console.WriteLine($"LOG event={eventId.Id}; type={exception.GetType().FullName}; message={exception.Message}");
        }
    }
}
