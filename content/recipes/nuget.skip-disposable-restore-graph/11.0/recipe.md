# Avoid writing a disposable CI restore graph

## Goal

For a build agent that discards its workspace after one build, choose whether NuGet
writes the dependency graph specification without losing restored dependencies or
a working build. Leave normal developer restores unchanged.

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

The official .NET11 Preview4 notes announce `RestoreDoNotWriteDependencyGraphSpec`.
The actual SDK10.0.402 baseline already honours it too. This is measured tooling
overlap, not evidence that every older SDK supports it. The catalogue's feature
release label follows those notes; it does not assert absence in serviced tooling.
Both real implementations below solve the same quay-invoice goal.

## Newer approach

Beacon Quay invoices three crates at seven credits each. Save the following files
in each comparison directory. Both versions use the same three C# files:

`Program.cs`:

{{snippet:program}}

`Rules/BerthCharge.cs`:

{{snippet:rules}}

`Rates/Tariff.cs`:

{{snippet:rates}}

The older `BeaconBuild.csproj`, `Rules/Beacon.Rules.csproj` and
`Rates/Beacon.Rates.csproj` are:

{{snippet:app-10}}

{{snippet:rules-project-10}}

{{snippet:rates-project-10}}

The current files at those same three paths are:

{{snippet:app-11}}

{{snippet:rules-project-11}}

{{snippet:rates-project-11}}

Give each directory its own `global.json`: the older file is
`{"sdk":{"version":"10.0.402","rollForward":"disable"}}`; the current file is
`{"sdk":{"version":"11.0.100-rc.2.26504.105","rollForward":"disable"}}`.
Use the matching complete SDK installation and runtime, not copied task DLLs.
Outside the repository, no shared build/package configuration is needed. Inside
another repository, inspect inherited configuration and isolate these examples
from central package/analyzer policies. The fictional package is locally authored;
`[1.0.0]` is an exact dependency, not a remote package recommendation.

Before the commands below, create `empty.config` containing
`<configuration><packageSources><clear/></packageSources></configuration>`.
Run from the chosen comparison directory with the matching SDK executable.
Pack first, then restore solely from the new `feed` folder. The first restore
creates `packages.lock.json`; review it and use locked mode for later restores.
Keep each comparison's feed and `NUGET_PACKAGES` cache separate. The authoring
helper sets those only in child processes and disables automatic PATH integration.

`dotnet pack Rates/Beacon.Rates.csproj -c Release -o feed --configfile empty.config -p:NuGetAudit=false -p:UseSharedCompilation=false`

`dotnet restore BeaconBuild.csproj --source ./feed -p:NuGetAudit=false`

The package contains a real compiled tariff DLL; its separate `.snupkg` contains
symbols and is not part of ordinary dependency restoration. Package auditing is
disabled here only for this original local fixture, not recommended as a production
policy. The fixture disables SDK source-control queries so a new Git commit does
not change its nuspec or assembly informational version and invalidate the local
package lock. Alongside a fixed timestamp and deterministic compilation, this
keeps repeated fixture packs stable with unchanged inputs/toolchains. Real published
packages may need their Git provenance; this fixture choice is not a general
signed-package guarantee.

## Migration

1. Pin the selected SDK independently of the application target. Build the original
   local package and establish the exact locked dependency closure as shown above.
2. Keep the default for incremental local work. Run a normal restore twice and
   retain `obj/BeaconBuild.csproj.nuget.dgspec.json`; the checked local control
   reports up-to-date restore and retains identical graph bytes.
3. For a genuinely disposable clean build, pass the opt-out to that restore only:

`dotnet restore BeaconBuild.csproj --locked-mode --source ./feed -p:NuGetAudit=false -p:RestoreDoNotWriteDependencyGraphSpec=true`

`dotnet build BeaconBuild.csproj -c Release --no-restore -p:UseSharedCompilation=false`

`dotnet bin/Release/net11.0/BeaconBuild.dll`

4. Compare separate clean copies and fresh package caches with `true`, `false`
   and the property omitted. The opt-out suppresses writing the dgspec; the other
   two retain it. Do not expect it to delete a graph left by an earlier restore.
5. Inspect `obj/project.assets.json`, generated NuGet props/targets and the cached
   original package hash. All modes retain the same selected compile/runtime
   assets and the two-library closure (`Beacon.Rates/1.0.0` plus the project
   `Beacon.Rules/1.0.0`). Build with `--no-restore` and execute; expect
   `BEACON: crates=3;credits=21`. Use `net10.0` in the final path for the older run.
6. Remove the only package source in a new copy with a fresh cache. Restore must
   fail with package-resolution error NU1100/NU1101. A previously populated cache
   is not evidence that this isolated package route works.

## Constraints

The opt-out does not remove `project.assets.json`, the lock file or the package
cache. Do not delete them as a substitute for suppressing a disposable diagnostic
file. If a later step needs the graph, retain the default. This check measures
output presence, selected assets and a working app; it establishes neither
cache/no-op equivalence nor a speed, cost or disk-saving amount.

## Evidence

`eng/Test-BeaconBuild.ps1 -Record` compiles both canonical consumers, packs the
actual local package, checks matching package hashes in fresh caches, restores
all three settings, builds and executes every clean copy, repeats the default
local restore and verifies the missing-package failure. Saved sample evidence
records commands, exact resolved dependencies, artifact identities and input
hashes. Ordinary build/pack rejects stale evidence; real installed stdio list/get
verifies complete offline Markdown, canonical snippets, target11/edition11 and
applicability. Unusable proxies establish proxy independence, not network isolation.
