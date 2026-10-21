# Reject damaged ZIP entries and TAR headers

## Goal

A survey import must fail clearly when stored ZIP data or a TAR header has changed in transit.

## Older approach

Create a net10.0 / C# 14 console project with implicit usings enabled. Import System.IO.Compression and System.Formats.Tar. Put the methods inside a static class in namespace Playbook.Samples. System, System.IO, System.Linq, System.Collections.Generic and System.Threading.Tasks come from implicit usings. No additional packages are required.

The .NET 10 reader can return damaged Stored ZIP bytes despite the recorded CRC, and can read a changed TAR header without validating its checksum. Successful decompression or opening metadata alone is not an integrity guarantee. Use independently authenticated transport/content where the threat model requires it.

{{snippet:older}}

## Newer approach

Create the corresponding net11.0 / C# 15 project with the same imports and implicit usings. These are framework APIs, not an independently versioned compression package. The canonical sample is verified against .NET 11 RC2.

Keep the complete read inside the error boundary. The reader methods return bytes only after the stream copy succeeds. Quarantine the input and reject the import on InvalidDataException; do not catch that failure and continue with partial results.

{{snippet:newer}}

The corresponding complete ZIP readers are:

.NET 10:

{{snippet:zip-older}}

.NET 11:

{{snippet:zip-newer}}

## Migration

1. Keep the consumer's actual target explicit. Upgrade to .NET 11 before adopting this edition; no conditional older-target route is advertised.
2. Test the original survey, the damaged/empty case and resource cleanup before replacing the previous path.
3. Use the declared matching SDK/framework artifacts and rerun the focused checks below when source, target or dependency inputs change.

## Constraints

ZIP CRC validation requires reading an entry through EOF; opening an archive or reading only a prefix is insufficient. TAR validates headers, not a cryptographic digest of entry payloads. CRC32 and TAR checksums detect damage and do not establish authenticity or resist deliberate tampering. Dispose readers and entry streams after failure. Extraction is not transactional; validate into staging before promoting output. Truncated/structurally invalid archives can fail earlier and may raise other documented I/O errors. No disable-validation switch is recommended. These bounded byte-array examples are not safe arbitrary-size ingestion: separately limit entry counts, expanded bytes, paths and links for untrusted input.

This edition has no external provider, workload, rendering requirement or experimental opt-in. Shared Blazor applicability means server-side code; it is not browser execution proof. All sample dependencies are framework-provided. Exact SDK pins 10.0.402 and 11.0.100-rc.2.26504.105 are reproducibility baselines, not mandatory exact consumer installations. RC verification does not establish GA behaviour or every platform's behaviour.

## Evidence

Original Stored ZIP payload bytes are flipped while preserving recorded CRCs; RC2 rejects the full read and .NET 10 returns changed bytes. A valid Ustar header name is changed without recomputing its checksum; RC2 rejects it while .NET 10 accepts it.

The actual declared .NET 10 and .NET 11 projects compile and execute on Windows with zero warnings/errors. Saved records identify source hashes, SDK/target/language, resolved dependencies, commands, host and output; freshness is checked at package build. These are execution records, not source-review-only, cross-compilation or mock claims. The existing installed-tool harness independently verifies the complete canonical body, target-11 applicability/edition and target-10 refusal offline from an unrelated directory. Comprehensive catalogue and public-release readiness remain separate gates.
