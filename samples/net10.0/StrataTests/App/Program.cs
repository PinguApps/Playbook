// snippet:complete
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace StrataApi;

public sealed class Program
{
    private Program() { }
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        // The early factory hook has already run here. Respect test/provider values.
        builder.Configuration["Strata:Source"] ??= "production";
        string startupSource = builder.Configuration["Strata:Source"]!;
        builder.Services.TryAddSingleton<CoreCatalog>();
        var app = builder.Build();
        app.MapGet("/strata", (CoreCatalog catalog, IConfiguration configuration) =>
            new { startupSource, currentSource = configuration["Strata:Source"], catalog = catalog.Name });
        app.Run();
    }
}

public sealed class CoreCatalog(string name = "production-core") : IAsyncDisposable
{
    public string Name { get; } = name;
    public bool Disposed { get; private set; }
    public async ValueTask DisposeAsync()
    {
        await Task.Yield();
        Disposed = true;
    }
}
// endsnippet:complete
