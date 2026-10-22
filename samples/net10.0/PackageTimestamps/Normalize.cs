using System.IO.Compression;

internal static class Normalize
{
    // snippet:normalize
    internal static void UnsignedArchive(string package)
    {
        using ZipArchive archive = ZipFile.Open(package, ZipArchiveMode.Update);
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            entry.LastWriteTime = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
        }
    }
    // endsnippet:normalize
}
