namespace PinguApps.Playbook.Recipes;

internal sealed record SelectionRoute
{
    public required string Target { get; init; }
    public required IReadOnlyList<string> SharedAreas { get; init; }
    public required IReadOnlyList<string> NeutralLanguages { get; init; }
    public required bool Conditional { get; init; }
    public required string Usage { get; init; }
    public required IReadOnlyList<string> SelectionNotes { get; init; }
    public required SelectionRequirements Requirements { get; init; }
}
