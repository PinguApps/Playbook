# Inspect the actual build import tree

## Goal

Beacon Quay shares build configuration between shift tools. Inspect which build
files actually participated in evaluation, which file imported each one, and which
imports came through an SDK. A handwritten list of XML Import elements cannot
resolve conditional and SDK imports by itself.

## Requirements and recorded artifacts

This edition advertises target11 only. The net10 example is a real comparison,
not another historical foundation or a general SDK11-on-net10 route. The exact
SDK, compiler, framework, reference pack, runtime and NuGet/MSBuild files are
separate verification baselines, not mandatory patch upgrades for every consumer.

| Layer | Older consumer | Current consumer |
| --- | --- | --- |
| SDK | 10.0.402 | 11.0.100-rc.2.26504.105 |
| C# compiler / language setting | 5.9.0-1.26505.111 / 14.0 | 5.12.0-1.26504.105 / 15.0 |
| Framework / ref / runtime | net10.0 / 10.0.13 | net11.0 / 11.0.0-rc.2.26504.105 |
| Bundled NuGet | 7.9.0-rc.611 | 8.0.0-rc.605 |
| MSBuild product | 18.9.15 | 18.12.0-1.26504.105 |

`artifact-pins.json` records and checks exact SDK/tooling/compiler/ref/runtime
hashes and product builds, including the native runtime host; no optional native
compiler is needed. No separately distributed package is assumed byte-identical
to a bundled tooling DLL. Windows x64 is the executed platform. There is no provider,
UI rendering or optional workload requirement. The XML/CLI controls are language
neutral; the compiled example uses supported C#14/15, without a preview-language
switch. RC2 verification is not GA proof; PIN-959/PIN-963 and existing Human gates
remain open. These controls carry no experimental designation in the cited sources.

## Older approach

The official Preview6 notes introduce `MSBuildProvideImportedProjects` and
`MSBuildImportedProject`. The actual SDK10.0.402 baseline already exposes them;
the same opt-in works in both measured consumers. The catalogue's feature-release
label follows the notes, not a claim that all older tooling lacks the items. Do not
replace this working older approach with an incomplete handwritten include list.

## Newer approach

Under a directory pinned to the matching SDK, create an `Imports` folder.
Save the older `Imports/Beacon.proj`:

{{snippet:imports-10}}

Use this current file at the same path for SDK11:

{{snippet:imports-11}}

Both use the following files. Save `Imports/Shared.props`:

{{snippet:shared}}

Save `Imports/Leaf.props`:

{{snippet:leaf}}

Save `Imports/Night.props`:

{{snippet:night}}

Create `global.json` beside `Imports`, with
`{"sdk":{"version":"10.0.402","rollForward":"disable"}}` for the older copy or
`{"sdk":{"version":"11.0.100-rc.2.26504.105","rollForward":"disable"}}` for the
current copy. Use that matching complete SDK executable. No NuGet package or
restore is needed for these evaluation queries; the companion compiled evidence
application has a separate local-only original package closure.

## Migration

1. Inspect the application's SDK selection and effective build imports. Run the
   real evaluator from the directory containing `Imports`:

`dotnet msbuild Imports/Beacon.proj -nologo -p:MSBuildProvideImportedProjects=true -getItem:MSBuildImportedProject`

2. Read each item's absolute `Identity`, its `ImportingProjectPath` and optional
   `Sdk` metadata. `Shared.props` has `Beacon.proj` as its parent; `Leaf.props`
   has `Shared.props` as its parent. Implicit `Sdk.props`/`Sdk.targets` imports
   name `Microsoft.NET.Sdk`. Ordinary custom imports have no SDK metadata;
   JSON may omit that metadata rather than emit an empty string.
3. Add `-p:BeaconNight=true` and verify exactly one additional custom import,
   `Night.props`, whose parent is `Beacon.proj`. Omission keeps it outside the
   evaluated closure. These are results for the selected properties/environment,
   not an enumeration of every possible condition or project-reference graph.
4. Add `-p:BeaconDuplicate=true`. MSBuild reports **MSB4011** for the duplicate
   Leaf import and ignores its second occurrence. The observed item list retains
   the original Shared parent. Do not count XML Import declarations as unique
   evaluated files; parse the diagnostic separately from JSON output.
5. Repeat the query and preserve the reported order if displaying a tree. These
   exact SDKs return parent before nested child in depth-first evaluation order,
   with identical repeated output. That observation is not a permanent sorting
   contract. SDK installations produce different complete closures and counts;
   do not hard-code the total or assume the project itself is an imported item.
6. Omit the opt-in, then set it explicitly false: both return an empty item list
   in the controls. That does **not** mean the project imported nothing.
7. For target-time reporting, execute the original target explicitly:

`dotnet msbuild Imports/Beacon.proj -nologo -t:ShowImports -p:MSBuildProvideImportedProjects=true -p:BeaconNight=true`

The target prints path, parent and SDK separated by pipes. The earlier `-getItem`
query performs evaluation; it does not execute this target. Imported files can
contain property functions and other evaluation logic: inspect only trusted build
inputs. MSBuild is not an untrusted-XML sandbox.

## Constraints

This differs from `msbuild.inspect-item-globs` (PIN-767): unevaluated include/
exclude/remove patterns explain file selection, whereas these items describe the
**evaluated import closure**. Neither alone proves which tasks ran or explains
all expanded application items. Framework MSBuild, arbitrary SDK resolvers and
other OS/toolchain closures are not qualified here.

## Evidence

`eng/Test-BeaconBuild.ps1 -Record` compiles/runs both accompanying real applications
and separately invokes each matching actual CLI on this fixture. It checks SDK and
nested parents, optional metadata, unset/false, conditional plus-one, repeated order,
duplicate warning/first-parent retention and real ShowImports execution. It saves
commands, artifact and input hashes through the existing sample-evidence workflow.
Installed offline stdio list/get checks canonical snippets, target11/edition11,
complete Markdown and applicability. No new recurring platform matrix is added.
