using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using SensorArchive;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
Console.WriteLine($"RUNTIME {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}");
float[] readings = [12.51f, 12.6f, 12.7f, 12.8f];
var stored = SensorBatch.Store(readings);
float expectedMean = readings.Sum() / readings.Length;
float actualMean = SensorBatch.Mean(stored);
Check(MathF.Abs(actualMean - expectedMean) <= 0.02f, "mean absolute error budget 0.02");
for (int i = 0; i < readings.Length; i++)
{
    float restored = (float)stored[i];
    Check(MathF.Abs(restored - readings[i]) <= 0.032f, "per-reading absolute error budget 0.032");
    Console.WriteLine($"READING input={readings[i]:R} stored={restored:R} absoluteError={MathF.Abs(restored - readings[i]):R}");
}
Console.WriteLine($"MEAN expected={expectedMean:R} actual={actualMean:R} error={MathF.Abs(actualMean - expectedMean):R} budget=0.02");
Reject<ArgumentException>(() => SensorBatch.Mean([]));
foreach (float value in new[] { 1e30f, 1e-30f, 1.5f, -2.75f })
{
    float restored = (float)SensorBatch.Store([value])[0];
    Check(float.IsFinite(restored) && restored != 0, "retain finite magnitude");
    Check(MathF.Abs((restored - value) / value) <= 1f / 256, "relative rounding budget for these normal values");
    Console.WriteLine($"MAGNITUDE input={value:R} restored={restored:R} Half={(float)(Half)value:R}");
}

#if NET11_0_OR_GREATER
foreach (float value in new[] { 0f, -0f, 1.5f, -2.75f, MathF.ScaleB(1, -126), MathF.ScaleB(1, -133) })
{
    float restored = (float)(BFloat16)value;
    Check(BitConverter.SingleToInt32Bits(restored) == BitConverter.SingleToInt32Bits(value), "exact conversion including signed zero and subnormal");
    Console.WriteLine($"EXACT input={value:R} restored={restored:R} negative={float.IsNegative(restored)}");
}
foreach (var pair in new[] { (1.00390625f, 1f), (1.01171875f, 1.015625f), (1.006f, 1.0078125f) })
{
    float rounded = (float)(BFloat16)pair.Item1;
    Check(rounded == pair.Item2, "nearest-even conversion");
    Console.WriteLine($"ROUND input={pair.Item1:R} actual={rounded:R} expected={pair.Item2:R}");
}
Check((float)(BFloat16)MathF.ScaleB(1, -134) == 0, "underflow tie rounds to zero");
Check(BFloat16.IsPositiveInfinity((BFloat16)float.MaxValue), "largest single rounds to infinity");
foreach (float value in new[] { float.PositiveInfinity, float.NegativeInfinity, float.NaN })
{
    float restored = (float)(BFloat16)value;
    Check(float.IsNaN(value) ? float.IsNaN(restored) : restored == value, "special-value classification");
    Console.WriteLine($"SPECIAL input={value:R} restored={restored:R}");
}
BFloat16 lowPrecisionSum = SensorMath.Add<BFloat16>((BFloat16)1, (BFloat16)0.001f);
float wideSum = SensorMath.Add<float>(1, 0.001f);
Check((float)lowPrecisionSum == 1 && wideSum > 1, "generic arithmetic rounds in its chosen type");
Check((float)(BFloat16)1.5f + (float)(BFloat16)2.25f == 3.75f, "widening arithmetic");
Check((float)((BFloat16)1.5f + (BFloat16)2.25f) == 3.75f, "exact BFloat16 arithmetic");
Console.WriteLine($"GENERIC IFloatingPointIeee754<T> BFloat16={ (float)lowPrecisionSum:R} float={wideSum:R}; signed-zero/NaN classification preserved, payload identity not claimed");
#else
Check(SensorBatch.Store(readings).SequenceEqual(readings), "old float storage preserves input");
Check(typeof(float).Assembly.GetType("System.Numerics.BFloat16") is null, "no BFloat16 framework type on old runtime");
Console.WriteLine("OLDER float retains input precision and range; Half overflows 1e30 and underflows 1e-30. No custom BFloat16 bit implementation.");
#endif
Console.WriteLine("PASS SensorArchive: precision, range, special values and declared calculation budget.");

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
static void Reject<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new InvalidOperationException($"Expected {typeof(T).Name}");
}
