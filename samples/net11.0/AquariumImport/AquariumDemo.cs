// snippet:program
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Playbook.Samples;

public static class AquariumDemo
{
    // Program.cs: return await Playbook.Samples.AquariumDemo.RunAsync();
    public static async Task<int> RunAsync()
    {
        using ILoggerFactory logs = LoggerFactory.Create(logging => logging.AddSimpleConsole());
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var builder = AquariumSetup.CreateBuilder(new ImportFailures(), async token =>
        {
            await ready.Task.WaitAsync(token);
            throw new InvalidOperationException("Aquarium sensor batch failed after startup.");
        });
        IHost host = builder.Build();
        var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        using var registration = lifetime.ApplicationStarted.Register(() =>
        {
            Console.WriteLine("DEMO STARTED");
            ready.TrySetResult();
        });
        return await HostBoundary.RunOwnedAsync(host, logs.CreateLogger("Aquarium"));
    }
}
// endsnippet:program
