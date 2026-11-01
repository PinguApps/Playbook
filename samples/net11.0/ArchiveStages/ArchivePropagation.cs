// snippet:archive-propagation
using System.Diagnostics;

namespace Playbook.Samples;

internal static class ArchivePropagation
{
    public static ActivityContext SetRecorded(ActivityContext parent, bool recorded) => new(
        parent.TraceId, parent.SpanId,
        recorded ? parent.TraceFlags | ActivityTraceFlags.Recorded
                 : parent.TraceFlags & ~ActivityTraceFlags.Recorded,
        parent.TraceState, parent.IsRemote);

    public static bool HasRandomId(ActivityContext context) =>
        (context.TraceFlags & ActivityTraceFlags.RandomTraceId) != 0;

    public static bool HasRandomId(Activity activity) => activity.HasRandomizedTraceId;

    public static string Format(ActivityContext context) =>
        $"00-{context.TraceId}-{context.SpanId}-{(byte)context.TraceFlags:x2}";
}
// endsnippet:archive-propagation
