using System.Numerics;

Check<Int128>(Int128.MaxValue);
Check<UInt128>(UInt128.MaxValue);
Equal(0, BatchMagnitude.Exponent(0));
Equal(1, BatchMagnitude.Width(0));
Throws<ArgumentOutOfRangeException>(() => BatchMagnitude.Exponent(-1));
Throws<ArgumentOutOfRangeException>(() => BatchMagnitude.Width(Int128.MinValue));
#if NET11_0_OR_GREATER
Equal(0, int.Log10(0));
Equal(2, int.Log10(999));
Equal((Int128)38, Int128.Log10(Int128.MaxValue));
Equal((UInt128)38, UInt128.Log10(UInt128.MaxValue));
Throws<ArgumentOutOfRangeException>(() => int.Log10(-1));
Throws<ArgumentOutOfRangeException>(() => Int128.Log10(Int128.MinValue));
#endif
Console.WriteLine($"PASS BatchEncoder: powers of ten ±1 through 10^38, Int128/UInt128 maxima, zero/negative/minimum; exact integer inputs. Runtime={Environment.Version}; CoreLib={typeof(Int128).Assembly.Location}");

static void Check<T>(T maximum) where T : IBinaryInteger<T>
{
    BigInteger max = BigInteger.CreateChecked(maximum);
    for (int exponent = 1; exponent <= 38; exponent++)
    {
        BigInteger power = BigInteger.Pow(10, exponent);
        foreach (int offset in new[] { -1, 0, 1 })
        {
            BigInteger input = power + offset;
            if (input > max) continue;
            T exact = T.CreateChecked(input);
            Equal(input, BigInteger.CreateChecked(exact));
            int expected = offset < 0 ? exponent - 1 : exponent;
            Equal(T.CreateChecked(expected), BatchMagnitude.Exponent(exact));
            Equal(expected + 1, BatchMagnitude.Width(exact));
        }
    }
    Equal(T.Zero, BatchMagnitude.Exponent(T.Zero));
    Equal(T.Zero, BatchMagnitude.Exponent(T.One));
    Equal(T.CreateChecked(38), BatchMagnitude.Exponent(maximum));
    Equal(39, BatchMagnitude.Width(maximum));
    Console.WriteLine($"{typeof(T).Name} maximum={maximum}; exponent=38; width=39");
}
static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new InvalidOperationException($"Expected {expected}, got {actual}");
}
static void Throws<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new InvalidOperationException($"Expected {typeof(T).Name}");
}
