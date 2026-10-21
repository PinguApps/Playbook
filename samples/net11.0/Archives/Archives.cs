using System.Formats.Tar;
using System.IO.Compression;

namespace Playbook.Samples;

public static class Archives
{
    public static byte[] CreatePlain(byte[] survey)
    {
        using MemoryStream output = new();
        using (ZipArchive archive = new(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            using Stream entry = archive.CreateEntry("depth.bin", CompressionLevel.NoCompression).Open(FileAccess.Write);
            entry.Write(survey);
        }
        return output.ToArray();
    }

    // snippet:access
    public static byte[] ReadEntry(byte[] zip)
    {
        using MemoryStream input = new(zip);
        using ZipArchive archive = new(input, ZipArchiveMode.Read);
        ZipArchiveEntry survey = archive.GetEntry("depth.bin") ?? throw new InvalidDataException("Missing survey entry.");
        if (survey.CompressionMethod is not (ZipCompressionMethod.Stored or ZipCompressionMethod.Deflate))
            throw new NotSupportedException("This receiver accepts Stored or Deflate survey entries.");
        using Stream entry = survey.Open(FileAccess.Read);
        using MemoryStream output = new();
        entry.CopyTo(output);
        return output.ToArray();
    }

    public static byte[] ReplaceEntry(byte[] zip, byte[] replacement)
    {
        using MemoryStream output = new();
        output.Write(zip);
        output.Position = 0;
        using (ZipArchive archive = new(output, ZipArchiveMode.Update, leaveOpen: true))
        {
            using Stream entry = archive.GetEntry("depth.bin")!.Open(FileAccess.Write);
            entry.Write(replacement); // Write deliberately discards the old content.
        }
        return output.ToArray();
    }
    // endsnippet:access

    // snippet:protection
    public static byte[] Protect(byte[] survey, string password, ZipEncryptionMethod method = ZipEncryptionMethod.Aes256)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);
        using MemoryStream output = new();
        using (ZipArchive archive = new(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            ZipArchiveEntry surveyEntry = archive.CreateEntry("depth.bin", password, method);
            using Stream entry = surveyEntry.Open(FileAccess.Write, password);
            entry.Write(survey);
        }
        return output.ToArray();
    }

    public static byte[] Unprotect(byte[] zip, string password)
    {
        using MemoryStream input = new(zip);
        using ZipArchive archive = new(input, ZipArchiveMode.Read);
        ZipArchiveEntry survey = archive.GetEntry("depth.bin") ?? throw new InvalidDataException("Missing survey entry.");
        if (survey.EncryptionMethod is ZipEncryptionMethod.None or ZipEncryptionMethod.Unknown)
            throw new NotSupportedException("Require a supported encrypted survey entry.");
        using Stream entry = survey.Open(FileAccess.Read, password);
        using MemoryStream output = new();
        entry.CopyTo(output); // Read through EOF before accepting plaintext: integrity can fail late.
        return output.ToArray();
    }
    public static async Task ProtectDirectoryAsync(string source, string zip, string restored, string password, CancellationToken cancellationToken)
    {
        ZipFileCreationOptions creation = new()
        {
            Password = password.AsMemory(), EncryptionMethod = ZipEncryptionMethod.Aes256,
            CompressionLevel = CompressionLevel.Optimal,
        };
        await ZipFile.CreateFromDirectoryAsync(source, zip, creation, cancellationToken);
        ZipExtractionOptions extraction = new() { Password = password.AsMemory(), OverwriteFiles = false };
        await ZipFile.ExtractToDirectoryAsync(zip, restored, extraction, cancellationToken);
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
    // Handle InvalidDataException around the complete ReadTar/ReadEntry operation.
    // TAR checks headers; ZIP validates entry CRCs at EOF. Neither authenticates the sender.
    // endsnippet:corruption

    // snippet:tar
    public static byte[] CreateTar(string surveyDirectory, TarEntryFormat format)
    {
        using MemoryStream output = new();
        TarFile.CreateFromDirectory(surveyDirectory, output, includeBaseDirectory: false, format);
        return output.ToArray();
    }
    // endsnippet:tar
}
