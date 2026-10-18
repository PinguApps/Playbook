namespace PinguApps.Playbook.Recipes;

internal sealed record HumanProof
{
    public required string Status { get; init; }
    public required string Scope { get; init; }
}
