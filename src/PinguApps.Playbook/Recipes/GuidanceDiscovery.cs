namespace PinguApps.Playbook.Recipes;

internal sealed record GuidanceDiscovery
{
    public required string RequestedTarget { get; init; }
    public required string ResolvedTarget { get; init; }
    public required IReadOnlyList<CoverageDisclosure> Coverage { get; init; }
    public required IReadOnlyList<GuidanceGoal> Goals { get; init; }
    public required string NextStep { get; init; }
}
