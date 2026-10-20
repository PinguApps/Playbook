# Pass existing buffers to stream consumers

## Goal

A beacon manifest already lives in memory, but an import reader accepts Stream. Keep the adapter alive only while the owner guarantees that its underlying memory is valid and unchanged.

## Older approach

Create a net10.0 / C# 14 console project. Use System.Buffers and System.Text; omit imports for namespaces not used by the selected snippet. All snippets belong inside a static class in namespace Playbook.Samples; System, System.IO, System.Linq, System.Collections.Generic and System.Threading.Tasks come from implicit usings.

The comparison copies into MemoryStream for the general ReadOnlyMemory and segmented cases and materializes encoded text bytes. An existing array can already be wrapped by MemoryStream without copying on .NET 10; these new adapters do not invalidate that efficient older choice.

{{snippet:older}}

## Newer approach

Create the corresponding net11.0 / C# 15 project, with the same imports and implicit usings.

Choose ReadOnlyMemoryStream for borrowed bytes, ReadOnlySequenceStream for segmented bytes, StringStream for lazily encoded text and WritableMemoryStream for a fixed writable region. Import System.Buffers and System.Text alongside the implicit System.IO import.

{{snippet:newer}}

## Migration

1. Select the adapter from the actual source representation. Preserve encoding and reader ownership/disposal. Test split multibyte UTF-8, partial reads and sliced writes, then keep the underlying owner alive for the complete asynchronous read.
2. Preserve the observable result and error policy; compile with the actual declared target and dependencies. A newer installed SDK does not establish an older runtime route.
3. Run the focused checks described below before adopting the replacement.

## Constraints

Adapters do not transfer ownership or extend a pool/pipe buffer’s valid lifetime. Finish and dispose the reader/stream before returning memory to a pool or advancing a PipeReader; do not mutate it concurrently. ReadOnly means the stream cannot write, not that the original owner cannot change bytes. WritableMemoryStream writes into the supplied slice and cannot grow. StringStream needs an explicit Encoding and can allocate encoding working storage; avoiding a whole-input byte array is not a zero-allocation promise. Its ReadOnlyMemory<char> source must also remain valid. Encoding fallback/BOM detection are independent choices; this example uses UTF-8 and StreamReader’s ordinary BOM detection. Capability flags differ by adapter: inspect CanRead/CanWrite/CanSeek rather than assuming MemoryStream’s full contract.

This edition targets .NET 11. It has no rendering mode, external provider, optional workload or experimental opt-in. It uses framework APIs and needs no additional package or package asset. The sample SDK pins 10.0.402 and 11.0.100-rc.2.26504.105 are reproducibility baselines, not mandatory exact consumer SDK installations. RC2 execution is not GA verification. No .NET 10 catalogue edition or conditional older-target package route is advertised.

## Evidence

The original BufferAdapters projects compile and run against their declared .NET 10/11 SDKs. Checks cover UTF-8 split across segments, encoded text, sliced writes, borrowed-memory mutation, fixed-size overflow and source-emptying with independent builder reuse.

Saved evidence includes exact input hashes, source commit/dirty marker, SDK/runtime host, commands, dependencies and output. No mocks or cross-compilation substitute for execution. Consult the embedded records for what actually ran; no complete-catalogue, native-deployment or product-publication readiness is implied.
