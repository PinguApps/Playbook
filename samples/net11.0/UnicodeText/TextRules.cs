using System.Text;
using System.Text.Unicode;
using System.Text.RegularExpressions;

namespace Playbook.Samples;

public static class TextRules
{
    // snippet:scalars
    public static string Rename(string text, Rune oldMark, Rune newMark) => text.Replace(oldMark, newMark);

    public static (int Index, int Length, bool Replaced)[] Positions(string text)
    {
        var result = new List<(int, int, bool)>();
        foreach (RunePosition position in RunePosition.EnumerateUtf16(text))
            result.Add((position.StartIndex, position.Length, position.WasReplaced));
        return result.ToArray();
    }
    // endsnippet:scalars

    // snippet:compare
    public static bool Same(char left, char right, StringComparison comparison) => left.Equals(right, comparison);

    public static bool SameRune(Rune left, Rune right, StringComparison comparison) => left.Equals(right, comparison);
    // endsnippet:compare

    // snippet:invalid
    public static int InvalidUtf16(ReadOnlySpan<char> text) => Utf16.IndexOfInvalidSubsequence(text);

    public static int InvalidUtf8(ReadOnlySpan<byte> bytes) => Utf8.IndexOfInvalidSubsequence(bytes);
    // endsnippet:invalid

    // snippet:newlines
    public static string[] Lines(string text) =>
        Regex.Matches(text, "^.+$", RegexOptions.Multiline | RegexOptions.AnyNewLine)
            .Select(match => match.Value).ToArray();
    // endsnippet:newlines
}
