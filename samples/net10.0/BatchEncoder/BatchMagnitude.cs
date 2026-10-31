using System.Numerics;

// snippet:magnitude
internal static class BatchMagnitude
{
    public static T Exponent<T>(T value) where T : IBinaryInteger<T>
    {
        if (T.IsNegative(value)) throw new ArgumentOutOfRangeException(nameof(value));
        T ten = T.CreateChecked(10), exponent = T.Zero;
        while (value >= ten)
        {
            value /= ten;
            exponent++;
        }
        return exponent;
    }

    // The batch format represents zero with one digit and rejects signed values.
    public static int Width<T>(T value) where T : IBinaryInteger<T> => int.CreateChecked(Exponent(value)) + 1;
}
// endsnippet:magnitude
