using System.Runtime.CompilerServices;

namespace Playbook.Samples;

// snippet:stream
public sealed record SensorReading(string Station, int Level, bool Valid);
public sealed record Alert(string Station, int Level, string Label);

public static class Readings
{
    public static async IAsyncEnumerable<Alert> Transform(
        IAsyncEnumerable<SensorReading> source,
        Func<string, CancellationToken, ValueTask<string>> lookup,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (SensorReading reading in source.WithCancellation(cancellationToken))
        {
            if (reading.Valid && reading.Level >= 70)
            {
                string label = await lookup(reading.Station, cancellationToken);
                yield return new Alert(reading.Station, reading.Level, label);
            }
        }
    }
}
// endsnippet:stream
