# Read CBOR records across incoming chunks

## Goal

A beacon instrument delivers the array [31,-8,1024] in pieces, including splits inside integer headers and bodies. Keep the reader’s structural state while replacing its byte window.

## Older approach

Create a net10.0 / C# 14 console project. Use System.Buffers.Text, System.Buffers and System.Formats.Cbor; omit imports for namespaces not used by the selected snippet. All snippets belong inside a static class in namespace Playbook.Samples; System, System.IO, System.Linq, System.Collections.Generic and System.Threading.Tasks come from implicit usings. Add System.Formats.Cbor 10.0.13 for the comparison project.

The .NET 10 reader consumes one contiguous input buffer, so this complete finite-input example collects all chunks first. That valid baseline needs storage proportional to the complete encoded document.

{{snippet:older}}

## Newer approach

Create the corresponding net11.0 / C# 15 project, with the same imports and implicit usings. Add System.Formats.Cbor 11.0.0-rc.2.26504.105; the verified asset is lib/net11.0.

Create CborReader with CborReaderOptions.ConformanceMode=Lax and isFinalBlock=false. Dispatch only complete states. When PeekState returns NeedsMoreData, retain the last BytesRemaining bytes from the current window, append the next chunk and call SlideData. Exhaustion marks the combined final window with isFinalBlock=true so truncated tokens fail. Retain the underlying byte window until it has been consumed.

{{snippet:newer}}

## Migration

1. Upgrade the sample’s System.Formats.Cbor package and target together. Preserve reader state, retain only the unconsumed tail, and finalise at actual end-of-input. Test every split and one-byte chunks, plus malformed/truncated final input and trailing records.
2. Preserve the observable result and error policy; compile with the actual declared target and dependencies. A newer installed SDK does not establish an older runtime route.
3. Run the focused checks described below before adopting the replacement.

## Constraints

Incremental mode requires Lax; Strict/Canonical modes reject a non-final initial block in RC2. NeedsMoreData means obtain a larger valid window, not retry the same read in a busy loop. SlideData must preserve unconsumed bytes; a chunk can end inside one token. This narrow parser accepts one definite/indefinite measurement array of Int32 values, rejects other states and trailing roots, and retains all decoded results in a List: it does not promise bounded total result memory. Bound incoming chunk/token size and total measurements at the application boundary for untrusted data. Lax is a conformance choice, not permission to trust malformed or unlimited input. Default nesting limits/options are a separate goal; this single-level parser does not change them.

This edition targets .NET 11. It has no rendering mode, external provider, optional workload or experimental opt-in. Its additional package and exact tested asset are stated above. The sample SDK pins 10.0.402 and 11.0.100-rc.2.26504.105 are reproducibility baselines, not mandatory exact consumer SDK installations. RC2 execution is not GA verification. No .NET 10 catalogue edition or conditional older-target package route is advertised.

## Evidence

The original BinaryData projects compile and run against their declared .NET 10/11 SDKs and locked matching CBOR packages. Checks round-trip the original payload through characters and UTF-8, verify short destinations and non-final Base64 tails, exercise every CBOR split plus one-byte chunks, reject a truncated final token and reject trailing roots. The RC2 malformed-Try exception is explicitly checked.

Saved evidence includes exact input hashes, source commit/dirty marker, SDK/runtime host, commands, dependencies and output. No mocks or cross-compilation substitute for execution. Consult the embedded records for what actually ran; no complete-catalogue, native-deployment or product-publication readiness is implied.
