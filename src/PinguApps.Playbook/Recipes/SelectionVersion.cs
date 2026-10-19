namespace PinguApps.Playbook.Recipes;

internal sealed record SelectionVersion
{
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required string Match { get; init; }

    public static SelectionVersion From(VersionRequirement requirement)
    {
        return new SelectionVersion { Name = requirement.Name, Version = requirement.Version, Match = requirement.Match };
    }
}
