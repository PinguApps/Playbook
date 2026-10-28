// snippet:complete
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
namespace ReedArchive;
public static class ArchiveServices
{
    public static IServiceCollection AddArchive(this IServiceCollection services)
    {
        services.AddScoped<ArchiveState>();
        services.AddCascadingValue(provider => provider.GetRequiredService<ArchiveState>().LettersSource);
        services.AddCascadingValue(provider => provider.GetRequiredService<ArchiveState>().MapsSource);
        return services;
    }
}
// endsnippet:complete
