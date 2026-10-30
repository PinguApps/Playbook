using System.Formats.Tar;

namespace Playbook.Samples;

public static class ObservatoryTar
{
#if NET11_0_OR_GREATER
    // snippet:newer
    public static void WriteCalibration(Stream destination, string directory, bool independent)
    {
        TarWriterOptions options = new()
        {
            Format = TarEntryFormat.Pax,
            HardLinkMode = independent ? TarHardLinkMode.CopyContents : TarHardLinkMode.PreserveLink
        };
        using TarWriter writer = new(destination, options, leaveOpen: true);
        foreach (string name in new[] { "calibration.dat", "alias.dat", "unrelated.dat" })
            writer.WriteEntry(Path.Combine(directory, name), name);
    }
    // endsnippet:newer

    // snippet:extract
    public static void ExtractCalibration(Stream archive, string staging, bool independent)
    {
        Directory.CreateDirectory(staging);
        TarFile.ExtractToDirectory(archive, staging, new TarExtractOptions
        {
            HardLinkMode = independent ? TarHardLinkMode.CopyContents : TarHardLinkMode.PreserveLink,
            OverwriteFiles = false
        });
    }

    public static async Task ExtractCalibrationAsync(
        Stream archive, string staging, bool independent, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(staging);
        await TarFile.ExtractToDirectoryAsync(archive, staging, new TarExtractOptions
        {
            HardLinkMode = independent ? TarHardLinkMode.CopyContents : TarHardLinkMode.PreserveLink,
            OverwriteFiles = false
        }, cancellationToken);
    }
    // endsnippet:extract
#else
    // snippet:older
    public static void WriteCalibration(Stream destination, string directory, bool independent)
    {
        if (!independent)
            throw new NotSupportedException("This .NET 10 path writer has no automatic hard-link policy option.");
        using TarWriter writer = new(destination, TarEntryFormat.Pax, leaveOpen: true);
        foreach (string name in new[] { "calibration.dat", "alias.dat", "unrelated.dat" })
            writer.WriteEntry(Path.Combine(directory, name), name);
    }
    // endsnippet:older
#endif
}
