// snippet:policy
using System.Numerics;
using System.Numerics.Tensors;

namespace Playbook.Samples;

// Both releases use the same application contract, independent of SIMD dispatch.
public static class CalibrationPolicy
{
    public static void Clamp<T>(ReadOnlySpan<T> readings, T lower, T upper, Span<T> output)
        where T : INumber<T>
    {
        if (output.Length < readings.Length) throw new ArgumentException("Output is too short.");
        if (!(lower <= upper)) throw new ArgumentException("Calibration lower bound must not exceed upper bound.");
        TensorPrimitives.Clamp(readings, lower, upper, output);
    }

    public static void Clamp<T>(ReadOnlySpan<T> readings, ReadOnlySpan<T> lower,
        ReadOnlySpan<T> upper, Span<T> output) where T : INumber<T>
    {
        if (lower.Length != readings.Length || upper.Length != readings.Length || output.Length < readings.Length)
            throw new ArgumentException("Every reading needs bounds and output capacity.");
        for (int i = 0; i < readings.Length; i++)
            if (!(lower[i] <= upper[i])) throw new ArgumentException($"Invalid calibration bounds at {i}.");
        TensorPrimitives.Clamp(readings, lower, upper, output);
    }
}
// endsnippet:policy
