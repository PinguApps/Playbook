# Explain build item patterns and selected files

## Goal

Show a build's include, exclude and remove patterns next to the files selected by those patterns. A pattern explains an input rule; an expanded item tells you which path survived evaluation. Neither output alone answers every question about the other.

The canonical Windows SDK10/SDK11 diagnostics are exercised by `eng/Test-BuildArtifacts.ps1`.

## Older approach

An older custom build can report its expanded items and inspect its own authored XML for literal rules. This original SDK-10 target uses `XmlPeek` against the same `.targets` file that declares `DispatchSheet`:

{{snippet:older}}

That deliberately bounded diagnostic is useful when the rules are all in one known file. It is **not** a replacement MSBuild evaluator: it does not follow imported projects, evaluate conditions or property/item expressions, correlate every rule across imports, or recover the full evaluation order. A tool that needs those capabilities can host the supported MSBuild object model separately, with its own assembly/SDK compatibility requirements. No unverified package dependency is added to pretend this simple comparison does that.

## Newer approach

SDK 11's MSBuild can supply `MSBuildItemGlob` items during the build when `MSBuildProvideItemGlobs` requests the relevant item types. The original newer configuration requests only the custom `DispatchSheet` type:

{{snippet:newer}}

The glob entries retain the item type plus `Include`, `Exclude` and `Remove` metadata as unevaluated patterns in project-file order. The separate `Expanded files` message prints `@(DispatchSheet)`. A wildcard such as `payload/**/*.txt` should still be a wildcard in the glob metadata; replacing it with a list of matching paths would lose the authored rule.

The fixture contains `payload/departures.txt`, `payload/draft-late.txt` and `payload/private/crew.txt`. The intended expanded set is only the departure sheet: the include finds text files, the exclude filters the private subtree, and the remove filters draft names. These are expected results from the authored rules; the recorded happy-path observations and their limits are listed below.

## Migration

1. Identify the item types the diagnostic actually needs. Set `MSBuildProvideItemGlobs` early enough for evaluation; use the project/imported property group as shown rather than setting it inside the reporting target after evaluation.
2. For multiple item types, use a project value such as `Compile;Content`. On a command line escape the separator as `%3B`, for example `-p:MSBuildProvideItemGlobs=Compile%3BContent`, so MSBuild does not parse it as another property assignment.
3. Keep a reporting target that prints each `MSBuildItemGlob` identity and its include/exclude/remove metadata. Print the relevant expanded item collection separately with an unmistakable label. The sample's candidate command is the selected SDK's absolute executable with `build -t:ExplainDispatchInputs` from its directory.
4. Interpret the rule sequence and expanded output together with project conditions/imports. An empty expanded set can be legitimate even when a glob is present, and a removed path can still appear in an earlier include pattern.
5. Run `eng/Test-BuildArtifacts.ps1` to check the three fixtures against both outputs: private and draft files are absent from expanded items while authored wildcard patterns remain visible. On SDK11 an unset `MSBuildProvideItemGlobs` supplies no glob patterns. These focused controls do not qualify a complete imported graph.
6. Preserve a source-inspection diagnostic on older SDKs when it is sufficient. Do not relabel it as complete evaluated-glob support, and do not teach SDK-11 behavior as an older-target route without the required compatible evidence.

## Constraints

This XML build configuration is language-neutral and can explain custom items in C#, F# or VB projects; the original sample consumer itself declares C# 15/net11.0. The SDK and MSBuild implementation supply the feature, independently of whether application code uses a new language construct. No extra package, workload, provider or rendering mode is required.

`MSBuildItemGlob` is diagnostic input metadata, not a filesystem enumeration, change watcher, live IDE synchronization contract or provenance record for every possible target mutation. Target-time item additions/removals and conditions still need their own context. Enabling glob reporting does not change archive or publish selection by itself.

The **archive tasks** in the companion recipe are unavailable in .NET Framework `MSBuild.exe`; that restriction must not be generalized into a claim that all MSBuild glob diagnostics are .NET-only. This sample specifies the .NET SDK-11/MSBuild route. It supplies no Visual Studio/.NET Framework execution or compatibility evidence.

## Evidence and limitations

The canonical Windows SDK 10.0.403 and 11.0.100 builds return authored include/exclude/remove patterns separately from expanded `payload/departures.txt`. Private and draft fixtures are excluded. An unset SDK11 reporting property produces no glob patterns. Exact inputs and SDK/task-host identity are retained in the sample receipts. The older XmlPeek diagnostic reads one authored file; neither check proves complete imported-graph behavior or Framework MSBuild execution. Archive behavior uses the supported helper fallback described by the companion recipe. PIN-967 retains final-candidate qualification.