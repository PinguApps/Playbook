using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Playbook.Samples;

// snippet:dual
public sealed class PortOptions { public int Quay { get; set; } = 2; }
public sealed class PortValidator : IAsyncValidateOptions<PortOptions>
{
    // Both contracts are required in RC2. Synchronous checks must remain meaningful.
    public ValidateOptionsResult Validate(string? name, PortOptions options) => name != "cargo"
        ? ValidateOptionsResult.Skip : options.Quay > 0
            ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail("A quay is required.");
    public async Task<ValidateOptionsResult> ValidateAsync(string? name, PortOptions options, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        return Validate(name, options);
    }
}

public sealed class PortStartup : IAsyncStartupValidator
{
    public bool Completed { get; private set; }
    public async Task ValidateAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        Completed = true;
    }
}
// endsnippet:dual

// snippet:generated
public sealed class CrewManifest { [Range(1, 6)] public int Crew { get; set; } = 3; }
[OptionsValidator]
public partial class ManifestValidator : IValidateOptions<CrewManifest>
{
#if EXPLICIT_VALIDATE
    // Deliberately failing build: choose generated OR handwritten synchronous ownership.
    ValidateOptionsResult IValidateOptions<CrewManifest>.Validate(string? name, CrewManifest options) => ValidateOptionsResult.Success;
#endif
}
// endsnippet:generated

internal static class OptionsContractChecks
{
    private static void CheckSynchronousNames()
    {
        PortValidator dual = new();
        if (!dual.Validate("passenger", new PortOptions { Quay = -1 }).Skipped
            || !dual.Validate("cargo", new PortOptions { Quay = -1 }).Failed)
        { throw new Exception("Named synchronous validation must be meaningful and name-aware."); }
    }

    public static async Task RunAsync()
    {
        using ConfigurationManager configuration = new();
        configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["Departure:Permit"] = "harbour-17" });
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        PortStartup gate = new();
        builder.Services.AddSingleton<IAsyncStartupValidator>(gate);
        builder.Services.AddOptions<PortOptions>("cargo").Validate<PortValidator>().ValidateOnStart();
        builder.Services.AddOptions<DepartureOptions>().Bind(configuration.GetSection("Departure"))
            .Validate(async (options, token) => { await Task.Delay(1, token); return options.Permit == "harbour-17"; }, "Permit rejected.")
            .ValidateOnStart();
        using IHost host = builder.Build();
        await host.StartAsync();
        if (!gate.Completed || host.Services.GetRequiredService<IOptionsMonitor<PortOptions>>().Get("cargo").Quay != 2)
        { throw new Exception("Async startup or named validation was skipped."); }
        CheckSynchronousNames();
        IOptionsMonitor<DepartureOptions> monitor = host.Services.GetRequiredService<IOptionsMonitor<DepartureOptions>>();
        int notifications = 0;
        using IDisposable? subscription = monitor.OnChange(_ => notifications++);
        configuration["Departure:Permit"] = "changed";
        try { ((IConfigurationRoot)configuration).Reload(); throw new Exception("Expected synchronous reload rejection."); }
        catch (AggregateException error) when (error.Flatten().InnerExceptions.Any(e => e is OptionsValidationException)) { }
        if (notifications != 0) { throw new Exception("Invalid reload notified listeners."); }
        try { _ = monitor.CurrentValue; throw new Exception("Faulted monitor cache unexpectedly recovered."); }
        catch (OptionsValidationException) { }
        await host.StopAsync();
        IValidateOptions<CrewManifest> generated = new ManifestValidator();
        if (!generated.Validate(null, new CrewManifest { Crew = 9 }).Failed) { throw new Exception("Generated validation was absent."); }
        Console.WriteLine("PASS custom IAsyncStartupValidator, dual named validator, generated range rejection and synchronous monitor reload failure with zero notifications");
    }
}
