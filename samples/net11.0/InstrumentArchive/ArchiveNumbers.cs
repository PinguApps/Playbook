using System.Globalization;
using System.Numerics;

namespace Playbook.Samples;

// snippet:policy
public static class ArchiveNumbers
{
    // An agreed archive representation; not a lossless conversion of binary input.
    public static decimal LegacyReading(float reading)
    {
        if (!float.IsFinite(reading)) throw new ArgumentOutOfRangeException(nameof(reading), "A finite reading is required.");
        string archiveText = reading.ToString("G7", CultureInfo.InvariantCulture);
        return decimal.Parse(archiveText, NumberStyles.Float, CultureInfo.InvariantCulture);
    }

    public static decimal LegacyReading(double reading)
    {
        if (!double.IsFinite(reading)) throw new ArgumentOutOfRangeException(nameof(reading), "A finite reading is required.");
        string archiveText = reading.ToString("G15", CultureInfo.InvariantCulture);
        return decimal.Parse(archiveText, NumberStyles.Float, CultureInfo.InvariantCulture);
    }

    // Preserve decimal source intent before any binary conversion.
    public static decimal DecimalSource(string text) => decimal.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
    public static decimal AnalysisReading(double reading) => (decimal)reading;
    public static decimal DisplayReading(decimal reading) => decimal.Round(reading, 2, MidpointRounding.ToEven);
    public static double ApproximateCount(BigInteger count) => (double)count;
}
// endsnippet:policy

// snippet:constants
public static class ArchiveConstants
{
    // Rebuilding can change these embedded values even when the target is net10.0.
    public const decimal NominalReading = (decimal)6.43d;
    public const double NominalScale = (double)3_000_000_000_000.015625m;
}
// endsnippet:constants
