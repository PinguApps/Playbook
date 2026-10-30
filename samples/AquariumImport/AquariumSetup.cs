// snippet:setup
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Playbook.Samples;

public static class AquariumSetup
{
    public static HostApplicationBuilder CreateBuilder(ImportFailures failures,
        Func<CancellationToken, Task> import)
    {
        // The standard Generic Host implementation; no config files/environment loading needed here.
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { DisableDefaults = true });
        builder.Logging.AddSimpleConsole();
        builder.Services.Configure<HostOptions>(options =>
            options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost);
        builder.Services.AddSingleton(failures);
        builder.Services.AddSingleton<IHostedService>(services =>
            new AquariumWorker(services.GetRequiredService<ImportFailures>(), import));
        return builder;
    }
}
// endsnippet:setup
