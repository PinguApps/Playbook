using System.Runtime.Intrinsics;
using System.Runtime.InteropServices;
using Playbook.Samples;

static void Equal(Vector128<int> value, int[] expected)
{
    for (int i = 0; i < expected.Length; i++)
    {
        if (value.GetElement(i) != expected[i]) throw new InvalidOperationException($"Lane {i}: {value} != {string.Join(',', expected)}");
    }
}

Vector128<int> left = Vector128.Create(7, 19, -3, 42);
Vector128<int> right = Vector128.Create(-2, 5, 11, 8);
(Vector128<int> lower, Vector128<int> upper) = LaneLayout.Interleave(left, right);
Equal(lower, [7, -2, 19, 5]);
Equal(upper, [-3, 11, 42, 8]);
(Vector128<int> even, Vector128<int> odd) = LaneLayout.Deinterleave(lower, upper);
if (even != left || odd != right) throw new InvalidOperationException("Planar/interleaved round trip");
Equal(LaneLayout.Reverse(left), [42, -3, 19, 7]);
Equal(LaneLayout.Concat(left, right, 0), [7, 19, -2, 5]);
Equal(LaneLayout.Concat(left, right, 1), [7, 19, 11, 8]);
Equal(LaneLayout.Concat(left, right, 2), [-3, 42, -2, 5]);
Equal(LaneLayout.Concat(left, right, 3), [-3, 42, 11, 8]);
Equal(LaneLayout.Geometric(), [3, 9, 27, 81]);
Equal(LaneLayout.Alternating(), [-4, 6, -4, 6]);
Vector128<float> harmonic = LaneLayout.Harmonic();
for (int i = 0; i < Vector128<float>.Count; i++)
{
    if (Math.Abs(harmonic.GetElement(i) - 1f / (2 + 2 * i)) > 0.000001f) throw new InvalidOperationException("Floating harmonic lanes");
}
Console.WriteLine($"PASS lane order, zip/unzip, concat/reverse and patterns; accelerated={Vector128.IsHardwareAccelerated}; {RuntimeInformation.FrameworkDescription}");
