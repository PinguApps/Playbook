// snippet:labels
using System.Globalization;

namespace OrchardRoutes;

public static class DomainLabels
{
    public static bool Convert(IdnMapping mapping, ReadOnlySpan<char> input, Span<char> destination,
        bool toAscii, out int written) => toAscii
            ? mapping.TryGetAscii(input, destination, out written)
            : mapping.TryGetUnicode(input, destination, out written);
}
// endsnippet:labels
