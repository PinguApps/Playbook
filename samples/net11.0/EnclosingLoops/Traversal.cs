namespace Playbook.Samples;

// snippet:traversal
public static class TransmissionReview
{
    // Zero discards the rest of this batch; a negative reading ends the review.
    public static List<string> Review(int[][] batches)
    {
        List<string> trace = [];
        batchesLoop: for (int batch = 0; batch < batches.Length; batch++)
        {
            using AuditScope batchScope = new(trace, $"batch:{batch}");
            try
            {
                foreach (int reading in batches[batch])
                {
                    using AuditScope readingScope = new(trace, $"reading:{reading}");
                    if (reading < 0) { break batchesLoop; }
                    if (reading == 0) { continue batchesLoop; }
                    trace.Add($"accept:{reading}");
                }
                trace.Add($"complete:{batch}");
            }
            finally { trace.Add($"finally:{batch}"); }
        }
        trace.Add("finished");
        return trace;
    }
}

public sealed class AuditScope(List<string> trace, string name) : IDisposable
{
    public void Dispose() => trace.Add($"dispose:{name}");
}
// endsnippet:traversal
