namespace MobileJournal;

// snippet:journal
// One short-lived open per update. On older iOS/tvOS, sharing also requests an
// advisory OS lock; it is not a safe way to hold ownership while suspended.
public sealed class Journal(string path)
{
    public async Task AppendAsync(byte[] entry, CancellationToken cancellationToken)
    {
        await using FileStream stream = new(path, FileMode.Append, FileAccess.Write,
            FileShare.None, 4096, FileOptions.Asynchronous);
        await stream.WriteAsync(entry, cancellationToken);
    }
}
// endsnippet:journal
