using System.Runtime.Intrinsics;

namespace Playbook.Samples;

// snippet:lanes
public static class LaneLayout
{
    public static (Vector128<int> Lower, Vector128<int> Upper) Interleave(Vector128<int> left, Vector128<int> right) =>
        (Vector128.Create(left.GetElement(0), right.GetElement(0), left.GetElement(1), right.GetElement(1)),
         Vector128.Create(left.GetElement(2), right.GetElement(2), left.GetElement(3), right.GetElement(3)));

    public static (Vector128<int> Even, Vector128<int> Odd) Deinterleave(Vector128<int> lower, Vector128<int> upper) =>
        (Vector128.Create(lower.GetElement(0), lower.GetElement(2), upper.GetElement(0), upper.GetElement(2)),
         Vector128.Create(lower.GetElement(1), lower.GetElement(3), upper.GetElement(1), upper.GetElement(3)));

    public static Vector128<int> Reverse(Vector128<int> value) =>
        Vector128.Shuffle(value, Vector128.Create(3, 2, 1, 0));

    public static Vector128<int> Concat(Vector128<int> left, Vector128<int> right, int halves)
    {
        int l = halves / 2 * 2;
        int r = halves % 2 * 2;
        return Vector128.Create(left.GetElement(l), left.GetElement(l + 1), right.GetElement(r), right.GetElement(r + 1));
    }

    public static Vector128<int> Geometric() => Vector128.Create(3, 9, 27, 81);
    public static Vector128<int> Alternating() => Vector128.Create(-4, 6, -4, 6);
    public static Vector128<float> Harmonic() => Vector128.Create(1f / 2, 1f / 4, 1f / 6, 1f / 8);
}
// endsnippet:lanes
