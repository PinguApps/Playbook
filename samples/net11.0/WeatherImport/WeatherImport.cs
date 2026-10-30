using System.IO.Compression;

namespace Playbook.Samples;

public static class WeatherImport
{
    // snippet:directory
    public static async Task<string[]?> ReadStationNamesAsync(
        Stream source, CancellationToken cancellationToken = default)
    {
        // The caller owns source, including when the factory fails.
        try
        {
            await using var archive = await ZipArchive.CreateAsync(
                source, ZipArchiveMode.Read, leaveOpen: true,
                entryNameEncoding: null, cancellationToken);
            return archive.Entries.Select(entry => entry.FullName).ToArray();
        }
        catch (InvalidDataException)
        {
            return null; // Reject this directory; do not publish partial names.
        }
    }
    // endsnippet:directory
}
