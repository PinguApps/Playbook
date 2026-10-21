namespace Playbook.Samples;

// snippet:stream
public sealed record SensorReading(string Station, int Level, bool Valid);
public sealed record Alert(string Station, int Level, string Label);

public static class Readings
{
    public static IAsyncEnumerable<Alert> Transform(
        IAsyncEnumerable<SensorReading> source,
        Func<string, CancellationToken, ValueTask<string>> lookup) =>
        source.Where(reading => reading.Valid && reading.Level >= 70)
            .Select(async (reading, token) =>
            {
                string label = await lookup(reading.Station, token);
                return new Alert(reading.Station, reading.Level, label);
            });
}
// endsnippet:stream
