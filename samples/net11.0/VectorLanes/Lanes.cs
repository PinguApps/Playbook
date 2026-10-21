using System.Runtime.Intrinsics;

namespace Playbook.Samples;

// snippet:lanes
public static class LaneLayout
{
    public static (Vector128<int> Lower, Vector128<int> Upper) Interleave(Vector128<int> left, Vector128<int> right) =>
        Vector128.Zip(left, right);

    public static (Vector128<int> Even, Vector128<int> Odd) Deinterleave(Vector128<int> lower, Vector128<int> upper) =>
        Vector128.Unzip(lower, upper);

    public static Vector128<int> Reverse(Vector128<int> value) => Vector128.Reverse(value);

    public static Vector128<int> Concat(Vector128<int> left, Vector128<int> right, int halves) => halves switch
    {
        0 => Vector128.ConcatLowerLower(left, right),
        1 => Vector128.ConcatLowerUpper(left, right),
        2 => Vector128.ConcatUpperLower(left, right),
        3 => Vector128.ConcatUpperUpper(left, right),
        _ => throw new ArgumentOutOfRangeException(nameof(halves)),
    };

    public static Vector128<int> Geometric() => Vector128.CreateGeometricSequence(3, 3);
    public static Vector128<int> Alternating() => Vector128.CreateAlternatingSequence(-4, 6);
    public static Vector128<float> Harmonic() => Vector128.CreateHarmonicSequence(2f, 2f);
}
// endsnippet:lanes
