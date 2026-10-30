using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Playbook.Samples;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
Console.WriteLine($"runtime={RuntimeInformation.FrameworkDescription}; os={RuntimeInformation.OSDescription}; arch={RuntimeInformation.ProcessArchitecture}; dynamicCode={RuntimeFeature.IsDynamicCodeCompiled}");
int checks = 0;
void Equal(long expected, long actual, string name)
{
    if (expected != actual) throw new InvalidOperationException($"{name}: expected {expected}, got {actual}");
    checks++;
}
void Throws<T>(Action action, string name) where T : Exception
{
    try { action(); } catch (T) { checks++; return; }
    throw new InvalidOperationException($"{name}: expected {typeof(T).Name}");
}
void Matrix<T>(long minimum, long maximum, Func<double, long> castDouble, Func<float, long> castFloat,
    Func<Half, long> castHalf, Func<double, long> checkedDouble, Func<float, long> checkedFloat,
    Func<Half, long> checkedHalf) where T : IBinaryInteger<T>
{
    double[] values = [double.Parse("23.875", CultureInfo.InvariantCulture), -23.875, minimum, maximum,
        minimum - 1d, maximum + 1d, minimum - 0.75, maximum + 0.75,
        70019, -70019, double.NegativeInfinity, double.PositiveInfinity, double.NaN];
    foreach (double x in values)
    {
        long expected = double.IsNaN(x) ? 0 : x <= minimum ? minimum : x >= maximum ? maximum : (long)Math.Truncate(x);
        long direct = castDouble(x);
        long generic = long.CreateChecked(double.ConvertToInteger<T>(x));
        if (Environment.Version.Major == 11)
        {
            Equal(expected, direct, "double cast saturation");
            Equal(expected, generic, "double ConvertToInteger saturation");
        }
        else if (double.IsFinite(x) && x >= minimum && x <= maximum)
        {
            Equal(expected, direct, "old in-range double cast");
            Equal(expected, generic, "old in-range ConvertToInteger");
        }
        bool fits = double.IsFinite(x) && Math.Truncate(x) >= minimum && Math.Truncate(x) <= maximum;
        if (fits) Equal((long)Math.Truncate(x), checkedDouble(x), "checked double");
        else Throws<OverflowException>(() => checkedDouble(x), "checked double overflow");
        long native = long.CreateChecked(double.ConvertToIntegerNative<T>(x));
        Console.WriteLine($"{typeof(T).Name} double input={x:R} cast={direct} convert={generic} native={native}");

        float f = (float)x;
        long single = castFloat(f);
        long singleGeneric = long.CreateChecked(float.ConvertToInteger<T>(f));
        if (Environment.Version.Major == 11)
        {
            Equal(expected, single, "float cast saturation");
            Equal(expected, singleGeneric, "float ConvertToInteger saturation");
        }
        else if (float.IsFinite(f) && f >= minimum && f <= maximum)
        {
            Equal(expected, single, "old in-range float cast");
            Equal(expected, singleGeneric, "old in-range float ConvertToInteger");
        }
        if (fits) Equal((long)MathF.Truncate(f), checkedFloat(f), "checked float");
        else Throws<OverflowException>(() => checkedFloat(f), "checked float overflow");
        Console.WriteLine($"{typeof(T).Name} float input={f:R} cast={single} convert={singleGeneric} native={long.CreateChecked(float.ConvertToIntegerNative<T>(f))}");
    }
    // Half has a smaller range/precision: derive expectations from the represented input.
    foreach (Half h in new Half[] { Half.Parse("23.875", CultureInfo.InvariantCulture), (Half)(-23.875f),
        (Half)minimum, (Half)maximum, (Half)(minimum - 1), (Half)(maximum + 1),
        Half.NegativeInfinity, Half.PositiveInfinity, Half.NaN })
    {
        double x = (double)h;
        long expected = double.IsNaN(x) ? 0 : x <= minimum ? minimum : x >= maximum ? maximum : (long)Math.Truncate(x);
        long actual = castHalf(h);
        if (Environment.Version.Major == 11 || (double.IsFinite(x) && x >= minimum && x <= maximum)) Equal(expected, actual, "Half cast");
        bool fits = double.IsFinite(x) && Math.Truncate(x) >= minimum && Math.Truncate(x) <= maximum;
        if (fits) Equal((long)Math.Truncate(x), checkedHalf(h), "checked Half");
        else Throws<OverflowException>(() => checkedHalf(h), "checked Half overflow");
        Console.WriteLine($"{typeof(T).Name} Half input={h} cast={actual}");
    }
}
Matrix<sbyte>(sbyte.MinValue, sbyte.MaxValue, x => unchecked((sbyte)x), x => unchecked((sbyte)x), x => unchecked((sbyte)x), x => checked((sbyte)x), x => checked((sbyte)x), x => checked((sbyte)x));
Matrix<byte>(byte.MinValue, byte.MaxValue, x => unchecked((byte)x), x => unchecked((byte)x), x => unchecked((byte)x), x => checked((byte)x), x => checked((byte)x), x => checked((byte)x));
Matrix<short>(short.MinValue, short.MaxValue, x => unchecked((short)x), x => unchecked((short)x), x => unchecked((short)x), x => checked((short)x), x => checked((short)x), x => checked((short)x));
Matrix<ushort>(ushort.MinValue, ushort.MaxValue, x => unchecked((ushort)x), x => unchecked((ushort)x), x => unchecked((ushort)x), x => checked((ushort)x), x => checked((ushort)x), x => checked((ushort)x));
Matrix<char>(char.MinValue, char.MaxValue, x => unchecked((char)x), x => unchecked((char)x), x => unchecked((char)x), x => checked((char)x), x => checked((char)x), x => checked((char)x));

