namespace PinguApps.Playbook.Recipes;

internal sealed record GuidanceRouteConstraints(string Usage, bool Conditional, IReadOnlyList<string> Constraints);
