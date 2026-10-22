using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace Playbook.Samples;

// snippet:observer
public sealed class HttpObservation : IDisposable
{
    private readonly MeterListener listener = new();
    private readonly Dictionary<string, long> totals = [];
    public ConcurrentDictionary<string, string> InstrumentTypes { get; } = new();

    public HttpObservation()
    {
        listener.InstrumentPublished = (instrument, owner) =>
        {
            if (instrument.Meter.Name == "System.Net.Http" &&
                instrument.Name is "http.client.active_requests" or "http.client.open_connections")
            {
                InstrumentTypes[instrument.Name] = instrument.GetType().Name;
                owner.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
        {
            // Sum the current observations across tag sets, not across poll intervals.
            lock (totals) totals[instrument.Name] = totals.GetValueOrDefault(instrument.Name) + value;
        });
        listener.Start();
    }

    public void Poll()
    {
        // Serialize polls/readout; absent series represent zero for this aggregate.
        lock (totals)
        {
            totals.Clear();
            listener.RecordObservableInstruments();
        }
    }
    public long Read(string name)
    {
        lock (totals) return totals.GetValueOrDefault(name);
    }
    public void Dispose() => listener.Dispose();
}
// endsnippet:observer
