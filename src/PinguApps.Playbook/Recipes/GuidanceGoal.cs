using System.Text.Json.Serialization;

namespace PinguApps.Playbook.Recipes;

internal sealed record GuidanceGoal(string Goal, string Status, IReadOnlyList<GuidanceMatch> Matches)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GuidanceRequest? SuggestedRequest { get; init; }
}
