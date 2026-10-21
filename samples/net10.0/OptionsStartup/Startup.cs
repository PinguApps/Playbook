using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Playbook.Samples;

// snippet:di
public sealed class SailingOptions { public int Crew { get; set; } = 3; }
public sealed class VesselRules { public int MaximumCrew => 6; }
public sealed class CrewValidator(VesselRules rules) : IValidateOptions<SailingOptions>
{
    public ValidateOptionsResult Validate(string? name, SailingOptions options) =>
        options.Crew is > 0 && options.Crew <= rules.MaximumCrew
            ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail("Crew exceeds vessel capacity.");
}

public static partial class StartupExamples
{
    private static void RegisterCrew(IServiceCollection services, int crew)
    {
        services.AddSingleton<VesselRules>();
        services.AddSingleton<IValidateOptions<SailingOptions>, CrewValidator>();
        services.AddOptions<SailingOptions>().Configure(o => o.Crew = crew).ValidateOnStart();
    }
}
// endsnippet:di

// snippet:async
public sealed class DepartureOptions { public string Permit { get; set; } = "harbour-17"; }
public sealed class PermitRegister(bool valid)
{
    // An in-process asynchronous policy service; no external registry is claimed.
    public async Task<bool> CheckAsync(string permit, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);
        return valid && permit == "harbour-17";
    }
}

public sealed class DepartureStartup(IOptions<DepartureOptions> options, PermitRegister register) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!await register.CheckAsync(options.Value.Permit, cancellationToken))
        {
            throw new OptionsValidationException(Options.DefaultName, typeof(DepartureOptions), ["Departure permit rejected."]);
        }
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

public static partial class StartupExamples
{
    private static void RegisterDeparture(IServiceCollection services, bool valid)
    {
        services.AddSingleton(new PermitRegister(valid));
        services.AddOptions<DepartureOptions>();
        // Keep this gate before any hosted service that accepts work.
        services.AddHostedService<DepartureStartup>();
    }
}
// endsnippet:async

public static partial class StartupExamples
{
    public static async Task RunAsync()
    {
        foreach ((int crew, bool valid, bool shouldFail) in new[] { (3, true, false), (9, true, true), (3, false, true) })
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder();
            RegisterCrew(builder.Services, crew);
            RegisterDeparture(builder.Services, valid);
            using IHost host = builder.Build();
            try
            {
                await host.StartAsync();
                if (shouldFail) { throw new Exception("Invalid startup succeeded."); }
                if (host.Services.GetRequiredService<IOptions<DepartureOptions>>().Value.Permit != "harbour-17") { throw new Exception("Invalid permit."); }
                await host.StopAsync();
            }
            catch (OptionsValidationException) when (shouldFail) { }
        }
    }
}
