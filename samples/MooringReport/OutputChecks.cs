using System.Runtime.InteropServices;
using System.Text;
using MooringReport;

Console.WriteLine($"RUNTIME {RuntimeInformation.FrameworkDescription} / {RuntimeInformation.OSDescription}");
using (var bytes = new MemoryStream())
{
    using TextWriter writer = new StreamWriter(bytes, new UTF8Encoding(false), 128, leaveOpen: true) { NewLine = "\n" };
    await ReportOutput.WriteAsync(writer, "Mooring 17: clear", CancellationToken.None);
    await writer.FlushAsync();
    Check(Encoding.UTF8.GetString(bytes.ToArray()) == "Mooring 17: clear | ready\n\n", "complete text and newlines");
}
using (var bytes = new MemoryStream())
{
    using TextWriter writer = new StreamWriter(bytes, new UTF8Encoding(false), 128, leaveOpen: true);
    using var cancelled = new CancellationTokenSource();
    await cancelled.CancelAsync();
    await ExpectCancellation(() => ReportOutput.WriteAsync(writer, "not written", cancelled.Token));
#if NET11_0_OR_GREATER
    await ExpectCancellation(() => writer.WriteLineAsync("not written", cancelled.Token));
    await ExpectCancellation(() => writer.WriteLineAsync(cancelled.Token));
#else
    await ExpectCancellation(() => writer.WriteLineAsync("not written".AsMemory(), cancelled.Token));
    await ExpectCancellation(() => writer.WriteLineAsync(ReadOnlyMemory<char>.Empty, cancelled.Token));
#endif
    await writer.FlushAsync();
    Check(bytes.Length == 0, "pre-cancelled concrete writer leaves no output");
}

// A real StreamWriter calls this stream. The first byte write succeeds; the
// second waits until cancellation, so no timer or scheduling race chooses the prefix.
using (var bytes = new PausingStream())
{
    using TextWriter writer = new StreamWriter(bytes, new UTF8Encoding(false), 128, leaveOpen: true);
    using var cancellation = new CancellationTokenSource();
    var text = new string('M', 8192);
    Task operation = ReportOutput.WriteAsync(writer, text, cancellation.Token);
    await bytes.Blocked.Task.WaitAsync(TimeSpan.FromSeconds(10));
    Check(bytes.ObservedToken == cancellation.Token, "operation token reaches underlying stream");
    await cancellation.CancelAsync();
    await ExpectCancellation(() => operation);
    byte[] prefix = bytes.ToArray();
    Check(prefix.Length > 0 && prefix.Length < text.Length, "cancellation retains a bounded partial write");
    Check(prefix.All(b => b == (byte)'M'), "retained prefix has only report bytes");
    Console.WriteLine($"PARTIAL acceptedBytes={prefix.Length}; requestedTextChars={text.Length}; no rollback");
    // Disposal may flush writer buffers without the cancelled operation token.
    // Check the operation boundary above, then allow cleanup rather than retrying it.
    bytes.AllowCleanup = true;
}

// Non-virtual convenience calls delegate to memory overrides. TextWriter.Null
// deliberately ignores cancellation: tokens do not impose a universal subclass contract.
await ReportOutput.WriteAsync(TextWriter.Null, "discarded", new CancellationToken(true));
Console.WriteLine("PASS output: success/newlines, pre-cancel, in-flight real StreamWriter, token forwarding, partial prefix, Null limitation");

static void Check(bool value, string name)
{
    if (!value) throw new InvalidOperationException(name);
}

static async Task ExpectCancellation(Func<Task> action)
{
    try { await action().WaitAsync(TimeSpan.FromSeconds(10)); }
    catch (OperationCanceledException) { return; }
    throw new InvalidOperationException("Expected cancellation");
}

sealed class PausingStream : MemoryStream
{
    private int writes;
    public TaskCompletionSource Blocked { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public CancellationToken ObservedToken { get; private set; }
    public bool AllowCleanup { get; set; }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (++writes == 2 && !AllowCleanup)
        {
            ObservedToken = cancellationToken;
            Blocked.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }
        await base.WriteAsync(buffer, cancellationToken);
    }
}
