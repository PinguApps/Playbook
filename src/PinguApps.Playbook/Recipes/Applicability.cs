namespace PinguApps.Playbook.Recipes;

internal sealed record Applicability
{
    public required string Runtime { get; init; }
    public required string Sdk { get; init; }
    public required string Language { get; init; }
    public required IReadOnlyList<string> Frameworks { get; init; }
    public required IReadOnlyList<string> Packages { get; init; }
    public required IReadOnlyList<string> Platforms { get; init; }
    public required IReadOnlyList<string> RenderingModes { get; init; }
    public required IReadOnlyList<string> Prerequisites { get; init; }
    public required IReadOnlyList<string> Constraints { get; init; }
}
