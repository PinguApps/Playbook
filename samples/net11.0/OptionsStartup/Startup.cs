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
        services.AddOptions<SailingOptions>().Configure(o => o.Crew = crew)
            .Validate<CrewValidator>().ValidateOnStart();
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

public static partial class StartupExamples
{
    private static void RegisterDeparture(IServiceCollection services, bool valid)
    {
        services.AddSingleton(new PermitRegister(valid));
        services.AddOptions<DepartureOptions>()
            .Validate<PermitRegister>((options, register, token) => register.CheckAsync(options.Permit, token),
                "Departure permit rejected.")
            .ValidateOnStart();
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
                DepartureOptions options = host.Services.GetRequiredService<IOptions<DepartureOptions>>().Value;
                if (!ReferenceEquals(options, host.Services.GetRequiredService<IOptionsMonitor<DepartureOptions>>().CurrentValue)) { throw new Exception("Startup did not seed both caches."); }
                using IServiceScope scope = host.Services.CreateScope();
                ExpectFailure(() => scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<DepartureOptions>>().Value);
                ExpectFailure(() => host.Services.GetRequiredService<IOptionsFactory<DepartureOptions>>().Create(Options.DefaultName));
                await host.StopAsync();
            }
            catch (OptionsValidationException) when (shouldFail) { }
        }

        await OptionsContractChecks.RunAsync();
        ServiceCollection early = new();
        RegisterDeparture(early, true);
        // A separate synchronous validator does not cancel the async-only validator's Fail result.
        early.AddOptions<DepartureOptions>().Validate(_ => true);
        using ServiceProvider provider = early.BuildServiceProvider();
        ExpectFailure(() => provider.GetRequiredService<IOptions<DepartureOptions>>().Value);
        Console.WriteLine("PASS async-only synchronous access fails before startup and in snapshot/factory after startup; both startup caches seeded");
    }

    private static void ExpectFailure(Func<DepartureOptions> action)
    {
        try { _ = action(); throw new Exception("Expected synchronous options access to fail."); }
        catch (OptionsValidationException) { }
    }
}
