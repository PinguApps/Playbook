using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PinguApps.Playbook.Recipes;

namespace PinguApps.Playbook;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args is [] or ["--help"] or ["-h"])
        {
            Console.WriteLine("Playbook — offline .NET recipes\n\nUsage: playbook mcp | --help | --version\n\nmcp starts a client-owned stdio MCP server.");
            return 0;
        }

        if (args is ["--version"])
        {
            Console.WriteLine(typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion);
            return 0;
        }

        if (args is not ["mcp"])
        {
            await Console.Error.WriteLineAsync("Unknown command. Use playbook --help.").ConfigureAwait(false);
            return 1;
        }

        HostApplicationBuilder builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = [],
            DisableDefaults = true,
        });
        builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
        builder.Services.AddSingleton<RecipeCatalogue>();
        builder.Services.AddMcpServer().WithStdioServerTransport().WithTools<RecipeTools>();
        using IHost host = builder.Build();
        await host.RunAsync().ConfigureAwait(false);
        return 0;
    }
}
