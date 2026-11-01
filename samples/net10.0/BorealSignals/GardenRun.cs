using System.Diagnostics.Tracing;
using Playbook.Samples;

internal static class GardenRun
{
    // snippet:garden-provider
    public static EventSource Open(string garden) => new(
        "Boreal-Observations-" + garden,
        EventSourceSettings.EtwSelfDescribingEventFormat,
        ["garden", garden]);
    // endsnippet:garden-provider

    public static void Run()
    {
        using EventSource source = Open("glasshouse");
        Guid derived = source.Guid;
        SignalChecks.Observe(source, "Boreal-Observations-glasshouse", null, "glasshouse");
        using EventSource reopened = Open("glasshouse");
        SignalChecks.Check(reopened.Guid == derived, "Name-derived identity changed after disposal");
        using EventSource renamed = Open("orchid-room");
        SignalChecks.Check(renamed.Guid != derived, "Different name should derive a different identity");
        using EventSource invalid = new("Boreal-Invalid", EventSourceSettings.ThrowOnEventWriteErrors, ["odd"]);
        SignalChecks.Check(invalid.ConstructionException is ArgumentException, "Odd traits construction reporting");
        Console.WriteLine("PASS BorealSignals: name-derived identity, payload, traits, disposal and construction reporting");
    }
}
