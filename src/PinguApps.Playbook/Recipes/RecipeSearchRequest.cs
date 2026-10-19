namespace PinguApps.Playbook.Recipes;

internal sealed record RecipeSearchRequest
{
    public required string TargetVersion { get; init; }
    public required IReadOnlyList<string> Areas { get; init; }
    public required IReadOnlyList<string> Languages { get; init; }
    public required string? Query { get; init; }
    public required bool IncludeShared { get; init; }
    public required int Offset { get; init; }
    public required int Limit { get; init; }
}
