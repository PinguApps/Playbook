# Choose ZIP entry access without retaining unwanted content

## Goal

A lighthouse crew replaces one depth record inside a ZIP without leaving bytes from the previous, longer record.

## Older approach

Create a net10.0 / C# 14 console project with implicit usings enabled. Import System.IO.Compression and System.Formats.Tar. Put the methods inside a static class in namespace Playbook.Samples. System, System.IO, System.Linq, System.Collections.Generic and System.Threading.Tasks come from implicit usings. No additional packages are required.

Open() infers access from the archive mode. In Update mode it provides existing content for read/write access; truncating explicitly is a valid way to replace it.

{{snippet:older}}

## Newer approach

Create the corresponding net11.0 / C# 15 project with the same imports and implicit usings. These are framework APIs, not an independently versioned compression package. The canonical sample is verified against .NET 11 RC2.

Open(FileAccess.Write) in Update mode discards existing content. Use Read for a read-only view or ReadWrite to preserve and edit it. CompressionMethod reports Stored, Deflate or Deflate64; this receiver deliberately accepts only Stored and Deflate.

{{snippet:newer}}

## Migration

1. Keep the consumer's actual target explicit. Upgrade to .NET 11 before adopting this edition; no conditional older-target route is advertised.
2. Test the original survey, the damaged/empty case and resource cleanup before replacing the previous path.
3. Use the declared matching SDK/framework artifacts and rerun the focused checks below when source, target or dependency inputs change.

## Constraints

Read archives accept only Read. Create accepts Write and ReadWrite, but both are write-only. Update accepts all three: Write truncates, ReadWrite retains content, and Read cannot write. Incompatible combinations throw InvalidOperationException; invalid enum values throw ArgumentOutOfRangeException. Parameterless Open keeps its previous inference. Create entries can only be written once. Update buffers entry data, so limit trusted input sizes before opening it. CompressionMethod is inspection, not a setter; Deflate64 can be read by the runtime but is not produced by this example. The byte-array helpers deliberately handle small trusted archives; they are not a bounded untrusted extraction service.

This edition has no external provider, workload, rendering requirement or experimental opt-in. Shared Blazor applicability means server-side code; it is not browser execution proof. All sample dependencies are framework-provided. Exact SDK pins 10.0.402 and 11.0.100-rc.2.26504.105 are reproducibility baselines, not mandatory exact consumer installations. RC verification does not establish GA behaviour or every platform's behaviour.

## Evidence

ZIP access/replacement round trips, Stored metadata, incompatible Create/Read combinations, Create ReadWrite capabilities and actual OpenAsync(Read) execution.

The actual declared .NET 10 and .NET 11 projects compile and execute on Windows with zero warnings/errors. Saved records identify source hashes, SDK/target/language, resolved dependencies, commands, host and output; freshness is checked at package build. These are execution records, not source-review-only, cross-compilation or mock claims. The existing installed-tool harness independently verifies the complete canonical body, target-11 applicability/edition and target-10 refusal offline from an unrelated directory. Comprehensive catalogue and public-release readiness remain separate gates.
