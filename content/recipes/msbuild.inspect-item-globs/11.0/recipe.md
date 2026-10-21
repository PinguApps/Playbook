# Explain build item patterns and selected files

## Goal

Show a build's include, exclude and remove patterns next to the files selected by those patterns. A pattern explains an input rule; an expanded item tells you which path survived evaluation. Neither output alone answers every question about the other.

This recipe is theoretical. The sample's item evaluation and diagnostic output have not been run.

## Older approach

An older custom build can report its expanded items and inspect its own authored XML for literal rules. This original SDK-10 target uses `XmlPeek` against the same `.targets` file that declares `DispatchSheet`:

{{snippet:older}}

That deliberately bounded diagnostic is useful when the rules are all in one known file. It is **not** a replacement MSBuild evaluator: it does not follow imported projects, evaluate conditions or property/item expressions, correlate every rule across imports, or recover the full evaluation order. A tool that needs those capabilities can host the supported MSBuild object model separately, with its own assembly/SDK compatibility requirements. No unverified package dependency is added to pretend this simple comparison does that.

## Newer approach

SDK 11's MSBuild can supply `MSBuildItemGlob` items during the build when `MSBuildProvideItemGlobs` requests the relevant item types. The original newer configuration requests only the custom `DispatchSheet` type:

{{snippet:newer}}

The glob entries retain the item type plus `Include`, `Exclude` and `Remove` metadata as unevaluated patterns in project-file order. The separate `Expanded files` message prints `@(DispatchSheet)`. A wildcard such as `payload/**/*.txt` should still be a wildcard in the glob metadata; replacing it with a list of matching paths would lose the authored rule.

The fixture contains `payload/departures.txt`, `payload/draft-late.txt` and `payload/private/crew.txt`. The intended expanded set is only the departure sheet: the include finds text files, the exclude filters the private subtree, and the remove filters draft names. These are expected results from the authored rules, not observed output.

## Migration

1. Identify the item types the diagnostic actually needs. Set `MSBuildProvideItemGlobs` early enough for evaluation; use the project/imported property group as shown rather than setting it inside the reporting target after evaluation.
2. For multiple item types, use a project value such as `Compile;Content`. On a command line escape the separator as `%3B`, for example `-p:MSBuildProvideItemGlobs=Compile%3BContent`, so MSBuild does not parse it as another property assignment.
3. Keep a reporting target that prints each `MSBuildItemGlob` identity and its include/exclude/remove metadata. Print the relevant expanded item collection separately with an unmistakable label. The sample's candidate command is the selected SDK's absolute executable with `build -t:ExplainDispatchInputs` from its directory.
4. Interpret the rule sequence and expanded output together with project conditions/imports. An empty expanded set can be legitimate even when a glob is present, and a removed path can still appear in an earlier include pattern.
5. In a future check, compare the three fixture paths with both outputs. Confirm the private and draft files are absent from expanded `DispatchSheet`, while the source wildcard strings remain visible. Also inspect a build without `MSBuildProvideItemGlobs`; no glob items should be supplied for an unset property. None of these commands has been run.
6. Preserve a source-inspection diagnostic on older SDKs when it is sufficient. Do not relabel it as complete evaluated-glob support, and do not teach SDK-11 behavior as an older-target route without the required compatible evidence.

## Constraints

This XML build configuration is language-neutral and can explain custom items in C#, F# or VB projects; the original sample consumer itself declares C# 15/net11.0. The SDK and MSBuild implementation supply the feature, independently of whether application code uses a new language construct. No extra package, workload, provider or rendering mode is required.

`MSBuildItemGlob` is diagnostic input metadata, not a filesystem enumeration, change watcher, live IDE synchronization contract or provenance record for every possible target mutation. Target-time item additions/removals and conditions still need their own context. Enabling glob reporting does not change archive or publish selection by itself.

The **archive tasks** in the companion recipe are unavailable in .NET Framework `MSBuild.exe`; that restriction must not be generalized into a claim that all MSBuild glob diagnostics are .NET-only. This sample specifies the .NET SDK-11/MSBuild route. It supplies no Visual Studio/.NET Framework execution or compatibility evidence.

## Evidence

Reviewed the official MSBuild RC1 glob contract and `XmlPeek` task documentation. Canonical older/newer imported targets preserve the original rules and reporting implementations. Saved source-review records hash the projects, targets and fixture inputs; SDK/language fields are intended unbuilt pins.

There is no compilation, item-evaluation output, Framework-MSBuild run, real archive interaction or installed offline MCP retrieval evidence. Pending proof remains visible in discovery/detail and blocks release-ready packaging. This target-11 edition adds no historical foundation or undocumented older-target route, and the RC documentation baseline is not GA verification.
