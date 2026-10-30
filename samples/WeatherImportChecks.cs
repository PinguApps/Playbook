using System.Buffers.Binary;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Playbook.Samples;

byte[] valid = MakeArchive();
int central = Find(valid, 0x02014b50);
int end = Find(valid, 0x06054b50);
byte[] badSignature = (byte[])valid.Clone();
badSignature[central] ^= 0x20;
byte[] badCount = (byte[])valid.Clone();
BinaryPrimitives.WriteUInt16LittleEndian(badCount.AsSpan(end + 8), 2);
BinaryPrimitives.WriteUInt16LittleEndian(badCount.AsSpan(end + 10), 2);
foreach (var (name, bytes) in new[] { ("signature", badSignature), ("entry-count", badCount) })
{
    using var stream = new TrackingStream(bytes);
    string stage = await FailureStage(stream);
#if NET11_0_OR_GREATER
        Require(stage == "factory", "RC2 must reject during initialization");
#else
        Require(stage == "Entries", ".NET 10 must defer this directory failure");
#endif
    Console.WriteLine($"FAILURE {name}: {stage}; fixture-sha256={Convert.ToHexStringLower(SHA256.HashData(bytes))}");
    Require(!stream.Disposed, "leaveOpen=true preserves caller ownership");
    using var importerSource = new MemoryStream(bytes);
    Require(await WeatherImport.ReadStationNamesAsync(importerSource) is null, "canonical boundary rejects malformed archive");
}

// Missing end record already fails at the factory on both releases. Ownership
// cannot be transferred to an archive that was never returned.
foreach (bool leaveOpen in new[] { false, true })
{
    var source = new TrackingStream(new byte[64]);
    try
    {
        await ExpectInvalid(async () => { await using var zip = await ZipArchive.CreateAsync(source, ZipArchiveMode.Read, leaveOpen, null); });
        Require(!source.Disposed, "failed factory leaves supplied seekable stream for caller cleanup");
    }
    finally { await source.DisposeAsync(); }
    Require(source.Disposed, "caller disposes after factory failure");
}
foreach (bool leaveOpen in new[] { false, true })
{
    var source = new TrackingStream(valid);
    await using (var archive = await ZipArchive.CreateAsync(source, ZipArchiveMode.Read, leaveOpen, null))
        Require(archive.Entries.Count == 1, "valid directory");
    Require(source.Disposed == !leaveOpen, "successful archive disposal follows leaveOpen");
    await source.DisposeAsync();
}

using (var asyncOnly = new TrackingStream(valid, rejectSync: true))
{
    await using var archive = await ZipArchive.CreateAsync(asyncOnly, ZipArchiveMode.Read, true, null);
    int reads = asyncOnly.AsyncReads;
    Require(reads > 0 && asyncOnly.SyncReads == 0, "factory uses asynchronous reads");
#if NET11_0_OR_GREATER
    Require(archive.Entries.Single().FullName == "gower-station.csv", "eager directory names");
    Require(asyncOnly.AsyncReads == reads && asyncOnly.SyncReads == 0, "Entries performs no reads");
#else
    try { _ = archive.Entries.Count; throw new InvalidOperationException("Expected deferred sync read."); }
    catch (NotSupportedException) { Require(asyncOnly.SyncReads > 0, "older Entries attempts sync read"); }
#endif
    Console.WriteLine($"ASYNC factory-reads={reads}; Entries additional async={asyncOnly.AsyncReads - reads}; sync={asyncOnly.SyncReads}");
}

using (var cancellation = new CancellationTokenSource())
{
    await cancellation.CancelAsync();
    await using var source = new TrackingStream(valid);
    try { _ = await WeatherImport.ReadStationNamesAsync(source, cancellation.Token); throw new InvalidOperationException("Expected cancellation."); }
    catch (OperationCanceledException) { Require(source.AsyncReads == 0 && !source.Disposed, "pre-cancellation and caller ownership"); }
}

// Change only Stored payload bytes: directory, lengths and recorded CRC remain.
byte[] damagedPayload = (byte[])valid.Clone();
int data = 30 + BinaryPrimitives.ReadUInt16LittleEndian(valid.AsSpan(26)) + BinaryPrimitives.ReadUInt16LittleEndian(valid.AsSpan(28));
damagedPayload[data] ^= 1;
using (var source = new MemoryStream(damagedPayload))
{
    await using var archive = await ZipArchive.CreateAsync(source, ZipArchiveMode.Read, true, null);
    Require(archive.Entries.Count == 1, "directory success does not validate payload");
    await using var payload = await archive.Entries[0].OpenAsync();
#if NET11_0_OR_GREATER
    await ExpectInvalid(async () => await payload.CopyToAsync(Stream.Null));
#else
    await payload.CopyToAsync(Stream.Null);
#endif
}
using (var source = new MemoryStream(valid))
    Require((await WeatherImport.ReadStationNamesAsync(source))?.Single() == "gower-station.csv", "canonical valid import");
var assembly = typeof(ZipArchive).Assembly;
Console.WriteLine($"ARTIFACT {assembly.FullName}; sha256={Convert.ToHexStringLower(SHA256.HashData(await File.ReadAllBytesAsync(assembly.Location)))}");
Console.WriteLine($"PASS: ZIP initialization boundaries, ownership, cancellation, real async-only reads and damaged Stored payload; runtime={RuntimeInformation.FrameworkDescription}");

static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
static async Task<string> FailureStage(Stream source)
{
    ZipArchive archive;
    try { archive = await ZipArchive.CreateAsync(source, ZipArchiveMode.Read, true, null); }
    catch (InvalidDataException) { return "factory"; }
    await using (archive)
    {
        try { _ = archive.Entries.Count; }
        catch (InvalidDataException) { return "Entries"; }
    }
    throw new InvalidOperationException("Malformed directory was accepted.");
}
static async Task ExpectInvalid(Func<Task> action)
{
    try { await action(); throw new InvalidOperationException("Expected InvalidDataException."); }
    catch (InvalidDataException) { }
}
static byte[] MakeArchive()
{
    using var stream = new MemoryStream();
    using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
    {
        var entry = archive.CreateEntry("gower-station.csv", CompressionLevel.NoCompression);
        entry.LastWriteTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        using var payload = entry.Open();
        payload.Write(Encoding.UTF8.GetBytes("station,rain_mm\ngower,12\n"));
    }
    return stream.ToArray();
}
static int Find(byte[] bytes, uint signature)
{
    for (int i = 0; i <= bytes.Length - 4; i++)
        if (BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(i)) == signature) return i;
    throw new InvalidOperationException("Missing fixture signature.");
}

sealed class TrackingStream(byte[] bytes, bool rejectSync = false) : Stream
{
    private readonly MemoryStream inner = new(bytes, writable: false);
    public int AsyncReads { get; private set; }
    public int SyncReads { get; private set; }
    public bool Disposed { get; private set; }
    public override bool CanRead => inner.CanRead;
    public override bool CanSeek => inner.CanSeek;
    public override bool CanWrite => false;
    public override long Length => inner.Length;
    public override long Position { get => inner.Position; set => inner.Position = value; }
    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
    public override int Read(Span<byte> buffer)
    {
        SyncReads++;
        if (rejectSync) throw new NotSupportedException("Synchronous reads prohibited by fixture.");
        return inner.Read(buffer);
    }
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        AsyncReads++;
        return inner.ReadAsync(buffer, cancellationToken);
    }
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);
    public override void Flush() => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    protected override void Dispose(bool disposing) { if (disposing) { Disposed = true; inner.Dispose(); } base.Dispose(disposing); }
}
