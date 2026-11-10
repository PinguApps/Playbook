# GA language and compiler reconciliation

PIN-961, reviewed 10 November 2026. This is a source/artifact reconciliation and
delivery register, not GA execution proof or stable-release approval.

## Authority and baseline

Scope: SPEC.md at `0a8989814ea11346354464b845c79c15bb71c5e9`, stories
41–44, 47, 93–95, 130, 132–133, 146 and 158–160; approved proposal `b3778cc`
(`GA_LANG`); ADRs 0001–0004; the compiler/F# contract audits and release plan.
Root PLAN.md, CONTEXT.md and CONTEXT-MAP.md are absent. Historical planning
statements that implementation is absent are superseded by the current catalogue.
All 21 native delivery prerequisites on PIN-961 were verified Done in the
PinguApps/Playbook boundary. Their completion permits comparison; it does not
convert their RC observations into GA evidence.

Microsoft's [release feed](https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/11.0/releases.json)
now reports runtime 11.0.0, SDK 11.0.100, active support and release date
10 November. The [GA release notes](https://github.com/dotnet/core/blob/main/release-notes/11.0/11.0.0/11.0.0.md)
and actual installed SDK clear the earlier availability wait. Installed `.version`
and assembly product metadata identify VMR
`297acf930fb0f5c07fd4e51d78e521b4ad2e5d1f`. The retained RC2 source is
`820230c386db5b0106c5430b3e5aee92a7826bf5`; current sample records also include
RC3. Neither baseline is overwritten.

The adjacent source manifest binds installed DLL hashes, immutable source URLs,
source hashes and all 29 prepared edition 11.0 language/compiler recipe identities,
stability labels and content hashes. The C# edition 10.0 foundations `csharp.extend-uneditable-types` and `csharp.validate-property-assignment` are unchanged
and outside this GA refresh. Framework-specific VB application/Designer guidance
remains with PIN-966 rather than inventing a new VB language version.

## Findings and exact delivery

| Surface | Observed GA comparison | Disposition |
| --- | --- | --- |
| Roslyn Core, C#, VB public declarations | Combined Shipped/Unshipped ordinal declaration sets have zero additions/removals from retained RC2 in all three surfaces. | No new compiler API goal established by this delta. Refresh actual consumer proof separately. |
| Bundled C#/VB compiler | Product version `5.12.0-1.26529.118`, source identity matches SDK VMR. | GA SDK distribution does not imply a stable Roslyn NuGet package. NuGet.org stable C#/VB observed at 5.9.0; preserve preview labels until actual package-route reconciliation. |
| VB language | GA source Default/Latest still map to VisualBasic17_13. | No VB 11 inference and no additional syntax goal. Existing compiler inspection VB callers remain required. |
| C# migration notes | GA VMR still describes pointer/safe/unsafe changes as C# 16 while C# 15 guidance exposes memory safety behind preview and feature opt-in. | PIN-1287 must resolve actual compiler gates and migration probes. Do not promote experimental memory safety to stable C# 15. Existing PIN-715/PIN-711/PIN-712 slices retain their identities. |
| F# contracts | Retained resumable intrinsics, array contracts, task Using, checker sharing API, language gates, runtime-async document and example builders match GA source. | Preserve experimental runtime coupling and exact supported routes. Equality establishes source continuity, not successful GA execution. |
| FSI | GA removes the per-fragment `ClearExtensionOperatorSolutions` call from the audited source. | Inspect affected interactive/SRTP behavior during PIN-1288; no arbitrary new recipe inferred from an implementation fix. |
| F# release-note delta | Adds preservation of extension SRTP witnesses across files/inline calls and unchanged code generation when extension constraint solutions are disabled (PRs 20717/20773). | Assess the existing generic-operation route during PIN-1288. Automatic compiler corrections alone do not justify new authoring content. |
| Bundled F# | Core `11.0.101-rtm.26529.118`, FCS `43.13.101-rtm.26529.118`. NuGet.org exposes stable Core 11.0.101; FCS feed latest is independently versioned. | PIN-1289 must verify exact package identity, dependency closure and selected assets before removing preview-package wording. Never choose a package merely because it is latest. |

The final C#/F# release-family notes describe quality/stability work and point to
the cumulative language documentation. The C# page still contains prerelease
wording at inspection time. That stale sentence cannot override the actual GA
feed/artifacts or establish feature behavior. The GA compiler breaking-change
document remains the explicit source for the unresolved memory-safety routing.

## Complete prepared catalogue register

Every identity below is bound by hashes in the adjacent manifest. These are
correction/evidence assignments, not claims that GA samples already pass.

| Owner | Edition 11.0 recipes | Required outcome |
| --- | --- | --- |
| [PIN-1287](https://linear.app/pinguapps/issue/PIN-1287) | `csharp.audit-memory-boundaries`, `bind-dynamic-conditions`, `closed-hierarchies`, `closed-results`, `configure-collection-construction`, `control-nested-loops`, `index-existing-types`, `preserve-span-storage`, `static-interface-helpers` (all with `csharp.` prefix) | Refresh GA pins and actual checks for nine recipes; resolve memory-safety/version/keyword probes; retain any existing experimental labels. |
| [PIN-1288](https://linear.app/pinguapps/issue/PIN-1288) | `fsharp.adapt-generic-operations`, `author-runtime-async-builder`, `author-runtime-async-sequence`, `bound-async-work`, `compose-records`, `construct-records`, `dispose-async-resources-in-fsi`, `format-without-reflection`, `migrate-based-arrays-for-aot`, `migrate-enum-flags`, `pass-methods-as-delegates`, `preserve-interpolation-culture`, `require-named-arguments`, `rethrow-in-computations`, `reuse-api-documentation`, `start-async-on-caller` (all with `fsharp.` prefix) | Refresh sixteen language/runtime routes and affected FSI/AOT/experimental behavior with GA artifacts. Preserve older comparisons and actual failures. |
| [PIN-1289](https://linear.app/pinguapps/issue/PIN-1289) | `fsharp.adapt-opaque-callbacks`, `fsharp.configure-compiler-service-memory`, `languages.inspect-coalescing-conversions`, `languages.inspect-type-layout` | Reconcile four package-bound routes, dependency/assets and actual hosts; retain preview routes where stable equivalents are unavailable. |

Each follow-on is Agent, L (5), in milestone 09 and current cycle 5
(9–16 November). Estimates include real consumer/artifact verification and
packaged list/get checks. The register adds no ninth foundation, provider/native
matrix, release action or speculative compiler-host feature. PIN-967 remains
the global candidate gate and must wait for all qualifying follow-ons. This audit
does not remove preview exceptions, change recipes or refresh compilation records.

## Reproduce the source/artifact comparison

Prerequisites: PowerShell 7, installed SDK 11.0.100 at the specified root, HTTPS
access to canonical Microsoft source/release endpoints. This only reads artifacts
and sources and writes disposable JSON; no SDK process, installer or profile
integration is invoked.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-04db8064'
& ./eng/Inspect-GaLanguageContracts.ps1 `
  -DotnetRoot 'C:/Users/pingu/AppData/Local/dotnetup/dotnet' `
  -OutputPath './artifacts/ga-language-contracts.json'
$audit = Get-Content './artifacts/ga-language-contracts.json' -Raw | ConvertFrom-Json
$audit.roslynApi | Select-Object area, added, removed
$audit.fsharpSources | Where-Object { -not $_.unchanged } | Select-Object path, added, removed
$audit.catalogue | Format-Table id, stability
```

Expected: PASS after exact retained artifact/source/catalogue comparison, 29 editions; zero Roslyn declaration changes; FSI, source-manifest
and FCS release-note deltas. Source hashes and installed product identities are
recorded, not asserted to be fresh execution evidence. No build, FSI behavior,
Native AOT, GA package-host restore or installed MCP check is claimed here.
