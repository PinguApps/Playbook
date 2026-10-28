// snippet:complete
namespace ReadingRoom;
public sealed class ReadingAudit
{
    private int _disposed, _disconnected, _referenceDisconnects;
    public int Disposed => Volatile.Read(ref _disposed);
    public int Disconnected => Volatile.Read(ref _disconnected);
    public int ReferenceDisconnects => Volatile.Read(ref _referenceDisconnects);
    public void RecordDisposal() => Interlocked.Increment(ref _disposed);
    public void RecordDisconnect() => Interlocked.Increment(ref _disconnected);
    public void RecordReferenceDisconnect() => Interlocked.Increment(ref _referenceDisconnects);
}
// endsnippet:complete
