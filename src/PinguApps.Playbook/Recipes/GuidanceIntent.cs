namespace PinguApps.Playbook.Recipes;

internal sealed record GuidanceIntent(string Description, string Api, IReadOnlyList<IReadOnlyList<string>> Signals);
