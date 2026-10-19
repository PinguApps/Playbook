namespace PinguApps.Playbook.Recipes;

internal sealed record RecipeSearchFilters
{
    public required IReadOnlyList<string> Areas { get; init; }
    public required IReadOnlyList<string> Languages { get; init; }
    public required string? Query { get; init; }
    public required bool IncludeShared { get; init; }
}
