# Choose single-file extraction and locate deployed files correctly

## Goal

Publish the pier manifest as a single executable while keeping file-location assumptions consistent with its extraction policy.

## Older approach

An SDK-10 publish can deliberately enable legacy full extraction. This remains a compatibility choice when an application really depends on managed assembly files on disk; it is not necessary for every single-file app. Save this as Properties/PublishProfiles/SingleFile.pubxml beside an ordinary executable DeliveryTool.csproj targeting net10.0:

{{snippet:older}}

The complete older executable project is:

{{snippet:olderproject}}

From its directory run `dotnet publish DeliveryTool.csproj -c Release -p:PublishProfile=SingleFile -o <empty-output-directory>`. The profile selects a framework-dependent win-x64 single-file output with no PDB. The host still needs the matching runtime. Running DeliveryTool.exe prints a non-empty assembly location under the extraction directory.

## Newer approach

For standalone C# projects, add `<ImplicitUsings>enable</ImplicitUsings>` and `<Nullable>enable</Nullable>` inside the project PropertyGroup. The canonical comparisons obtain these settings from their isolated shared sample properties; do not depend on the Playbook checkout to supply them.

The SDK-11 profile removes full extraction and uses the normal in-memory managed-assembly bundle:

{{snippet:newer}}

The corresponding independently compiled executable project is:

{{snippet:newerproject}}

Use an executable project targeting net11.0 and the SDK 11.0.100-rc.2.26504.105 baseline. The original diagnostic program observes the artifact directly:

{{snippet:program}}

Its suppression is local to the intentional Assembly.Location observation, not a recommendation to suppress migration warnings throughout an application. In the default bundle, Assembly.Location is empty. AppContext.BaseDirectory identifies the directory containing the executable; use it for deliberate sidecar files instead of appending paths to an assembly location.

## Migration

1. Find IncludeAllContentForSelfExtract and file APIs that depend on physical assembly files. Under SDK 11 the property emits NETSDK1244, a warning, while still publishing the compatibility layout.
2. Remove the property when full extraction is unnecessary. Keep application content external only when required; for a sidecar set CopyToPublishDirectory and ExcludeFromSingleFile on its project item, then deploy that file alongside the executable.
3. If native libraries must be extracted, consider IncludeNativeLibrariesForSelfExtract=true rather than extracting every managed assembly. It is a narrower policy, not a guarantee that all content is bundled or that native dependencies exist for another RID.
4. Publish to a fresh directory with an explicit RID, configuration and self-contained choice. Distribute all files actually produced; do not assume a project named “single file” proves there are no sidecars or native assets.
5. Run the delivered executable outside the checkout. Check base-directory lookups and any extraction-location requirements under the real deployment identity. Retain full extraction if that measured compatibility requirement still warrants it.

## Constraints

This is not a cross-platform binary: the recorded profile is win-x64, framework-dependent, without trimming or AOT. Publish and verify separately for other RIDs. The sample has no native package, so publishing successfully with native-only extraction does not prove that a native library was extracted. Full extraction can still be appropriate; NETSDK1244 does not remove it. SDK 10 already supports the default in-memory route, so the newer distinction is the diagnostic and deliberate migration, not an invented new bundler. No provider, rendering mode, platform workload or experimental opt-in is involved.

## Evidence

The real SDK-10 full-extraction artifact and SDK-11 default artifact execute from unrelated directories with the expected non-empty versus empty Assembly.Location. The SDK-11 compatibility publish actually emits NETSDK1244 and succeeds. A native-only-extraction publish is checked for acceptance, without claiming native extraction behavior absent a native dependency. Compilation records are separate from actual publish/executable output. Installed Playbook retrieval verifies the complete profiles/program snippets, discovery goal, target edition and applicability offline. These are Windows RC2 results, not cross-compilation, mock, AOT or GA evidence.
