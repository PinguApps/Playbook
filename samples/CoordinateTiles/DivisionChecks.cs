using System.Numerics;
#if NET11_0_OR_GREATER
using Rounding = System.Numerics.DivisionRounding;
#else
using Rounding = TileRounding;
#endif

Check<int>();
Check<long>();
Check<Int128>();
Check<BigInteger>();
Throws<DivideByZeroException>(() => TileDivision.Split(7, 0, Rounding.Floor));
Throws<OverflowException>(() => TileDivision.Split(int.MinValue, -1, Rounding.Truncate));
Throws<OverflowException>(() => TileDivision.Split(long.MinValue, -1L, Rounding.Floor));
Throws<OverflowException>(() => TileDivision.Split(Int128.MinValue, -Int128.One, Rounding.Ceiling));
foreach (int invalid in new[] { -1, 5, int.MaxValue })
{
    Throws<ArgumentOutOfRangeException>(() => TileDivision.Split(7, 3, (Rounding)invalid));
    Throws<ArgumentOutOfRangeException>(() => TileDivision.Split(6, 3, (Rounding)invalid));
}
#if NET11_0_OR_GREATER
// Execute the raw interface defaults separately from the application's validation.
Equal((2, 0), Raw(6, 3, (Rounding)99));
Throws<ArgumentException>(() => Raw(7, 3, (Rounding)99));
Equal((2u, 1u), Raw(7u, 3u, Rounding.Truncate));
Console.WriteLine("Raw RC2: invalid mode exact division=(2,0); inexact=ArgumentException; uint truncate=(2,1).");

static (T, T) Raw<T>(T a, T b, Rounding mode) where T : IBinaryInteger<T> => T.DivRem(a, b, mode);
static T Quotient<T>(T a, T b, Rounding mode) where T : IBinaryInteger<T> => T.Divide(a, b, mode);
static T Remainder<T>(T a, T b, Rounding mode) where T : IBinaryInteger<T> => T.Remainder(a, b, mode);
#endif
Console.WriteLine($"PASS CoordinateTiles: five signed modes, four generic types, signs/exact/zero/overflow/invalid. Runtime={Environment.Version}; CoreLib={typeof(int).Assembly.Location}");

static void Check<T>() where T : IBinaryInteger<T>, ISignedNumber<T>
{
    // Independent small rational oracle; BigInteger prevents overflow in the identity check.
    foreach (int a in new[] { -7, -6, -1, 0, 1, 6, 7 })
    foreach (int b in new[] { -3, -1, 1, 3 })
    foreach (Rounding mode in Enum.GetValues<Rounding>())
    {
        int q = a / b;
        if (a % b != 0)
        {
            q = mode switch
            {
                Rounding.Floor => (int)Math.Floor((decimal)a / b),
                Rounding.Ceiling => (int)Math.Ceiling((decimal)a / b),
                Rounding.AwayFromZero => Math.Sign(a * b) * ((Math.Abs(a) + Math.Abs(b) - 1) / Math.Abs(b)),
                Rounding.Euclidean => b > 0 ? (int)Math.Floor((decimal)a / b) : (int)Math.Ceiling((decimal)a / b),
                _ => q
            };
        }
        int r = a - q * b;
        T left = T.CreateChecked(a), right = T.CreateChecked(b);
        var actual = TileDivision.Split(left, right, mode);
        Equal((T.CreateChecked(q), T.CreateChecked(r)), actual);
        Equal(new BigInteger(a), BigInteger.CreateChecked(actual.Quotient) * b + BigInteger.CreateChecked(actual.Remainder));
        if (Math.Abs(r) >= Math.Abs(b)) throw new InvalidOperationException("Remainder bound");
        if (r != 0 && mode == Rounding.Euclidean && r < 0) throw new InvalidOperationException("Euclidean sign");
#if NET11_0_OR_GREATER
        Equal(actual.Quotient, Quotient(left, right, mode));
        Equal(actual.Remainder, Remainder(left, right, mode));
#endif
    }
    Console.WriteLine($"{typeof(T).Name}: -7/3 Floor={TileDivision.Split(T.CreateChecked(-7), T.CreateChecked(3), Rounding.Floor)}; -7/-3 Euclidean={TileDivision.Split(T.CreateChecked(-7), T.CreateChecked(-3), Rounding.Euclidean)}");
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
