// snippet:library
namespace Berwyn.Wildlife;

public static class TagRules
{
    public static bool Admit(string tag, int massGrams) =>
        tag.StartsWith("TERN-", StringComparison.Ordinal) && massGrams is >= 80 and <= 160;
}
// endsnippet:library
