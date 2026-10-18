namespace PinguApps.Playbook.Recipes;

internal sealed record VersionRequirement
{
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required string Match { get; init; }
    public required string Reason { get; init; }
}
