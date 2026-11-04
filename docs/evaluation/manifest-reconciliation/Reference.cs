namespace ManifestEvaluation;

public sealed record Entry(string Key, int Units);
public sealed record Row(string Key, int? ExpectedUnits, int? ObservedUnits);

public static class Reconciliation
{
    public static Row[] Compare(Entry[] expected, Entry[] observed)
    {
        Validate(expected);
        Validate(observed);
        return expected.FullJoin(observed, x => x.Key, x => x.Key,
            (left, right) => new Row((left ?? right!).Key, left?.Units, right?.Units),
            StringComparer.Ordinal).OrderBy(x => x.Key, StringComparer.Ordinal).ToArray();
    }

    private static void Validate(Entry[] entries)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in entries)
            if (!keys.Add(entry.Key)) throw new ArgumentException("Duplicate key");
    }
}
