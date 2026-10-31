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
                // Exception may be IO or a custom provider exception. Never assume a cast.
                report(context.Exception);
                context.Ignore = true; // Deliberate policy: a failed reload can clear values.
            }
        });

    internal static void Load(JsonConfigurationProvider provider, Action<Exception> report)
        => provider.Load(); // File-open failures now reach the source callback.
}
// endsnippet:policy
