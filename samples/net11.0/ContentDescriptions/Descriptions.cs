using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;

namespace Playbook.Samples;

// snippet:syntax
public static class LessonDraft
{
    public static string CSharp([StringSyntax(StringSyntaxAttribute.CSharp)] string source) => source;

    public static string FSharp([StringSyntax(StringSyntaxAttribute.FSharp)] string source) => source;

    public static string VisualBasic([StringSyntax(StringSyntaxAttribute.VisualBasic)] string source) => source;
}
// endsnippet:syntax

// snippet:media
public readonly record struct ClipDescription(bool Embedded, string DeclaredMediaType);

public static class HarbourClips
{
    public static ClipDescription Describe(string address, string format)
    {
        if (!Uri.TryCreate(address, UriKind.Absolute, out Uri? uri))
        {
            throw new ArgumentException("An absolute clip URI is required.", nameof(address));
        }

        string mediaType = format switch
        {
            "mp4" => MediaTypeNames.Video.Mp4,
            "mpeg" => MediaTypeNames.Video.Mpeg,
            "ogg" => MediaTypeNames.Video.Ogg,
            "mov" => MediaTypeNames.Video.QuickTime,
            "webm" => MediaTypeNames.Video.WebM,
            _ => throw new ArgumentException("Unknown clip format.", nameof(format)),
        };

        bool embedded = string.Equals(uri.Scheme, Uri.UriSchemeData, StringComparison.OrdinalIgnoreCase);
        return new ClipDescription(embedded, mediaType);
    }
}
// endsnippet:media
