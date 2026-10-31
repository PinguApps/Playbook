// snippet:extrema
internal static class InstrumentExtrema
{
    public static T? Min<T>(ReadOnlySpan<T> values, IComparer<T>? comparer = null)
        => Select(values, comparer, minimum: true);
    public static T? Max<T>(ReadOnlySpan<T> values, IComparer<T>? comparer = null)
        => Select(values, comparer, minimum: false);

    private static T? Select<T>(ReadOnlySpan<T> values, IComparer<T>? comparer, bool minimum)
    {
        comparer ??= Comparer<T>.Default;
        bool found = false;
        T? selected = default;
        foreach (T candidate in values)
        {
            if (candidate is null) continue;
            if (!found)
            {
                selected = candidate;
                found = true;
                continue;
            }
            int order = comparer.Compare(candidate, selected!);
            if (minimum ? order < 0 : order > 0) selected = candidate;
        }
        if (!found && default(T) is not null) throw new InvalidOperationException("No instrument readings.");
        return selected;
    }
}
// endsnippet:extrema
