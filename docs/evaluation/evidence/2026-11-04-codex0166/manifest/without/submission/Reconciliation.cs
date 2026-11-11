namespace ManifestEvaluation;

public sealed record Entry(string Key, int Units);
public sealed record Row(string Key, int? ExpectedUnits, int? ObservedUnits);

public static class Reconciliation
{
    public static Row[] Compare(Entry[] expected, Entry[] observed)
    {
        var expectedByKey = Index(expected, nameof(expected));
        var observedByKey = Index(observed, nameof(observed));

        return expectedByKey.Keys
            .Union(observedByKey.Keys, StringComparer.Ordinal)
            .OrderBy(key => key, StringComparer.Ordinal)
            .Select(key => new Row(
                key,
                expectedByKey.TryGetValue(key, out var expectedUnits) ? expectedUnits : (int?)null,
                observedByKey.TryGetValue(key, out var observedUnits) ? observedUnits : (int?)null))
            .ToArray();
    }

    private static Dictionary<string, int> Index(Entry[] entries, string parameterName)
    {
        var quantities = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            if (!quantities.TryAdd(entry.Key, entry.Units))
            {
                throw new ArgumentException("Manifest keys must be unique.", parameterName);
            }
        }

        return quantities;
    }
}
