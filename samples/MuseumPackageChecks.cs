using System.IO.Packaging;
using System.Reflection;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using Playbook.Samples;

byte[] content = Encoding.UTF8.GetBytes("Museum index v1\nroom=7;object=clock\n");
string directory = Path.Combine(Path.GetTempPath(), $"playbook-museum-{Guid.NewGuid():N}");
Directory.CreateDirectory(directory);
try
{
    Assembly assembly = typeof(Package).Assembly;
    Console.WriteLine($"Runtime={Environment.Version}; OS={System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
    Console.WriteLine($"Packaging={assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion}; asset={assembly.GetCustomAttribute<TargetFrameworkAttribute>()!.FrameworkName}; SHA256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location)))}");
    if (typeof(PackagePart).GetCustomAttributesData().Any(a => a.AttributeType.Name == "ExperimentalAttribute"))
        throw new Exception("Unexpected experimental contract.");
    foreach (string scenario in new[] { "compressed", "stored", "modified", "read-only" })
    {
        string file = Path.Combine(directory, scenario + ".opc");
        Uri uri = new("/catalogue/index.txt", UriKind.Relative);
        using (Package package = Package.Open(file, FileMode.Create, FileAccess.ReadWrite))
        {
            PackagePart part = package.CreatePart(uri, "text/plain", scenario == "stored" ? CompressionOption.NotCompressed : CompressionOption.Normal);
            using Stream output = part.GetStream(FileMode.Create, FileAccess.Write);
            output.Write(content);
        }
        using Package reopened = Package.Open(file, FileMode.Open, scenario == "read-only" ? FileAccess.Read : FileAccess.ReadWrite);
        PackagePart current = reopened.GetPart(uri);
        if (scenario == "modified")
        {
            using Stream edit = current.GetStream(FileMode.Open, FileAccess.ReadWrite);
            edit.Position = 0;
            edit.Write(content); // A real write in this session, preserving the same logical bytes.
        }
        using (Stream stream = current.GetStream(FileMode.Open, FileAccess.Read))
        {
            bool expectedSeek = scenario != "read-only";
#if NET11_0_OR_GREATER
            expectedSeek = scenario is "stored" or "modified";
#endif
            if (stream.CanSeek != expectedSeek || stream.Length != content.Length) throw new Exception($"Wrong capabilities: {scenario}");
            string seek = Probe(() => stream.Seek(0, SeekOrigin.Begin));
            string positionGet = Probe(() => { _ = stream.Position; });
            string positionSet = Probe(() => stream.Position = 0);
            if (expectedSeek ? seek != "OK" || positionGet != "OK" || positionSet != "OK" : seek != "NotSupportedException" || positionGet != "NotSupportedException" || positionSet != "NotSupportedException")
                throw new Exception("Seek/Position failure contract changed.");
            using MemoryStream read = new();
            stream.CopyTo(read);
            if (!read.ToArray().SequenceEqual(content)) throw new Exception("Sequential content changed.");
            Console.WriteLine($"PASS: {scenario}: CanSeek={stream.CanSeek}; Seek={seek}; Position.get={positionGet}; Position.set={positionSet}; Length={stream.Length}; sequential bytes={read.Length}");
        }
        if (MuseumPackage.CountRecords(current) != 2) throw new Exception("Sequential parser failed.");
        if (scenario == "compressed")
        {
#if NET11_0_OR_GREATER
            Expect<NotSupportedException>(() => MuseumPackage.ReadIndexAssumingSeek(current));
#else
            MuseumPackage.ReadIndexAssumingSeek(current);
#endif
        }
        MuseumPackage.ReadIndex(current, content.Length);
        using (Stream stream = current.GetStream(FileMode.Open, FileAccess.Read))
        using (MemoryStream buffered = MuseumPackage.BufferForIndex(stream, content.Length))
        {
            if (buffered.Position != 0 || !buffered.ToArray().SequenceEqual(content)) throw new Exception("Buffer was not complete and rewound.");
            MuseumPackage.ReadLastThenFirst(buffered);
        }
        using (Stream stream = current.GetStream(FileMode.Open, FileAccess.Read))
        {
            Expect<InvalidDataException>(() => MuseumPackage.BufferForIndex(stream, content.Length - 1));
            if (stream.ReadByte() != content[0]) throw new Exception("Oversized part was read before rejection.");
        }
    }
    using (GrowthStream growing = new(content, declaredLength: 0))
        Expect<InvalidDataException>(() => MuseumPackage.BufferForIndex(growing, 8));
    Console.WriteLine("PASS: sequential parser; exact-budget buffer preserves/rewinds; genuine end/start parser; oversized pre-read rejection and growth guard.");
}
finally { Directory.Delete(directory, recursive: true); }
if (Directory.Exists(directory)) throw new Exception("Temporary package cleanup failed.");
Console.WriteLine("PASS: package handles closed and isolated directory removed.");

static string Probe(Action action)
{
    try { action(); return "OK"; }
    catch (NotSupportedException) { return "NotSupportedException"; }
}

static void Expect<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new Exception($"Expected {typeof(T).Name}.");
}

// Instruments only the budget-growth guard; the access matrix above uses real packages.
sealed class GrowthStream(byte[] data, long declaredLength) : MemoryStream(data)
{
    public override long Length => declaredLength;
}
