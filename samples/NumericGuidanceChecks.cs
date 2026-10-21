using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using Playbook.Samples;

static void Require(bool condition, string claim)
{
    if (!condition) throw new InvalidOperationException(claim);
}

static void Reject<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new InvalidOperationException($"Expected {typeof(T).Name}");
}

#if NET11_0_OR_GREATER
Require(NumericGuidance.Accumulate<Decimal32>([(Decimal32)10_000_000, (Decimal32)1]) == (Decimal32)10_000_000, "Decimal32 seven-digit rounding");
Require(NumericGuidance.Accumulate<Decimal64>([(Decimal64)10_000_000, (Decimal64)1]) == (Decimal64)10_000_001, "Decimal64 retains small increment");
Decimal128 large = Decimal128.Parse("100000000000000000000000000000000", CultureInfo.InvariantCulture);
Require(NumericGuidance.Accumulate<Decimal128>([large, Decimal128.One]) - large == Decimal128.One, "Decimal128 34-digit precision");
Require(Decimal32.IsPositiveInfinity(Decimal32.One / Decimal32.Zero), "IEEE decimal infinity");
Require(Decimal64.IsNaN(Decimal64.Zero / Decimal64.Zero), "IEEE decimal NaN");
Require(NumericGuidance.Accumulate<Decimal64>([Decimal64.Parse("0.07", CultureInfo.InvariantCulture), Decimal64.Parse("0.13", CultureInfo.InvariantCulture)]) == Decimal64.Parse("0.20", CultureInfo.InvariantCulture), "decimal intent");
Complex<float> rotated = NumericGuidance.Rotate(new Complex<float>(4, -3));
Require(rotated.Real == 3 && rotated.Imaginary == 4, "float complex rotation");
Complex<Decimal64> decimalRotated = NumericGuidance.Rotate(new Complex<Decimal64>((Decimal64)4, (Decimal64)(-3)));
Require(decimalRotated.Real == (Decimal64)3 && decimalRotated.Imaginary == (Decimal64)4, "decimal complex rotation");
#else
Require(NumericGuidance.Accumulate([10_000_000m, 1m]) == 10_000_001m, "System.Decimal finite accumulation");
Require(NumericGuidance.Accumulate([0.07m, 0.13m]) == 0.20m, "decimal intent");
Reject<DivideByZeroException>(() => { decimal zero = 0m; _ = 1m / zero; });
Complex rotated = NumericGuidance.Rotate(new Complex(4, -3));
Require(rotated.Real == 3 && rotated.Imaginary == 4, "double complex rotation");
#endif

Complex dividedByZero = new Complex(3, -4) / Complex.Zero;
Console.WriteLine($"Complex (3,-4)/zero: {dividedByZero}");
#if NET11_0_OR_GREATER
Require(double.IsPositiveInfinity(dividedByZero.Real) && double.IsNegativeInfinity(dividedByZero.Imaginary), "directed complex infinities");
#else
Require(double.IsNaN(dividedByZero.Real) && double.IsNaN(dividedByZero.Imaginary), "older complex special-value result");
#endif

double binaryReading = double.Parse("2.71", CultureInfo.InvariantCulture);
decimal converted = NumericGuidance.FromBinary(binaryReading);
Console.WriteLine($"binary 2.71 -> decimal {converted.ToString("G29", CultureInfo.InvariantCulture)}");
#if NET11_0_OR_GREATER
Require(converted == 2.709999999999999964472863212m, "single rounded binary source");
#else
Require(converted == 2.71m, "older 15-digit conversion");
#endif
Require(NumericGuidance.InvoiceAmount("2.71") == 2.71m, "decimal source survives upgrade");
Require(NumericGuidance.RoundInvoice(8.125m) == 8.12m && NumericGuidance.RoundInvoice(8.135m) == 8.14m, "explicit ties-to-even");
Reject<OverflowException>(() => NumericGuidance.FromBinary(double.NaN));
Reject<OverflowException>(() => NumericGuidance.FromBinary(double.PositiveInfinity));
BigInteger count = (BigInteger.One << 80) + (BigInteger.One << 27) + 1;
double approximate = NumericGuidance.ApproximateCount(count);
double exactLower = Math.ScaleB(1d, 80);
#if NET11_0_OR_GREATER
Require(approximate == Math.BitIncrement(exactLower), "BigInteger rounds upward after midpoint");
#else
Require(approximate == exactLower, "older BigInteger truncation");
#endif
Console.WriteLine($"count -> double bits {BitConverter.DoubleToUInt64Bits(approximate):X16}");

Require(NumericGuidance.TryReadCell<decimal>(" -14.25;tail", out decimal cell, out int consumed) && cell == -14.25m && consumed == 7, "character cell boundary");
Require(NumericGuidance.TryReadUtf8Cell<double>("6.25e1;tail"u8, out double utf8, out int bytes) && utf8 == 62.5 && bytes == 6, "UTF8 byte boundary");
foreach (string malformed in new[] { "", ";tail", "12x;tail", "1e;tail", "1,25;tail", "NaN;tail", "Infinity;tail", "1e9999;tail" })
{
    Require(!NumericGuidance.TryReadCell<double>(malformed, out _, out _), $"Reject malformed/nonfinite cell {malformed}");
    Require(!NumericGuidance.TryReadUtf8Cell<double>(System.Text.Encoding.UTF8.GetBytes(malformed), out _, out _), $"Reject malformed/nonfinite UTF8 cell {malformed}");
}
Require(NumericGuidance.TryReadCell<int>("37", out int last, out int end) && last == 37 && end == 2, "last undelimited field");
Require(!NumericGuidance.TryReadCell<int>("9999999999999999999999;", out _, out _), "integer overflow rejected");

