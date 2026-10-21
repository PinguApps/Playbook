using System.Buffers.Binary;
using System.Formats.Tar;
using System.IO.Compression;
using System.Text;
using Playbook.Samples;

CheckSync();
#if NET11_0_OR_GREATER
await CheckAsync();
#endif

static void CheckSync()
{
    byte[] payload = Encoding.UTF8.GetBytes("Lighthouse survey: berth 27, depth 14m.");
    byte[] zip = Archives.CreatePlain(payload);
    if (!Archives.ReadEntry(zip).SequenceEqual(payload)) throw new Exception("Entry round trip failed.");
    if (!Archives.ReadEntry(Archives.ReplaceEntry(zip, [8, 3])).SequenceEqual(new byte[] { 8, 3 })) throw new Exception("Replacement retained old bytes.");
    byte[] damaged = (byte[])zip.Clone();
    int dataOffset = 30 + BinaryPrimitives.ReadUInt16LittleEndian(zip.AsSpan(26)) + BinaryPrimitives.ReadUInt16LittleEndian(zip.AsSpan(28));
    damaged[dataOffset] ^= 1; // Stored data changes, while both recorded CRCs remain unchanged.
#if NET11_0_OR_GREATER
    Expect<InvalidDataException>(() => Archives.ReadEntry(damaged));
    Expect<ArgumentOutOfRangeException>(() => Archives.Protect(payload, "survey-passphrase-57", ZipEncryptionMethod.None));
    Expect<ArgumentOutOfRangeException>(() => Archives.Protect(payload, "survey-passphrase-57", ZipEncryptionMethod.Unknown));
    foreach (ZipEncryptionMethod method in new[] { ZipEncryptionMethod.ZipCrypto, ZipEncryptionMethod.Aes128, ZipEncryptionMethod.Aes192, ZipEncryptionMethod.Aes256 })
    {
        byte[] protectedZip = Archives.Protect(payload, "survey-passphrase-57", method);
        if (!Archives.Unprotect(protectedZip, "survey-passphrase-57").SequenceEqual(payload)) throw new Exception("Password round trip failed.");
        Expect<InvalidDataException>(() => Archives.Unprotect(protectedZip, "wrong-password"));
        Expect<ArgumentException>(() => Archives.Unprotect(protectedZip, ""));
        if (method != ZipEncryptionMethod.ZipCrypto)
        {
            using (MemoryStream inspection = new(protectedZip))
            using (ZipArchive archive = new(inspection, ZipArchiveMode.Read))
            {
                int protectedData = 30 + BinaryPrimitives.ReadUInt16LittleEndian(protectedZip.AsSpan(26)) + BinaryPrimitives.ReadUInt16LittleEndian(protectedZip.AsSpan(28));
                protectedZip[protectedData + checked((int)archive.Entries.Single().CompressedLength) - 1] ^= 1;
            }
            Expect<InvalidDataException>(() => Archives.Unprotect(protectedZip, "survey-passphrase-57"));
        }
        Console.WriteLine($"PASS: protected ZIP {method} round trip; wrong/missing password rejected; AES authentication checked where applicable.");
    }
    using (MemoryStream input = new(zip))
    using (ZipArchive archive = new(input, ZipArchiveMode.Read))
    {
        ZipArchiveEntry entry = archive.Entries.Single();
        if (entry.CompressionMethod != ZipCompressionMethod.Stored) throw new Exception("Stored method not reported.");
        Expect<InvalidOperationException>(() => entry.Open(FileAccess.Write));
    }
    using (MemoryStream output = new())
    using (ZipArchive archive = new(output, ZipArchiveMode.Create))
    {
        ZipArchiveEntry entry = archive.CreateEntry("depth.bin");
        Expect<InvalidOperationException>(() => entry.Open(FileAccess.Read));
        using Stream stream = entry.Open(FileAccess.ReadWrite);
        if (stream.CanRead || !stream.CanWrite) throw new Exception("Create ReadWrite must remain write-only.");
    }
#else
    if (Archives.ReadEntry(damaged).SequenceEqual(payload)) throw new Exception("Corruption fixture ineffective.");
    Expect<NotSupportedException>(() => Archives.Protect(payload, "survey-passphrase-57"));
#endif

    byte[] sparse = TarSparseFixture.Create();
#if NET11_0_OR_GREATER
    byte[] logical = new byte[16];
    "ABC"u8.CopyTo(logical);
    "DE"u8.CopyTo(logical.AsSpan(12));
    if (!Archives.ReadTar(sparse).SequenceEqual(logical)) throw new Exception("GNU sparse 1.0 holes were not restored.");
    Console.WriteLine("PASS: original GNU sparse 1.0 PAX map restored logical bytes and holes.");
#else
    if (Archives.ReadTar(sparse).Length != 517) throw new Exception("Expected .NET 10 to expose stored sparse map bytes, not the logical file.");
    Console.WriteLine("PASS: .NET 10 exposes the 517-byte stored sparse body; not the 16-byte logical file.");
#endif

    string directory = Path.Combine(Path.GetTempPath(), $"playbook-tar-{Guid.NewGuid():N}");
    Directory.CreateDirectory(directory);
    try
    {
        File.WriteAllBytes(Path.Combine(directory, "depth.bin"), payload);
        foreach (TarEntryFormat format in new[] { TarEntryFormat.V7, TarEntryFormat.Ustar, TarEntryFormat.Pax, TarEntryFormat.Gnu })
        {
            byte[] tar = Archives.CreateTar(directory, format);
            using MemoryStream input = new(tar);
            using TarReader reader = new(input);
            TarEntry entry = reader.GetNextEntry()!;
            if (entry.Format != format) throw new Exception($"Wrong TAR format: {entry.Format}.");
            using MemoryStream restored = new();
            entry.DataStream!.CopyTo(restored);
            if (!restored.ToArray().SequenceEqual(payload) || reader.GetNextEntry() is not null) throw new Exception("TAR round trip failed.");
            Console.WriteLine($"PASS: TAR {format} selected and restored.");
            if (format == TarEntryFormat.Ustar)
            {
                tar[0] ^= 1; // Valid name bytes, invalid header checksum.
#if NET11_0_OR_GREATER
                Expect<InvalidDataException>(() => Archives.ReadTar(tar));
#else
                Archives.ReadTar(tar);
#endif
            }
        }
    }
    finally { Directory.Delete(directory, recursive: true); }
    Console.WriteLine($"PASS: archive access/replacement, corruption policy and formats; runtime {Environment.Version}.");

}

