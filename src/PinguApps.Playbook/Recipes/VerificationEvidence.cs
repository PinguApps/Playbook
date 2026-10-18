namespace PinguApps.Playbook.Recipes;

internal sealed record VerificationEvidence
{
    public required string Role { get; init; }
    public required string Kind { get; init; }
    public required string Sample { get; init; }
    public required string Target { get; init; }
    public required string Sdk { get; init; }
    public required string Language { get; init; }
    public required string Command { get; init; }
    public required string Scope { get; init; }
    public required string RecordedAt { get; init; }
    public required string SourceCommit { get; init; }
    public required bool SourceDirty { get; init; }
    public required IReadOnlyDictionary<string, string> Inputs { get; init; }
    public required IReadOnlyList<string> Dependencies { get; init; }
    public required IReadOnlyList<string> Workloads { get; init; }
    public required IReadOnlyList<string> NativeToolchains { get; init; }
    public required string Host { get; init; }
    public required string SdkInfo { get; init; }
    public required string WorkloadInfo { get; init; }
    public required string Output { get; init; }
    public required string Freshness { get; init; }
}