foreach (double value in new[] { 19.375, -0d, double.Epsilon, double.MaxValue, -0.03125 })
{
    string encoded = NumericGuidance.WriteCalibration(value);
    Require(NumericGuidance.TryReadCalibration(encoded, out double restored) && BitConverter.DoubleToUInt64Bits(restored) == BitConverter.DoubleToUInt64Bits(value), "exact finite bits including signed zero");
    Console.WriteLine($"calibration {encoded}");
}
Require(!NumericGuidance.TryReadCalibration("broken-calibration", out _), "malformed bit text rejected");
#if NET11_0_OR_GREATER
foreach (ushort bits in new ushort[] { 0x3F88, 0x8000, 0x7F80, 0x7FC5 })
{
    BFloat16 value = BitConverter.UInt16BitsToBFloat16(bits);
    byte[] wire = NumericGuidance.WriteBFloat(value);
    Require(wire[0] == (byte)(bits >> 8) && wire[1] == (byte)bits, "explicit big endian wire");
    Require(BitConverter.BFloat16ToUInt16Bits(NumericGuidance.ReadBFloat(wire)) == bits, "BFloat raw payload including NaN");
    Require(BitConverter.BFloat16ToUInt16Bits(BitConverter.ToBFloat16(BitConverter.GetBytes(value))) == bits, "native endian BitConverter pair");
}
Reject<ArgumentException>(() => NumericGuidance.ReadBFloat([0x3F]));
// Textual NaN is a value marker, not a contract to preserve a NaN payload.
double payloadNaN = BitConverter.UInt64BitsToDouble(0x7FF8000000000037);
double textNaN = double.Parse(payloadNaN.ToString("X", CultureInfo.InvariantCulture), NumberStyles.HexFloat, CultureInfo.InvariantCulture);
Require(double.IsNaN(textNaN), "NaN remains NaN");
Console.WriteLine($"NaN payload before/after text: {BitConverter.DoubleToUInt64Bits(payloadNaN):X16}/{BitConverter.DoubleToUInt64Bits(textNaN):X16}");
foreach (float value in new[] { 0.15625f, -0f, float.Epsilon })
    Require(BitConverter.SingleToUInt32Bits(float.Parse(value.ToString("X", CultureInfo.InvariantCulture), NumberStyles.HexFloat, CultureInfo.InvariantCulture)) == BitConverter.SingleToUInt32Bits(value), "float hex bits");
foreach (Half value in new[] { (Half)0.15625f, (Half)(-0f), Half.Epsilon })
    Require(BitConverter.HalfToUInt16Bits(Half.Parse(value.ToString("X", CultureInfo.InvariantCulture), NumberStyles.HexFloat, CultureInfo.InvariantCulture)) == BitConverter.HalfToUInt16Bits(value), "Half hex bits");
#endif

Random random = new(731);
for (int i = 0; i < 4096; i++)
{
#if NET11_0_OR_GREATER
    int bay = NumericGuidance.PickBay(random, -9, 17);
    byte small = NumericGuidance.PickBay(random, (byte)3, (byte)13);
    Int128 wide = NumericGuidance.PickBay(random, Int128.MinValue, Int128.MaxValue);
    Require(small >= 3 && small < 13 && wide < Int128.MaxValue, "generic integer bounds");
    Half fraction = NumericGuidance.UnitFraction<Half>(random);
    BFloat16 brain = NumericGuidance.UnitFraction<BFloat16>(random);
    Require(fraction >= (Half)0 && fraction < (Half)1 && brain >= BFloat16.Zero && brain < BFloat16.One, "reduced binary float bounds");
    double unit = NumericGuidance.UnitFraction<double>(random);
#else
    int bay = NumericGuidance.PickBay(random, -9, 17);
    double unit = NumericGuidance.UnitFraction(random);
#endif
    Require(bay >= -9 && bay < 17 && unit >= 0 && unit < 1, "simulation ranges");
}
Require(NumericGuidance.PickBay(random, 5, 5) == 5, "equal integer bounds");
Reject<ArgumentOutOfRangeException>(() => NumericGuidance.PickBay(random, 5, 4));
#if NET11_0_OR_GREATER
Require(random.NextInteger<int>(0) == 0, "zero maximum");
Reject<ArgumentOutOfRangeException>(() => random.NextInteger<int>(-1));
for (int i = 0; i < 64; i++) Require(random.NextInteger<int>() >= 0, "parameterless nonnegative inclusive maximum contract");
#endif
int securityIndex = NumericGuidance.SecurityIndex(23);
Require(securityIndex >= 0 && securityIndex < 23, "cryptographic bounded alternative");
Console.WriteLine($"PASS: decimal/complex representations, conversion, partial parsing, exact bits and random bounds; runtime {RuntimeInformation.FrameworkDescription}");
