namespace PinguApps.Playbook.Recipes;

internal sealed record GuidanceRequest(string TargetVersion, IReadOnlyList<string> Goals);
