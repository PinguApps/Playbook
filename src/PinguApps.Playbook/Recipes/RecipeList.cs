using System.Text.Json.Serialization;

namespace PinguApps.Playbook.Recipes;

internal sealed record RecipeList
{
    public required string ResolvedTarget { get; init; }
    public required string RequestedTarget { get; init; }
    public required bool TargetAssumed { get; init; }
    public required IReadOnlyList<string> SupportedTargets { get; init; }
    public required IReadOnlyList<string> SupportedAreas { get; init; }
    public required IReadOnlyList<string> SupportedExampleLanguages { get; init; }
    public required IReadOnlyList<CoverageDisclosure> Coverage { get; init; }
    public required int Total { get; init; }
    public required int Offset { get; init; }
    public required int Limit { get; init; }
    public required int? NextOffset { get; init; }
    public required IReadOnlyList<RecipeSummary> Recipes { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RecipeSearchRecovery? Recovery { get; init; }
}
