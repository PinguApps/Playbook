using System.Numerics.Tensors;

namespace Playbook.Samples;

// snippet:policy
public static class BatchPolicy
{
    // This instrument accepts dense three-channel readings with leading padding.
    // Explicit channel indices avoid relying on implicit shape alignment.
    public static void AddAligned(ReadOnlyTensorSpan<int> left,
        ReadOnlyTensorSpan<int> right, in TensorSpan<int> destination)
    {
        RequireChannels(left.Lengths);
        RequireChannels(right.Lengths);
        RequireChannels(destination.Lengths);
        var leftIndex = new nint[left.Rank];
        var rightIndex = new nint[right.Rank];
        var outputIndex = new nint[destination.Rank];
        for (nint channel = 0; channel < 3; channel++)
        {
            leftIndex[^1] = rightIndex[^1] = outputIndex[^1] = channel;
            destination[outputIndex] = left[leftIndex] + right[rightIndex];
        }
    }

    private static void RequireChannels(ReadOnlySpan<nint> lengths)
    {
        int first = 0;
        while (first < lengths.Length && lengths[first] == 1) first++;
        if (!lengths[first..].SequenceEqual((ReadOnlySpan<nint>)[3]))
            throw new ArgumentException("Expected three channels, with leading singleton padding only.");
    }
}
// endsnippet:policy
