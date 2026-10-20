# Stream JSON records without collecting the input

## Goal

Write parcel scans as independently readable JSON Lines while the producer is still running. Each line is a complete JSON value. A consumer can process a completed scan without retaining previous scans or waiting for a closing array bracket.

## Older approach

In a net10.0 / C# 14 console project, import `System.Text.Json` and use namespace `Playbook.Samples` (standard System/IO/Threading/Tasks imports may use ImplicitUsings). Serialize one scan at a time, append LF, then flush. This valid approach remains useful when the application needs explicit per-record flushing. It buffers one record, not the whole input.

{{snippet:older}}

## Newer approach

Target net11.0 / C# 15 with the same imports. `SerializeAsyncEnumerable` owns enumeration and framing. Pass `topLevelValues: true`; its default false writes an array. The deliberately conflicting indentation and CRLF settings below demonstrate that JSONL remains compact with LF, including after the last record.

{{snippet:newer}}

Call `await ScanFeed.WriteAsync(destination, scans, cancellationToken)` with a caller-owned writable stream and an `IAsyncEnumerable<ParcelScan>`. Read from a caller-owned stream with `await foreach (ParcelScan? scan in ScanFeed.ReadAsync(source, cancellationToken))`; choose how to handle JSON null records. The reader API already exists in .NET 10. A JSON string containing a newline is escaped within its record.

## Migration

1. Keep the input as an asynchronous sequence; remove any ToList/ToArray over the whole sequence.
2. Select JSONL explicitly and pass the cancellation token to serialization and the producing iterator.
3. Await the write before disposing or reusing the stream. Stream ownership stays with the caller.
4. Give readers top-level-value mode. Check complete record payloads and final LF against the receiving system.
5. Retain the manual loop if a strict per-record latency/flush policy is required; this recipe makes no universal latency or memory-size guarantee.

## Constraints

Use framework System.Text.Json, SDK/runtime 11 and C# 15 for this route. No package asset, workload, provider, UI rendering or experimental opt-in is required. No older-target package route, trimming or Native AOT publication is established. The parallel overload families accept Stream or PipeWriter and options or JsonTypeInfo; this sample executes the Stream/options route only. Do not infer PipeWriter completion/ownership proof from it. A single record can still be large, and backpressure/cancellation must be honored by the source and destination. Output produced before a failure is a partial feed, not a committed batch.

## Evidence

Real .NET 10.0.13 and .NET 11 RC2 builds/executions check exact LF bytes, read-back values, cancellation and a gated producer. The gate opens only after the destination has parsed the first complete scan, before the producer can finish. This proves incremental visibility on the measured stream, not a network latency guarantee. Saved records contain exact input hashes and toolchain output; installed MCP tests verify this complete body offline. RC2 execution is not GA proof. No mocks or required Human proof are used.
