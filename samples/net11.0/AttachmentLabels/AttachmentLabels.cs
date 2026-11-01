// snippet:attachment-labels
using System.Net.Mime;

namespace Playbook.Samples;

internal static class AttachmentLabels
{
    public static string? MediaType(string pathOrExtension) => MediaTypeMap.GetMediaType(pathOrExtension);
    public static string? MediaType(ReadOnlySpan<char> pathOrExtension) => MediaTypeMap.GetMediaType(pathOrExtension);
    public static string? Extension(string mediaType) => MediaTypeMap.GetExtension(mediaType);
    public static string? Extension(ReadOnlySpan<char> mediaType) => MediaTypeMap.GetExtension(mediaType);
}
// endsnippet:attachment-labels
