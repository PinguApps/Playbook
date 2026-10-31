// snippet:labels
using System.Globalization;

namespace OrchardRoutes;

public static class DomainLabels
{
    public static bool Convert(IdnMapping mapping, string input, Span<char> destination,
        bool toAscii, out int written)
    {
        string converted = toAscii ? mapping.GetAscii(input) : mapping.GetUnicode(input);
        if (!converted.AsSpan().TryCopyTo(destination))
        {
            written = 0;
            return false;
        }
        written = converted.Length;
        return true;
    }
}
// endsnippet:labels
