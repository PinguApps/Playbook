using System.Diagnostics.Tracing;
using Playbook.Samples;

internal static class GardenRun
{
    // snippet:garden-provider
    public static EventSource Open(string garden, Guid identity) => new(
        "Boreal-Observations-" + garden, identity,
        EventSourceSettings.EtwSelfDescribingEventFormat,
        ["garden", garden]);
    // endsnippet:garden-provider

    public static void Run()
    {
        Guid identity = new("1755c71d-5b43-4e24-81fa-631b15d372da");
        using EventSource source = Open("glasshouse", identity);
        SignalChecks.Check(source.Settings == EventSourceSettings.EtwSelfDescribingEventFormat, "Selected settings");
        SignalChecks.Observe(source, "Boreal-Observations-glasshouse", identity, "glasshouse");
        using EventSource simple = new("Boreal-Simple", identity);
        SignalChecks.Check(simple.Settings == EventSourceSettings.EtwManifestEventFormat, "Simple overload default");
        SignalChecks.Observe(simple, "Boreal-Simple", identity);
        foreach (EventSourceSettings settings in new[] { EventSourceSettings.Default, EventSourceSettings.ThrowOnEventWriteErrors })
        {
            using EventSource empty = new("Boreal-Empty", Guid.Empty, settings);
            SignalChecks.Check(empty.ConstructionException is ArgumentException, "Empty GUID reporting");
            using EventSource odd = new("Boreal-Odd", Guid.NewGuid(), settings, ["odd"]);
            SignalChecks.Check(odd.ConstructionException is ArgumentException, "Odd trait reporting");
            Console.WriteLine($"INVALID settings={settings} empty={empty.ConstructionException!.GetType().Name} odd={odd.ConstructionException!.GetType().Name}; no constructor throw");
        }
        try
        {
            using EventSource invalidSettings = new("Boreal-Formats", Guid.NewGuid(),
                EventSourceSettings.EtwManifestEventFormat | EventSourceSettings.EtwSelfDescribingEventFormat);
            throw new InvalidOperationException("Conflicting formats unexpectedly accepted");
        }
        catch (ArgumentException) { Console.WriteLine("INVALID conflicting format flags: constructor throws ArgumentException"); }
        using (EventSource first = new("Boreal-UniqueLifetime", Guid.NewGuid()))
        using (ObservationListener listener = new())
        {
            listener.EnableEvents(first, EventLevel.Verbose);
            using EventSource duplicate = new(first.Name, first.Guid);
            listener.EnableEvents(duplicate, EventLevel.Verbose);
            string before = duplicate.ConstructionException?.GetType().Name ?? "none";
            first.Write("BloomCount", new { Count = 1 });
            duplicate.Write("BloomCount", new { Count = 2 });
            SignalChecks.Check(listener.Received.Count == 1 && first.ConstructionException is null &&
                duplicate.ConstructionException is null, "Pinned duplicate lifetime observation changed");
            Console.WriteLine($"LIFETIME duplicate dynamic GUID: before-write={before}; first={first.ConstructionException?.GetType().Name ?? "none"}; duplicate={duplicate.ConstructionException?.GetType().Name ?? "none"}; delivered={listener.Received.Count}. No native collector guarantee.");
        }
        // A throwing managed listener supplies a real write failure, separate from initialization.
        foreach (bool throwWrites in new[] { false, true })
        {
            using EventSource writes = new("Boreal-WriteErrors", Guid.NewGuid(),
                EventSourceSettings.EtwSelfDescribingEventFormat | (throwWrites ? EventSourceSettings.ThrowOnEventWriteErrors : 0));
            using FailingListener listener = new();
            listener.EnableEvents(writes, EventLevel.Verbose);
            bool threw = false;
            try { writes.Write("BloomCount", new { Count = 1 }); }
            catch (EventSourceException) { threw = true; }
            SignalChecks.Check(threw == throwWrites, "Write error flag conflated with construction errors");
        }
        Console.WriteLine("PASS BorealSignals: both explicit identity constructors, payload, traits/settings, disposal, initialization and write failures");
    }

    private sealed class FailingListener : EventListener
    {
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData.EventName == "BloomCount") throw new InvalidOperationException("Injected consumer failure");
        }
    }
}
