// snippet:publisher
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Foundry.Staging;

internal static class Publisher
{
    public static SourceRepository Open(string index, bool allowLoopbackHttp = false)
    {
        CheckLoopbackOptIn(new Uri(index), allowLoopbackHttp);
        var source = new PackageSource(index) { AllowInsecureConnections = allowLoopbackHttp };
        return new SourceRepository(source, Repository.Provider.GetCoreV3());
    }

    private static void CheckLoopbackOptIn(Uri endpoint, bool allowed)
    {
        if (allowed && (endpoint.Scheme != "http" || endpoint.Host != "127.0.0.1"))
            throw new InvalidOperationException("The HTTP opt-in is only for the disposable 127.0.0.1 fixture.");
    }

    internal static void CheckAdvertisedEndpoint(Uri index, Uri endpoint, bool allowLoopbackHttp)
    {
        CheckLoopbackOptIn(endpoint, allowLoopbackHttp);
        // This publisher authorizes only the index's origin, with either flag value.
        // Cross-origin providers need a separately reviewed endpoint policy.
        if (index.Scheme != endpoint.Scheme || index.Port != endpoint.Port
            || !string.Equals(index.Host, endpoint.Host, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The advertised upload endpoint is outside the approved service-index origin.");
    }

#if NET11_0
    public static async Task Stage(SourceRepository repository, string package, string symbols,
        string? apiKey, string? groupId, bool allowLoopbackHttp, CancellationToken cancellationToken)
    {
        // The provider discovers PackageStaging/1.0.0 in the real service index.
        // No internal constructor, substituted resource or guessed publishing URL.
        var resource = await repository.GetResourceAsync<PackageStagingResourceV3>(cancellationToken)
            ?? throw new InvalidOperationException("This source advertises no supported staging resource.");
        CheckAdvertisedEndpoint(new Uri(repository.PackageSource.Source), resource.SourceUri, allowLoopbackHttp);
        await resource.PushPackageAsync(package, apiKey, groupId, TimeSpan.FromSeconds(30),
            allowLoopbackHttp, NullLogger.Instance, cancellationToken);
        await resource.PushSymbolsAsync(symbols, apiKey, groupId, TimeSpan.FromSeconds(30),
            allowLoopbackHttp, NullLogger.Instance, cancellationToken);
    }
#else
    // SDK10 has no staging resource. Ordinary push remains a valid final-publication
    // alternative only for an endpoint explicitly advertising PackagePublish/2.0.0.
    // It does not stage, support groupId, or promise transactional publication.
    public static async Task Publish(SourceRepository repository, string package,
        string fixtureKey, CancellationToken cancellationToken, bool allowLoopbackHttp = false)
    {
        var update = await repository.GetResourceAsync<PackageUpdateResource>(cancellationToken)
            ?? throw new InvalidOperationException("This source offers no package publication resource.");
        CheckAdvertisedEndpoint(new Uri(repository.PackageSource.Source), update.SourceUri, allowLoopbackHttp);
        await update.Push([package], null, 30, false, _ => fixtureKey, _ => fixtureKey,
            true, false, null, allowLoopbackHttp, NullLogger.Instance);
    }
#endif
}
// endsnippet:publisher
