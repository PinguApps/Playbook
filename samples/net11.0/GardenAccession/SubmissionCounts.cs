namespace Playbook.Samples;

// snippet:counts
public sealed class SubmissionCounts
{
    private int posts, accepted;
    public int Posts => Volatile.Read(ref posts);
    public int Accepted => Volatile.Read(ref accepted);
    public void Post() => Interlocked.Increment(ref posts);
    public void Accept() => Interlocked.Increment(ref accepted);
}
// endsnippet:counts
