using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Playbook.Samples;

int checks = 0;
void Require(bool condition, string claim)
{
    if (!condition) throw new InvalidOperationException(claim);
    checks++;
}
void Reject<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { checks++; return; }
    throw new InvalidOperationException($"Expected {typeof(T).Name}");
}

foreach ((double input, decimal expected) in new (double, decimal)[]
{
    (6.43, 6.43m), (-6.43, -6.43m), (0, 0m), (-0d, 0m),
    (1.234567890123456e-16, 0.0000000000000001234567890123m),
    (1.234567890123456e25, 12345678901234600000000000m),
    (123456789012344.5, 123456789012344m), (123456789012345.5, 123456789012346m),
    (-123456789012344.5, -123456789012344m), (-123456789012345.5, -123456789012346m),
    (double.Epsilon, 0m), (1e-28, 0.0000000000000000000000000001m)
})
{
    Require(ArchiveNumbers.LegacyReading(input) == expected, $"G15 archive {input:R}");
    Console.WriteLine($"G15 {input:R} -> {ArchiveNumbers.LegacyReading(input):G29}");
}
foreach ((float input, decimal expected) in new (float, decimal)[]
{
    (6.43f, 6.43m), (-6.43f, -6.43m), (0f, 0m),
    (1.2345678e-12f, 0.000000000001234568m), (1.2345678e25f, 12345680000000000000000000m),
    (1234566.5f, 1234566m), (1234567.5f, 1234568m),
    (-1234566.5f, -1234566m), (-1234567.5f, -1234568m), (float.Epsilon, 0m)
}) Require(ArchiveNumbers.LegacyReading(input) == expected, $"G7 archive {input:R}");

