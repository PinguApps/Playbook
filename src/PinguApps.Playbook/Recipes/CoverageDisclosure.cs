namespace PinguApps.Playbook.Recipes;

internal sealed record CoverageDisclosure
{
    public required string Target { get; init; }
    public required string Scope { get; init; }
    public required bool Complete { get; init; }
    public required string Description { get; init; }
}
