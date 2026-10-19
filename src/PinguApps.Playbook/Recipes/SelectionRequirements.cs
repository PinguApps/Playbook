namespace PinguApps.Playbook.Recipes;

internal sealed record SelectionRequirements
{
    public required SelectionVersion Runtime { get; init; }
    public required SelectionVersion Sdk { get; init; }
    public required SelectionVersion Language { get; init; }
    public required IReadOnlyList<string> Frameworks { get; init; }
    public required IReadOnlyList<SelectionVersion> Packages { get; init; }
    public required IReadOnlyList<SelectionVersion> Workloads { get; init; }
    public required IReadOnlyList<string> PackageAssets { get; init; }
    public required IReadOnlyList<string> Providers { get; init; }
    public required IReadOnlyList<string> Platforms { get; init; }
    public required IReadOnlyList<string> RenderingModes { get; init; }
    public required IReadOnlyList<string> OptIns { get; init; }
    public required IReadOnlyList<string> StabilityLimitations { get; init; }
}