#if NET11_0_OR_GREATER
static async Task CheckAsync()
{
    byte[] payload = [4, 7, 2];
    using MemoryStream input = new(Archives.CreatePlain(payload));
    using ZipArchive archive = new(input, ZipArchiveMode.Read);
    await using Stream stream = await archive.Entries.Single().OpenAsync(FileAccess.Read);
    using MemoryStream restored = new();
    await stream.CopyToAsync(restored);
    if (!restored.ToArray().SequenceEqual(payload)) throw new Exception("Async entry failed.");
    string directory = Path.Combine(Path.GetTempPath(), $"playbook-zip-{Guid.NewGuid():N}");
    Directory.CreateDirectory(Path.Combine(directory, "source"));
    try
    {
        await File.WriteAllBytesAsync(Path.Combine(directory, "source/depth.bin"), payload);
        string zip = Path.Combine(directory, "survey.zip");
        string destination = Path.Combine(directory, "restored");
        await Archives.ProtectDirectoryAsync(Path.Combine(directory, "source"), zip, destination, "survey-passphrase-57", CancellationToken.None);
        if (!(await File.ReadAllBytesAsync(Path.Combine(destination, "depth.bin"))).SequenceEqual(payload)) throw new Exception("Bulk extraction failed.");
        try
        {
            await ZipFile.ExtractToDirectoryAsync(zip, destination, new ZipExtractionOptions { Password = "survey-passphrase-57".AsMemory(), OverwriteFiles = false });
            throw new Exception("Overwrite policy was ignored.");
        }
        catch (IOException) { }
        using CancellationTokenSource cancelled = new();
        await cancelled.CancelAsync();
        try
        {
            await ZipFile.ExtractToDirectoryAsync(zip, Path.Combine(directory, "cancelled"), new ZipExtractionOptions { Password = "survey-passphrase-57".AsMemory() }, cancelled.Token);
            throw new Exception("Cancellation was ignored.");
        }
        catch (OperationCanceledException) { }
        Console.WriteLine("PASS: async encrypted directory round trip, overwrite rejection and cancellation.");
    }
    finally { Directory.Delete(directory, recursive: true); }

}
#endif

static void Expect<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new Exception($"Expected {typeof(T).Name}.");
}
