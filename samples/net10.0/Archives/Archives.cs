using System.Formats.Tar;
using System.IO.Compression;

namespace Playbook.Samples;

public static class Archives
{
    // snippet:access
    public static byte[] CreatePlain(byte[] survey)
    {
        using MemoryStream output = new();
        using (ZipArchive archive = new(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            using Stream entry = archive.CreateEntry("depth.bin", CompressionLevel.NoCompression).Open();
            entry.Write(survey);
        }
        return output.ToArray();
    }

    // snippet:zipread
    public static byte[] ReadEntry(byte[] zip)
    {
        using MemoryStream input = new(zip);
        using ZipArchive archive = new(input, ZipArchiveMode.Read);
        using Stream entry = archive.GetEntry("depth.bin")!.Open();
        using MemoryStream output = new();
        entry.CopyTo(output);
        return output.ToArray();
    }

    // endsnippet:zipread

    public static byte[] ReplaceEntry(byte[] zip, byte[] replacement)
    {
        using MemoryStream output = new();
        output.Write(zip);
        output.Position = 0;
        using (ZipArchive archive = new(output, ZipArchiveMode.Update, leaveOpen: true))
        {
            using Stream entry = archive.GetEntry("depth.bin")!.Open();
            entry.SetLength(0); // Open() in Update retains the old content until explicitly truncated.
            entry.Write(replacement);
        }
        return output.ToArray();
    }
    // endsnippet:access

    // snippet:protection
    public static byte[] Protect(byte[] survey, string password)
    {
        throw new NotSupportedException(".NET 10 has no built-in ZIP password encryption. Do not return a plain archive as protected.");
    }
    // endsnippet:protection

    // snippet:corruption
    public static byte[] ReadTar(byte[] tar)
    {
        using MemoryStream input = new(tar);
        using TarReader reader = new(input);
        TarEntry entry = reader.GetNextEntry() ?? throw new InvalidDataException("Missing survey entry.");
        using MemoryStream output = new();
        entry.DataStream!.CopyTo(output);
        return output.ToArray();
    }
    // .NET 10's ZIP reader does not validate stored CRCs and its TAR reader does not
    // validate header checksums. Successful reading alone is not integrity proof.
    // endsnippet:corruption

    // snippet:tar
    public static byte[] CreateTar(string surveyDirectory, TarEntryFormat format)
    {
        using MemoryStream output = new();
        using (TarWriter writer = new(output, format, leaveOpen: true))
        {
            foreach (string file in Directory.EnumerateFiles(surveyDirectory).Order(StringComparer.Ordinal))
                writer.WriteEntry(file, Path.GetFileName(file));
        }
        return output.ToArray();
    }
    // endsnippet:tar
}
