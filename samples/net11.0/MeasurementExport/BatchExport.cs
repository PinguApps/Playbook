using System.Numerics.Tensors;

namespace Playbook.Samples;

// snippet:batch
public static class BatchExport
{
    public static void RoundMeasurements(ReadOnlySpan<double> input, Span<double> output) =>
        TensorPrimitives.Round(input, 2, MidpointRounding.ToEven, output);
}
// endsnippet:batch
