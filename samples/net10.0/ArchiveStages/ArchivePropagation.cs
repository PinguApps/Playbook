#region archive-propagation
using System.Diagnostics;

namespace Playbook.Samples;

internal static class ArchivePropagation
{
    // Preserve all incoming bits, including ones this runtime cannot name.
    public static ActivityContext SetRecorded(ActivityContext parent, bool recorded) => new(
        parent.TraceId, parent.SpanId,
        recorded ? parent.TraceFlags | ActivityTraceFlags.Recorded
                 : parent.TraceFlags & ~ActivityTraceFlags.Recorded,
        parent.TraceState, parent.IsRemote);

    public static string Format(ActivityContext context) =>
        $"00-{context.TraceId}-{context.SpanId}-{(byte)context.TraceFlags:x2}";
}
#endregion archive-propagation
