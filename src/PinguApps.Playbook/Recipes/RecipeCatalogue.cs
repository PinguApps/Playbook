using System.Text.Json;
using ModelContextProtocol;

namespace PinguApps.Playbook.Recipes;

internal sealed class RecipeCatalogue
{
    private readonly RecipeEdition[] _editions;
    private readonly string[] _targets;

    public RecipeCatalogue()
    {
        using Stream stream = typeof(RecipeCatalogue).Assembly.GetManifestResourceStream("Playbook.Catalogue")
            ?? throw new InvalidOperationException("Embedded catalogue is missing.");
        _editions = JsonSerializer.Deserialize<RecipeEdition[]>(stream, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Embedded catalogue is empty.");
        _targets = [.. _editions.SelectMany(edition => edition.SupportedTargets)
            .Distinct(StringComparer.Ordinal).OrderBy(Version.Parse)];
    }

    public RecipeList List(string? targetVersion)
    {
        string target = ResolveTarget(targetVersion);
        RecipeSummary[] summaries = [.. _editions
            .Where(edition => edition.SupportedTargets.Contains(target, StringComparer.Ordinal))
            .GroupBy(edition => edition.Id, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => group.OrderByDescending(edition => Version.Parse(edition.Edition)).First())
            .Select(edition => new RecipeSummary
            {
                Id = edition.Id,
                Edition = edition.Edition,
                Title = edition.Title,
                Goal = edition.Goal,
                Areas = edition.Areas,
                ExampleLanguages = edition.ExampleLanguages,
                SupportedTargets = edition.SupportedTargets,
                Stability = edition.Stability,
                Applicability = edition.Applicability,
            })];
        return new RecipeList
        {
            ResolvedTarget = target,
            TargetAssumed = targetVersion is null,
            SupportedTargets = _targets,
            Recipes = summaries,
        };
    }

    public RecipeDetail Get(string id, string? targetVersion)
    {
        string target = ResolveTarget(targetVersion);
        RecipeEdition edition = _editions
            .Where(candidate => candidate.Id == id && candidate.SupportedTargets.Contains(target, StringComparer.Ordinal))
            .OrderByDescending(candidate => Version.Parse(candidate.Edition))
            .FirstOrDefault() ?? throw new McpException($"No edition of '{id}' declares target '{target}'. Use list_recipes to discover installed guidance.");
        return new RecipeDetail { ResolvedTarget = target, TargetAssumed = targetVersion is null, Recipe = edition };
    }

    private string ResolveTarget(string? targetVersion)
    {
        string target = targetVersion ?? _targets[^1];
        return _targets.Contains(target, StringComparer.Ordinal)
            ? target
            : throw new McpException($"Unsupported target '{target}'. Declared targets: {string.Join(", ", _targets)}.");
    }
}
