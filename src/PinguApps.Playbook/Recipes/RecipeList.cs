namespace PinguApps.Playbook.Recipes;

internal sealed record RecipeList
{
    public required string ResolvedTarget { get; init; }
    public required bool TargetAssumed { get; init; }
    public required IReadOnlyList<string> SupportedTargets { get; init; }
    public required IReadOnlyList<string> SupportedAreas { get; init; }
    public required IReadOnlyList<string> SupportedExampleLanguages { get; init; }
    public required IReadOnlyList<CoverageDisclosure> Coverage { get; init; }
    public required IReadOnlyList<RecipeSummary> Recipes { get; init; }
}
