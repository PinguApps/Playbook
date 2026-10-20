using System.Globalization;

namespace Playbook.Samples;

// snippet:helpers
public interface IObservationSource
{
    string Station { get; }
}

public static class ObservationLabels
{
    public static string CreateLabel(string station, int sequence)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(station);
        ArgumentOutOfRangeException.ThrowIfNegative(sequence);
        return NormalizeStation(station) + "-" + sequence.ToString("D3", CultureInfo.InvariantCulture);
    }

    private static string NormalizeStation(string station) => station.Trim().ToUpperInvariant();
}
// endsnippet:helpers

// snippet:consumer
public sealed class RoofSensor : IObservationSource
{
    public string Station => " roof ";
}

public static class ObservationReport
{
    public static string Label(IObservationSource source, int sequence) =>
        ObservationLabels.CreateLabel(source.Station, sequence);
}
// endsnippet:consumer
