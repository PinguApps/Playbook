namespace Playbook.Samples;

internal static class MimeChecks
{
    public static void Run()
    {
        foreach ((string input, string? expected) in new (string, string?)[] {
            (".PDF", "application/pdf"), ("pdf", "application/pdf"),
            ("/mossbank/parcel-48.PdF", "application/pdf"),
            ("ledger.json", "application/json"), ("shelf.jpeg", "image/jpeg"), ("shelf.jpg", "image/jpeg"),
            ("bundle.tar.gz", "application/gzip"), ("unknown.mossbank", null),
            ("", null), ("parcel-48", null), ("parcel-48.", null),
            ("folder.pdf/parcel-48", null), ("parcel.pdf?revision=3", null), (" pdf ", null) })
        {
            Equal(expected, AttachmentLabels.MediaType(input));
            Equal(expected, AttachmentLabels.MediaType(input.AsSpan()));
            Console.WriteLine($"MIME [{input}] => {expected ?? "<null>"}");
        }
        foreach ((string input, string? expected) in new (string, string?)[] {
            ("application/pdf", ".pdf"), (" APPLICATION/PDF ; revision=3 ", ".pdf"),
            ("application/json; charset=utf-8", ".json"), ("image/jpeg", ".jpg"),
            ("application/x-mossbank", null), ("", null), ("image/*", null) })
        {
            Equal(expected, AttachmentLabels.Extension(input));
            Equal(expected, AttachmentLabels.Extension(input.AsSpan()));
        }
        // Two filenames share a type. Reverse lookup selects one suffix, not the original.
        Equal(AttachmentLabels.MediaType("shelf.jpeg"), AttachmentLabels.MediaType("shelf.jpg"));
        Equal(".jpg", AttachmentLabels.Extension(AttachmentLabels.MediaType("shelf.jpeg")!));
        // This is host Path.GetExtension behaviour, not a URL parser or cross-platform path parser.
        if (OperatingSystem.IsWindows())
        {
            Equal("application/pdf", AttachmentLabels.MediaType(@"C:\mossbank\parcel-48.PDF"));
            Equal(null, AttachmentLabels.MediaType(@"C:\folder.pdf\parcel-48"));
        }
        ThrowsNull(() => AttachmentLabels.MediaType((string)null!));
        ThrowsNull(() => AttachmentLabels.Extension((string)null!));
        Equal(null, AttachmentLabels.MediaType(default(ReadOnlySpan<char>)));
        Equal(null, AttachmentLabels.Extension(default(ReadOnlySpan<char>)));
    }

    private static void Equal(string? expected, string? actual)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
            throw new InvalidOperationException($"Expected '{expected ?? "<null>"}', got '{actual ?? "<null>"}'.");
    }

    private static void ThrowsNull(Action action)
    {
        try { action(); }
        catch (ArgumentNullException) { return; }
        throw new InvalidOperationException("Expected ArgumentNullException.");
    }
}
