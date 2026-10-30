using System.Formats.Tar;
using System.Reflection;
using System.Security.Cryptography;
using Playbook.Samples;

#pragma warning disable CA1849 // This proof intentionally exercises both synchronous and asynchronous APIs.

if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("This proof uses measured Windows file IDs; use another real identity probe for other platforms.");
string root = Path.Combine(Path.GetTempPath(), $"playbook-observatory-{Guid.NewGuid():N}");
string source = Path.Combine(root, "source");
Directory.CreateDirectory(source);
byte[] payload = "Observatory calibration: gain=17; offset=3\n"u8.ToArray();
try
{
    string original = Path.Combine(source, "calibration.dat"), alias = Path.Combine(source, "alias.dat"), unrelated = Path.Combine(source, "unrelated.dat");
    File.WriteAllBytes(original, payload);
    ObservatoryFileIdentity.Link(alias, original);
    File.WriteAllBytes(unrelated, payload);
    string id = ObservatoryFileIdentity.Read(original);
    if (id != ObservatoryFileIdentity.Read(alias) || id == ObservatoryFileIdentity.Read(unrelated)) throw new Exception("Hard-link fixture is not genuine.");
    Console.WriteLine($"OS={System.Runtime.InteropServices.RuntimeInformation.OSDescription}; filesystem={new DriveInfo(Path.GetPathRoot(root)!).DriveFormat}; root={root}; runtime={Environment.Version}");
    Assembly assembly = typeof(TarWriter).Assembly;
    Console.WriteLine($"TAR={assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion}; SHA256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location)))}");
    Console.WriteLine($"PASS: actual identities calibration={id}; alias={ObservatoryFileIdentity.Read(alias)}; equal-byte unrelated={ObservatoryFileIdentity.Read(unrelated)}");

    using (MemoryStream baseline = new())
    {
        using (TarWriter writer = new(baseline, TarEntryFormat.Pax, leaveOpen: true))
            foreach (string name in Names()) writer.WriteEntry(Path.Combine(source, name), name);
#if NET11_0_OR_GREATER
        Inspect(baseline.ToArray(), true);
        if (new TarWriterOptions().HardLinkMode != TarHardLinkMode.PreserveLink || new TarWriterOptions().Format != TarEntryFormat.Pax || new TarExtractOptions().OverwriteFiles || new TarExtractOptions().HardLinkMode != TarHardLinkMode.PreserveLink) throw new Exception("Options defaults changed.");
        foreach (Type type in new[] { typeof(TarWriterOptions), typeof(TarExtractOptions), typeof(TarHardLinkMode) })
            if (type.GetCustomAttributesData().Any(a => a.AttributeType.Name == "ExperimentalAttribute")) throw new Exception("Unexpected experimental option.");
#else
        Inspect(baseline.ToArray(), false);
        Expect<NotSupportedException>(() => ObservatoryTar.WriteCalibration(new MemoryStream(), source, false));
#endif
        Console.WriteLine("PASS: default path writer inspected; first occurrence owns content; equal bytes do not establish identity.");
    }

#if NET11_0_OR_GREATER
    foreach (bool independentWrite in new[] { false, true })
    foreach (bool asyncWrite in new[] { false, true })
    {
        using MemoryStream output = new();
        if (!asyncWrite) ObservatoryTar.WriteCalibration(output, source, independentWrite);
        else
        {
            await using TarWriter writer = new(output, new TarWriterOptions { HardLinkMode = independentWrite ? TarHardLinkMode.CopyContents : TarHardLinkMode.PreserveLink }, leaveOpen: true);
            foreach (string name in Names()) await writer.WriteEntryAsync(Path.Combine(source, name), name);
        }
        byte[] archive = output.ToArray();
        Inspect(archive, !independentWrite);
        string tarPath = Path.Combine(root, $"{independentWrite}-{asyncWrite}.tar");
        File.WriteAllBytes(tarPath, archive);
        foreach (bool independentExtract in new[] { false, true })
        foreach (bool asyncExtract in new[] { false, true })
        foreach (bool pathExtract in new[] { false, true })
        {
            string destination = Path.Combine(root, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(destination);
            TarExtractOptions options = new() { HardLinkMode = independentExtract ? TarHardLinkMode.CopyContents : TarHardLinkMode.PreserveLink };
            using MemoryStream input = new(archive);
            if (pathExtract)
            {
                if (asyncExtract) await TarFile.ExtractToDirectoryAsync(tarPath, destination, options);
                else TarFile.ExtractToDirectory(tarPath, destination, options);
            }
            else if (asyncExtract) await ObservatoryTar.ExtractCalibrationAsync(input, destination, independentExtract, CancellationToken.None);
            else ObservatoryTar.ExtractCalibration(input, destination, independentExtract);
            VerifyFiles(destination, !independentWrite && !independentExtract);
            string first = Path.Combine(destination, Names()[0]);
            File.WriteAllText(first, "protected");
            using MemoryStream repeat = new(archive);
            if (asyncExtract) await ExpectAsync<IOException>(() => TarFile.ExtractToDirectoryAsync(repeat, destination, options));
            else Expect<IOException>(() => TarFile.ExtractToDirectory(repeat, destination, options));
            if (File.ReadAllText(first) != "protected") throw new Exception("Overwrite rejection changed existing bytes.");
            Directory.Delete(destination, recursive: true);
            Console.WriteLine($"PASS: writer={(independentWrite ? "CopyContents" : "PreserveLink")} async={asyncWrite}; extract={(independentExtract ? "CopyContents" : "PreserveLink")} async={asyncExtract} path={pathExtract}; bytes, file identity, overwrite rejection and cleanup.");
        }
    }
    using (MemoryStream reversed = new())
    {
        using (TarWriter writer = new(reversed, new TarWriterOptions(), leaveOpen: true))
        {
            writer.WriteEntry(alias, "alias.dat");
            writer.WriteEntry(original, "calibration.dat");
        }
        reversed.Position = 0;
        using TarReader reader = new(reversed);
        if (reader.GetNextEntry()!.EntryType != TarEntryType.RegularFile) throw new Exception("First alias must own data.");
        TarEntry link = reader.GetNextEntry()!;
        if (link.EntryType != TarEntryType.HardLink || link.LinkName != "alias.dat") throw new Exception("Link order was lost.");
    }
    using (CancellationTokenSource cancelled = new())
    {
        cancelled.Cancel();
        using MemoryStream cancelledArchive = new();
        await using (TarWriter writer = new(cancelledArchive, new TarWriterOptions(), leaveOpen: true))
            await ExpectAsync<OperationCanceledException>(async () => await writer.WriteEntryAsync(original, "calibration.dat", cancelled.Token));
        string cancelledDirectory = Path.Combine(root, "cancelled");
        Directory.CreateDirectory(cancelledDirectory);
        foreach (TarHardLinkMode mode in new[] { TarHardLinkMode.PreserveLink, TarHardLinkMode.CopyContents })
        {
            using MemoryStream input = new(CreateLinkArchive());
            await ExpectAsync<OperationCanceledException>(() => TarFile.ExtractToDirectoryAsync(input, cancelledDirectory, new TarExtractOptions { HardLinkMode = mode }, cancelled.Token));
        }
        if (Directory.EnumerateFileSystemEntries(cancelledDirectory).Any()) throw new Exception("Pre-cancelled extraction wrote files.");
    }
    foreach (bool maliciousLink in new[] { false, true })
    {
        using MemoryStream unsafeArchive = new();
        using (TarWriter writer = new(unsafeArchive, leaveOpen: true))
        {
            PaxTarEntry entry = new(maliciousLink ? TarEntryType.HardLink : TarEntryType.RegularFile, maliciousLink ? "alias.dat" : "../escaped.dat");
            if (maliciousLink) entry.LinkName = "../source/calibration.dat";
            else entry.DataStream = new MemoryStream(payload);
            writer.WriteEntry(entry);
            entry.DataStream?.Dispose();
        }
        string destination = Path.Combine(root, "unsafe-" + maliciousLink);
        Directory.CreateDirectory(destination);
        unsafeArchive.Position = 0;
        Expect<IOException>(() => TarFile.ExtractToDirectory(unsafeArchive, destination, new TarExtractOptions { HardLinkMode = TarHardLinkMode.CopyContents }));
        if (File.Exists(Path.Combine(root, "escaped.dat"))) throw new Exception("Traversal escaped staging.");
    }
    string partial = Path.Combine(root, "partial");
    Directory.CreateDirectory(partial);
    File.WriteAllText(Path.Combine(partial, "alias.dat"), "existing");
    using (MemoryStream input = new(CreateLinkArchive()))
        Expect<IOException>(() => TarFile.ExtractToDirectory(input, partial, new TarExtractOptions()));
    if (!File.ReadAllBytes(Path.Combine(partial, "calibration.dat")).SequenceEqual(payload)) throw new Exception("Partial-extraction fixture did not write its first file.");
    Console.WriteLine("PASS: reverse ordering; cancellation; traversal/link-target rejection; partial extraction is not transactional.");
#else
    using (MemoryStream output = new()) { ObservatoryTar.WriteCalibration(output, source, true); Inspect(output.ToArray(), false); }
    foreach (bool asyncExtract in new[] { false, true })
    {
        string destination = Path.Combine(root, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(destination);
        using MemoryStream input = new(CreateLinkArchive());
        if (asyncExtract) await TarFile.ExtractToDirectoryAsync(input, destination, overwriteFiles: false);
        else TarFile.ExtractToDirectory(input, destination, overwriteFiles: false);
        VerifyFiles(destination, true);
    }
    Console.WriteLine("PASS: .NET 10 path writes independent regular files; explicit TAR hard links still extract as shared identities sync/async. New policy options are unavailable.");
#endif
}
finally { Directory.Delete(root, recursive: true); }
if (Directory.Exists(root)) throw new Exception("Cleanup failed.");
Console.WriteLine("PASS: archive/file handles closed; all links and isolated directories removed.");

string[] Names() => ["calibration.dat", "alias.dat", "unrelated.dat"];

void Inspect(byte[] archive, bool linked)
{
    using TarReader reader = new(new MemoryStream(archive));
    int index = 0;
    foreach (string name in Names())
    {
        TarEntry entry = reader.GetNextEntry() ?? throw new Exception("Missing entry.");
        bool expectedLink = linked && index == 1;
        if (entry.Name != name || entry.EntryType != (expectedLink ? TarEntryType.HardLink : TarEntryType.RegularFile)) throw new Exception("Wrong entry order/type.");
        if (expectedLink)
        {
            if (entry.LinkName != Names()[0] || entry.Length != 0 || entry.DataStream is not null) throw new Exception("Wrong hard-link representation.");
        }
        else
        {
            using MemoryStream content = new();
            entry.DataStream!.CopyTo(content);
            if (!content.ToArray().SequenceEqual(payload) || entry.LinkName.Length != 0) throw new Exception("Regular entry content changed.");
        }
        Console.WriteLine($"ENTRY {index++}: {entry.Name}; type={entry.EntryType}; LinkName={entry.LinkName}; length={entry.Length}");
    }
    if (reader.GetNextEntry() is not null) throw new Exception("Unexpected extra entry.");
}

void VerifyFiles(string directory, bool linked)
{
    foreach (string name in Names())
        if (!File.ReadAllBytes(Path.Combine(directory, name)).SequenceEqual(payload)) throw new Exception("Extracted contents changed.");
    string first = ObservatoryFileIdentity.Read(Path.Combine(directory, Names()[0]));
    bool same = first == ObservatoryFileIdentity.Read(Path.Combine(directory, Names()[1]));
    if (same != linked || first == ObservatoryFileIdentity.Read(Path.Combine(directory, Names()[2]))) throw new Exception("Extracted file identities violate policy.");
}

byte[] CreateLinkArchive()
{
    using MemoryStream output = new();
    using (TarWriter writer = new(output, leaveOpen: true))
    {
        foreach (string name in Names())
        {
            PaxTarEntry entry = new(name == "alias.dat" ? TarEntryType.HardLink : TarEntryType.RegularFile, name);
            if (name == "alias.dat") entry.LinkName = "calibration.dat";
            else entry.DataStream = new MemoryStream(payload);
            writer.WriteEntry(entry);
            entry.DataStream?.Dispose();
        }
    }
    return output.ToArray();
}

static void Expect<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new Exception($"Expected {typeof(T).Name}.");
}

#if NET11_0_OR_GREATER
static async Task ExpectAsync<T>(Func<Task> action) where T : Exception
{
    try { await action(); }
    catch (T) { return; }
    throw new Exception($"Expected {typeof(T).Name}.");
}
#endif
