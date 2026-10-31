namespace MobileJournal;

// snippet:journal
// Register exactly one Journal per path in the app. This gate covers only callers
// using that instance; it is not coordination with an extension or another process.
public sealed class Journal(string path) : IDisposable
{
    private readonly SemaphoreSlim gate = new(1, 1);

    public async Task AppendAsync(byte[] entry, CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            await using FileStream stream = new(path, FileMode.Append, FileAccess.Write,
                FileShare.ReadWrite, 4096, FileOptions.Asynchronous);
            await stream.WriteAsync(entry, cancellationToken);
        }
        finally { gate.Release(); }
    }

    // Dispose only after all callers have stopped and their tasks have completed.
    public void Dispose() => gate.Dispose();
}
// endsnippet:journal
