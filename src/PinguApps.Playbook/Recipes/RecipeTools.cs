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
    [Description("List concise installed recipe summaries and their requirements. No consumer project inspection or network access.")]
    public CallToolResult ListRecipes(
        [Description("Release: 10/10.0/net10.0 or 11/11.0/net11.0. Omit for the newest installed target, marked assumed. Patch, RC and platform-qualified strings fail; inspect returned prerequisites separately.")] string? targetVersion = null,
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
    [Description("Get complete offline Markdown, applicability, official sources and verification evidence for a stable recipe ID.")]
    public CallToolResult GetRecipe(
        [Description("Stable goal-based recipe ID from list_recipes.")] string id,
        [Description("Release: 10/10.0/net10.0 or 11/11.0/net11.0. Omit for the newest installed target, marked assumed. An explicit target is never silently replaced; conditions are disclosed, not inspected.")] string? targetVersion = null)
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
