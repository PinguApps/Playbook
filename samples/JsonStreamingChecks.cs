using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Playbook.Samples;

internal static class Program
{
    private static async Task Main()
    {
        using MemoryStream output = new();
        await ScanFeed.WriteAsync(output, Scans(), CancellationToken.None);
        string json = Encoding.UTF8.GetString(output.ToArray());
        Check(json == "{\"Depot\":\"Cwm\",\"Sequence\":1}\n{\"Depot\":\"Bay\",\"Sequence\":2}\n", "compact LF records including final LF");
        output.Position = 0;
        List<ParcelScan> scans = new();
        await foreach (ParcelScan? scan in ScanFeed.ReadAsync(output, CancellationToken.None)) scans.Add(scan!);
        Check(scans.Count == 2 && scans[1] == new ParcelScan("Bay", 2), "independent deserialization");

        // The producer cannot finish until an actual complete record reaches the stream.
        using RecordObserver observer = new();
        await ScanFeed.WriteAsync(observer, GatedScans(observer), CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(10));
        Check(observer.Observed.Task.IsCompletedSuccessfully, "first record visible before producer completes");
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        try { await ScanFeed.WriteAsync(output, Scans(cancellation.Token), cancellation.Token); throw new Exception("cancellation ignored"); }
        catch (OperationCanceledException) { }

        using MemoryStream first = new();
        using MemoryStream second = new();
        LabelDocuments.Write(first, second);
        Check(Encoding.UTF8.GetString(first.ToArray()).Contains('\n', StringComparison.Ordinal), "first document indented");
        Check(Encoding.UTF8.GetString(second.ToArray()) == "{\"depot\":\"Bay\"}", "second options and target");
        Check(first.CanWrite && second.CanWrite, "writer does not own streams");
#if NET11_0_OR_GREATER
        System.Buffers.ArrayBufferWriter<byte> buffer = new();
        using Utf8JsonWriter writer = new(first);
        writer.Reset(buffer, new JsonWriterOptions { Indented = false });
        writer.WriteNumberValue(7);
        await writer.FlushAsync();
        Check(Encoding.UTF8.GetString(buffer.WrittenSpan) == "7", "buffer reset overload");
#endif
        Console.WriteLine($"PASS JSONL framing, gated consumption, cancellation, writer flush/reset/ownership; runtime {Environment.Version}");
    }

    private static async IAsyncEnumerable<ParcelScan> Scans([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        yield return new("Cwm", 1);
        await Task.Yield();
        yield return new("Bay", 2);
    }

    private static async IAsyncEnumerable<ParcelScan> GatedScans(RecordObserver stream)
    {
        yield return new("Cwm", 1);
        await stream.Observed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        yield return new("Bay", 2);
    }

    private static void Check(bool condition, string claim) { if (!condition) throw new Exception(claim); }

    private sealed class RecordObserver : MemoryStream
    {
        public TaskCompletionSource Observed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override void Write(byte[] buffer, int offset, int count) { base.Write(buffer, offset, count); Inspect(); }
        public override void Write(ReadOnlySpan<byte> buffer) { base.Write(buffer); Inspect(); }
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Write(buffer.Span);
            return ValueTask.CompletedTask;
        }
        private void Inspect()
        {
            byte[] bytes = ToArray();
            int lineEnd = Array.IndexOf(bytes, (byte)'\n');
            if (lineEnd >= 0 && JsonSerializer.Deserialize<ParcelScan>(bytes.AsSpan(0, lineEnd)) == new ParcelScan("Cwm", 1))
                Observed.TrySetResult();
        }
    }
}
