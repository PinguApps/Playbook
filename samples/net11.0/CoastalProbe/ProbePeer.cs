// snippet:complete
namespace CoastalProbe;

// A fixed Java name keeps callers independent of generated package naming.
[Android.Runtime.Register("org/coastal/probe/ProbePeer")]
public sealed class ProbePeer : Java.Lang.Object
{
    private Stream? readings = new MemoryStream();

    [Java.Interop.Export("readingCount")]
    public int ReadingCount() => checked((int)(readings?.Length ?? 0));

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            readings?.Dispose();
            readings = null;
        }
        base.Dispose(disposing);
    }
}
// endsnippet:complete
