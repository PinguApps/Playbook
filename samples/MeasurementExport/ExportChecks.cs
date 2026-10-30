using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Playbook.Samples;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
Console.WriteLine($"runtime={RuntimeInformation.FrameworkDescription}; os={RuntimeInformation.OSDescription}; arch={RuntimeInformation.ProcessArchitecture}; dynamicCode={System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeCompiled}");
int checks = 0;
void Equal<T>(T expected, T actual, string name) where T : IEquatable<T>
{
    if (!expected.Equals(actual)) throw new InvalidOperationException($"{name}: expected {expected}, got {actual}");
    checks++;
}
void Throws<T>(Action action, string name) where T : Exception
{
    try { action(); } catch (T) { checks++; return; }
    throw new InvalidOperationException($"{name}: expected {typeof(T).Name}");
}
bool current = Environment.Version.Major == 11;
foreach (var item in new[] { (text: "10.025", old: 10.02, exact: 10.03), (text: "10.055", old: 10.06, exact: 10.05) })
{
    // Parse at run time: this is runtime behaviour, not compiler constant folding.
    double value = double.Parse(item.text, CultureInfo.InvariantCulture);
    foreach (int sign in new[] { 1, -1 })
    {
        double x = sign * value;
        double expected = sign * (current ? item.exact : item.old);
        Equal(expected, ExportPolicy.Binary(x, 2), "double midpoint");
        Equal(expected, double.Round(x, 2), "double delegate");
        Equal(expected, RoundGeneric(x, 2), "double interface");
        if (NFloat.Size == 8) Equal(expected, (double)NFloat.Round(new NFloat(x), 2), "NFloat delegate");
        Equal(sign * item.old, ExportPolicy.ScaleRoundDivide(x, 2), "explicit scaling");
        Console.WriteLine($"double input={x:R} bits={BitConverter.DoubleToUInt64Bits(x):X16} digits=2 binary={expected:R} scale={ExportPolicy.ScaleRoundDivide(x, 2):R}");
    }
}
foreach (var item in new[] { (text: "4.015", old: 4.02f, exact: 4.01f), (text: "4.025", old: 4.02f, exact: 4.03f) })
{
    float value = float.Parse(item.text, CultureInfo.InvariantCulture);
    foreach (int sign in new[] { 1, -1 })
    {
        float x = sign * value;
        float expected = sign * (current ? item.exact : item.old);
        Equal(expected, MathF.Round(x, 2), "float midpoint");
        Equal(expected, float.Round(x, 2), "float delegate");
        Equal(expected, RoundGeneric(x, 2), "float interface");
        Console.WriteLine($"float input={x:R} bits={BitConverter.SingleToUInt32Bits(x):X8} digits=2 result={expected:R}");
    }
}
double large = double.Parse("222222222222222.5", CultureInfo.InvariantCulture);
Equal(current ? large : double.Parse("222222222222222.47", CultureInfo.InvariantCulture), Math.Round(large, 2), "large finite");
Equal(double.MaxValue, Math.Round(double.MaxValue, 2), "largest finite");
Throws<OverflowException>(() => ExportPolicy.ScaleRoundDivide(double.MaxValue, 2), "scaling overflow guard");
Throws<ArgumentOutOfRangeException>(() => ExportPolicy.ScaleRoundDivide(double.NaN, 2), "nonfinite explicit policy");
Throws<ArgumentOutOfRangeException>(() => ExportPolicy.ScaleRoundDivide(1.25, 16), "bounded explicit digits");
foreach (int digits in new[] { 16, 17, int.MaxValue })
{
    double x = double.Parse("1.2345678901234567", CultureInfo.InvariantCulture);
    if (current) Equal(x, Math.Round(x, digits), "uncapped double digits");
    else Throws<ArgumentOutOfRangeException>(() => Math.Round(x, digits), "old double cap");
}
foreach (int digits in new[] { 7, 9, int.MaxValue })
{
    float x = float.Parse("1.2345678", CultureInfo.InvariantCulture);
    if (current) Equal(x, MathF.Round(x, digits), "uncapped float digits");
    else Throws<ArgumentOutOfRangeException>(() => MathF.Round(x, digits), "old float cap");
}
Half half = Half.Parse("4.015625", CultureInfo.InvariantCulture);
Equal((Half)4.02f, Half.Round(half, 2), "Half delegate");
Equal(Half.Round(half, 2), RoundGeneric(half, 2), "Half interface");
if (current) Equal(half, Half.Round(half, 9), "Half high digits");
else Throws<ArgumentOutOfRangeException>(() => Half.Round(half, 9), "old Half cap");
foreach (int digits in new[] { 0, 2, 6 })
{
    Throws<ArgumentException>(() => Math.Round(1.25, digits, (MidpointRounding)99), "invalid double mode");
    Throws<ArgumentException>(() => MathF.Round(1.25f, digits, (MidpointRounding)99), "invalid float mode");
}
Throws<ArgumentOutOfRangeException>(() => Math.Round(1.25, -1), "negative double digits");
Throws<ArgumentOutOfRangeException>(() => MathF.Round(1.25f, -1), "negative float digits");
if (current)
{
    Throws<ArgumentException>(() => Math.Round(1.25, 17, (MidpointRounding)99), "high digits invalid double mode");
    Throws<ArgumentException>(() => MathF.Round(1.25f, 9, (MidpointRounding)99), "high digits invalid float mode");
}
Equal(10.02m, ExportPolicy.DecimalQuantity(10.025m, 2), "decimal exact tie unchanged");
Equal(10.06m, ExportPolicy.DecimalQuantity(10.055m, 2), "decimal second tie unchanged");
Equal(2d, Math.Round(2.5), "no-digits double unchanged");
Equal(2f, MathF.Round(2.5f), "no-digits float unchanged");
Equal(-3d, Math.Round(-2.5, MidpointRounding.AwayFromZero), "explicit midpoint mode");

