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

## C# GA compiler refresh

PIN-1287 retains the RC audit and historical receipts in Git at
`0d455e8759b6a7181f79568e8da5d294f5c9171b`; new receipts record actual GA runs.
SDK 11.0.100 (Roslyn 5.12.0-1.26529.118), runtime and reference pack 11.0.0
were used for the nine C# recipes. The shared ClosedOutcomes consumer also
requires the two JSON recipes' evidence SDK declarations to follow its pin.
ReadonlyMetadata follows the same compiler-migration runner and GA artifact set.
Older .NET 10 pins and language modes remain unchanged. Refreshed .NET 10
receipts account for the runner/input hash changes rather than claiming GA there.

The GA breaking notes' C# 16 wording does not establish a released language
version: the actual GA compiler rejects updated memory-safety syntax in C# 15
with CS8652 and accepts the explicit preview/updated-memory-safety-rules route.
The unsafe caller/body checks, accessibility, metadata markers, pointer-only
relaxation and pointer-span constructor controls still pass. Pointer-span
construction requires an unsafe context (CS9362); unsafe syntax still needs
AllowUnsafeBlocks (CS0227). This remains experimental guidance.

The CollectionLike consumer executes configured/default builder overloads and
escaped `@with` calls; ClosedOutcomes checks escaped `@closed`/`@union` names.
Compiler migration controls preserve the SDK10.0.100 accepted Span escape and
legacy dynamic binder failures, then require CS8352/CS7083 on serviced .NET 10
and GA .NET 11. No fabricated references or new historical foundation is added.
PIN-967 remains the global stable-readiness gate; this refresh does not close it.

## F# GA refresh

PIN-1288 refreshes sixteen edition 11 recipes and their twelve .NET 11 consumers.
The two FCS/package-host recipes remain with PIN-1289. Historical RC pins,
receipts and failures remain available at `307dc932a162370fd4c7166152a31a1e03c5a8ef`;
the original contract/source manifests above are retained unchanged.

Actual SDK 11.0.100/runtime 11.0.0 and restored FSharp.Core 11.0.101
(`lib/net10.0/FSharp.Core.dll`) compile and execute the newer consumers.
Record constructors, extension SRTP, named arguments, computation rethrow and
runtime async still require preview; GA does not promote those language gates.
The supplied named-argument attribute is still required: the GA BCL lacks it.
External SRTP calls with the defining module open pass; ordinary F#11 and a
missing module still fail FS0193. An inline forwarding call in separate FSI
fragments also passes, assessing the witness-preservation/FSI cache source delta
without claiming exhaustive compiler regression coverage or adding a new recipe.

Actual pending Task/ValueTask waits, failure identity, asynchronous builder cleanup,
sequence cancellation, early/exhaustion/nested disposal and exception precedence
pass. The separately reproduced pending Task<int>-to-Task upcast still raises
InvalidProgramException on GA; the bounded builder keeps its real non-generic Task
route. FSI loads the SDK net10 Core (SHA256
`48d8d12b8b0e7b7b24d132d24a8f0460cdd20806c06e410c0c16bc4c378913b8`):
all four gated cleanup cases complete exactly once. Ordinary task Using still loses
a pending cleanup exception; the unchanged explicit cleanup alternative propagates
it. These retained RC limitations are not silently declared fixed.

Windows x64 native publish/run passes with ILCompiler/linker 11.0.0, MSVC
14.51.36231 and WindowsSDK 10.0.26100.0. All three Based calls fail IL3050,
including literal-zero bases; the migrated native executable reports dynamic
code=False. Linux/macOS native execution is not-tested; no cross-platform,
runtime-async AOT or debugger claim is made. Older .NET 10 pins remain unchanged;
records depending on changed probe scripts were refreshed with SDK10.0.403.
PIN-967 remains the global stable gate. No release/package publication is authorized.

Focused commands (PowerShell7; native compiler prerequisites as above):

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-1ead8027'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
$ga = 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe'
& ./eng/Test-FSharpGuidance.ps1 -Net11Dotnet $ga
& ./eng/Test-FSharpBoundaries.ps1 -Net11Dotnet $ga
& ./eng/Test-FSharpRuntimeGa.ps1 -Net11Dotnet $ga
& ./eng/Test-FSharpGaInterop.ps1 -Net11Dotnet $ga
& ./eng/Test-FSharpLifetimes.ps1 -Net11Dotnet $ga
```

Each command must exit successfully. Expected negative diagnostics are printed as
PASS controls. The original shared runtime-async runner reached its F# controls,
then failed on an incomplete RC SDK used by its unrelated C# section; the focused
GA runner avoids that dependency. The root SDK10.0.403 installation was also
incomplete; an untracked portable installation under ignored artifacts restored
build capability without changing PATH. User/machine PATH comparisons passed.

Installed discovery/detail reproduction (the inspected local package is never published):

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-1ead8027'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
& 'C:\Users\pingu\.dotnetup\dotnetup.exe' install 10.0.403 --install-path 'V:\_worktrees\Playbook\t3-1ead8027\artifacts\dotnet10' --untracked --no-progress
$host10 = 'V:\_worktrees\Playbook\t3-1ead8027\artifacts\dotnet10\dotnet.exe'
& $host10 build ./src/PinguApps.Playbook -c Release -p:Version=0.0.0-preview.1288
& $host10 pack ./src/PinguApps.Playbook -c Release --no-build -p:PackageVersion=0.0.0-preview.1288 --output ./artifacts
$ids = @(Get-ChildItem ./content/recipes -Directory -Filter 'fsharp.*' |
  Where-Object Name -notin @('fsharp.adapt-opaque-callbacks','fsharp.configure-compiler-service-memory') |
  Select-Object -ExpandProperty Name)
$ids += 'json.serialize-fsharp-unions'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 -RecipeId $ids -Query '' `
  -RecipeEdition 11.0 -TargetVersion 11.0 -PackageVersion 0.0.0-preview.1288 `
  -Dotnet $host10 -Node 'C:\Program Files\nodejs\node.exe' `
  -OutputDirectory ./artifacts/pin-1288/inspection
```

Expect seventeen ID/title/goal entries (sixteen F# recipes plus the shared JSON union consumer), complete canonical Markdown and applicability,
including experimental gates and retained cleanup/upcast limitations. The harness
installs the actual local artifact and calls list/get through stdio from an
unrelated directory. This proves served guidance, not other-platform execution.
