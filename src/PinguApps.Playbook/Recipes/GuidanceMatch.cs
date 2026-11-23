namespace PinguApps.Playbook.Recipes;

internal sealed record GuidanceMatch(
    string Id,
    string Edition,
    string Title,
    string Goal,
    string Stability,
    string WhyMatched,
    IReadOnlyList<GuidanceRouteConstraints> RouteConstraints,
    ApplicabilitySummary Applicability);
