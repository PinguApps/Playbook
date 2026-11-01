using System.Diagnostics;

namespace Playbook.Samples;

internal static class Program
{
    private static void Require([System.Diagnostics.CodeAnalysis.DoesNotReturnIf(false)] bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public static void Main()
    {
        RuntimeIdentity.Assert();
        var previousGenerator = Activity.TraceIdGenerator;
        var previousCurrent = Activity.Current;
        var previousFormat = Activity.DefaultIdFormat;
        bool previousForce = Activity.ForceDefaultIdFormat;
        List<string> started = [], stopped = [];
        ActivitySamplingResult decision = ActivitySamplingResult.PropagationData;
        try
        {
            Activity.Current = null;
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;
            Activity.TraceIdGenerator = null;
            using var source = new ActivitySource("HearthArchive.Import");
            using var listener = new ActivityListener
            {
                ShouldListenTo = s => ReferenceEquals(s, source),
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => decision,
                SampleUsingParentId = (ref ActivityCreationOptions<string> _) => decision,
                ActivityStarted = a => started.Add(a.OperationName), ActivityStopped = a => stopped.Add(a.OperationName)
            };
            ActivitySource.AddActivityListener(listener);
            const string trace = "112233445566778899aabbccddeeff00";
            const string span = "1234567890abcdef";
            foreach (byte flags in new byte[] { 0, 1, 2, 3 })
            {
                string header = $"00-{trace}-{span}-{flags:x2}";
                Require(ActivityContext.TryParse(header, "archive=import", true, out var parent), "Parse failed");
                Require((byte)parent.TraceFlags == flags && parent.IsRemote && parent.TraceState == "archive=import", "Parse lost metadata");
                Require(ArchivePropagation.Format(parent) == header, "Format lost flags");
                foreach (bool recorded in new[] { false, true })
                {
                    var forwarded = ArchivePropagation.SetRecorded(parent, recorded);
                    int expected = (flags & ~1) | (recorded ? 1 : 0);
                    Require((byte)forwarded.TraceFlags == expected && forwarded.TraceId == parent.TraceId &&
                        forwarded.SpanId == parent.SpanId && forwarded.TraceState == parent.TraceState && forwarded.IsRemote, "Forwarding lost context");
                    decision = recorded ? ActivitySamplingResult.AllDataAndRecorded : ActivitySamplingResult.PropagationData;
                    using (var child = source.StartActivity("catalogue", ActivityKind.Internal, parent)!)
                    {
                        Require(child is not null && (byte)child.ActivityTraceFlags == expected && child.Recorded == recorded, "Sampling flags differ");
                        Require(child.TraceId == parent.TraceId && child.ParentSpanId == parent.SpanId && child.TraceStateString == parent.TraceState, "Parent identity lost");
#if NET11_0_OR_GREATER
                        Require(ArchivePropagation.HasRandomId(child) == ((flags & 2) != 0) &&
                            ArchivePropagation.HasRandomId(child.Context) == child.HasRandomizedTraceId, "Random and Recorded conflated");
#endif
                        Require(ActivityContext.TryParse(child.Id, child.TraceStateString, out var reparsed) &&
                            reparsed.TraceFlags == child.ActivityTraceFlags, "Activity formatting lost bits");
                    }
                    using (var child = source.StartActivity("index", ActivityKind.Internal, parentId: header)!)
                        Require(child is not null && (byte)child.ActivityTraceFlags == expected, "String-parent sampling lost bit");
                }
                using var direct = new Activity("manual-stage").SetIdFormat(ActivityIdFormat.W3C)
                    .SetParentId(parent.TraceId, parent.SpanId, parent.TraceFlags).Start();
                Require((byte)direct.ActivityTraceFlags == flags, "Direct Activity parent flags lost");
            }
            decision = ActivitySamplingResult.PropagationData;
            using (var root = source.StartActivity("default-root")!)
            {
                Require(root is not null && !root.Recorded && root.TraceId != default, "Default root missing");
#if NET11_0_OR_GREATER
                Require(root.HasRandomizedTraceId, "Default root not marked random");
#else
                Require((byte)root.ActivityTraceFlags == 0, "Older runtime unexpectedly marks root");
#endif
                Console.WriteLine($"ROOT default flags={(byte)root.ActivityTraceFlags:x2}");
            }
            var fixedId = ActivityTraceId.CreateFromString(trace);
            Activity.TraceIdGenerator = () => fixedId;
            using (var root = source.StartActivity("custom-root")!)
            {
                Require(root is not null && root.TraceId == fixedId && (byte)root.ActivityTraceFlags == 0, "Custom deterministic root mislabeled");
                Console.WriteLine($"ROOT custom flags={(byte)root.ActivityTraceFlags:x2}");
            }
            Require(started.Count == 18 && stopped.Count == 18 && Activity.Current is null, "Listener lifetime/current leak");
            Console.WriteLine("PASS ArchiveStages: four flags; context/string/direct parents; recording changes; default/custom roots; balanced listeners.");
        }
        finally
        {
            Activity.TraceIdGenerator = previousGenerator;
            Activity.DefaultIdFormat = previousFormat;
            Activity.ForceDefaultIdFormat = previousForce;
            Activity.Current = previousCurrent;
        }
    }
}
