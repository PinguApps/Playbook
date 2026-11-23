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

    // Each signal group represents a different part of an intent. One phrase
    // from every group must appear, in any order, before matching a catalogue API.
    private static readonly GuidanceIntent[] _intents =
    [
        new("run a C# file directly", "dotnet run file",
            [["c source file", "c file", "file based", "dotnet run file"], ["single", "one file", "directly", "without a project file", "without a csproj"]]),
        new("keep one item per key", "enumerable distinct",
            [["deduplicate", "deduplication", "dedupe", "remove duplicates"], ["first arrival", "first occurrence", "first seen", "case insensitive", "ignoring case", "per key", "per id"]]),
        new("retain the earliest item in each group", "enumerable distinct",
            [["keep the first", "keep first", "retain the first", "retain first", "preserve the first", "keep the earliest"],
             ["occurrence", "for each", "per", "for every"],
             ["case insensitive", "ignoring case", "arrival order", "input order", "key", "id", "identifier"]]),
        new("retain the first occurrence using case-insensitive equality", "enumerable distinct",
            [["keep", "retain", "preserve"], ["first", "earliest"], ["case insensitive", "ignoring case"],
             ["equality", "comparison", "matching"]]),
        new("find both extrema directly on a span", "memoryextensions min t",
            [["span", "readonlyspan"], ["min", "minimum"], ["max", "maximum"]]),
        new("write newline-delimited JSON", "jsonserializer serializeasyncenumerable",
            [["json"], ["one", "each", "separate", "newline"], ["per line", "own line", "separate line", "separate lines", "newline delimited"]]),
        new("restore grid state from the URL", "queryparameternameoptions",
            [["grid", "quickgrid", "table"], ["sort", "sorted", "sorting", "page", "paging", "pagination"], ["url", "query string", "bookmark", "bookmarkable", "shareable link"]]),
        new("validate a static form in the browser and on the server", "disableclientvalidation",
            [["static ssr", "static server rendered", "server rendered form", "statically rendered form", "server side rendered form"], ["validation", "validate", "invalid"], ["browser", "before posting", "before submission", "client side"]]),
        new("carry temporary progress between pages in the same session", "supplyparameterfromsessionattribute",
            [["session"], ["progress", "draft", "selection", "state"], ["remember", "restore", "retain", "resume", "persist", "preserve", "keep", "store", "save"], ["page", "pages", "refresh", "reload", "redirect", "navigation", "requests"]]),
        new("persist session state across server-rendered requests", "supplyparameterfromsessionattribute",
            [["session"], ["persist", "preserve", "retain", "store", "save", "keep"], ["static ssr", "server rendered"], ["request", "requests", "page", "pages", "navigation"]]),
        new("show one-use feedback after a redirect", "supplyparameterfromtempdataattribute",
            [["confirmation", "message", "messages", "notification", "notice", "feedback"], ["redirect", "redirected", "redirecting", "post redirect get"], ["once", "one use", "one time", "disappear", "refresh", "reload", "first view"]]),
        new("consume a one-time confirmation without clearing the session draft", "supplyparameterfromtempdataattribute",
            [["flash message", "flash messages", "one time confirmation", "one use confirmation"], ["static ssr", "server rendered", "consume", "redirect", "refresh", "reload"]]),
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
        GuidanceIntent[] intents = [.. _intents.Where(intent => intent.Signals.All(group => group.Any(signal => ContainsPhrase(phrase, signal))))];
        return entries.Select(entry => Rank(entry, phrase, words, distinctByKey, intents, frequency))
            .OfType<RankedMatch>()
            .OrderByDescending(match => match.Tier)
            .ThenByDescending(match => match.Score)
            .ThenByDescending(match => match.TagOverlap)
            .ThenBy(match => match.Edition.Id, StringComparer.Ordinal);
    }

    private static RankedMatch? Rank(SearchEntry entry, string phrase, string[] words, bool distinctByKey, GuidanceIntent[] intents,
        Dictionary<string, int> frequency)
    {
        if (phrase == entry.Id)
        {
            return new RankedMatch(entry.Edition, 3, 0, 0, "Exact recipe ID.");
        }

        if (distinctByKey && entry.Labels.Contains("enumerable distinct", StringComparer.Ordinal))
        {
            return new RankedMatch(entry.Edition, 2, 0, 0, "Recognized intent: keep one item per key.");
        }

        GuidanceIntent? intent = intents.FirstOrDefault(intent => entry.Apis.Contains(intent.Api));
        if (intent is not null)
        {
            return new RankedMatch(entry.Edition, 2, intent.Signals.Count, 0, $"Recognized intent: {intent.Description}.");
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
            edition.Apis.Select(Normalize).ToHashSet(StringComparer.Ordinal),
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

    private sealed record SearchEntry(RecipeEdition Edition, string Id, IReadOnlyList<string> Labels, HashSet<string> Words, HashSet<string> Tags,
        HashSet<string> Apis, IReadOnlyList<string> Aliases);

    private sealed record RankedMatch(RecipeEdition Edition, int Tier, double Score, int TagOverlap, string Reason);
}
