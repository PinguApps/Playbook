using Microsoft.Extensions.DependencyInjection;

namespace Playbook.Samples;

internal static class ServiceChecks
{
    public static async Task Run()
    {
#if NET11_0_OR_GREATER
        IServiceProvider services = EmptyServiceProvider.Instance;
        if (!ReferenceEquals(services, EmptyServiceProvider.Instance)) throw new Exception("Singleton changed.");
#else
        using ServiceProvider container = new ServiceCollection().BuildServiceProvider();
        IServiceProvider services = container;
#endif
        if (!MuseumRules.Preview("lunar-clock") || !MuseumRules.Allows("lunar-clock", services)) throw new Exception("Optional rule default failed.");
        IKeyedServiceProvider keyed = (IKeyedServiceProvider)services;
        IServiceProviderIsService query = services.GetRequiredService<IServiceProviderIsService>();
        IServiceProviderIsKeyedService keyedQuery = services.GetRequiredService<IServiceProviderIsKeyedService>();
        if (services.GetService(typeof(IExhibitRule)) is not null || keyed.GetKeyedService(typeof(IExhibitRule), "night") is not null ||
            query.IsService(typeof(IExhibitRule)) || keyedQuery.IsKeyedService(typeof(IExhibitRule), "night")) throw new Exception("Application service unexpectedly present.");
        Throws<InvalidOperationException>(() => services.GetRequiredService<IExhibitRule>());
        Throws<InvalidOperationException>(() => keyed.GetRequiredKeyedService(typeof(IExhibitRule), "night"));
        Throws<InvalidOperationException>(() => keyed.GetRequiredKeyedService(typeof(IExhibitRule), null));
        foreach (Type type in new[] { typeof(IServiceProvider), typeof(IServiceScopeFactory), typeof(IServiceProviderIsService), typeof(IServiceProviderIsKeyedService) })
        {
            object builtin = services.GetService(type) ?? throw new Exception("Missing built-in.");
#if NET11_0_OR_GREATER
            bool namedQuery = false;
#else
            // The old container reports these built-ins available even with a named key,
            // although resolving them with that key returns null. Do not assume parity.
            bool namedQuery = true;
#endif
            if (!query.IsService(type) || !keyedQuery.IsKeyedService(type, null) || keyedQuery.IsKeyedService(type, "night") != namedQuery ||
                !ReferenceEquals(builtin, keyed.GetKeyedService(type, null)) || keyed.GetKeyedService(type, "night") is not null) throw new Exception($"Built-in key/query contract failed for {type}.");
        }
        IServiceProvider resolvedProvider = services.GetRequiredService<IServiceProvider>();
        if (!ReferenceEquals(resolvedProvider, resolvedProvider.GetService(typeof(IServiceProvider)))) throw new Exception("Provider self-resolution failed.");
#if NET11_0_OR_GREATER
        if (!ReferenceEquals(services, resolvedProvider) || !ReferenceEquals(services, query) || !ReferenceEquals(services, keyedQuery))
            throw new Exception("Empty provider built-in identity failed.");
#endif
        if (query.IsService(typeof(IKeyedServiceProvider)) || services.GetService(typeof(IKeyedServiceProvider)) is not null)
            throw new Exception("Implemented keyed interface was incorrectly treated as a registration.");
        foreach (object? key in new object?[] { null, "night", KeyedService.AnyKey })
        {
            if (((IEnumerable<IExhibitRule>)keyed.GetRequiredKeyedService(typeof(IEnumerable<IExhibitRule>), key)).Any() ||
                ((IEnumerable<int>)keyed.GetRequiredKeyedService(typeof(IEnumerable<int>), key)).Any() ||
                !keyedQuery.IsKeyedService(typeof(IEnumerable<IExhibitRule>), key)) throw new Exception("Empty enumerable contract failed.");
        }
        if (services.GetServices<IExhibitRule>().Any() || !query.IsService(typeof(IEnumerable<int>))) throw new Exception("Unkeyed enumerable failed.");
        if (query.IsService(typeof(IEnumerable<>)) || keyedQuery.IsKeyedService(typeof(IEnumerable<>), "night") ||
            services.GetService(typeof(IEnumerable<>)) is not null || keyed.GetKeyedService(typeof(IEnumerable<>), "night") is not null) throw new Exception("Open generic resolved.");
        Throws<InvalidOperationException>(() => keyed.GetKeyedService(typeof(IExhibitRule), KeyedService.AnyKey));
        Throws<InvalidOperationException>(() => keyed.GetKeyedService(typeof(IServiceProvider), KeyedService.AnyKey));
        using (IServiceScope scope = services.CreateScope())
        {
            if (scope.ServiceProvider.GetService<IExhibitRule>() is not null || !MuseumRules.Allows("lunar-clock", scope.ServiceProvider)) throw new Exception("Scope failed.");
#if NET11_0_OR_GREATER
            if (!ReferenceEquals(services, scope.ServiceProvider)) throw new Exception("Empty scope should share provider.");
#endif
        }
        await using (AsyncServiceScope scope = services.CreateAsyncScope())
            if (scope.ServiceProvider.GetServices<IExhibitRule>().Any()) throw new Exception("Async scope failed.");
        if (!MuseumRules.Allows("lunar-clock", services)) throw new Exception("Scope disposal broke provider.");
        Console.WriteLine("PASS service interfaces: absent/required/keyed, built-ins/self, scopes/disposal, closed reference/value enumerables, open generics and AnyKey; managed JIT only");
    }

    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new Exception($"Expected {typeof(T).Name}.");
    }
}
