// snippet:complete
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BreakwaterLab;

// An explicit policy for this application's closed component set.
public sealed class StationPropertyPolicy : IComponentPropertyActivator
{
    public Action<IServiceProvider, IComponent> GetActivator(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type componentType)
    {
        if (componentType == typeof(StationPanel))
        {
            return static (services, component) =>
            {
                StationPanelBase panel = (StationPanelBase)component;
                panel.Station = services.GetRequiredService<StationName>();
                panel.Scale = services.GetRequiredKeyedService<Scale>("depth");
                panel.Session = services.GetRequiredService<SessionStamp>();
                panel.JS = services.GetRequiredService<IJSRuntime>();
            };
        }

        for (Type? current = componentType; current is not null; current = current.BaseType)
        {
            foreach (PropertyInfo property in current.GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (property.IsDefined(typeof(InjectAttribute), inherit: false))
                {
                    throw new NotSupportedException($"No property policy for {componentType.FullName}.{property.Name}.");
                }
            }
        }

        // Framework render-mode boundaries and App have no injected properties.
        return static (_, _) => { };
    }
}
// endsnippet:complete
