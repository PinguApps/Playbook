// snippet:batch
namespace SensorArchive;

public static class SensorBatch
{
    // Retain single precision when the exponent range rules out Half.
    public static float[] Store(ReadOnlySpan<float> readings) => readings.ToArray();

    public static float Mean(ReadOnlySpan<float> stored)
    {
        if (stored.IsEmpty) throw new ArgumentException("A batch must contain readings.", nameof(stored));
        float sum = 0;
        foreach (float value in stored) sum += value;
        return sum / stored.Length;
    }
}
// endsnippet:batch
