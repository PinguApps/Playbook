# Create and recover build archives on the GA SDK

## Goal

Package the Tidal freight dispatch application's build output into an uncompressed tar and recover its files. Keep the archive outside its input directory and choose overwrite behavior explicitly.

## Older approach

.NET 10 already provides the supported `System.Formats.Tar` directory APIs. The original helper deletes an existing tar before creation and overwrites matching files during extraction:

{{snippet:helper}}

{{snippet:older-project}}

{{snippet:older}}

## Newer approach

SDK 11.0.100 supplies inbox `TarDirectory` and `Untar`, but the actual Windows GA invocation failed with MSB4018/MSB0001: `Cannot call Reacquire() on thread ... when Yield() was called on thread ...`. This is a GA observation, not an RC-only limitation. Ordinary compilation succeeded first; one build node and disabled node reuse did not repair execution.

The [immutable GA implementation](https://github.com/dotnet/dotnet/blob/297acf930fb0f5c07fd4e51d78e521b4ad2e5d1f/src/msbuild/src/Tasks/TarDirectory.cs) calls `BuildEngine3.Yield()` before asynchronous writes using `ConfigureAwait(false)` and calls `Reacquire()` in its finally block. That path permits continuation on another thread, consistent with the observed engine thread check. This source diagnosis does not establish a supported task invocation that avoids the failure.

Retain the supported helper on the current SDK. This canonical target is a fallback through an extra process, **not successful inbox-task execution**:

{{snippet:newer-project}}

{{snippet:newer}}

Both projects copy only `payload/departures.txt` into output. The target archives all build output, including runtime files. Private/draft glob fixtures are not copied. The helper uses uncompressed tar; a filename suffix alone does not add compression.

## Migration

1. Select the intended SDK using the project's exact global.json. Keep the app's intended framework and shared configuration.
2. Import the target into the project. Supply an absolute SDK executable as `BuildDotnet`; the helper runtime is separate from the MSBuild task host.
3. From either canonical sample directory, invoke the selected absolute executable with `build -c Release -t:ArchiveDispatch -p:BuildDotnet=<absolute-dotnet-path>`. Expect `obj/dispatch.tar` and `obj/unpacked/payload/departures.txt`.
4. Inspect tar entries and compare extracted payload bytes. `eng/Test-BuildArtifacts.ps1` checks both declared SDKs, existing archive replacement, repeated extraction, deliberately changed payload and retention of an unrelated destination file.
5. Keep the fallback until a supported inbox-task route is independently verified against the exact selected artifacts. Do not remove the helper on the strength of compilation alone.

## Constraints

The inbox archive tasks require .NET-hosted MSBuild; this does not establish Framework MSBuild execution. The helper uses supported managed APIs and requires its compatible runtime. Only the target-10/SDK10 and target-11/SDK11 consumers are exercised here.

Creation deletes the previous destination archive. Extraction overwrites matching paths and leaves unrelated files in place: it does not mirror an exact directory. Use a separately managed clean destination when that matters. The helper does not implement inbox `SkipUnchangedFiles`, question mode or read-only overwrite policy.

Use trusted build artifacts. No hostile-path/link/archive-bomb safety, byte reproducibility or metadata normalization is claimed. Glob inspection does not itself select archive inputs.

## Evidence and limitations

Windows execution uses SDK 10.0.403 and SDK 11.0.100 with their actual task-host/runtime identities in the canonical receipts. Both supported helper routes pass entries and payload-byte controls, existing archive replacement, repeated and changed-file extraction, and retained destination file checks. The GA inbox failure remains explicit; no successful `TarDirectory` or `Untar` execution is claimed. Historical RC source references remain historical context. Installed stdio retrieval establishes offline guidance delivery, not additional platform execution. PIN-967 owns the wider final-candidate qualification.
