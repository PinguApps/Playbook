namespace Playbook.Samples;

// snippet:policy
public static class OffsetPolicy
{
    // CoreCLR .NET 11: truncate finite fractions, saturate bounds, NaN becomes zero.
    public static ushort Saturate(double value) => unchecked((ushort)value);

    public static ushort CheckedOffset(double value, int recordLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(recordLength);
        if (!double.IsFinite(value) || value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        ushort offset = checked((ushort)value); // Reject representation overflow; truncate fractions.
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((int)offset, recordLength, nameof(value));
        return offset;
    }

    // .NET 11 CoreCLR small-destination saturation, for double and float sources.
    public static ushort Convert(double value) => double.ConvertToInteger<ushort>(value);
    public static ushort Convert(float value) => float.ConvertToInteger<ushort>(value);

    // These select different policies; neither promises all historical outputs.
    public static ushort Native(double value) => double.ConvertToIntegerNative<ushort>(value);
    public static ushort ViaSaturatedInt(double value) => unchecked((ushort)(int)value);
    public static ushort ViaNativeInt(double value) => unchecked((ushort)double.ConvertToIntegerNative<int>(value));
}
// endsnippet:policy
