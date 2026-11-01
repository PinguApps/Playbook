using System.Diagnostics.Tracing;

namespace Playbook.Samples;

internal sealed class ObservationListener : EventListener
{
    public List<(string Name, string[] Keys, object?[] Values)> Received { get; } = [];

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (eventData.EventName == "BloomCount")
            Received.Add((eventData.EventName, eventData.PayloadNames!.ToArray(), eventData.Payload!.ToArray()));
    }
}

internal static class SignalChecks
{
    public static void Check(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }

    public static void Observe(EventSource source, string expectedName, Guid? expectedGuid, string? trait = null)
    {
        Check(source.ConstructionException is null, $"Construction failed: {source.ConstructionException}");
        Check(source.Name == expectedName && source.Guid != Guid.Empty, "Provider identity");
        if (expectedGuid.HasValue) Check(source.Guid == expectedGuid, "Explicit GUID changed");
        if (trait is not null) Check(source.GetTrait("garden") == trait, "Trait lost");
        using ObservationListener listener = new();
        listener.EnableEvents(source, EventLevel.Informational);
        source.Write("BloomCount", new EventSourceOptions { Level = EventLevel.Informational }, new { Bed = "fern-house", Count = 7 });
        Check(listener.Received.Count == 1, "Expected one actual event");
        var observation = listener.Received[0];
        Check(observation.Keys.SequenceEqual(["Bed", "Count"]) &&
            Equals(observation.Values[0], "fern-house") && Equals(observation.Values[1], 7), "Payload changed");
        listener.DisableEvents(source);
        source.Write("BloomCount", new { Bed = "disabled", Count = 8 });
        Check(listener.Received.Count == 1, "Disabled listener received event");
        listener.EnableEvents(source, EventLevel.Verbose);
        source.Dispose();
        source.Write("BloomCount", new { Bed = "disposed", Count = 9 });
        Check(listener.Received.Count == 1 && !source.IsEnabled(), "Disposed provider delivered");
        source.Dispose();
        Console.WriteLine($"OBSERVED {expectedName} guid={source.Guid} settings={source.Settings} payload=fern-house/7; disable/dispose clean");
    }
}
