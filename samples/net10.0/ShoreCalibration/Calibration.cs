namespace ShoreCalibration;

// snippet:calibration
// Managed fallback for the same bounded calculation; this does not reuse a C library.
public static class Calibration
{
    public static int ConvertReading(int raw)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(raw);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(raw, 10000);
        return (raw * 7 + 5) / 10;
    }
}
// endsnippet:calibration
