# Emit valid compressed frames for empty messages

## Goal

A lighthouse sends an empty survey response using the same compressed message protocol as a nonempty response.

## Older approach

Create a net10.0 / C# 14 console project with implicit usings enabled. Import System.IO.Compression and System.Buffers. Put the methods inside a static class in namespace Playbook.Samples. System, System.IO, System.Linq, System.Collections.Generic and System.Threading.Tasks come from implicit usings. No additional packages are required.

Disposing a .NET 10 DeflateStream or GZipStream without writing emits zero bytes. This is the measured previous behaviour, not a valid complete compressed frame. A zero-byte application message and an empty compressed frame are different protocol states.

{{snippet:older}}

## Newer approach

Create the corresponding net11.0 / C# 15 project with the same imports and implicit usings. These are framework APIs, not an independently versioned compression package. The canonical sample is verified against .NET 11 RC2.

The same code on .NET 11 finalises a valid empty compressed stream. Dispose the compressor before inspecting output. Do not test compressed length==0 to infer whether the original payload was empty.

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

The exact two-byte raw Deflate and twenty-byte GZip outputs are measured baselines, not a protocol promise about all settings/future encoders. Deflate, ZLib and GZip use different framing and are not interchangeable; Brotli and Zstandard are separate formats too. This change concerns no writes at all; do not infer every stream operation from that case. Use decompressed content length or an explicit application envelope to distinguish empty payload from missing data. Flush alone does not finish a frame. leaveOpen retains ownership of the underlying stream; disposing the compressor still finalises its frame.

This edition has no external provider, workload, rendering requirement or experimental opt-in. Shared Blazor applicability means server-side code; it is not browser execution proof. All sample dependencies are framework-provided. Exact SDK pins 10.0.402 and 11.0.100-rc.2.26504.105 are reproducibility baselines, not mandatory exact consumer installations. RC verification does not establish GA behaviour or every platform's behaviour.

## Evidence

The same original source executes on .NET 10 and RC2: no-write outputs are 0/0 bytes versus 2/20 bytes, and both APIs restore empty content. Nonempty payload round trips are checked separately.

The actual declared .NET 10 and .NET 11 projects compile and execute on Windows with zero warnings/errors. Saved records identify source hashes, SDK/target/language, resolved dependencies, commands, host and output; freshness is checked at package build. These are execution records, not source-review-only, cross-compilation or mock claims. The existing installed-tool harness independently verifies the complete canonical body, target-11 applicability/edition and target-10 refusal offline from an unrelated directory. Comprehensive catalogue and public-release readiness remain separate gates.
