// snippet:museum-rules
using Microsoft.Extensions.DependencyInjection;

namespace Playbook.Samples;

internal interface IExhibitRule { bool Allows(string exhibit); }

internal static class MuseumRules
{
    public static bool Allows(string exhibit, IServiceProvider services) =>
        services.GetService<IExhibitRule>()?.Allows(exhibit) ?? true;

    public static bool Preview(string exhibit)
    {
        using ServiceProvider services = new ServiceCollection().BuildServiceProvider();
        return Allows(exhibit, services);
    }
}
// endsnippet:museum-rules
