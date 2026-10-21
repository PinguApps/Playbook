using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;

namespace Playbook.Samples;

// snippet:decimal
public static partial class NumericGuidance
{
    public static decimal Accumulate(ReadOnlySpan<decimal> deliveries)
    {
        decimal total = 0m;
        foreach (decimal delivery in deliveries) total += delivery;
        return total;
    }
}
// endsnippet:decimal

// snippet:complex
public static partial class NumericGuidance
{
    public static Complex Rotate(Complex observation) => observation * new Complex(0, 1);
}
// endsnippet:complex

// snippet:conversion
public static partial class NumericGuidance
{
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
        int delimiter = input.IndexOf(';');
        ReadOnlySpan<char> cell = delimiter < 0 ? input : input[..delimiter];
        consumed = 0;
        if (!T.TryParse(cell, NumberStyles.Float, CultureInfo.InvariantCulture, out value) || !T.IsFinite(value)) return false;
        consumed = cell.Length;
        return true;
    }

    public static bool TryReadUtf8Cell<T>(ReadOnlySpan<byte> input, out T value, out int consumed)
        where T : struct, INumberBase<T>
    {
        int delimiter = input.IndexOf((byte)';');
        ReadOnlySpan<byte> cell = delimiter < 0 ? input : input[..delimiter];
        consumed = 0;
        if (!T.TryParse(cell, NumberStyles.Float, CultureInfo.InvariantCulture, out value) || !T.IsFinite(value)) return false;
        consumed = cell.Length;
        return true;
    }
}
// endsnippet:parsing

// snippet:bits
public static partial class NumericGuidance
{
    public static string WriteCalibration(double reading) => BitConverter.DoubleToUInt64Bits(reading).ToString("X16", CultureInfo.InvariantCulture);

    public static bool TryReadCalibration(string text, out double reading)
    {
        reading = default;
        if (text.Length != 16 || !ulong.TryParse(text, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out ulong bits)) return false;
        reading = BitConverter.UInt64BitsToDouble(bits);
        return true;
    }
}
// endsnippet:bits

// snippet:random
public static partial class NumericGuidance
{
    public static int PickBay(Random random, int minimum, int maximum) => random.Next(minimum, maximum);
    public static double UnitFraction(Random random) => random.NextDouble();
    public static int SecurityIndex(int count) => RandomNumberGenerator.GetInt32(count);
}
// endsnippet:random
