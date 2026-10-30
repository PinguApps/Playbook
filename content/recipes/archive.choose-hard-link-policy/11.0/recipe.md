# Choose TAR hard-link preservation or independent copies

## Goal

An observatory exports calibration.dat and alias.dat: two real names for the same calibration file. A third file, unrelated.dat, has equal bytes but a different identity. Decide whether a receiver should preserve the alias relationship or receive independent editable copies.

## Older approach

Use a net10.0 / C# 14 console project with implicit usings. Import System.Formats.Tar and place these static methods inside a class. No additional NuGet package is needed. The path-based .NET 10 writer emits regular file contents for both hard-linked paths; independent=true retains that useful behavior:

{{snippet:older}}

The false branch explicitly refuses an automatic policy the older path writer does not offer. This does not mean .NET 10 cannot represent or extract TAR HardLink entries: callers can author entries with a known LinkName and its existing TarFile extraction preserves links. Automatic filesystem-identity discovery is the changed path here; a correct general older detector is outside this bounded sample.

## Newer approach

Use a net11.0 / C# 15 console project, implicit usings and System.Formats.Tar from the framework. There is no separate TAR package dependency. The tested reference/runtime artifact is 11.0.0-rc.2.26504.105.

Specify the writer policy for the receiver. PreserveLink writes one regular file then a HardLink pointing at its first archive name; CopyContents writes independent regular file data. The third equal-byte file remains regular in both cases:

{{snippet:newer}}

Make the extraction choice independently. CopyContents also turns HardLink entries from an existing archive into independent files. PreserveLink needs real filesystem support and permissions. Pre-create a fresh, owned staging directory; the example does so idempotently, but the caller must select a new path:

{{snippet:extract}}

Call WriteCalibration(destinationStream, sourceDirectory, independent: true) for a receiver that needs separate editable copies. Call ExtractCalibration(archiveStream, freshStaging, independent: true) when receiving an archive that already contains HardLink entries. The source directory must contain the three stated files; how they were linked is separate from this archive policy. TarWriter.WriteEntryAsync(path, entryName, cancellationToken) accepts the same writer options selected at construction. TarFile also supplies path-based sync/async extraction overloads with TarExtractOptions.

## Migration

1. Decide whether aliases should share subsequent edits or become independent. Compare real file identities; equal contents alone cannot answer this.
2. On .NET 11, set TarWriterOptions.HardLinkMode explicitly when upgrading code that previously produced duplicate regular entries. CopyContents retains that old result.
3. Choose TarExtractOptions.HardLinkMode separately for the receiving filesystem. PreserveLink is the default; CopyContents avoids creating hard links for hard-link entries.
4. Keep OverwriteFiles=false unless replacement is intentional. Extract into a new staging directory; validate complete success before promoting it. Dispose readers/writers before cleanup.
5. Exercise both policies and verify extracted bytes plus actual shared/independent file IDs. Pass cancellation through the async operations and discard partial staging on failure.

## Constraints and alternatives

PAX is the writer default and OverwriteFiles defaults to false. Choosing PAX, GNU or another representation is separate from choosing a hard-link policy. Path-based WriteEntry/WriteEntryAsync performs identity detection; manually constructed entries are not equivalent. The first encountered file owns the content and later HardLink entries reference that archive name, so preserve meaningful order. Independent regular entries cannot be re-linked automatically merely because their bytes match.

PreserveLink can fail on filesystems or accounts that cannot create hard links. CopyContents is the appropriate policy for those receivers, but this execution proof covers real Windows NTFS only; no link-incapable receiver was tested. The sample uses a small Windows native helper solely to create/measure the fixture in an isolated temporary directory, not as part of the recipe API. Do not infer Linux/network filesystem identity or privileges from that check.

The framework validates entry paths and link targets during extraction; the focused checks reject traversal and hard-link targets outside staging. These checks do not make arbitrary archives safe or authenticate their authors. CopyContents controls hard links, not symbolic links, archive size quotas or every hostile filesystem race. Use trusted inputs and isolated staging; apply a separate rejection policy if your receiver prohibits other entry types. Do not create arbitrary links outside an owned test/import directory.

Extraction is not transactional: a late collision, cancellation or other failure can leave earlier files. The caller owns rollback/discard and publication of staging. A pre-cancelled task is tested; no atomic cancellation or rollback guarantee is made. TarEntry.ExtractToFile has no TarExtractOptions overload and rejects HardLink entries; use TarFile for this policy.

SDK 10.0.402 / 11.0.100-rc.2.26504.105 and reference/runtime packs 10.0.13 / 11.0.0-rc.2.26504.105 are exact evidence baselines, separate from release-family prerequisites. The options are not experimental on the inspected RC2 assets and need no experimental opt-in. No separate NuGet/older-target route is advertised. There is no provider, workload or rendering requirement; shared Blazor applicability is server-side file processing, not browser execution. RC2 proof is not GA proof.

## Evidence

Paired real consumers create two actual NTFS hard links and a distinct equal-byte control. Native volume/file IDs prove input identity. They inspect entry types, names, LinkName, order and content; .NET 10 writes duplicates and .NET 11 defaults to linking. RC2 exercises both writer modes sync/async, both extraction modes with path/stream sync/async, verifies actual output identities and bytes, rejects overwrite, checks reverse input order, pre-cancellation, unsafe paths/targets and a demonstrated partial extraction. Handles close and owned directories are removed.

Saved compilation/execution records include source hashes, exact artifact assembly identities/SHA256, OS/filesystem, commands and dependencies. Ordinary build/pack validates freshness; the installed ordinary tool serves canonical Markdown, discovery and target11/edition11 applicability offline and refuses target10. Existing format/corruption recipes, the eight foundations, PIN-959 comprehensive gate and PIN-962 GA reconciliation remain separate.
