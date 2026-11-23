using System.Text.RegularExpressions;

namespace PinguApps.Playbook.Recipes;

internal static partial class GuidanceSearch
{
    // Alternate developer terminology for concepts whose catalogue labels use different words.
    private static readonly Dictionary<string, string[]> _aliases = new(StringComparer.Ordinal)
    {
        ["collections.compare-by-key"] = ["deduplicate by", "dedupe by", "deduplicate records", "deduplicate stations", "case insensitive deduplication", "case insensitive station ids", "first record per key"],
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
        return [.. goals.Select(goal => FindGoal(goal, requestedTarget, entries))];
    }

    private static GuidanceGoal FindGoal(string goal, string requestedTarget, SearchEntry[] entries)
    {
        string phrase = Normalize(goal);
        string[] words = [.. Words(phrase).Where(word => !_commonWords.Contains(word)).Distinct(StringComparer.Ordinal)];
        RankedMatch[] ranked = [.. entries.Select(entry => Rank(entry, phrase, words))
            .Where(match => match.Score > 0)
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.Edition.Id, StringComparer.Ordinal)];
        GuidanceMatch[] matches = [.. ranked.Where(match => match.Confident).Take(3).Select(match =>
        {
            RecipeEdition edition = match.Edition;
            return new GuidanceMatch(edition.Id, edition.Edition, edition.Title, edition.Goal, edition.Stability, match.Reason,
                ApplicabilitySummary.From(edition.Applicability));
        })];

        // Suggest a narrower query only when a particular installed candidate had substantial overlap.
        RankedMatch? near = matches.Length == 0 ? ranked.FirstOrDefault() : null;
        return new GuidanceGoal(goal, matches.Length == 0 ? "no_match" : "matched", matches)
        {
            SuggestedRequest = near is null ? null : new GuidanceRequest(requestedTarget, [near.Edition.Id]),
        };
    }

    private static RankedMatch Rank(SearchEntry entry, string phrase, string[] words)
    {
        if (phrase == entry.Id)
        {
            return new RankedMatch(entry.Edition, 1000, true, "Exact recipe ID.");
        }

        string? alias = entry.Aliases.FirstOrDefault(value => ContainsPhrase(phrase, value));
        if (alias is not null)
        {
            return new RankedMatch(entry.Edition, 200, true, $"Recognized wording: {alias}.");
        }

        string? label = entry.Labels.FirstOrDefault(value => words.Length != 0 && ContainsPhrase(value, phrase));
        if (label is not null)
        {
            return new RankedMatch(entry.Edition, 100, true, $"Catalogue phrase: {phrase}.");
        }

        string[] overlap = [.. words.Where(entry.Words.Contains)];
        bool confident = overlap.Length >= 2 && overlap.Length * 5 >= words.Length * 3;
        return new RankedMatch(entry.Edition, overlap.Length >= 2 ? overlap.Length : 0, confident,
            $"Shared catalogue terms: {string.Join(", ", overlap)}.");
    }

    private static SearchEntry CreateEntry(RecipeEdition edition)
    {
        string[] labels = [.. new[] { edition.Id, edition.Title, edition.Goal }.Concat(edition.Tags).Concat(edition.Apis).Select(Normalize)];
        return new SearchEntry(edition, labels[0], labels, labels.SelectMany(Words).ToHashSet(StringComparer.Ordinal),
            [.. (_aliases.GetValueOrDefault(edition.Id) ?? []).Select(Normalize)]);
    }

    private static bool ContainsPhrase(string text, string value) =>
        $" {text} ".Contains($" {value} ", StringComparison.Ordinal);

    private static string Normalize(string text) =>
        string.Join(' ', WordPattern().Matches(text.ToLowerInvariant()).Select(match => match.Value));

    private static string[] Words(string normalized) => normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    [GeneratedRegex(@"[\p{L}\p{Nd}]+")]
    private static partial Regex WordPattern();

    private sealed record SearchEntry(RecipeEdition Edition, string Id, IReadOnlyList<string> Labels, HashSet<string> Words, IReadOnlyList<string> Aliases);

    private sealed record RankedMatch(RecipeEdition Edition, int Score, bool Confident, string Reason);
}
