using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
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
        builder.Services.AddMcpServer(options => options.ServerInstructions =
            "Before applying .NET guidance, inspect the application's TargetFramework/TargetFrameworks and relevant project files, Directory.Build.props and global.json. "
            + "Choose the application's target release, not the exact SDK version; global.json selects an SDK and does not establish the target framework. "
            + "Supply targetVersion explicitly on both list_recipes and get_recipe, repeating it on every call and page; calls do not share a target. "
            + "Use latest deliberately for exploratory/theoretical requests or an unknown target: it resolves numerically to the newest target release represented in this installed catalogue, without consulting the network or installed SDKs. "
            + "latest does not establish consumer compatibility. Discovery contains compact selection conditions, not complete prerequisites or evidence. Before applying any recipe, retrieve get_recipe and check each route's full requirements and limitations against the application. Primary routes and optional alternatives are separate choices. "
            + "Evidence records are exact verification baselines, not mandatory installation versions; RC evidence is not GA verification. Then apply and verify the guidance.")
            .WithStdioServerTransport().WithTools<RecipeTools>()
            .WithRequestFilters(filters => filters.AddCallToolFilter(next => async (request, cancellationToken) =>
            {
                // The SDK rejects missing required arguments before invoking the tool, hiding its helpful validation.
                if (request.Params is { Name: "list_recipes" or "get_recipe" } parameters
                    && (parameters.Arguments is null || !parameters.Arguments.ContainsKey("targetVersion")))
                {
                    return new CallToolResult
                    {
                        IsError = true,
                        Content = [new TextContentBlock
                        {
                            Text = JsonSerializer.Serialize(new
                            {
                                code = "required_target",
                                message = "targetVersion is required on every list_recipes and get_recipe call. Supply a supported target release or explicitly choose latest; inspect consumer prerequisites separately.",
                            }),
                        }],
                    };
                }

                return await next(request, cancellationToken).ConfigureAwait(false);
            }));
        using IHost host = builder.Build();
        await host.RunAsync().ConfigureAwait(false);
        return 0;
    }
}
