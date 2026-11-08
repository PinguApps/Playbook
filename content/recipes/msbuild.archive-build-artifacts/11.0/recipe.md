# Create and extract build archives with MSBuild tasks

## Goal

Package a dispatch application's build output into a tar archive, then extract it into a separate intermediate directory. Keep the archive outside the directory being archived, and make the intended overwrite behavior explicit.

This provisional walkthrough retains documented GA intent. Consult the evidence section for actual local observations and the remaining unverified behavior.

## Older approach

.NET 10 already provides `System.Formats.Tar`. The older original application includes a small helper for creating and extracting tar files:

{{snippet:helper}}

An original build target invokes that compiled helper with an explicitly supplied `BuildDotnet` executable path:

{{snippet:older-project}}

{{snippet:older}}

The project copies `payload/departures.txt` into its output. The target archives that output, including the managed executable and its runtime files, then expects the dispatch sheet under `obj/unpacked/payload/departures.txt`. The helper deletes an existing destination tar deliberately and allows extraction overwrite. This is a valid earlier approach with an extra process and helper implementation; it is not a claim that .NET 10 lacked tar support.

## Newer approach

.NET-hosted MSBuild in SDK 11 supplies inbox `TarDirectory` and `Untar` tasks. The newer target expresses the same build-artifact workflow directly:

{{snippet:newer-project}}

{{snippet:newer}}

The default archive is an uncompressed `.tar`. `Compression="GZip"` is the documented `TarDirectory` option for a gzip-compressed archive; use a `.tar.gz` destination when deliberately choosing that format. Changing the suffix alone does not request compression.

`Overwrite="true"` controls an existing archive destination. `Untar` separately controls extracted files: `SkipUnchangedFiles="true"` and `OverwriteReadOnlyFiles="false"` are explicit here. Archive creation and extraction do not share one global overwrite policy.

## Migration

1. Keep the app project on its intended framework and inspect its SDK/global.json. Copy the canonical target into a `.targets` file imported by that project, or place it in the project itself. The original projects import `BuildRecipes.targets` and explicitly copy the dispatch sheet.
2. Keep `DispatchArchive` and `DispatchExtract` outside `TargetDir`. The sample uses the project's intermediate `obj` directory. An archive placed inside its own source tree can accidentally become an input.
3. On the older comparison, build and invoke `ArchiveDispatch` with the absolute selected SDK executable also supplied as `-p:BuildDotnet=<absolute-dotnet-path>`. The helper's runtime and tar behavior are separate from the MSBuild host.
4. On the newer comparison, the candidate command from its directory is `dotnet build -c Release -t:ArchiveDispatch` using the selected SDK's absolute executable. The target depends on `Build`; successful execution should create `obj/dispatch.tar` and the extracted output. This command has not been executed.
5. In a future check, inspect the actual tar entries and compare extracted `payload/departures.txt` bytes with the copied build output. Check the existing-archive case, a repeated extraction and a changed file deliberately. An `Exists` guard only detects a missing file, not content equality or incremental correctness.
6. Remove the old helper only when its other callers no longer need it. Keep a custom helper or another archive format when the inbox tasks do not meet the workflow's requirements.

## Constraints

The SDK-11 archive tasks depend on `System.Formats.Tar` and are available in **.NET-hosted MSBuild**, not Visual Studio's .NET Framework `MSBuild.exe`. Use the intended `dotnet` SDK to run them. The managed application target and the MSBuild task-host runtime are separate facts; this edition demonstrates target 11 and does not infer a verified older-target/newer-SDK route.

`FailIfNotIncremental` supports question-mode failure when archive work would be needed; it is not a promise that creating a tar is always incremental. `Untar` skips unchanged files by default and does not make an existing extraction directory an exact mirror: files removed from a later archive can remain there. Use a separately managed clean destination when that matters. Read-only extraction overwrite is opt-in.

The tar helper and tasks are for trusted build artifacts. This recipe does not establish safe handling of hostile paths, links or archive bombs. It does not claim tar-byte reproducibility, normalize all metadata, or equate archive timestamps with NuGet-package or container reproducibility. There are no extra packages, workloads, providers or rendering modes in the original console projects.

The private/draft fixture files support the independent glob recipe. They are not copied to the output by this project. Item inspection does not itself change which files a publish/archive target consumes.

## Evidence and limitations

The canonical .NET10 and .NET11 projects now have actual locked compilation records from Windows, using their declared SDKs and dependencies. The original official-source review is retained separately. Compilation of the rules/decoder library does not compile illustrative .cs.txt host integration or establish behavior.

The pre-GA preview is explicitly provisional. Pending proof remains visible in discovery/detail and blocks stable readiness. The approved preview evidence register names this exact edition, canonical samples, limitations and its final-reconciliation owner; it does not claim successful unperformed checks.

The older archive helper extracted payload bytes correctly. SDK11 RC3 TarDirectory failed internally, including a single-node retry. The documented GA implementation is retained; newer extraction and incremental behavior are not verified.
