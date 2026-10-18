namespace PinguApps.Playbook.Recipes;

internal sealed record RecipeEdition
{
    public required string Id { get; init; }
    public required string Edition { get; init; }
    public required string Title { get; init; }
    public required string Goal { get; init; }
    public required IReadOnlyList<string> Areas { get; init; }
    public required IReadOnlyList<string> ExampleLanguages { get; init; }
    public required IReadOnlyList<string> Tags { get; init; }
    public required IReadOnlyList<string> Apis { get; init; }
    public required string IntroducedIn { get; init; }
    public required IReadOnlyList<string> SupportedTargets { get; init; }
    public required string Stability { get; init; }
    public required Applicability Applicability { get; init; }
    public required IReadOnlyList<SourceReference> Sources { get; init; }
    public required IReadOnlyList<VerificationEvidence> Evidence { get; init; }
    public required HumanProof HumanProof { get; init; }
    public required string Markdown { get; init; }
}
