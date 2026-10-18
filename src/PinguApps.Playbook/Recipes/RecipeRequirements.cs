namespace PinguApps.Playbook.Recipes;

internal sealed record RecipeRequirements
{
    public required VersionRequirement Runtime { get; init; }
    public required VersionRequirement Sdk { get; init; }
    public required VersionRequirement Language { get; init; }
    public required IReadOnlyList<string> Frameworks { get; init; }
    public required IReadOnlyList<VersionRequirement> Packages { get; init; }
    public required IReadOnlyList<VersionRequirement> Workloads { get; init; }
    public required IReadOnlyList<string> PackageAssets { get; init; }
    public required IReadOnlyList<string> Providers { get; init; }
    public required IReadOnlyList<string> Platforms { get; init; }
    public required IReadOnlyList<string> RenderingModes { get; init; }
    public required IReadOnlyList<string> Prerequisites { get; init; }
    public required IReadOnlyList<string> Constraints { get; init; }
    public required IReadOnlyList<string> OptIns { get; init; }
    public required IReadOnlyList<string> StabilityLimitations { get; init; }
}
