namespace PinguApps.Playbook.Recipes;

internal sealed record ApplicabilitySummary
{
    public required IReadOnlyList<SelectionRoute> Routes { get; init; }

    public static ApplicabilitySummary From(Applicability applicability)
    {
        return new ApplicabilitySummary
        {
            Routes = [.. applicability.Routes.Select(route => new SelectionRoute
            {
                Target = route.Target,
                SharedAreas = route.SharedAreas,
                NeutralLanguages = route.NeutralLanguages,
                Conditional = route.Conditional,
                Usage = route.Usage,
                SelectionNotes = route.SelectionNotes,
                Requirements = new SelectionRequirements
                {
                    Runtime = SelectionVersion.From(route.Requirements.Runtime),
                    Sdk = SelectionVersion.From(route.Requirements.Sdk),
                    Language = SelectionVersion.From(route.Requirements.Language),
                    Frameworks = route.Requirements.Frameworks,
                    Packages = [.. route.Requirements.Packages.Select(SelectionVersion.From)],
                    Workloads = [.. route.Requirements.Workloads.Select(SelectionVersion.From)],
                    PackageAssets = route.Requirements.PackageAssets,
                    Providers = route.Requirements.Providers,
                    Platforms = route.Requirements.Platforms,
                    RenderingModes = route.Requirements.RenderingModes,
                    OptIns = route.Requirements.OptIns,
                    StabilityLimitations = route.Requirements.StabilityLimitations,
                },
            })],
        };
    }
}
