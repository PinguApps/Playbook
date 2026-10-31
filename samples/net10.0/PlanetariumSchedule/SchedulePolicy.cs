// snippet:policy
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;

internal static class SchedulePolicy
{
    internal static JsonConfigurationProvider Create(IFileProvider files, Action<Exception> report)
        => new(new JsonConfigurationSource
        {
            FileProvider = files, Path = "display.json", Optional = false,
            ReloadOnChange = true, ReloadDelay = 100,
            OnLoadException = context =>
            {
                report(context.Exception);
                context.Ignore = true; // Deliberate availability policy, not successful loading.
            }
        });

    internal static void Load(JsonConfigurationProvider provider, Action<Exception> report)
    {
        // In .NET 10, OnLoadException does not receive file-open failures.
        try { provider.Load(); }
        catch (IOException failure) { report(failure); }
    }
}
// endsnippet:policy
