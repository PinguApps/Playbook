namespace PinguApps.Playbook.Recipes;

internal sealed record Applicability
{
    public required IReadOnlyList<ApplicabilityRoute> Routes { get; init; }
}
