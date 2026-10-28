// snippet:complete
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace ReedArchive;

[AttributeUsage(AttributeTargets.Property)]
public sealed class FromArchiveAttribute(string collection) : CascadingParameterAttributeBase
{
    public string Collection { get; } = collection;
}

public static class ArchiveServices
{
    public static IServiceCollection AddArchive(this IServiceCollection services)
    {
        services.AddScoped<ArchiveState>();
        services.TryAddCascadingValueSupplier<FromArchiveAttribute>(provider =>
        {
            var state = provider.GetRequiredService<ArchiveState>();
            return (subscriber, attribute, parameter) =>
            {
                if (parameter.PropertyType != typeof(CollectionContext))
                    throw new InvalidOperationException("FromArchive requires CollectionContext.");
                var context = attribute.Collection switch
                {
                    "letters" => state.Letters,
                    "maps" => state.Maps,
                    _ => throw new InvalidOperationException("Unknown archive collection.")
                };
                return new ArchiveSubscription(context, subscriber);
            };
        });
        return services;
    }
}

public sealed class ArchiveSubscription : CascadingParameterSubscription
{
    private readonly CollectionContext context;
    private readonly Func<Task> refresh;
    private bool disposed;

    // ComponentState remains a framework seam even when this SDK emits no BL0006.
    public ArchiveSubscription(CollectionContext context, ComponentState subscriber)
    {
        this.context = context;
        refresh = subscriber.Component is IArchiveSubscriber receiver
            ? receiver.RefreshArchiveAsync
            : throw new InvalidOperationException("Archive recipient must implement IArchiveSubscriber.");
        context.Subscribe(refresh, Dispose);
        context.SubscriptionsCreated++;
        context.ActiveSubscribers++;
    }

    public override object GetCurrentValue() => context;

    public override void Dispose()
    {
        context.DisposeCalls++;
        if (disposed) return;
        disposed = true;
        context.Unsubscribe(refresh);
        context.ActiveSubscribers--;
    }
}
// endsnippet:complete
