using System.Buffers.Binary;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;

namespace Playbook.Samples;

// snippet:decimal
public static partial class NumericGuidance
{
    public static T Accumulate<T>(ReadOnlySpan<T> deliveries) where T : IFloatingPointIeee754<T>
    {
        T total = T.Zero;
        foreach (T delivery in deliveries) total += delivery;
        return total;
    }
}
// endsnippet:decimal

// snippet:complex
public static partial class NumericGuidance
{
    public static Complex<T> Rotate<T>(Complex<T> observation)
        where T : IFloatingPointIeee754<T>, IMinMaxValue<T> => observation * new Complex<T>(T.Zero, T.One);
}
// endsnippet:complex

// snippet:conversion
public static partial class NumericGuidance
{
    // Preserve the source's binary value; do not interpret this as decimal source text.
    public static decimal FromBinary(double reading) => (decimal)reading;
    public static decimal InvoiceAmount(string text) => decimal.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
    public static decimal RoundInvoice(decimal amount) => decimal.Round(amount, 2, MidpointRounding.ToEven);
    public static double ApproximateCount(BigInteger count) => (double)count;
}
// endsnippet:conversion

// snippet:parsing
public static partial class NumericGuidance
{
    public static bool TryReadCell<T>(ReadOnlySpan<char> input, out T value, out int consumed)
        where T : struct, INumberBase<T>
    {
        consumed = 0;
        if (!T.TryParsePartial(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value, out int length)
            || !T.IsFinite(value) || (length < input.Length && input[length] != ';')) return false;
        consumed = length;
        return true;
    }

    public static bool TryReadUtf8Cell<T>(ReadOnlySpan<byte> input, out T value, out int consumed)
        where T : struct, INumberBase<T>
    {
        consumed = 0;
        if (!T.TryParsePartial(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value, out int length)
            || !T.IsFinite(value) || (length < input.Length && input[length] != (byte)';')) return false;
        consumed = length;
        return true;
    }
}
// endsnippet:parsing

// snippet:bits
public static partial class NumericGuidance
{
    public static string WriteCalibration(double reading) => reading.ToString("X", CultureInfo.InvariantCulture);
    public static bool TryReadCalibration(string text, out double reading) => double.TryParse(text, NumberStyles.HexFloat, CultureInfo.InvariantCulture, out reading);

    public static byte[] WriteBFloat(BFloat16 reading)
    {
        byte[] wire = new byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(wire, BitConverter.BFloat16ToUInt16Bits(reading));
        return wire;
    }

    public static BFloat16 ReadBFloat(ReadOnlySpan<byte> wire)
    {
        if (wire.Length != 2) throw new ArgumentException("Exactly two bytes required.", nameof(wire));
        return BitConverter.UInt16BitsToBFloat16(BinaryPrimitives.ReadUInt16BigEndian(wire));
    }
}
// endsnippet:bits

// snippet:random
public static partial class NumericGuidance
{
    public static T PickBay<T>(Random random, T minimum, T maximum) where T : IBinaryInteger<T> => random.NextInteger(minimum, maximum);
    public static T UnitFraction<T>(Random random) where T : IBinaryFloatingPointIeee754<T> => random.NextBinaryFloat<T>();
    public static int SecurityIndex(int count) => RandomNumberGenerator.GetInt32(count);
}
// endsnippet:random
