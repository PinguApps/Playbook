# Transfer accumulated text and empty its builder

## Goal

A beacon writer finishes a batch and hands it to another stage. Transfer the builder’s accumulated content and make the empty-source boundary visible to both stages.

## Older approach

Create a net10.0 / C# 14 console project. Use System.Buffers and System.Text; omit imports for namespaces not used by the selected snippet. All snippets belong inside a static class in namespace Playbook.Samples; System, System.IO, System.Linq, System.Collections.Generic and System.Threading.Tasks come from implicit usings.

The older implementation appends each existing chunk into a new builder and then clears the source. It preserves content and the emptied-source contract, while copying characters. It remains appropriate when independent copies are wanted.

{{snippet:older}}

## Newer approach

Create the corresponding net11.0 / C# 15 project, with the same imports and implicit usings.

StringBuilder.MoveChunks returns a builder holding the accumulated content and empties the source. The sample immediately appends a new batch to the source and verifies the transferred text remains unchanged.

{{snippet:newer}}

## Migration

1. Replace copy-and-clear only where the source must become empty. Pass the returned builder to its new owner and prevent concurrent access during the handoff. Verify both the old content and independent source reuse.
2. Preserve the observable result and error policy; compile with the actual declared target and dependencies. A newer installed SDK does not establish an older runtime route.
3. Run the focused checks described below before adopting the replacement.

## Constraints

This is an ownership handoff, not a thread-safety primitive. Serialize access and stop enumerating old GetChunks results before transfer or subsequent mutation; those borrowed views do not become immutable snapshots. Do not assume capacity, maximum capacity, chunk layout or allocation counts remain identical to the copying version. Convert to a string separately if the consumer needs immutable text.

This edition targets .NET 11. It has no rendering mode, external provider, optional workload or experimental opt-in. It uses framework APIs and needs no additional package or package asset. The sample SDK pins 10.0.402 and 11.0.100-rc.2.26504.105 are reproducibility baselines, not mandatory exact consumer SDK installations. RC2 execution is not GA verification. No .NET 10 catalogue edition or conditional older-target package route is advertised.

## Evidence

The original BufferAdapters projects compile and run against their declared .NET 10/11 SDKs. Checks cover UTF-8 split across segments, encoded text, sliced writes, borrowed-memory mutation, fixed-size overflow and source-emptying with independent builder reuse.

Saved evidence includes exact input hashes, source commit/dirty marker, SDK/runtime host, commands, dependencies and output. No mocks or cross-compilation substitute for execution. Consult the embedded records for what actually ran; no complete-catalogue, native-deployment or product-publication readiness is implied.
