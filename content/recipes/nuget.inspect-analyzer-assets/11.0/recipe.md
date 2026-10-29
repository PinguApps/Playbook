# Inspect restore's analyzer assets without assuming execution

## Goal

Gannet Harbour's package contains an original rule warning about the class name
`UnsafeHarbour`. Inspect three separate facts: what restore records, which DLL
the SDK passes to the compiler, and whether known source actually triggers the
diagnostic. An assets entry or `_._` placeholder proves neither execution nor
enforcement of a package author's rule.

## Requirements and artifacts

Exact tested Windows x64 hosts are SDK10.0.402/runtime+ref10.0.13/C#14 and
SDK11.0.100-rc.2.26504.105/runtime+ref11.0.0-rc.2.26504.105/C#15. Their actual
NuGet SDK assemblies are7.9.0-rc.611 (net8.0) and8.0.0-rc.605 (net10.0).
Those DLL TFMs do not determine the application target or Roslyn version.
The actual Roslyn API versions are **5.9.0.0** and **5.12.0.0**, from each
complete SDK's `Roslyn/bincore` closure. Exact compiler/SDK/runtime/ref/native-host
products/hashes are separate inputs in `samples/net11.0/GannetWorkshop/Shared/artifact-pins.json`.
No independent package8 or matching-by-major NuGet/Roslyn substitution is claimed.

The original local package `Gannet.HarbourRules/1.0.0` is compiled with the real
SDK10 Roslyn5.9 references and net10 runtime, then actually loaded by both
compilers. It has no third-party dependencies. Each real restore creates a lock
and is repeated with `--locked-mode` before build; package/lock hashes and actual
closure are captured. These are author-built local artifacts, not a distributed
package/version promise. A consumer's durable feed/lock must refer to its own
reviewed artifact bytes; do not reuse a hash from a different pack.

No workload, provider, account, native compiler, device, UI/rendering mode or
experimental diagnostic suppression is needed. Only the SDK itself is preview.
General analyzer portability, other compilers/languages/OSes, redistribution and
actual GA remain unqualified. The source retains VB/future-version folders as
selection controls; it does not claim a VB analyzer implementation.

## Older approach

SDK10 keeps its working language/compiler-version package-path selector and actually runs the rule. It records no new analyzer metadata, even with the new property set true. Use actual compiler arguments plus known positive/negative diagnostics to verify execution.

## Newer approach

{{snippet:rule}}

{{snippet:analyzer-project}}

The four folders intentionally contain the same actual old-compatible DLL. They
make language/compiler-path selection observable without inventing a newer API
dependency. SDK10 selects `roslyn5.9/cs`; SDK11 selects `roslyn5.12/cs`, including
when that newer compiler targets net10. Neither selects `vb` nor `roslyn99.0`.
A real version-specific analyzer implementation needs its own compatible API
closure and execution proof.

The project inherits this sample-only boundary; there is no root automated
package permission:

{{snippet:boundary}}

Pin SDK10.0.402 with `rollForward=disable`, clear remote package sources, and
pack with that absolute dotnet executable. Pack the reviewed DLL into an isolated
local feed; restore consumers with that feed and a fresh `NUGET_PACKAGES` cache.
The canonical consumer is:

{{snippet:analyzer-consumer}}

{{snippet:positive}}

Set `HarbourTarget=net10.0`/`net11.0` and `HarbourLanguage=14.0`/`15.0` under
the corresponding pinned SDK. `Mixed=true` runs both real target compilations
under SDK11. In a separate SDK installation, pass `HarbourNet10Packs` as the
absolute SDK10 `packs` directory: the net10 inner build needs the actual10.0.13
reference packs, while net11 uses its matching RC2 packs. No SDK files are
installed/copied or persistent settings changed. A missing targeting pack is a
prerequisite failure, not an empty metadata result.

For the positive control, warnings-as-errors makes `GANNET001` a real nonzero
compiler failure. Replacing only `UnsafeHarbour` with `SafeHarbour` must succeed
with the same analyzer still selected. Do not disable analyzers, suppress that
diagnostic or blanket-ignore warnings to manufacture a passing control.

## Constraints and measured results