foreach (int digits in new[] { 0, 2 })
foreach (MidpointRounding mode in Enum.GetValues<MidpointRounding>())
{
    double[] input = Enumerable.Range(0, 37).Select(i => i % 2 == 0 ? 10.025 : -10.055).ToArray();
    double[] output = Enumerable.Repeat(-999d, 39).ToArray();
    TensorPrimitives.Round<double>(input, digits, mode, output);
    for (int i = 0; i < input.Length; i++) Equal(Math.Round(input[i], digits, mode), output[i], "tensor independent");
    Equal(-999d, output[^1], "tensor unused tail");
    TensorPrimitives.Round<double>(input, digits, mode, input);
    for (int i = 0; i < input.Length; i++) Equal(output[i], input[i], "tensor in-place");
    float[] single = [4.015f, -4.025f];
    float[] singleOutput = new float[2];
    TensorPrimitives.Round<float>(single, digits, mode, singleOutput);
    Equal(MathF.Round(single[0], digits, mode), singleOutput[0], "tensor float");
    Equal(MathF.Round(single[1], digits, mode), singleOutput[1], "tensor negative float");
    double[] untouched = [-999];
    Throws<ArgumentException>(() => TensorPrimitives.Round<double>(new double[] { 1, 2 }, digits, mode, untouched), "tensor capacity");
    Equal(-999d, untouched[0], "capacity failure leaves output");
    Throws<ArgumentException>(() => TensorPrimitives.Round<double>(new double[] { 1 }, digits, (MidpointRounding)99, untouched), "tensor mode");
    Equal(-999d, untouched[0], "mode failure leaves output");
}
Throws<ArgumentOutOfRangeException>(() => TensorPrimitives.Round<double>(new double[] { 1 }, -1, new double[1]), "tensor negative digits");
double[] overlap = [1, 2, 3];
Throws<ArgumentException>(() => TensorPrimitives.Round<double>(overlap.AsSpan(0, 2), 2, overlap.AsSpan(1, 2)), "shifted overlap");
double[] noDigits = [1.5, -2.5];
TensorPrimitives.Round<double>(noDigits, noDigits);
Equal(2d, noDigits[0], "tensor no digits");
Equal(-2d, noDigits[1], "tensor no digits negative");
#if NET11_0_OR_GREATER
double[] batch = [double.Parse("10.025", CultureInfo.InvariantCulture)];
BatchExport.RoundMeasurements(batch, batch);
Equal(10.03, batch[0], "canonical batch export");
#endif
foreach (Type type in new[] { typeof(double), typeof(float), typeof(Half), typeof(NFloat), typeof(TensorPrimitives) })
{
    if (type.GetCustomAttributesData().Any(a => a.AttributeType.Name == "ExperimentalAttribute") ||
        type.GetMethods().Where(m => m.Name == "Round").Any(m => m.GetCustomAttributesData().Any(a => a.AttributeType.Name == "ExperimentalAttribute")))
        throw new InvalidOperationException($"Unexpected experimental Round surface on {type}.");
}
foreach (Assembly assembly in new[] { typeof(double).Assembly, typeof(NFloat).Assembly, typeof(TensorPrimitives).Assembly }.Distinct())
    Console.WriteLine($"RUNTIME ARTIFACT {assembly.Location} version={assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion} sha256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location)))}");
Console.WriteLine($"PASS MeasurementExport {checks} checks; tensor={typeof(TensorPrimitives).Assembly.GetName().Version}; location={typeof(TensorPrimitives).Assembly.Location}; large={Math.Round(large, 2):R}");

static T RoundGeneric<T>(T value, int digits) where T : IFloatingPoint<T> => T.Round(value, digits);
