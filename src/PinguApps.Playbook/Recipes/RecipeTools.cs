using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace PinguApps.Playbook.Recipes;

[McpServerToolType]
internal sealed class RecipeTools
{
    private readonly RecipeCatalogue _catalogue;

    public RecipeTools(RecipeCatalogue catalogue)
    {
        _catalogue = catalogue;
    }

    [McpServerTool(Name = "list_recipes", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false, UseStructuredContent = true, OutputSchemaType = typeof(RecipeList))]
    [Description("List concise installed recipe summaries and their requirements. Inspect the application's TargetFramework/TargetFrameworks and relevant project/global.json context; repeat targetVersion on get_recipe. No consumer inspection or network access.")]
    public CallToolResult ListRecipes(
        [Description("Required target release: 10/10.0/net10.0 or 11/11.0/net11.0; not an exact SDK version. Use latest deliberately for exploratory/theoretical guidance or an unknown target: newest installed catalogue release only, not proof of consumer compatibility. Missing/null/blank, patch, RC and platform-qualified values fail; check prerequisites separately.")] string targetVersion,
        [Description("Area identifiers from supportedAreas. Omitted/empty means all; multiple values match any. Different filter types combine with AND.")] string[]? areas = null,
        [Description("Language identifiers from supportedExampleLanguages. Match any demonstrated language or explicitly applicable language-neutral configuration. Omitted/empty means all.")] string[]? languages = null,
        [Description("All nonblank whitespace-separated terms must match title, goal, tags or API names, case-insensitively. Blank means no keyword filter.")] string? query = null,
        [Description("Include explicitly applicable shared guidance when filtering areas. Does not bypass other filters or platform conditions. Defaults true; false matches directly tagged areas only.")] bool includeShared = true,
        [Description("Nonnegative offset into ordinal stable-ID order. Defaults 0; beyond the total succeeds empty.")] int offset = 0,
        [Description("Page size from 1 to 50, default 20. Use nextOffset to continue with the same filters.")] int limit = 20)
    {
        return Invoke(() => _catalogue.List(targetVersion, areas, languages, query, includeShared, offset, limit));
    }

    [McpServerTool(Name = "get_recipe", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false, UseStructuredContent = true, OutputSchemaType = typeof(RecipeDetail))]
    [Description("Get complete offline Markdown, application requirements, optional alternative routes, official sources and exact verification baselines (not installation requirements) for a stable recipe ID. Inspect the application's TargetFramework/TargetFrameworks and relevant project/global.json context; repeat the chosen targetVersion independently of list_recipes.")]
    public CallToolResult GetRecipe(
        [Description("Stable goal-based recipe ID from list_recipes.")] string id,
        [Description("Required target release: 10/10.0/net10.0 or 11/11.0/net11.0; not an exact SDK version. Use latest deliberately for exploratory/theoretical guidance or an unknown target: newest installed catalogue release only, not proof of consumer compatibility. Missing/null/blank, patch, RC and platform-qualified values fail; check prerequisites separately.")] string targetVersion)
    {
        return Invoke(() => _catalogue.Get(id, targetVersion));
    }

    private static CallToolResult Invoke<T>(Func<T> operation)
    {
        try
        {
            JsonElement content = JsonSerializer.SerializeToElement(operation(), JsonSerializerOptions.Web);
            return new CallToolResult
            {
                StructuredContent = content,
                Content = [new TextContentBlock { Text = content.GetRawText() }],
            };
        }
        catch (McpException exception)
        {
            return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = exception.Message }] };
        }
    }
}
