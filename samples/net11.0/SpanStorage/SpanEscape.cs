using System;

namespace Playbook.Samples;

// snippet:escape
public static class SpanEscape
{
    public static int Retain(int manifestNumber)
    {
        scoped Span<int> retained = default;
        {
            Span<int> parcelNumbers = [manifestNumber, manifestNumber + 1];
            retained = parcelNumbers;
        }
        return retained[0];
    }
}
// endsnippet:escape
