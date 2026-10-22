# Poll observable HTTP request and connection instruments

## Goal

A custom diagnostics listener needs the current number of outbound HTTP requests and open connections. Preserve the distinction between event deltas and observable snapshots instead of silently reporting zero or accumulating repeated snapshots.

## Older approach

In the tested .NET 10 baseline, System.Net.Http's http.client.active_requests and http.client.open_connections are UpDownCounter instruments. Subscribe before creating the client and aggregate their positive/negative deltas over the listener's window. This small sample combines tag sets; it cannot reconstruct work that started before subscription.

{{snippet:older}}

## Newer approach

The tested .NET 11 instruments are ObservableUpDownCounter values. Starting a MeterListener and enabling measurement events does not itself collect them. Poll with RecordObservableInstruments, clear the previous aggregate, and sum each poll's current tag sets once. Read returns the latest poll's aggregate, not a lifetime sum.

{{snippet:newer}}

## Migration

1. Use a net11.0 C# 15.0 console project with nullable/implicit usings enabled. Import System.Diagnostics.Metrics and System.Collections.Concurrent. These APIs and HTTP instrumentation come from Microsoft.NETCore.App; no package, account or exporter is required.
2. Create and retain HttpObservation before the HttpClient traffic you intend to inspect. Let it enable only the named instruments on the System.Net.Http meter. Use a real SocketsHttpHandler transport for these HTTP client metrics; a replacement mock handler is not transport evidence.
3. Call Poll at the collection interval, then Read with http.client.active_requests or http.client.open_connections. Poll on a serialized collection path; dispose the observer when the diagnostics owner stops. The sample's lock serializes poll/readout and the publication map is concurrent.
4. Replace old delta accumulation for these two instruments with current-snapshot handling. Reset the aggregate once per poll, not per individual callback. A repeated unchanged poll must not double the total. Missing series contribute zero to this explicitly combined aggregate.
5. If dimensions matter, preserve the complete instrument/tag-set key and replace each collection's values. This teaching sample deliberately sums all tag sets. Keep high-cardinality tags bounded and avoid leaking private request information. Check your exporter before writing a custom polling loop: an exporter that already collects observables may need no application change.

## Constraints

This recipe covers the two named outbound System.Net.Http instruments, not every HTTP instrument or ASP.NET server metric. Do not apply snapshot accumulation rules to event counters or histograms such as request duration. Instrument runtime type and semantics matter more than the meter name alone.

Polls are observations, not a transactional cross-instrument snapshot or a request audit trail. A process-wide shared meter can include other clients; the sample uses an isolated process with one loopback client. A late .NET 10 listener's delta total describes only its subscribed window. Production dimensional storage, scheduling, exporter integration and TLS/remote-service behavior need their own policy/evidence. Browser/mobile transports are not covered.

## Evidence

Both actual projects send a real HTTP request over a TcpListener loopback socket with SocketsHttpHandler and no proxy. A barrier holds the response while active requests and open connections equal one. After the real response, active becomes zero while the keep-alive connection remains open; disposing the client/handler closes it and open becomes zero. The .NET 11 check proves no observable callbacks before the first poll and that repeated polls do not accumulate. Runtime instrument types distinguish UpDownCounter from ObservableUpDownCounter. These are real socket/HTTP measurements, not mocks, exporter proof or a new OS matrix. Exact .NET 10 and RC2 artifacts and hashed inputs accompany the recipe.
