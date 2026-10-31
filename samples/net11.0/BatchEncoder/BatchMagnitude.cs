using System.Numerics;

// snippet:magnitude
internal static class BatchMagnitude
{
    public static T Exponent<T>(T value) where T : IBinaryInteger<T> => T.Log10(value);

    // Log10 is an integer exponent. This width rule belongs to the batch format.
    public static int Width<T>(T value) where T : IBinaryInteger<T> => int.CreateChecked(Exponent(value)) + 1;
}
// endsnippet:magnitude
