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

    public RecipeList List(string targetVersion, string[]? areas, string[]? languages, string? query, bool includeShared, int offset, int limit)
    {
        string target = ResolveTarget(targetVersion);
        string[] supportedAreas = [.. _editions.SelectMany(edition => edition.Areas.Concat(edition.Applicability.Routes.SelectMany(route => route.SharedAreas)))
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
        string[] supportedLanguages = [.. _editions.SelectMany(edition => edition.ExampleLanguages.Concat(edition.Applicability.Routes.SelectMany(route => route.NeutralLanguages)))
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
        ValidateFilter(areas, supportedAreas, "areas", "unknown_area");
        ValidateFilter(languages, supportedLanguages, "languages", "unknown_language");
        if (offset < 0 || limit is < 1 or > 50)
        {
            throw Failure("invalid_range", "Use offset >= 0 and limit between 1 and 50 (default 20).", targetVersion, null, []);
        }

        string[] terms = query?.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries) ?? [];
        RecipeEdition[] candidates = [.. _editions
            .Where(edition => edition.SupportedTargets.Contains(target, StringComparer.Ordinal))
            .GroupBy(edition => edition.Id, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => group.OrderByDescending(edition => Version.Parse(edition.Edition)).First())
            .Select(edition => edition with
            {
                Applicability = new Applicability
                {
                    Routes = [.. edition.Applicability.Routes.Where(route => route.Target == target
                        && (areas is null or [] || edition.Areas.Intersect(areas, StringComparer.Ordinal).Any()
                            || (includeShared && route.SharedAreas.Intersect(areas, StringComparer.Ordinal).Any()))
                        && (languages is null or [] || edition.ExampleLanguages.Intersect(languages, StringComparer.Ordinal).Any()
                            || (edition.LanguageNeutral && route.NeutralLanguages.Intersect(languages, StringComparer.Ordinal).Any())))],
                },
            })
            .Where(edition => edition.Applicability.Routes.Count > 0)];
        RecipeSummary[] summaries = [.. candidates
            .Where(edition => terms.All(term => edition.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
                || edition.Goal.Contains(term, StringComparison.OrdinalIgnoreCase)
                || edition.Tags.Any(tag => tag.Contains(term, StringComparison.OrdinalIgnoreCase))
                || edition.Apis.Any(api => api.Contains(term, StringComparison.OrdinalIgnoreCase))))
            .Select(edition => new RecipeSummary
            {
                Id = edition.Id,
                Edition = edition.Edition,
                Title = edition.Title,
                Goal = edition.Goal,
                IntroducedIn = edition.IntroducedIn,
                Areas = edition.Areas,
                ExampleLanguages = edition.ExampleLanguages,
                LanguageNeutral = edition.LanguageNeutral,
                SupportedTargets = edition.SupportedTargets,
                Stability = edition.Stability,
                Applicability = ApplicabilitySummary.From(edition.Applicability),
            })];
        RecipeSummary[] page = [.. summaries.Skip(offset).Take(limit)];
        bool keywordRetry = summaries.Length == 0 && terms.Length > 0 && candidates.Length > 0;
        RecipeSearchRecovery? recovery = null;
        if (page.Length == 0)
        {
            string message = (summaries.Length > 0, keywordRetry) switch
            {
                (true, _) => "Matching recipes exist, but this offset is outside the result set. Retry from offset 0 with the same target and filters.",
                (_, true) => "No installed recipe matches all keywords with this target and filters. You may retry without keywords; compatibility filters are unchanged. This is not proof that the capability does not exist; inspect coverage.",
                _ => "No compatible installed recipe matches this target and filter combination, even without keywords. No safe filter retry is available. This is not proof that the capability does not exist; inspect coverage.",
            };
            string? effectiveQuery = terms.Length == 0 ? null : string.Join(' ', terms);
            RecipeSearchRequest? suggestedRequest = null;
            if (summaries.Length > 0 || keywordRetry)
            {
                suggestedRequest = new RecipeSearchRequest
                {
                    TargetVersion = targetVersion,
                    Areas = areas ?? [],
                    Languages = languages ?? [],
                    Query = keywordRetry ? null : effectiveQuery,
                    IncludeShared = includeShared,
                    Offset = 0,
                    Limit = limit,
                };
            }

            recovery = new RecipeSearchRecovery
            {
                Reason = summaries.Length > 0 ? "offset_out_of_range" : "no_matches",
                Message = message,
                Filters = new RecipeSearchFilters
                {
                    Areas = areas ?? [],
                    Languages = languages ?? [],
                    Query = effectiveQuery,
                    IncludeShared = includeShared,
                },
                SuggestedRequest = suggestedRequest,
            };
        }
        return new RecipeList
        {
            ResolvedTarget = target,
            RequestedTarget = targetVersion,
            TargetAssumed = false,
            SupportedTargets = _targets,
            SupportedAreas = supportedAreas,
            SupportedExampleLanguages = supportedLanguages,
            Coverage = _coverage,
            Total = summaries.Length,
            Offset = offset,
            Limit = limit,
            NextOffset = offset < summaries.Length && page.Length < summaries.Length - offset ? offset + page.Length : null,
            Recipes = page,
            Recovery = recovery,
        };
    }

    private static void ValidateFilter(string[]? requested, string[] supported, string filter, string code)
    {
        string[] invalid = [.. (requested ?? []).Except(supported, StringComparer.Ordinal)];
        if (invalid.Length > 0)
        {
            throw new McpException(JsonSerializer.Serialize(new RecipeFailure
            {
                Code = code,
                Message = $"Unknown {filter}: {string.Join(", ", invalid)}. Choose identifiers from supportedValues.",
                Filter = filter,
                InvalidValues = invalid,
                SupportedValues = supported,
                SupportedTargets = [],
                AvailableEditions = [],
            }, JsonSerializerOptions.Web));
        }
    }

    public RecipeDetail Get(string id, string targetVersion)
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
            RequestedTarget = targetVersion,
            TargetAssumed = false,
            SupportedTargets = _targets,
            Coverage = _coverage,
            Recipe = edition with { Applicability = ForTarget(edition.Applicability, target) },
        };
    }

    private string ResolveTarget(string? targetVersion)
    {
        if (string.IsNullOrWhiteSpace(targetVersion))
        {
            throw Failure("required_target",
                "targetVersion is required on every list_recipes and get_recipe call. Choose a supported target release or explicitly use latest for the newest installed catalogue release; inspect consumer prerequisites separately.",
                targetVersion, null, []);
        }

        string target = targetVersion switch
        {
            "latest" => _targets[^1],
            "10" or "10.0" or "net10.0" => "10.0",
            "11" or "11.0" or "net11.0" => "11.0",
            _ => targetVersion,
        };
        return _targets.Contains(target, StringComparer.Ordinal)
            ? target
            : throw Failure("unsupported_target",
                $"Unsupported target '{targetVersion}'. Choose an installed release ({string.Join(", ", _targets)}), using 10/10.0/net10.0 or 11/11.0/net11.0, or explicitly choose latest. A target release is not an exact SDK version. Patch, RC and platform-qualified strings are not accepted: choose the release and inspect recipe platform prerequisites separately. .NET 9 is comparison-only.",
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
