# Exchange Zstandard frames with built-in codecs

## Goal

A lighthouse receiver has explicitly agreed to Zstandard frames, so a GZip payload would be a protocol error even if both sides can decompress it separately.

## Older approach

Create a net10.0 / C# 14 console project with implicit usings enabled. Import System.IO.Compression and System.Buffers. Put the methods inside a static class in namespace Playbook.Samples. System, System.IO, System.Linq, System.Collections.Generic and System.Threading.Tasks come from implicit usings. No additional packages are required.

.NET 10 has no built-in Zstandard codec. The comparison throws explicitly. A separately reviewed external library is a valid older option, but this recipe does not advertise or verify one; GZip is not a Zstandard wire-compatible substitute.

{{snippet:older}}

## Newer approach

Create the corresponding net11.0 / C# 15 project with the same imports and implicit usings. These are framework APIs, not an independently versioned compression package. The canonical sample is verified against .NET 11 RC2.

Use System.IO.Compression directly: no separate System.IO.Compression.Zstandard package is required or restored. The stream helper finalises before returning. The buffer helper reuses an encoder/decoder, resets per independent message and limits the decoder window to the sender's configured 20 bits.

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

Zstandard is a distinct frame format; both ends must support it. The APIs moved into System.IO.Compression before RC2; do not use the removed preview namespace/package as a down-level route. Browser and WASI are unsupported. Quality=3 and WindowLog2=20 are deliberate example settings, not mandatory framework defaults. MaxWindowLog2 limits codec memory and does not cap total decompressed bytes; impose an independent output budget. AppendChecksum is a noncryptographic integrity choice, not authentication. A dictionary or SetPrefix changes the decoding contract and must be agreed and retained with its lifetime; this example uses neither and claims no dictionary interoperability. Reset is for independent frames, not continuation; dispose all codec resources. Finite buffer helpers require Done and full consumption; handle other statuses explicitly in an incremental implementation. No external archive-tool, native ABI, trimming or AOT compatibility is established.

This edition has no external provider, workload, rendering requirement or experimental opt-in. Shared Blazor applicability means server-side code; it is not browser execution proof. All sample dependencies are framework-provided. Exact SDK pins 10.0.402 and 11.0.100-rc.2.26504.105 are reproducibility baselines, not mandatory exact consumer installations. RC verification does not establish GA behaviour or every platform's behaviour.

## Evidence

RC2 stream and buffer codecs restore empty/nonempty original messages, including reuse after Reset. Compilation resolves no NuGet packages: the actual ref/net11.0 framework asset contains the APIs. No .NET 10 package-asset support is inferred.

The actual declared .NET 10 and .NET 11 projects compile and execute on Windows with zero warnings/errors. Saved records identify source hashes, SDK/target/language, resolved dependencies, commands, host and output; freshness is checked at package build. These are execution records, not source-review-only, cross-compilation or mock claims. The existing installed-tool harness independently verifies the complete canonical body, target-11 applicability/edition and target-10 refusal offline from an unrelated directory. Comprehensive catalogue and public-release readiness remain separate gates.
