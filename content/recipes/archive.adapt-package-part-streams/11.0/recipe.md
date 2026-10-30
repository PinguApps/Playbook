# Supply seekable package data only when required

## Goal

A museum imports a packaged catalogue. Its line reader only moves forward; its index reader examines the end marker and then returns to the first byte. Only the latter needs random access.

## Older approach

Create a net10.0 / C# 14 console project with implicit usings and System.IO.Packaging 10.0.13. Import System.IO.Packaging and put these static methods inside a class. The source project uses namespace Playbook.Samples. No Open XML SDK is involved.

This older caller works for an unmodified compressed part in a ReadWrite package because the older asset buffers it. It already fails for a forward-only compressed part in a read-only package; it is not a general stream contract. ReadLastThenFirst is supplied below.

{{snippet:older}}

Keep a sequential parser sequential on both versions. StreamReader owns its stream here, and the caller keeps the Package alive for the entire operation:

{{snippet:sequential}}

## Newer approach

Use a net11.0 / C# 15 project with System.IO.Packaging 11. The reviewed/locked sample uses 11.0.0-rc.2.26504.105. For ReadWrite packages, GetStream(FileMode.Open, FileAccess.Read) can now stream an unmodified compressed part directly. Read access does not promise seeking. Test CanSeek; retain an already-seekable stream and buffer only the parser that actually needs it:

{{snippet:adapt}}

Include the complete bounded adapter and the random-access parser in the same class. Open a fresh part stream at its beginning. The adapter leaves its input open, checks the uncompressed Length before destination allocation or source reads, enforces the bound during copying, disposes a failed buffer and rewinds a successful one. The caller disposes the returned MemoryStream:

{{snippet:bounded}}

For example, keep Package.Open(path, FileMode.Open, FileAccess.ReadWrite) alive, obtain package.GetPart(new Uri("/catalogue/index.txt", UriKind.Relative)), then call CountRecords(part) or ReadIndex(part, 1_048_576). The 1 MiB cap is the museum's chosen buffering policy, not a library default. Seekable inputs bypass buffering; impose a separate document-size policy if all imports must have a size limit.

## Migration

1. Identify the consumer target and actual System.IO.Packaging asset, not just its package version. Preserve the valid .NET 10 code where its assumptions hold.
2. Classify each parser: keep forward-only readers unchanged; remove accidental Length/Position assumptions from general stream code.
3. For genuine random access, open a fresh Read stream, check CanSeek and explicitly buffer under an application byte budget only when needed. Rewind before parsing and dispose both resources.
4. Test compressed/unmodified, stored, already-modified and read-only package cases. Test oversize rejection before reads and exact-budget success.
5. Keep source/lock/evidence inputs together and repeat the focused consumers when changing them. Do not label RC2 behavior as GA proof.

## Constraints and alternatives

Length remains available on these PackagePart streams; Seek and both Position accessors throw NotSupportedException when CanSeek is false. A read-only package is not a seeking workaround. Stored and previously modified parts are seekable in the tested ReadWrite sessions; capabilities should still drive the caller.

The buffer bounds retained logical bytes, not total memory: allocation growth, its 4096-byte scratch block and parser overhead add cost. For large inputs, design a bounded temporary-file adapter with quotas and cleanup, or choose a sequential parser. Neither alternative is implemented or measured here. Do not open a part for writing merely to force allocation.

The exact packaging baselines are 10.0.13 and 11.0.0-rc.2.26504.105; SDKs are 10.0.402 / 11.0.100-rc.2.26504.105, with runtime/reference packs 10.0.13 / 11.0.0-rc.2.26504.105. These are reproducibility pins, separate from the release-family requirements in applicability. The RC2 package's real net10 consumer selects lib/net10.0 and keeps the old compressed ReadWrite behavior: NET11_0_OR_GREATER controls the implementation. No older-target route is advertised by this target11 edition.

There is no workload, provider, rendering mode or experimental opt-in. This is desktop/server file processing; shared Blazor applicability means server code, not browser proof. APIs are not marked experimental on the inspected artifacts; RC2 execution still needs GA reconciliation.

## Evidence

All three real consumers create, close and reopen original packages and record CanSeek, Seek, both Position accessors, Length and exact sequential bytes for the four access cases. They run the sequential and end/start parsers, prove rewound buffer identity, reject an oversized part without reading it, exercise a separate instrumented growth-guard control, and close handles before deleting the isolated directory. The matrix itself uses real PackagePart streams. Saved evidence includes actual assembly TFM/version/SHA256, exact commands, dependency locks and input hashes.

Ordinary package build/pack validates freshness. Installed stdio inspection checks ID/title/goal, full canonical Markdown, explicit target11/edition11 applicability and evidence; target10 retrieval is refused. The eight foundations, comprehensive coverage and actual-GA gates remain unchanged.
