// snippet:reef-host
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Validation;

namespace Playbook.Samples;

internal static class ReefExample
{
    public static async Task<ValidateContext> CheckAsync(TankReport report, CancellationToken token)
    {
        var services = new ServiceCollection();
        services.AddSingleton(new MaintenanceLedger());
        services.AddValidation(o => o.Resolvers.Add(new ReefResolver()));
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ValidationOptions>>().Value;
        var context = ReefImport.Context(provider, options);
        if (!options.TryGetValidatableTypeInfo(typeof(TankReport), out var metadata))
            throw new InvalidOperationException("Missing report metadata.");
        await metadata.ValidateAsync(report, context, token);
        return context;
    }
}
// endsnippet:reef-host
