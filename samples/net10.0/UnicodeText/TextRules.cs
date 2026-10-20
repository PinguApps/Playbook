using System.Buffers;
using System.Text;
using System.Text.RegularExpressions;

namespace Playbook.Samples;

public static class TextRules
{
    // snippet:scalars
    public static string Rename(string text, Rune oldMark, Rune newMark) =>
        text.Replace(oldMark.ToString(), newMark.ToString(), StringComparison.Ordinal);

    public static (int Index, int Length, bool Replaced)[] Positions(string text)
    {
        var result = new List<(int, int, bool)>();
        int offset = 0;
        while (offset < text.Length)
        {
            OperationStatus status = Rune.DecodeFromUtf16(text.AsSpan(offset), out _, out int consumed);
            result.Add((offset, consumed, status != OperationStatus.Done));
            offset += consumed;
        }

        return result.ToArray();
    }
    // endsnippet:scalars

    // snippet:compare
    public static bool Same(char left, char right, StringComparison comparison) =>
        string.Equals(left.ToString(), right.ToString(), comparison);

    public static bool SameRune(Rune left, Rune right, StringComparison comparison) =>
        string.Equals(left.ToString(), right.ToString(), comparison);
    // endsnippet:compare

    // snippet:invalid
    public static int InvalidUtf16(ReadOnlySpan<char> text)
    {
        int offset = 0;
        while (offset < text.Length)
        {
            if (Rune.DecodeFromUtf16(text[offset..], out _, out int consumed) != OperationStatus.Done)
                return offset;
            offset += consumed;
        }
        return -1;
    }

    public static int InvalidUtf8(ReadOnlySpan<byte> bytes)
    {
        int offset = 0;
        while (offset < bytes.Length)
        {
            if (Rune.DecodeFromUtf8(bytes[offset..], out _, out int consumed) != OperationStatus.Done)
                return offset;
            offset += consumed;
        }
        return -1;
    }
    // endsnippet:invalid

    // snippet:newlines
    public static string[] Lines(string text)
    {
        // Normalize explicitly before applying the ordinary LF-based regex policy.
        string normalized = Regex.Replace(text, "\\r\\n|[\\r\\u0085\\u2028\\u2029]", "\n");
        return Regex.Matches(normalized, "^.+$", RegexOptions.Multiline)
            .Select(match => match.Value).ToArray();
    }
    // endsnippet:newlines
}
