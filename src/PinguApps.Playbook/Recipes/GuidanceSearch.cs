using System.Text.RegularExpressions;

namespace PinguApps.Playbook.Recipes;

internal static partial class GuidanceSearch
{
    // Alternate developer terminology for concepts whose catalogue labels use different words.
    private static readonly Dictionary<string, string[]> _aliases = new(StringComparer.Ordinal)
    {
        ["collections.compare-by-key"] = ["deduplicate by", "dedupe by", "deduplicate records", "case insensitive deduplication"],
        ["collections.find-span-extrema"] = ["span extrema", "span min max", "min max directly", "minimum and maximum directly", "min and max directly", "minimum and maximum from a span"],
        ["json.stream-json-lines"] = ["ndjson", "jsonl", "json lines", "one json object per line", "one json record per line", "newline delimited json"],
        ["sdk.run-file-utility"] = ["file based app", "file based console", "single file console", "single file c#", "run a c# file directly", "dotnet run file", "without a csproj"],
    };

    private static readonly HashSet<string> _commonWords =
    [
        "a", "an", "and", "app", "as", "at", "before", "by", "can", "code", "directly", "do", "dotnet",
        "each", "for", "from", "how", "i", "in", "into", "is", "it", "of", "on", "or", "the", "to", "use",
        "using", "want", "when", "while", "with", "without",
    ];

    public static GuidanceGoal[] Find(string[] goals, string requestedTarget, IEnumerable<RecipeEdition> editions)
    {
        SearchEntry[] entries = [.. editions.Select(CreateEntry)];
        Dictionary<string, int> frequency = entries.SelectMany(entry => entry.Words)
            .CountBy(word => word, StringComparer.Ordinal).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        return [.. goals.Select(goal => FindGoal(goal, requestedTarget, entries, frequency))];
    }

    private static GuidanceGoal FindGoal(string goal, string requestedTarget, SearchEntry[] entries, Dictionary<string, int> frequency)
    {
        // A retry may separate decisions that the caller already supplied. It never replaces the goal with a guessed recipe ID.
        string[] clauses = goal.Split([';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (clauses.Length is >= 2 and <= 6 && clauses.All(clause => Search(clause, entries, frequency).Any()))
        {
            return new GuidanceGoal(goal, "no_match", [])
            {
                SuggestedRequest = new GuidanceRequest(requestedTarget, clauses),
            };
        }

        GuidanceMatch[] matches = [.. Search(goal, entries, frequency).Take(3).Select(match =>
        {
            RecipeEdition edition = match.Edition;
            return new GuidanceMatch(edition.Id, edition.Edition, edition.Title, edition.Goal, edition.Stability, match.Reason,
                ApplicabilitySummary.From(edition.Applicability));
        })];
        return new GuidanceGoal(goal, matches.Length == 0 ? "no_match" : "matched", matches);
    }

    private static IEnumerable<RankedMatch> Search(string goal, SearchEntry[] entries, Dictionary<string, int> frequency)
    {
        string phrase = Normalize(goal);
        string[] words = [.. Words(phrase).Where(word => !_commonWords.Contains(word)).Distinct(StringComparer.Ordinal)];
        bool distinctByKey = DistinctByKeyPattern().IsMatch(phrase);
        return entries.Select(entry => Rank(entry, phrase, words, distinctByKey, frequency))
            .OfType<RankedMatch>()
            .OrderByDescending(match => match.Tier)
            .ThenByDescending(match => match.Score)
            .ThenByDescending(match => match.TagOverlap)
            .ThenBy(match => match.Edition.Id, StringComparer.Ordinal);
    }

    private static RankedMatch? Rank(SearchEntry entry, string phrase, string[] words, bool distinctByKey, Dictionary<string, int> frequency)
    {
        if (phrase == entry.Id)
        {
            return new RankedMatch(entry.Edition, 3, 0, 0, "Exact recipe ID.");
        }

        if (distinctByKey && entry.Labels.Contains("enumerable distinct", StringComparer.Ordinal))
        {
            return new RankedMatch(entry.Edition, 2, 0, 0, "Recognized intent: keep one item per key.");
        }

        string? alias = entry.Aliases.FirstOrDefault(value => ContainsPhrase(phrase, value));
        if (alias is not null)
        {
            return new RankedMatch(entry.Edition, 2, Words(alias).Length, 0, $"Recognized wording: {alias}.");
        }

        string? label = entry.Labels.FirstOrDefault(value => words.Length != 0 && ContainsPhrase(value, phrase));
        if (label is not null)
        {
            return new RankedMatch(entry.Edition, 1, 0, 0, $"Catalogue phrase: {phrase}.");
        }

        string[] overlap = [.. words.Where(entry.Words.Contains)];
        if (overlap.Length < 2 || overlap.Length * 5 < words.Length * 3)
        {
            return null;
        }

        // Rare catalogue terms carry more information than terms shared by many unrelated recipes.
        double score = overlap.Sum(word => 1 / Math.Sqrt(frequency[word]));
        int tagOverlap = words.Count(entry.Tags.Contains);
        return new RankedMatch(entry.Edition, 0, score, tagOverlap, $"Shared catalogue terms: {string.Join(", ", overlap)}.");
    }

    private static SearchEntry CreateEntry(RecipeEdition edition)
    {
        string[] labels = [.. new[] { edition.Id, edition.Title, edition.Goal }.Concat(edition.Apis).Select(Normalize)];
        return new SearchEntry(edition, labels[0], labels, labels.SelectMany(Words).ToHashSet(StringComparer.Ordinal),
            edition.Tags.SelectMany(tag => Words(Normalize(tag))).ToHashSet(StringComparer.Ordinal),
            [.. (_aliases.GetValueOrDefault(edition.Id) ?? []).Select(Normalize)]);
    }

    private static bool ContainsPhrase(string text, string value) =>
        $" {text} ".Contains($" {value} ", StringComparison.Ordinal);

    private static string Normalize(string text) =>
        string.Join(' ', WordPattern().Matches(text.ToLowerInvariant()).Select(match => match.Value));

    private static string[] Words(string normalized) => normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    [GeneratedRegex(@"[\p{L}\p{Nd}]+")]
    private static partial Regex WordPattern();

    [GeneratedRegex(@"\b(?:(?:first|one)(?: \w+){0,4} (?:per|for each|by)(?: \w+){0,4} (?:keys?|ids?|identifiers?)|(?:keep|retain|take|preserve) (?:only )?(?:the )?first(?: \w+){0,4} (?:key|id|identifier) (?:using |with )?(?:ordinal )?(?:case insensitive|ignoring case))\b")]
    private static partial Regex DistinctByKeyPattern();

    private sealed record SearchEntry(RecipeEdition Edition, string Id, IReadOnlyList<string> Labels, HashSet<string> Words, HashSet<string> Tags, IReadOnlyList<string> Aliases);

    private sealed record RankedMatch(RecipeEdition Edition, int Tier, double Score, int TagOverlap, string Reason);
}
