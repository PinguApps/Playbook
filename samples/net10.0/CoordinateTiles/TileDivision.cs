using System.Numerics;

// snippet:division
internal enum TileRounding { Truncate, Floor, Ceiling, AwayFromZero, Euclidean }

internal static class TileDivision
{
    public static (T Quotient, T Remainder) Split<T>(T coordinate, T tileSize, TileRounding mode)
        where T : IBinaryInteger<T>, ISignedNumber<T>
    {
        if ((uint)mode > (uint)TileRounding.Euclidean) throw new ArgumentOutOfRangeException(nameof(mode));
        var (q, r) = T.DivRem(coordinate, tileSize);
        if (r == T.Zero) return (q, r);

        bool oppositeSigns = T.IsNegative(coordinate) != T.IsNegative(tileSize);
        bool lower = mode == TileRounding.Floor && oppositeSigns
            || mode == TileRounding.AwayFromZero && oppositeSigns
            || mode == TileRounding.Euclidean && r < T.Zero && tileSize > T.Zero;
        bool higher = mode == TileRounding.Ceiling && !oppositeSigns
            || mode == TileRounding.AwayFromZero && !oppositeSigns
            || mode == TileRounding.Euclidean && r < T.Zero && tileSize < T.Zero;
        if (lower) return (q - T.One, r + tileSize);
        if (higher) return (q + T.One, r - tileSize);
        return (q, r);
    }
}
// endsnippet:division
