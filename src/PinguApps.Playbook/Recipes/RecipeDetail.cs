namespace PinguApps.Playbook.Recipes;

internal sealed record RecipeDetail
{
    public required string ResolvedTarget { get; init; }
    public required bool TargetAssumed { get; init; }
    public required IReadOnlyList<string> SupportedTargets { get; init; }
    public required IReadOnlyList<CoverageDisclosure> Coverage { get; init; }
    public required RecipeEdition Recipe { get; init; }
}
