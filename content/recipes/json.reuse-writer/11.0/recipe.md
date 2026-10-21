# Reuse a flushed JSON writer with different options

## Goal

Write a readable depot label followed by a compact machine label without keeping separate writer objects alive. Flush each document before changing the target and keep stream ownership explicit.

## Older approach

In a net10.0 / C# 14 console project, import `System.Text.Json` and use namespace `Playbook.Samples`, with standard implicit System/IO imports. Creating a second writer is correct when options differ. The older Reset overloads can change the output target but retain the writer's existing options.

{{snippet:older}}

## Newer approach

In net11.0 / C# 15 with the same imports, pass both the new target and options to Reset. Complete the object and flush before switching. The stream supplied by the caller stays open when the writer is disposed.

{{snippet:newer}}

Call `LabelDocuments.Write(firstStream, secondStream)`. The first contains indented JSON for Cwm; the second is exactly `{"depot":"Bay"}`. The buffer overload similarly takes an `IBufferWriter<byte>` plus JsonWriterOptions. Pooling policy is an application concern; this example reuses a single local writer.

## Migration

1. Keep one writer only where measured usage benefits from reuse; constructing writers remains valid.
2. Finish and flush the current document before Reset. Reset is not a substitute for delivering pending bytes.
3. Pass the intended destination and full JsonWriterOptions on every option-changing reset.
4. Await any asynchronous write/flush before reuse. Never concurrently use a writer or return it to a pool while an operation is pending.
5. Dispose the writer, then separately dispose caller-owned streams when their work ends.

## Constraints

Requires framework System.Text.Json on net11.0 with SDK 11 / C# 15. No packages, workloads, providers, rendering mode or experimental flags. No optional older-target route or Native AOT publication is claimed. This checks synchronous memory-backed writes and a buffer reset; it does not benchmark pooling, exercise concurrent use or establish durability of a file/network flush. Changing the options can change the wire format. The IBufferWriter destination also retains its own ownership/lifetime.

## Evidence

Actual .NET 10 and RC2 .NET 11 samples compile and execute. Checks inspect the first indented document, exact compact second output, still-writable streams after disposal and the .NET 11 buffer/options Reset overload. The .NET 10 sample constructs two writers honestly rather than pretending its Reset can replace options. Installed list/get verifies the complete original comparison and target constraints offline. No mocks or targeted Human proof; GA reconciliation remains pending.
