namespace PinguApps.Playbook.Recipes;

internal sealed record RecipeSummary
{
    public required string Id { get; init; }
    public required string Edition { get; init; }
    public required string Title { get; init; }
    public required string Goal { get; init; }
    public required string IntroducedIn { get; init; }
    public required IReadOnlyList<string> Areas { get; init; }
    public required IReadOnlyList<string> ExampleLanguages { get; init; }
    public required bool LanguageNeutral { get; init; }
    public required IReadOnlyList<string> SupportedTargets { get; init; }
    public required string Stability { get; init; }
    public required ApplicabilitySummary Applicability { get; init; }
}
