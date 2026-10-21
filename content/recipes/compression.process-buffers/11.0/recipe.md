# Compress independent buffers and reuse codec state

## Goal

A lighthouse processes a sequence of finite survey buffers, including empty responses, without allocating stream adapters for every buffer.

## Older approach

Create a net10.0 / C# 14 console project with implicit usings enabled. Import System.IO.Compression and System.Buffers. Put the methods inside a static class in namespace Playbook.Samples. System, System.IO, System.Linq, System.Collections.Generic and System.Threading.Tasks come from implicit usings. No additional packages are required.

Fresh stream wrappers remain a valid implementation. Each wrapper is disposed before reading its output and a new wrapper starts each independent message.

{{snippet:older}}

## Newer approach

Create the corresponding net11.0 / C# 15 project with the same imports and implicit usings. These are framework APIs, not an independently versioned compression package. The canonical sample is verified against .NET 11 RC2.

The three format-specific methods demonstrate actual encoder/decoder pairs. Allocate from the matching GetMaxCompressedLength bound, check consumed/written counts and OperationStatus, and Reset both instances before each independent message. A one-byte decode margin lets the decoder finish an empty message and detect output beyond the known logical length.

{{snippet:newer}}

For the finite trusted payloads in this example, restore with the matching format. The old reader accepts deflate/zlib/gzip; the current reader also accepts zstd.

.NET 10:

{{snippet:restore-older}}

.NET 11:

{{snippet:restore-newer}}

## Migration

1. Keep the consumer's actual target explicit. Upgrade to .NET 11 before adopting this edition; no conditional older-target route is advertised.
2. Negotiate the exact format and message boundary, then test empty/nonempty payloads and independent resource reuse.
3. Use the declared matching SDK/framework artifacts and rerun the focused checks below when source, target or dependency inputs change.

## Constraints

Deflate is raw framing, ZLib adds its wrapper/checksum, and GZip has different headers/trailers; select the agreed wire format at both ends. Reset discards previous state and does not finish a pending frame: finalise and consume it first. Dispose all codec instances and avoid concurrent use. These complete finite-message helpers reject any status other than Done; they are not an incremental network parser. For incremental callers retain unconsumed input, advance only by bytesConsumed/bytesWritten, provide more output on DestinationTooSmall, provide new input on NeedMoreData, reject InvalidData, and mark isFinalBlock only at the actual end. Never spin on unchanged buffers. Bound compressed input and expanded output for untrusted peers; GetMaxCompressedLength is a compression allocation bound, not a safe decompression-size hint. Decoding into a known size here is for original trusted test messages, not arbitrary size metadata from an attacker.

This edition has no external provider, workload, rendering requirement or experimental opt-in. Shared Blazor applicability means server-side code; it is not browser execution proof. All sample dependencies are framework-provided. Exact SDK pins 10.0.402 and 11.0.100-rc.2.26504.105 are reproducibility baselines, not mandatory exact consumer installations. RC verification does not establish GA behaviour or every platform's behaviour.

## Evidence

Actual .NET 10 stream and RC2 span implementations round-trip empty, short, repetitive 8192-byte and subsequent independent payloads in all three formats. GZip checks additionally observe DestinationTooSmall for encode/decode and NeedMoreData for a partial header.

The actual declared .NET 10 and .NET 11 projects compile and execute on Windows with zero warnings/errors. Saved records identify source hashes, SDK/target/language, resolved dependencies, commands, host and output; freshness is checked at package build. These are execution records, not source-review-only, cross-compilation or mock claims. The existing installed-tool harness independently verifies the complete canonical body, target-11 applicability/edition and target-10 refusal offline from an unrelated directory. Comprehensive catalogue and public-release readiness remain separate gates.
