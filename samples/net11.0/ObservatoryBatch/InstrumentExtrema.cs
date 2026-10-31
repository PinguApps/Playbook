// snippet:extrema
internal static class InstrumentExtrema
{
    // Explicit generic static calls avoid confusion with Enumerable.Min/Max.
    public static T? Min<T>(ReadOnlySpan<T> values, IComparer<T>? comparer = null)
        => MemoryExtensions.Min<T>(values, comparer);
    public static T? Max<T>(ReadOnlySpan<T> values, IComparer<T>? comparer = null)
        => MemoryExtensions.Max<T>(values, comparer);
}
// endsnippet:extrema
