using System.Text.Json;
using ModelContextProtocol;

namespace PinguApps.Playbook.Recipes;

internal sealed class RecipeCatalogue
{
    private readonly RecipeEdition[] _editions;
    private readonly string[] _targets;
    private readonly CoverageDisclosure[] _coverage;

    public RecipeCatalogue()
    {
        using Stream stream = typeof(RecipeCatalogue).Assembly.GetManifestResourceStream("Playbook.Catalogue")
            ?? throw new InvalidOperationException("Embedded catalogue is missing.");
        _editions = JsonSerializer.Deserialize<RecipeEdition[]>(stream, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Embedded catalogue is empty.");
        _targets = [.. _editions.SelectMany(edition => edition.SupportedTargets)
            .Distinct(StringComparer.Ordinal).OrderBy(Version.Parse)];
        using Stream coverageStream = typeof(RecipeCatalogue).Assembly.GetManifestResourceStream("Playbook.Coverage")
            ?? throw new InvalidOperationException("Embedded coverage disclosure is missing.");
        _coverage = JsonSerializer.Deserialize<CoverageDisclosure[]>(coverageStream, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Embedded coverage disclosure is empty.");
    }

    public RecipeList List(string? targetVersion)
    {
        string target = ResolveTarget(targetVersion);
        RecipeSummary[] summaries = [.. _editions
            .Where(edition => edition.SupportedTargets.Contains(target, StringComparer.Ordinal))
            .GroupBy(edition => edition.Id, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => group.OrderByDescending(edition => Version.Parse(edition.Edition)).First())
            .Select(edition => new RecipeSummary
            {
                Id = edition.Id,
                Edition = edition.Edition,
                Title = edition.Title,
                Goal = edition.Goal,
                IntroducedIn = edition.IntroducedIn,
                Areas = edition.Areas,
                ExampleLanguages = edition.ExampleLanguages,
                SupportedTargets = edition.SupportedTargets,
                Stability = edition.Stability,
                Applicability = ForTarget(edition.Applicability, target),
            })];
        return new RecipeList
        {
            ResolvedTarget = target,
            TargetAssumed = targetVersion is null,
            SupportedTargets = _targets,
            SupportedAreas = [.. _editions.SelectMany(edition => edition.Areas).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)],
            SupportedExampleLanguages = [.. _editions.SelectMany(edition => edition.ExampleLanguages).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)],
            Coverage = _coverage,
            Recipes = summaries,
        };
    }

    public RecipeDetail Get(string id, string? targetVersion)
    {
        string target = ResolveTarget(targetVersion);
        RecipeEdition[] available = [.. _editions.Where(candidate => candidate.Id == id)];
        if (available.Length == 0)
        {
            throw Failure("unknown_recipe", $"Unknown recipe '{id}'. Use list_recipes to discover installed IDs.", targetVersion, id, []);
        }

        RecipeEdition edition = available
            .Where(candidate => candidate.SupportedTargets.Contains(target, StringComparer.Ordinal))
            .OrderByDescending(candidate => Version.Parse(candidate.Edition))
            .FirstOrDefault() ?? throw Failure("no_applicable_edition",
                $"Recipe '{id}' has no applicable edition for '{target}'. Inspect available editions; the explicit target was not replaced.",
                targetVersion, id, [.. available.Select(candidate => candidate.Edition).OrderBy(Version.Parse)]);
        return new RecipeDetail
        {
            ResolvedTarget = target,
            TargetAssumed = targetVersion is null,
            SupportedTargets = _targets,
            Coverage = _coverage,
            Recipe = edition with { Applicability = ForTarget(edition.Applicability, target) },
        };
    }

    private string ResolveTarget(string? targetVersion)
    {
        string target = targetVersion switch
        {
            null => _targets[^1],
            "10" or "10.0" or "net10.0" => "10.0",
            "11" or "11.0" or "net11.0" => "11.0",
            _ => targetVersion,
        };
        return _targets.Contains(target, StringComparer.Ordinal)
            ? target
            : throw Failure("unsupported_target",
                $"Unsupported target '{targetVersion}'. Choose an installed release ({string.Join(", ", _targets)}), using 10/10.0/net10.0 or 11/11.0/net11.0. Patch, RC and platform-qualified strings are not accepted: choose the release and inspect recipe platform prerequisites separately. .NET 9 is comparison-only.",
                targetVersion, null, []);
    }

    private static Applicability ForTarget(Applicability applicability, string target)
    {
        return new Applicability { Routes = [.. applicability.Routes.Where(route => route.Target == target)] };
    }

    private McpException Failure(string code, string message, string? target, string? id, string[] editions)
    {
        return new McpException(JsonSerializer.Serialize(new RecipeFailure
        {
            Code = code,
            Message = message,
            RecipeId = id,
            RequestedTarget = target,
            SupportedTargets = _targets,
            AvailableEditions = editions,
        }, JsonSerializerOptions.Web));
    }
}
