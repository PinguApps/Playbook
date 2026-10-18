namespace PinguApps.Playbook.Recipes;

internal sealed record ApplicabilityRoute
{
    public required string Target { get; init; }
    public required bool Conditional { get; init; }
    public required string Runtime { get; init; }
    public required string Sdk { get; init; }
    public required string Language { get; init; }
    public required IReadOnlyList<string> Frameworks { get; init; }
    public required IReadOnlyList<string> Packages { get; init; }
    public required IReadOnlyList<string> PackageAssets { get; init; }
    public required IReadOnlyList<string> Providers { get; init; }
    public required IReadOnlyList<string> Platforms { get; init; }
    public required IReadOnlyList<string> RenderingModes { get; init; }
    public required IReadOnlyList<string> Prerequisites { get; init; }
    public required IReadOnlyList<string> Constraints { get; init; }
    public required IReadOnlyList<string> SourceUrls { get; init; }
    public required IReadOnlyList<string> EvidenceSamples { get; init; }
    public required IReadOnlyList<string> OptIns { get; init; }
    public required IReadOnlyList<string> StabilityLimitations { get; init; }
}