Equal(23, OffsetPolicy.CheckedOffset(double.Parse("23.875", CultureInfo.InvariantCulture), 24), "bounded record write");
byte[] buffer = new byte[24];
buffer[OffsetPolicy.CheckedOffset(double.Parse("23.875", CultureInfo.InvariantCulture), buffer.Length)] = 81;
Equal(81, buffer[23], "actual record slot");
foreach (double value in new[] { -0.25, -1, 24, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
    Throws<ArgumentOutOfRangeException>(() => OffsetPolicy.CheckedOffset(value, 24), "invalid buffer offset");
Throws<OverflowException>(() => OffsetPolicy.CheckedOffset(70019, 100000), "checked representation overflow");
Throws<ArgumentOutOfRangeException>(() => OffsetPolicy.CheckedOffset(0, 0), "invalid buffer capacity");
foreach (double x in new[] { -1d, 23.875, 70019, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
{
    long expected = double.IsNaN(x) || x <= 0 ? 0 : x >= ushort.MaxValue ? ushort.MaxValue : (long)x;
    Equal(expected, OffsetPolicy.Saturate(x), "explicit writer saturation");
    Equal(double.ConvertToInteger<ushort>(x), OffsetPolicy.Convert(x), "writer double API");
    Equal(float.ConvertToInteger<ushort>((float)x), OffsetPolicy.Convert((float)x), "writer float API");
    // This is a selected .NET9+ wider saturation followed by integer narrowing.
    long intermediate = double.IsNaN(x) ? 0 : x <= int.MinValue ? int.MinValue : x >= int.MaxValue ? int.MaxValue : (long)x;
    Equal(intermediate & 0xFFFF, OffsetPolicy.ViaSaturatedInt(x), "deliberate narrowing");
    Console.WriteLine($"writer input={x:R} saturation={OffsetPolicy.Saturate(x)} native={OffsetPolicy.Native(x)} viaInt={OffsetPolicy.ViaSaturatedInt(x)} viaNativeInt={OffsetPolicy.ViaNativeInt(x)}");
}
foreach (Assembly assembly in new[] { typeof(double).Assembly })
{
    if (assembly.GetCustomAttributesData().Any(a => a.AttributeType.Name == "ExperimentalAttribute")) throw new InvalidOperationException("Unexpected experimental assembly.");
    Console.WriteLine($"RUNTIME ARTIFACT {assembly.Location} version={assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion} sha256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location)))}");
}
Console.WriteLine($"PASS RecordOffsets {checks} checks; measured Windows x64 CoreCLR JIT only; native outputs are observations, not portable historical contracts.");