// These must remain invariant even in a comma-decimal UI culture.
CultureInfo original = CultureInfo.CurrentCulture;
try
{
    CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
    Require(ArchiveNumbers.LegacyReading(6.43) == 6.43m && ArchiveNumbers.LegacyReading(6.43f) == 6.43m, "invariant archive");
}
finally { CultureInfo.CurrentCulture = original; }
foreach (double value in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
    Reject<ArgumentOutOfRangeException>(() => ArchiveNumbers.LegacyReading(value));
foreach (float value in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
    Reject<ArgumentOutOfRangeException>(() => ArchiveNumbers.LegacyReading(value));
foreach (double value in new[] { double.MaxValue, -double.MaxValue, 1e29, -1e29 })
    Reject<OverflowException>(() => ArchiveNumbers.LegacyReading(value));
foreach (float value in new[] { float.MaxValue, -float.MaxValue, 1e29f, -1e29f })
    Reject<OverflowException>(() => ArchiveNumbers.LegacyReading(value));
Require(ArchiveNumbers.DecimalSource("6.43") == 6.43m, "decimal source intent");
Require(ArchiveNumbers.DisplayReading(8.125m) == 8.12m && ArchiveNumbers.DisplayReading(-8.135m) == -8.14m, "explicit decimal places and ties");

double reading = ReadDouble("6.43");
decimal direct = ArchiveNumbers.AnalysisReading(reading);
#if NET11_0_OR_GREATER
Require(direct == 6.429999999999999715782905696m, "current exact binary source");
#else
Require(direct == 6.43m, "previous significant digits");
#endif
foreach (decimal route in new[] { new decimal(reading), Convert.ToDecimal(reading), decimal.CreateChecked(reading), decimal.CreateSaturating(reading), decimal.CreateTruncating(reading), ((IConvertible)reading).ToDecimal(CultureInfo.InvariantCulture) })
    Require(route == direct, "double constructor/Convert/generic/IConvertible routes");
float single = ReadSingle("6.43");
decimal singleDirect = (decimal)single;
foreach (decimal route in new[] { new decimal(single), Convert.ToDecimal(single), decimal.CreateChecked(single), decimal.CreateSaturating(single), decimal.CreateTruncating(single), ((IConvertible)single).ToDecimal(CultureInfo.InvariantCulture) })
    Require(route == singleDirect, "float constructor/Convert/generic/IConvertible routes");
Require(singleDirect == (Environment.Version.Major >= 11 ? 6.429999828338623046875m : 6.43m), "float binary intent");
Reject<OverflowException>(() => ArchiveNumbers.AnalysisReading(double.NaN));
Reject<OverflowException>(() => ArchiveNumbers.AnalysisReading(double.PositiveInfinity));

// Original near-midpoint values expose intermediate rounding. Inputs are parsed at runtime.
decimal scale = ReadDecimal("3000000000000.015625");
decimal narrow = ReadDecimal("3.000000119209289550781250001");
decimal half = ReadDecimal("5.00195312501");
Console.WriteLine($"Decimal->double {BitConverter.DoubleToUInt64Bits((double)scale):X16}; ->float {BitConverter.SingleToUInt32Bits((float)narrow):X8}; ->Half {BitConverter.HalfToUInt16Bits((Half)half):X4}");
foreach (double route in new[] { decimal.ToDouble(scale), Convert.ToDouble(scale), double.CreateChecked(scale), double.CreateSaturating(scale), double.CreateTruncating(scale), ((IConvertible)scale).ToDouble(CultureInfo.InvariantCulture) })
    Require(BitConverter.DoubleToUInt64Bits(route) == BitConverter.DoubleToUInt64Bits((double)scale), "decimal to double routes");
foreach (float route in new[] { decimal.ToSingle(narrow), Convert.ToSingle(narrow), float.CreateChecked(narrow), float.CreateSaturating(narrow), float.CreateTruncating(narrow), ((IConvertible)narrow).ToSingle(CultureInfo.InvariantCulture) })
    Require(BitConverter.SingleToUInt32Bits(route) == BitConverter.SingleToUInt32Bits((float)narrow), "decimal to float routes");
foreach (Half route in new[] { Half.CreateChecked(half), Half.CreateSaturating(half), Half.CreateTruncating(half) })
    Require(route == (Half)half, "decimal to Half generic routes");
#if NET11_0_OR_GREATER
Require(BitConverter.DoubleToUInt64Bits((double)scale) == 0x4285D3EF79800020, "decimal double retains exact representable fraction");
Require((float)narrow == float.BitIncrement(3f), "decimal float rounds above midpoint");
Require((Half)half == Half.BitIncrement((Half)5), "decimal Half rounds above midpoint");
#else
Require(BitConverter.DoubleToUInt64Bits((double)scale) == 0x4285D3EF79800021, "previous decimal double differs by one ulp");
Require((float)narrow == 3f, "previous intermediate float midpoint");
Require((Half)half == (Half)5, "previous intermediate Half midpoint");
#endif
Require((double)(-scale) == -(double)scale && (float)(-narrow) == -(float)narrow && (Half)(-half) == -(Half)half, "decimal negative symmetry");
decimal veryCloseHalf = ReadDecimal("5.001953125000000000000000001");
Require((Half)veryCloseHalf == (Half)5, "measured double-intermediate limitation remains near a Half midpoint");
Console.WriteLine("LIMIT: Decimal 5.001953125000000000000000001 -> Half 4500 on both artifacts; no universal single-rounding claim.");

BigInteger count64 = (BigInteger.One << 90) + (BigInteger.One << 37) + 1;
BigInteger count32 = (BigInteger.One << 70) + (BigInteger.One << 46) + 1;
BigInteger count16 = new(16_393); // All finite Half-range integer inputs are exact in double.
foreach (int sign in new[] { 1, -1 })
{
    double converted64 = (double)(sign * count64);
    float converted32 = (float)(sign * count32);
    Half converted16 = (Half)(sign * count16);
    Console.WriteLine($"BigInteger sign={sign} double={BitConverter.DoubleToUInt64Bits(converted64):X16} float={BitConverter.SingleToUInt32Bits(converted32):X8} Half={BitConverter.HalfToUInt16Bits(converted16):X4}");
#if NET11_0_OR_GREATER
    Require(converted64 == sign * double.BitIncrement(Math.ScaleB(1d, 90)), "BigInteger double nearest");
    Require(converted32 == sign * float.BitIncrement(MathF.ScaleB(1f, 70)), "BigInteger float single rounding");
#else
    Require(converted64 == sign * Math.ScaleB(1d, 90), "previous BigInteger double truncation");
    Require(converted32 == sign * MathF.ScaleB(1f, 70), "previous BigInteger float midpoint");
#endif
    Require(converted16 == (Half)(sign * 16_400), "BigInteger Half nearest control (no changed finite result claimed)");
    Require(double.CreateChecked(sign * count64) == converted64 && double.CreateSaturating(sign * count64) == converted64 && double.CreateTruncating(sign * count64) == converted64, "BigInteger double generic routes");
    Require(float.CreateChecked(sign * count32) == converted32 && float.CreateSaturating(sign * count32) == converted32 && float.CreateTruncating(sign * count32) == converted32, "BigInteger float generic routes");
    Require(Half.CreateChecked(sign * count16) == converted16 && Half.CreateSaturating(sign * count16) == converted16 && Half.CreateTruncating(sign * count16) == converted16, "BigInteger Half generic routes");
}
Require(Half.IsPositiveInfinity((Half)new BigInteger(65_520)), "Half integer overflow midpoint");
Require(Half.IsNegativeInfinity((Half)new BigInteger(-65_520)), "Half negative overflow midpoint");
Require((Half)new BigInteger(65_504) == Half.MaxValue, "Half maximum finite integer");
Require((Half)new BigInteger(2_049) == (Half)2_048 && (Half)new BigInteger(2_051) == (Half)2_052, "Half integer ties to even");

#if NET11_0_OR_GREATER
decimal brain = ReadDecimal("5.01562500001");
BFloat16 brainResult = (BFloat16)brain;
Require(brainResult == BFloat16.BitIncrement((BFloat16)5), "Decimal BFloat16 avoids float intermediate");
BigInteger brainCount = (BigInteger.One << 70) + (BigInteger.One << 62) + 1;
Require((BFloat16)brainCount == BFloat16.BitIncrement((BFloat16)Math.ScaleB(1d, 70)), "BigInteger BFloat16 single rounding");
Require(BFloat16.CreateChecked(brain) == brainResult && BFloat16.CreateSaturating(brain) == brainResult && BFloat16.CreateTruncating(brain) == brainResult, "BFloat16 decimal generic routes");
Console.WriteLine($"BFloat16 decimal={BitConverter.BFloat16ToUInt16Bits(brainResult):X4}; integer={BitConverter.BFloat16ToUInt16Bits((BFloat16)brainCount):X4}; unavailable on net10.");
#endif

// Reflection reads embedded metadata, separately from runtime-input casts.
decimal embedded = (decimal)typeof(ArchiveConstants).GetField(nameof(ArchiveConstants.NominalReading))!.GetValue(null)!;
double embeddedScale = (double)typeof(ArchiveConstants).GetField(nameof(ArchiveConstants.NominalScale))!.GetRawConstantValue()!;
#if NET11_0_OR_GREATER || SDK11_COMPILER
Require(embedded == 6.429999999999999715782905696m, "new SDK folded decimal");
Require(BitConverter.DoubleToUInt64Bits(embeddedScale) == 0x4285D3EF79800020, "new SDK folded double");
#else
Require(embedded == 6.43m, "old SDK folded decimal");
Require(BitConverter.DoubleToUInt64Bits(embeddedScale) == 0x4285D3EF79800021, "old SDK folded double");
#endif
Console.WriteLine($"EMBEDDED nominal={embedded:G29}; scaleBits={BitConverter.DoubleToUInt64Bits(embeddedScale):X16}; RUNTIME nominal={direct:G29}");
foreach (Assembly assembly in new[] { typeof(decimal).Assembly, typeof(BigInteger).Assembly })
    Console.WriteLine($"ARTIFACT {assembly.GetName()} sha256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location)))}");
foreach (Type type in new[] { typeof(decimal), typeof(BigInteger), typeof(Half) })
{
    Require(!type.GetCustomAttributesData().Any(attribute => attribute.AttributeType.Name == "ExperimentalAttribute"), $"no experimental opt-in for {type}");
    Require(!type.GetMethods().Any(method => method.GetCustomAttributesData().Any(attribute => attribute.AttributeType.Name == "ExperimentalAttribute")), $"no experimental method opt-in for {type}");
}
#if NET11_0_OR_GREATER
Require(!typeof(BFloat16).GetCustomAttributesData().Any(attribute => attribute.AttributeType.Name == "ExperimentalAttribute"), "BFloat16 has no experimental type opt-in");
#endif
Console.WriteLine($"PASS {checks} instrument checks; runtime={RuntimeInformation.FrameworkDescription}; OS={RuntimeInformation.OSDescription}; arch={RuntimeInformation.ProcessArchitecture}; culture={CultureInfo.CurrentCulture.Name}");

[MethodImpl(MethodImplOptions.NoInlining)]
static double ReadDouble(string text) => double.Parse(text, CultureInfo.InvariantCulture);
[MethodImpl(MethodImplOptions.NoInlining)]
static float ReadSingle(string text) => float.Parse(text, CultureInfo.InvariantCulture);
[MethodImpl(MethodImplOptions.NoInlining)]
static decimal ReadDecimal(string text) => decimal.Parse(text, CultureInfo.InvariantCulture);
