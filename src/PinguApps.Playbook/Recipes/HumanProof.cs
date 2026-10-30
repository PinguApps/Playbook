using System.Text.Json.Serialization;

namespace PinguApps.Playbook.Recipes;

internal sealed record HumanProof
{
    public required string Status { get; init; }
    public required string Scope { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? UntestedSamples { get; init; }
}
