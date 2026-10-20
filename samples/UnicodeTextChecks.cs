using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Playbook.Samples;

if (TextRules.Rename("a\U0001F680b", new Rune(0x1F680), new Rune(0x1F6F0)) != "a\U0001F6F0b")
    throw new InvalidOperationException("Split supplementary scalar.");
var positions = TextRules.Positions("A\U0001F680\uD800Z");
if (!positions.SequenceEqual(new[] { (0, 1, false), (1, 2, false), (3, 1, true), (4, 1, false) }))
    throw new InvalidOperationException("Wrong UTF-16 positions/replacement flag.");
if (TextRules.InvalidUtf16("A\U0001F680\uD800") != 3 || TextRules.InvalidUtf8([0x41, 0xF0, 0x9F, 0x9A, 0x80, 0xFF]) != 5 ||
    TextRules.InvalidUtf16("A\U0001F680") != -1 || TextRules.InvalidUtf8([0xC3]) != 0)
    throw new InvalidOperationException("Invalid-input byte/code-unit offsets differ.");
CultureInfo saved = CultureInfo.CurrentCulture;
try
{
    CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
    if (!TextRules.Same('i', 'I', StringComparison.OrdinalIgnoreCase) || TextRules.Same('i', 'I', StringComparison.CurrentCultureIgnoreCase) ||
        !TextRules.Same('i', '\u0130', StringComparison.CurrentCultureIgnoreCase) ||
        !TextRules.SameRune(new Rune(0x10428), new Rune(0x10400), StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Character comparison lost culture/scalar policy.");
}
finally { CultureInfo.CurrentCulture = saved; }
string[] lines = TextRules.Lines("north\r\nsouth\rwest\neast\u0085roof\u2028dome\u2029end");
if (!lines.SequenceEqual(new[] { "north", "south", "west", "east", "roof", "dome", "end" }))
    throw new InvalidOperationException("Mixed-newline anchors/dot policy changed: " + string.Join("|", lines));
#if NET11_0_OR_GREATER
if (!"\U0001F680cargo\U0001F680".Trim(new Rune(0x1F680)).Equals("cargo", StringComparison.Ordinal) ||
    "a\U0001F680b".IndexOf(new Rune(0x1F680), StringComparison.Ordinal) != 1 || "x\U0001F680y".Split(new Rune(0x1F680)).Length != 2 ||
    char.ToUpperOrdinal('i') != 'I' || !"i".ToUpperOrdinal().Equals("I", StringComparison.Ordinal))
    throw new InvalidOperationException("Scalar search/trim/split or ordinal casing failed.");
if (Regex.IsMatch("\u2028", ".", RegexOptions.AnyNewLine) || !Regex.IsMatch("\u2028", ".") ||
    !Regex.IsMatch("\u2028", ".", RegexOptions.AnyNewLine | RegexOptions.Singleline))
    throw new InvalidOperationException("AnyNewLine/Singleline dot distinction failed.");
#endif
Console.WriteLine("PASS: supplementary scalars, replacement positions, Turkish comparison, malformed UTF offsets and seven mixed-newline records.");