| Actual case | Restore metadata | Compiler/execution |
| --- | --- | --- |
| SDK10, default/false/true | No analyzer section; this old restore ignores the new opt-in | `roslyn5.9/cs`, real GANNET001 |
| SDK11 single target, default or false | No analyzer section | `roslyn5.12/cs`, real GANNET001 |
| SDK11 single target, true | All four DLLs, with `codeLanguage` and `compilerApiVersion` metadata | Only `roslyn5.12/cs`, real GANNET001 |
| SDK11 true plus `ExcludeAssets=analyzers` or `IncludeAssets=runtime` | `_._` placeholder | `roslyn5.12/cs` still runs and reports GANNET001 |
| Direct `PrivateAssets=all` | Current true records all four | Direct rule still runs; privacy is not a local exclusion |
| SDK11 mixed net10/net11, default | Both target sections omit analyzer metadata | Both actually compile with `roslyn5.12/cs` and report GANNET001 |
| SDK11 mixed net10/net11, true | Both target sections record all four, including net10 | Both actually compile with `roslyn5.12/cs` and report GANNET001 |
| Transitive project reference, `PrivateAssets=none` | Package/analyzers reach downstream | Rule runs downstream |
| Same reference, `PrivateAssets=all` | Package/analyzers absent downstream | No selected Gannet analyzer or diagnostic downstream |
| Corrected source | Same enabled metadata/selected DLL | Exit0, no GANNET001 |

The pinned SDK's `ResolvePackageAssets.WriteAnalyzers` still enumerates
`library.Files`; it does **not** consume `AnalyzerAssets`. Its existing path
selector handles language/compiler-version folders independently. Therefore
exclude/include metadata is not a rule-enforcement contract on this SDK.
The transitive privacy control removes the package from the downstream graph;
that result does not establish new per-analyzer metadata consumption.

The pinned RC2 also corrects the older Preview7 description: explicit true on
this mixed project records metadata in **both** net10 and net11 target sections.
Do not teach a net11-only gate or a new default from the older note. Unset and
false are measured independently; the NuGet.targets default expression alone
does not prove a multi-target result.

## Migration

1. Pin the actual compiler, runtime, target references and original analyzer
   package independently. Confirm the known positive/negative source is valid.
2. Restore with default, false and true `RestoreEnableAnalyzerAssets` in separate
   intermediate/cache directories. Read each target's actual `project.assets.json`.
3. Record every analyzer path and its `codeLanguage`/`compilerApiVersion`; a list
   includes eligible inventory, not necessarily the single compiler-selected DLL.
4. Repeat include/exclude/direct-private/transitive-private and mixed-target
   controls using the same actual package, then restore the generated closure
   with `--locked-mode`. Preserve lock/package hashes for the artifact tested.
5. Build with `UseSharedCompilation=false`, capture the real `/analyzer:` compiler
   arguments and a pre-CoreCompile `@(Analyzer)` receipt. Prove GANNET001 on the
   known positive source and exit0 on the corrected source for actual targets.
6. Publish only the controls this SDK actually consumes. Retain the older
   supported package-path selector here; requalify any future SDK's metadata
   consumer before relying on placeholders or asset controls as enforcement.

## Evidence and limits

`eng/Record-PackageWorkshop.ps1` captures both real paired consumers through the
existing sample-evidence workflow. The compiled `AnalyzerChecks` packs the
original analyzer from canonical sources, restores/builds the cases above in
isolated local feed/cache fixtures, saves locks and normal compiler invocations
under ignored `artifacts/gannet-assets`, and checks real diagnostic failure,
corrected success, language/version paths and both mixed target errors.
Package/lock SHA identities, exact commands and fresh canonical input hashes
remain in sample evidence; routine raw files are not committed. Metadata is
restore evidence, compiler arguments are selection evidence, and diagnostics
are execution evidence. Neither metadata nor cross-compilation substitutes for
the latter. There is no assertion that a future SDK behaves identically.

Real stdio list/get against the exact isolated installed ordinary Playbook
artifact verifies ID/title/goal, complete expanded canonical Markdown,
target11/edition11, applicability and fresh evidence from an unrelated directory.
Target10 has no selectable edition, while its valid old compiler comparison
remains in this body. Retrieval executes no analyzer; unusable proxies establish
proxy independence, not network isolation. The eight approved .NET10 foundations,
PIN-959 qualifying coverage/evidence, PIN-963 actual-GA and unrelated Human gates
remain open.

## Official references

- [Pinned restore inventory, metadata and exclusion placeholder](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/nuget-client/src/NuGet.Core/NuGet.Commands/RestoreCommand/Utility/LockFileUtils.cs).
- [Pinned actual SDK analyzer selector](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/sdk/src/Tasks/Microsoft.NET.Build.Tasks/ResolvePackageAssets.cs).
- [Pinned default and restore forwarding](https://github.com/dotnet/dotnet/blob/820230c386db5b0106c5430b3e5aee92a7826bf5/src/nuget-client/src/NuGet.Core/NuGet.Build.Tasks/NuGet.targets).
- [Historical Preview7 scope, not the current mixed-target contract](https://github.com/dotnet/core/blob/ffed591b0fb8f9aab50ecc28f2c765049fb28d8f/release-notes/11.0/preview/preview7/nuget.md).
