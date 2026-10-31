// snippet:batch
using System.Numerics;

namespace SensorArchive;

public static class SensorBatch
{
    public static BFloat16[] Store(ReadOnlySpan<float> readings)
    {
        var stored = new BFloat16[readings.Length];
        for (int i = 0; i < readings.Length; i++) stored[i] = (BFloat16)readings[i];
        return stored;
    }

    // Widen before adding: reduced storage precision need not mean reduced accumulation precision.
    public static float Mean(ReadOnlySpan<BFloat16> stored)
    {
        if (stored.IsEmpty) throw new ArgumentException("A batch must contain readings.", nameof(stored));
        float sum = 0;
        foreach (BFloat16 value in stored) sum += (float)value;
        return sum / stored.Length;
    }
}
// endsnippet:batch
