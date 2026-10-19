namespace PinguApps.Playbook.Recipes;

internal sealed record ApplicabilityRoute
{
    public required string Target { get; init; }
    public IReadOnlyList<string> SharedAreas { get; init; } = [];
    public IReadOnlyList<string> NeutralLanguages { get; init; } = [];
    public required bool Conditional { get; init; }
    public required string Usage { get; init; }
    public required IReadOnlyList<string> SelectionNotes { get; init; }
    public required RecipeRequirements Requirements { get; init; }
    public required IReadOnlyList<string> SourceUrls { get; init; }
    public required IReadOnlyList<string> EvidenceSamples { get; init; }
}
