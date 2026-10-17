namespace PinguApps.Playbook.Recipes;

internal sealed record RecipeDetail
{
    public required string ResolvedTarget { get; init; }
    public required bool TargetAssumed { get; init; }
    public required RecipeEdition Recipe { get; init; }
}
