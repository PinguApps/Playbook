// snippet:attachment-labels
namespace Playbook.Samples;

internal static class AttachmentLabels
{
    // A deliberately bounded application policy, not a copy of the framework table.
    public static string? MediaType(string pathOrExtension)
    {
        ArgumentNullException.ThrowIfNull(pathOrExtension);
        return MediaType(pathOrExtension.AsSpan());
    }

    public static string? MediaType(ReadOnlySpan<char> pathOrExtension)
    {
        ReadOnlySpan<char> extension = Path.GetExtension(pathOrExtension);
        if (extension.IsEmpty) extension = pathOrExtension;
        if (extension.StartsWith(".")) extension = extension[1..];
        if (extension.Equals("pdf", StringComparison.OrdinalIgnoreCase)) return "application/pdf";
        if (extension.Equals("json", StringComparison.OrdinalIgnoreCase)) return "application/json";
        if (extension.Equals("jpg", StringComparison.OrdinalIgnoreCase) || extension.Equals("jpeg", StringComparison.OrdinalIgnoreCase)) return "image/jpeg";
        if (extension.Equals("gz", StringComparison.OrdinalIgnoreCase)) return "application/gzip";
        return null;
    }

    public static string? Extension(string mediaType)
    {
        ArgumentNullException.ThrowIfNull(mediaType);
        return Extension(mediaType.AsSpan());
    }

    public static string? Extension(ReadOnlySpan<char> mediaType)
    {
        int separator = mediaType.IndexOf(';');
        if (separator >= 0) mediaType = mediaType[..separator];
        mediaType = mediaType.Trim();
        if (mediaType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)) return ".pdf";
        if (mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase)) return ".json";
        if (mediaType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase)) return ".jpg";
        if (mediaType.Equals("application/gzip", StringComparison.OrdinalIgnoreCase)) return ".gz";
        return null;
    }
}
// endsnippet:attachment-labels
