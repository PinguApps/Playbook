# Choose a TAR format the receiver can read

## Goal

A lighthouse exports one directory of readings to a receiver with a declared TAR-format requirement.

## Older approach

Create a net10.0 / C# 14 console project with implicit usings enabled. Import System.IO.Compression and System.Formats.Tar. Put the methods inside a static class in namespace Playbook.Samples. System, System.IO, System.Linq, System.Collections.Generic and System.Threading.Tasks come from implicit usings. No additional packages are required.

TarWriter already selects a format. This finite example explicitly walks immediate files in ordinal order. It is valid for this flat survey directory, but does not emulate the directory convenience method for nested directories, permissions or links.

{{snippet:older}}

## Newer approach

Create the corresponding net11.0 / C# 15 project with the same imports and implicit usings. These are framework APIs, not an independently versioned compression package. The canonical sample is verified against .NET 11 RC2.

The format overload on TarFile.CreateFromDirectory removes the manual walk and handles the directory tree. Pass the receiver's agreed TarEntryFormat rather than assuming every tool accepts PAX. The corresponding CreateFromDirectoryAsync overload accepts format and a cancellation token.

{{snippet:newer}}

Read the selected representation through TarReader; sparse 1.0 reconstruction is automatic in .NET 11:

{{snippet:reader}}

## Migration

1. Keep the consumer's actual target explicit. Upgrade to .NET 11 before adopting this edition; no conditional older-target route is advertised.
2. Test the original survey, the damaged/empty case and resource cleanup before replacing the previous path.
3. Use the declared matching SDK/framework artifacts and rerun the focused checks below when source, target or dependency inputs change.

## Constraints

TAR stores entries without compressing them. V7 has the oldest/narrowest metadata/name representation; Ustar improves compatibility with constrained legacy readers; PAX carries extended names/metadata; GNU uses GNU extensions. Choose from receiver requirements, not operating-system branding. The earlier directory convenience overload still defaults to PAX. Format limits can reject data rather than preserving every name/metadata field. GNU sparse 1.0 is a PAX representation: RC2 TarReader reconstructs logical content and holes from its map automatically. This does not mean selecting TarEntryFormat.Gnu creates sparse 1.0 output or preserves physical filesystem allocation. The original fixture exposes 517 stored map/data bytes on .NET 10, but 16 logical bytes on RC2. For older receivers request a compatible/materialised representation; do not treat raw sparse map bytes as the file. General hard-link preservation/extraction policy is separate migration work (PIN-684). No sparse writer or physical sparse-file allocation claim is made.

This edition has no external provider, workload, rendering requirement or experimental opt-in. Shared Blazor applicability means server-side code; it is not browser execution proof. All sample dependencies are framework-provided. Exact SDK pins 10.0.402 and 11.0.100-rc.2.26504.105 are reproducibility baselines, not mandatory exact consumer installations. RC verification does not establish GA behaviour or every platform's behaviour.

## Evidence

Both actual target projects create/read all four formats and restore identical contents. An independently authored GNU sparse 1.0 PAX fixture with two extents verifies RC2 logical size, data and zero-filled holes, alongside the older stored-byte behaviour.

The actual declared .NET 10 and .NET 11 projects compile and execute on Windows with zero warnings/errors. Saved records identify source hashes, SDK/target/language, resolved dependencies, commands, host and output; freshness is checked at package build. These are execution records, not source-review-only, cross-compilation or mock claims. The existing installed-tool harness independently verifies the complete canonical body, target-11 applicability/edition and target-10 refusal offline from an unrelated directory. Comprehensive catalogue and public-release readiness remain separate gates.
