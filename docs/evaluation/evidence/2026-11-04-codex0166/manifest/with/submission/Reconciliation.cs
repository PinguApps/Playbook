namespace ManifestEvaluation;

public sealed record Entry(string Key, int Units);
public sealed record Row(string Key, int? ExpectedUnits, int? ObservedUnits);

public static class Reconciliation
{
    public static Row[] Compare(Entry[] expected, Entry[] observed)
    {
        ValidateUniqueKeys(expected, nameof(expected));
        ValidateUniqueKeys(observed, nameof(observed));

        return expected.FullJoin(observed,
                entry => entry.Key,
                entry => entry.Key,
                (left, right) => new Row(
                    left?.Key ?? right!.Key, left?.Units, right?.Units),
                StringComparer.Ordinal)
            .OrderBy(row => row.Key, StringComparer.Ordinal)
            .ToArray();
    }

    private static void ValidateUniqueKeys(Entry[] entries, string parameterName)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            if (!keys.Add(entry.Key))
                throw new ArgumentException("Manifest keys must be unique.", parameterName);
        }
    }
}
