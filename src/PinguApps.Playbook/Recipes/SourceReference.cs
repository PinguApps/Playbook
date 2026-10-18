namespace PinguApps.Playbook.Recipes;

internal sealed record SourceReference
{
    public required string Title { get; init; }
    public required string Url { get; init; }
}
