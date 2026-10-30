namespace Playbook.Samples;

// snippet:policy
public static class OffsetPolicy
{
    // Explicit saturation also works before small floating-point casts saturate.
    public static ushort Saturate(double value) => double.IsNaN(value) || value <= 0
        ? (ushort)0 : value >= ushort.MaxValue ? ushort.MaxValue : checked((ushort)value);

    public static ushort CheckedOffset(double value, int recordLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(recordLength);
        if (!double.IsFinite(value) || value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        ushort offset = checked((ushort)value); // Reject representation overflow; truncate fractions.
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((int)offset, recordLength, nameof(value));
        return offset;
    }

    // Already available on .NET 10; small-destination results follow that runtime.
    public static ushort Convert(double value) => double.ConvertToInteger<ushort>(value);
    public static ushort Convert(float value) => float.ConvertToInteger<ushort>(value);

    // These select different policies; neither promises all historical outputs.
    public static ushort Native(double value) => double.ConvertToIntegerNative<ushort>(value);
    public static ushort ViaSaturatedInt(double value) => unchecked((ushort)(int)value);
    public static ushort ViaNativeInt(double value) => unchecked((ushort)double.ConvertToIntegerNative<int>(value));
}
// endsnippet:policy
