using System.ComponentModel;
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

    [McpServerTool(Name = "list_recipes", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("List concise installed recipe summaries and their requirements. No consumer project inspection or network access.")]
    public RecipeList ListRecipes([Description("Declared target release, for example 10.0. Omit to use the newest installed target.")] string? targetVersion = null)
    {
        return _catalogue.List(targetVersion);
    }

    [McpServerTool(Name = "get_recipe", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Get complete offline Markdown, applicability, official sources and verification evidence for a stable recipe ID.")]
    public RecipeDetail GetRecipe(
        [Description("Stable goal-based recipe ID from list_recipes.")] string id,
        [Description("Declared target release. An explicit target is never silently replaced.")] string? targetVersion = null)
    {
        return _catalogue.Get(id, targetVersion);
    }
}
