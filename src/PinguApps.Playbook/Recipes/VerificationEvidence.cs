namespace PinguApps.Playbook.Recipes;

internal sealed record VerificationEvidence
{
    public required string Kind { get; init; }
    public required string Sample { get; init; }
    public required string Target { get; init; }
    public required string Sdk { get; init; }
    public required string Language { get; init; }
    public required string Command { get; init; }
    public required string Scope { get; init; }
}
