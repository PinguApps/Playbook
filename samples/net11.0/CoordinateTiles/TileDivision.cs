using System.Numerics;

// snippet:division
internal static class TileDivision
{
    public static (T Quotient, T Remainder) Split<T>(T coordinate, T tileSize, DivisionRounding mode)
        where T : IBinaryInteger<T>, ISignedNumber<T>
    {
        // Application policy: reject unknown modes even when division is exact.
        if ((uint)mode > (uint)DivisionRounding.Euclidean) throw new ArgumentOutOfRangeException(nameof(mode));
        return T.DivRem(coordinate, tileSize, mode);
    }
}
// endsnippet:division
