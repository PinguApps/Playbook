namespace Playbook.Samples;

// snippet:counts
public sealed class SubmissionCounts
{
    private int _posts;
    private int _accepted;
    public int Posts => Volatile.Read(ref _posts);
    public int Accepted => Volatile.Read(ref _accepted);
    public void RecordPost() => Interlocked.Increment(ref _posts);
    public void RecordAcceptance() => Interlocked.Increment(ref _accepted);
}
// endsnippet:counts
