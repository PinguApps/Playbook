# Include referenced symbols in published output

## Goal

Beacon Quay keeps portable debug symbols beside an invoice application so a failure
in the referenced charging library can be investigated. Choose the publish policy
explicitly and inspect actual files rather than assuming every symbol is available.

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

Use **PublishReferencesSymbols**, with plural `References`. The Preview1 release
note's singular **PublishReferenceSymbols** spelling is superseded by the actual
SDK targets. SDK11 RC2 defaults the real property to **true**. Its false setting
omits the project's referenced PDB without removing the application's own PDB.

The actual SDK10 baseline already publishes that project-reference PDB, even with
the new plural property set false. There is no need for an explicit file-copy
workaround in this older consumer. Unknown MSBuild property names are accepted
silently; a successful publish does not establish that a switch was recognised.

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
policy. A fixed pack timestamp and deterministic compilation make the local package
re-creatable with unchanged inputs; they are not a general signed-package guarantee.

## Migration

1. Keep portable PDB generation enabled on the original application and library.
   Restore the exact original package closure using the preceding local setup.
2. With SDK11, request reference symbols explicitly when publishing a framework-
   dependent folder. The omission/default also includes them in this measured SDK:

`dotnet publish BeaconBuild.csproj -c Release --no-restore -p:PublishReferencesSymbols=true -p:UseSharedCompilation=false -o publish-with-symbols`

`dotnet publish-with-symbols/BeaconBuild.dll`

3. Repeat into **new** output folders with `false`, the property omitted and the
   deliberately wrong `-p:PublishReferenceSymbols=false`. Fresh folders prevent
   stale PDBs from masquerading as newly published output. Inspect the results:

| Setting | SDK10 referenced Rules PDB | SDK11 referenced Rules PDB | Application PDB |
| --- | --- | --- | --- |
| Plural true | Present | Present | Present |
| Plural false | Present; property has no effect here | Absent | Present |
| Omitted | Present | Present | Present |
| Singular false | Present | Present; wrong spelling has no effect | Present |

4. All four outputs retain `BeaconBuild.dll`, `Beacon.Rules.dll` and
   `Beacon.Rates.dll`. Compare each published DLL/PDB with the matching build
   output, verify portable PDB headers and execute every application. Expect
   `BEACON: crates=3;credits=21`; mere PDB file presence is not a debugger session.
5. For the application's own symbols, use the separate supported
   `CopyOutputSymbolsToPublishDirectory=false`. With plural reference symbols
   true, SDK11 still publishes `Beacon.Rules.pdb` while omitting `BeaconBuild.pdb`.
   The helper exercises and executes that output too.
6. Inspect the package boundary: the original `Beacon.Rates` ordinary package
   carries its DLL, while its separate snupkg is not restored as an application
   dependency. None of these publishes gains `Beacon.Rates.pdb` from that snupkg.
   Acquire matching package symbols through the publisher's supported route when
   needed; this property does not download or synthesize missing symbols.

## Constraints

This is a Windows x64, framework-dependent **folder** publish with portable PDBs.
It is not proof of single-file embedding/extraction, embedded/Windows PDB modes,
trimming, NativeAOT, a symbol server, Source Link or arbitrary transitive libraries.
Set policies deliberately; excluding PDBs does not remove embedded source/path
information from every possible artifact. No debugger or performance claim is made.

## Evidence

`eng/Test-BeaconBuild.ps1 -Record` compiles both real dependency sets and exercises
true/false/default/wrong-singular settings against their matching complete SDKs.
It checks exact DLL/PDB bytes, portable headers, own/reference and snupkg boundaries,
and executes every published application. Sample evidence retains exact commands,
dependencies, input hashes and artifacts. Ordinary build/pack and unrelated-directory
installed stdio inspection preserve the complete canonical body and requirements.
