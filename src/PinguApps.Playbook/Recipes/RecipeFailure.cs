namespace PinguApps.Playbook.Recipes;

internal sealed record RecipeFailure
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public string? RecipeId { get; init; }
    public string? RequestedTarget { get; init; }
    public required IReadOnlyList<string> SupportedTargets { get; init; }
    public required IReadOnlyList<string> AvailableEditions { get; init; }
    public string? Filter { get; init; }
    public IReadOnlyList<string> InvalidValues { get; init; } = [];
    public IReadOnlyList<string> SupportedValues { get; init; } = [];
}
