int[] readings = [42, -7, 18, 105, 3];
Equal(-7, InstrumentExtrema.Min<int>(readings)); Equal(105, InstrumentExtrema.Max<int>(readings));
Equal(-7, InstrumentExtrema.Min<int>(readings, null)); Equal(105, InstrumentExtrema.Max<int>(readings, null));
if (!readings.AsSpan().SequenceEqual(new int[] { 42, -7, 18, 105, 3 })) throw new Exception("Input buffer changed");
var descending = Comparer<int>.Create((a, b) => b.CompareTo(a));
Equal(105, InstrumentExtrema.Min<int>(readings, descending)); Equal(-7, InstrumentExtrema.Max<int>(readings, descending));
Throws<InvalidOperationException>(() => InstrumentExtrema.Min<int>([]));
Throws<InvalidOperationException>(() => InstrumentExtrema.Max<int>([]));
Throws<InvalidOperationException>(() => InstrumentExtrema.Min<int>([], descending));
Throws<InvalidOperationException>(() => InstrumentExtrema.Max<int>([], descending));
Equal(18, InstrumentExtrema.Min<int>(readings.AsSpan(2, 1)));
Equal(18, InstrumentExtrema.Max<int>(readings.AsSpan(2, 1)));
string?[] stationNames = [null, "Kestrel", "Aster", null, "Zephyr"];
Equal("Aster", InstrumentExtrema.Min<string?>(stationNames));
Equal("Zephyr", InstrumentExtrema.Max<string?>(stationNames));
Equal("Aster", InstrumentExtrema.Min<string?>(stationNames, StringComparer.Ordinal));
Equal("Zephyr", InstrumentExtrema.Max<string?>(stationNames, StringComparer.Ordinal));
// This comparer rejects null: ignored entries must never reach it.
var byLength = Comparer<string?>.Create((a, b) => (a ?? throw new Exception("Null comparer input")).Length.CompareTo((b ?? throw new Exception("Null comparer input")).Length));
Equal("Aster", InstrumentExtrema.Min<string?>(stationNames, byLength));
Equal("Kestrel", InstrumentExtrema.Max<string?>(stationNames, byLength));
Equal<string?>(null, InstrumentExtrema.Min<string?>([]));
Equal<string?>(null, InstrumentExtrema.Max<string?>([]));
Equal<string?>(null, InstrumentExtrema.Min<string?>([], byLength));
Equal<string?>(null, InstrumentExtrema.Max<string?>([], byLength));
Equal<string?>(null, InstrumentExtrema.Min<string?>([null, null], byLength));
Equal<string?>(null, InstrumentExtrema.Max<string?>([null, null], byLength));
Equal<int?>(null, InstrumentExtrema.Min<int?>([])); Equal<int?>(null, InstrumentExtrema.Max<int?>([]));
Equal<int?>(null, InstrumentExtrema.Min<int?>([null, null])); Equal<int?>(null, InstrumentExtrema.Max<int?>([null, null]));
Equal<int?>(-7, InstrumentExtrema.Min<int?>([null, 42, -7, null]));
Equal<int?>(42, InstrumentExtrema.Max<int?>([null, 42, -7, null]));
var stations = new[] { new Station("Beacon", 8), new Station("Harbour", 2), new Station("Ridge", 19) };
var byLoad = Comparer<Station>.Create((a, b) => a.Load.CompareTo(b.Load));
Equal(stations[1], InstrumentExtrema.Min<Station>(stations, byLoad));
Equal(stations[2], InstrumentExtrema.Max<Station>(stations, byLoad));
#if NET11_0_OR_GREATER
// Compile extension syntax on an existing mutable span as well as generic static calls.
Span<int> mutable = readings;
Equal(-7, mutable.Min()); Equal(105, mutable.Max());
ReadOnlySpan<string?> names = stationNames;
Equal("Aster", names.Min(StringComparer.Ordinal)); Equal("Zephyr", names.Max(StringComparer.Ordinal));
#endif
Console.WriteLine($"PASS ObservatoryBatch: values=-7..105; reverse/default/explicit comparers; reference/null/nullable/all-null; distinct empty contracts; generic and extension overloads. Runtime={Environment.Version}; CoreLib={typeof(object).Assembly.Location}");
static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception($"Expected {expected}, got {actual}");
}
static void Throws<T>(Action action) where T : Exception
{
    try { action(); } catch (T) { return; }
    throw new Exception($"Expected {typeof(T).Name}");
}
internal sealed record Station(string Name, int Load);
