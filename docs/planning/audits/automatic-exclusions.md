# Automatic/no-change exclusions: PIN-690

Reviewed **25 October 2026** against the **.NET 11 RC2 preparation baseline**.
All thirteen assigned rows have a disposition. Three compound exclusions need
partial reversal: F# callback authoring, IEEE-decimal result comparisons and Linux
Negotiate channel-binding policy. Delivery drafts **A–C** below await Pingu's
backlog approval through **PIN-986**.
Neither is created or implemented. Automatic residuals remain excluded; earlier
qualifying migration/API owners retain their work. This audit supplies no recipe,
consumer execution, GA evidence or publication permission.

## Boundary and evidence

[PIN-690](https://linear.app/pinguapps/issue/PIN-690), planning key **SH_Q10**;
`SPEC.md` at `0a8989814ea11346354464b845c79c15bb71c5e9`, stories **47, 158, 159**;
approved proposal **b3778cc**. ADRs 0001–0004, current specification, planning
inventory/implementation/verification guidance, contribution rules and existing
family audits constrain this review. Root PLAN.md, CONTEXT.md and CONTEXT-MAP.md
are absent. Every explicit target provenance pointer was resolved. Historical
planning counts and claims that implementation has not begun describe the initial
inventory, not today's delivered catalogue.

Linear boundary: PinguApps team **62f80270-b641-4fa3-b60c-46be4c4df2c8**, Playbook
project **b22380c7-6fde-49f4-8826-e5631dc5df7a**. A paginated archived-inclusive
query returned **319** issues; both IDs were checked on every result. PIN-690 has
no native blocker. PIN-986 already depends on this audit and owns approval and
ticket creation; PIN-959 retains comprehensive coverage closure. No related issue
is changed. Shared Linear agent notes were unavailable (`Tool get_agent_context
not found`); repository and tracker evidence supplied the context.

The [source manifest](automatic-exclusions.sources.json) pins **34 full official
files** by immutable URL and SHA-256: fourteen RC1/RC2 component notes (F#,
libraries, runtime, SDK, MSBuild, NuGet, containers), the three cumulative
library/runtime/SDK documents, the compatibility index, the full watcher and
native-CLI compatibility articles, paired watcher implementations, and five F#
declaration/language-gate/test files, plus seven paired Negotiate policy/managed
server-option/Unix implementation and current native GSSAPI files. Manifest IDs
below identify exact files,
rather than mutable overview links. Full sections and their old/new behavior,
mitigations and limits informed each disposition. The compatibility index remains
explicitly incomplete. This is the assigned thirteen-row recheck, not another
release-wide API audit.

- Canonical notes: dotnet/core **ffed591b0fb8f9aab50ecc28f2c765049fb28d8f**.
- Cumulative libraries/runtime: dotnet/docs **dfe6ae54d3cf986ba03d95075e61ae006fefb9f9**;
  SDK/index/mitigations: **f0021fde66c59559fa1ba3c9a044148738f12faf**.
- Paired watcher source: dotnet/dotnet **4099f83977f4d29604a9aeb971fb0a7bc1d5f699**
  (.NET 10.0.13) / **820230c386db5b0106c5430b3e5aee92a7826bf5** (RC2).
- Paired F# source: SDK 10.0.402 VMR **a8dd0577c5c13f43a2f57fe33b4ff11b90f6371e** /
  that same RC2 VMR. Source inspection does not execute compiler tests.

The [small asset report](automatic-exclusions.assets.json) records fresh,
metadata-only inspection of seven FSharp.Core assets: both installed SDKs, both
assets of **10.1.402**, and all three assets of **11.0.101-rc2.26504.105**. Both
downloaded package ZIPs are hash-verified. The attribute is absent from all three
older assets and present in all four newer assets. Exact file hashes, framework
labels, assembly versions, constructor and attribute usage are retained. The new
SDK's Core DLL and the package's net10.0 DLL have different hashes: neither SDK
identity nor assembly version establishes package byte identity.

Existing [data/I/O](data-io.md), [Extensions](extensions.md),
[runtime/security](runtime-security.md), [SDK/build](sdk-build.md),
[hosting/configuration](hosting-configuration.md), [numeric migration](numeric-and-parsing.md)
and [web](web.md) audits provide separately recorded wider asset comparisons and
existing proposal ownership. Those dumps were consulted, not regenerated or
relabeled as fresh execution here.

## Thirteen-row disposition register

**Included** means additional authored work remains in a draft. **Grouped** means
an existing delivery/proposal owns the qualifying subcase, not that an audit
delivers it. **Excluded** applies only to the named unchanged-code residual.

| Assigned inventory ID | Exact evidence and authoring decision | Disposition and delivery owner |
| --- | --- | --- |
| `fsharp.collection-compiler-speedups` | `rc2-fsharp`, lower-allocation/compiler-memory/fix sections; `fsharp-10/11-prim-types`, `fsharp-10/11-LanguageFeatures`, `fsharp-11-OptimizeClosureIfNotInlined`, actual seven-asset report. Calling ordinary List/Array functions gains inlining/empty-array reuse without editing code. However, authors can now put **OptimizeClosureIfNotInlinedAttribute** on their own callback parameter with InlineIfLambda. The selected old source/assets lack it; the new source gates declarations at F# 11. | **Partial reversal: included → A.** Compiler memory/default collection benefits stay excluded. Explicit FCS sharing is already delivered by **PIN-981**; runtime builders, AOT migration and FSI cleanup remain their separate existing goals. |
| `regex.automatic-fixes` | `cumulative-libraries`, regex engine/source-generator section: nested-loop non-backtracking correctness/performance, conditional-loop resumeAt, SYSLIB1045 fixer duplicate partial-class names. No new option, diagnostic or required authored replacement is established by these fixes. PIN-703's actual generator/descriptor comparison retains SYSLIB1040–1045. | **Excluded residual.** Authored **RegexOptions.AnyNewLine** remains delivered **PIN-724**, not excluded by this row. No concrete application workaround removal was established. |
| `numerics.automatic-speedups` | `cumulative-libraries`, determinant and queue sections; `rc1-runtime`, Half section; `rc2-libraries`, improved IEEE decimal accuracy. Matrix4x4.GetDeterminant vectorization, supported Half FP16 lowering and queue-growth correction need no changed implementation. RC2 decimal transcendental results intentionally change, though signatures do not; stored exact expected outputs may need an explicit comparison/persistence policy. The note does not guarantee correct rounding. | **Partial reversal: included → B** for result expectations. Automatic determinant/Half/queue effects stay excluded. Decimal/BigInteger conversion, Math.Round, Complex and tensor migrations remain **PIN-683**'s distinct drafts; choosing decimal precision is delivered **PIN-736**. Neither proves this transcendental-result goal. |
| `configuration.automatic-watcher-fixes` | `cumulative-libraries`, file-change reliability; `watcher-paths`, `watcher-10/11`. Relative-path normalization, missing-root recovery and reduced watching are automatic for correctly composed consumers. Both PhysicalFilesWatcher constructors now validate/normalize root and reject an unrelated nonempty FileSystemWatcher.Path; a nonexistent root and empty watcher path remain valid. | **Grouped qualification → PIN-685 draft C**, watcher-root composition, through **PIN-986**. Retain the automatic residual only. Async callback/options work is separately delivered **PIN-745/746**; it does not deliver draft C. |
| `networking.automatic-benefits` | `rc1-libraries`, `cumulative-libraries` and `negotiate-10/11-*`: malformed TLS-frame checking, Linux TLS alerts, Apple/Pipelines performance are automatic. However, the new Unix server forwards Binding to GSSAPI; ExtendedProtectionPolicy.OSSupportsExtendedProtection changes from Windows-only to Windows **or Linux**. The source explicitly excludes macOS GSS.framework mismatch enforcement. Binding/Policy options already exist; their usable server platform changes. HTTP/2 NTLM/Negotiate downgrade can separately permit removing a manual request-version workaround, subject to policy/auth/protocol checks. | **Partial reversal: included → C**, Linux policy and actual matching/mismatched TLS-binding proof. **Grouped → PIN-705 R17** for HTTP/2 workaround removal through PIN-986. PIN-831's full criteria explicitly require Windows HTTP.sys; PIN-709's audit does not draft Linux GSSAPI delivery, so neither silently absorbs C. Automatic hardening/performance stays excluded. No universal Unix CBT API or security-policy bypass is invented. |
| `runtime.automatic-improvements` | `cumulative-runtime`, JIT/R2R/VM/CPU/FP16 sections and runtime-async performance subsections; `rc1-runtime`. Ordinary devirtualization, guard/branch/constant folding, default comparer specialization, dispatch/GUID caching and >1024-CPU startup require no authored switch. | **Excluded residual.** Explicit runtime-async opt-in/removed-switch migration stays delivered **PIN-757/980**; explicit intrinsics **PIN-739**; hardware/crash settings **PIN-756**. New SVE APIs and unsupported native-tooling questions remain **PIN-705**, not erased by an optimization headline. |
| `runtime.automatic-wasm-diagnostics` | `cumulative-runtime`, WebAssembly section. Default WebCIL V1 loading, improved symbols, direct float/span marshaling, Release JS minification, satellite repair and test-suite progress are automatic in ordinary generated apps. A custom loader or native relink is authored code/configuration. | **Excluded residual; grouped authored exceptions → PIN-705 R14** (actual custom Mono/CoreCLR loader contracts) and **PIN-758** (NativeFileReference/Emscripten). R14 awaits **PIN-986**; PIN-758 remains Backlog. No workload/native/browser execution is claimed. |
| `runtime.automatic-reliability` | `rc1/rc2-runtime` and `cumulative-runtime`, reliability/diagnostics sections: GC/DATAS/cgroup, unload/queue/CPU-group repairs and missing-final-semicolon parsing for CORECLR_NOTIFICATION_PROFILERS. Existing profilers gaining consistent async events does not alone establish a supported new native authoring contract. | **Excluded demonstrated fixes.** iOS/tvOS locking is an explicit **PIN-684/PIN-741** migration; authored 32-bit heap limits remain **PIN-705 R24**. Profiler/cDAC/Mono/perf-map supported consumer routes remain **unresolved PIN-705 U1/U2**, carried to **PIN-986/PIN-959** and final reconciliation, not pronounced automatic or covered. |
| `sdk.cli-build-internals` | `cumulative-sdk`, `rc1-sdk`, native-CLI full mitigation (`docs_core_compatibility_sdk_11_native-cli-command-handling-enabled.md`). Installer deduplication, default native command handling/server startup, inbox partial-R2R, shell completion and Hot Reload recovery need no ordinary project edit. DOTNET_CLI_ENABLEAOT=false is a diagnostic escape hatch when behavior differs; affected public APIs: none. | **Excluded residual.** Explicit telemetry **PIN-766**, watch/scaffolding **PIN-761** and authored partial-R2R **PIN-707 B15** retain ownership. Do not turn an escape hatch without a demonstrated failing workflow into another recipe. |
| `sdk.cli-output-and-fixes` | `cumulative-sdk`, `rc1/rc2-sdk`: reference-directory fallback, launch notice on stderr, dnx help/approval fixes, unchanged-file Native AOT cache reuse and corrected workload-version messages do not prescribe a new implementation. A default-output fix does not justify a new command wrapper. | **Excluded residual.** File directives/artifact layout/formatting **PIN-759/760**, Configuration defaults/launch arguments **PIN-760**, runner artifact policy **PIN-762**, extraction warning **PIN-763**, custom console encoding **PIN-707 B25** remain authored owners. No claim of unchanged parsing semantics for every custom automation client. |
| `msbuild.automatic-improvements` | `rc2-msbuild`: wildcard matcher accelerates existing include/exclude evaluation; `rc1-msbuild`: server identity/state reset, BuildCheck/XML-cache/telemetry/task-host corrections. No project rewrite is required for these residual fixes. | **Excluded residual.** TarDirectory/Untar/glob inspection **PIN-767**, DTD/retired waves **PIN-768**, custom task/host/public API changes **PIN-707 B01–B06** retain ownership. No blanket no-MSBuild-change conclusion. |
| `nuget.automatic-pack-fixes` | `rc1-nuget`: reuse prior project evaluations, nuspec substitution/PackageVersion fallback, enabling sources through UpdatePackageSource and PMC supplementary-Unicode backspace correction. Existing correctly authored pack/source operations receive the fixes; no new caller API or concrete workaround removal is established. | **Excluded residual.** DeterministicTimestamp is **opt-in**, delivered **PIN-770**; direct client host targeting **PIN-771**; cooldown/vulnerable updates **PIN-769**. Custom reused-host state reset remains **PIN-707 B08**. RC2's --min-publish-age-hours is authored policy, not an automatic pack fix. |
| `containers.rc-image-review` | `rc1-containers` and `rc2-containers` each explicitly say there are no container-image changes for that milestone. This is positive source evidence for these two files, not an empty search result. | **Excluded, bounded to RC1/RC2 image notes.** SDK SOURCE_DATE_EPOCH/ContainerPushNoCache/multi-architecture publishing stays **PIN-765**. Earlier base-image/signature migration and registry validation remain **PIN-707 B20/B21/B27**. No claim that the whole .NET 11 container family has no changes. |

## A. Adapt an opaque curried callback once — M (3), approval required

**Story:** As an F# library author, I want a reusable loop to accept supplied
functions efficiently so callers can keep a clear API without repeated callback
adaptation inside the loop.

**Outcome:** One complete recipe, proposed ID
`fsharp.adapt-opaque-callbacks`, edition/introduction 11.0. Use a wholly original
fictional seed-laboratory batch fold with both a literal capturing lambda and a
callback supplied by a non-inlined factory. Compare the valid .NET 10/F# 10
implementation fairly; the older Core has no new attribute.

Exact contract: `Microsoft.FSharp.Core.OptimizeClosureIfNotInlinedAttribute()`,
parameter-only, sealed, paired with `InlineIfLambda` on a **separately curried**
parameter of arity **2–5** in a suitable inline function/method. It allows one
`OptimizedClosures.FSharpFunc` adaptation after the enclosing function inlines
when the supplied argument is not a known lambda. It does not eliminate every
closure, make opaque code inline or guarantee a throughput/allocation percentage.
Declaration gate is **F# 11**, not preview. The pinned upstream tests reject
unsupported shapes with **FS3916** and F# 10 declaration with **FS3350**; they
compile an attributed F# 11 library with a newer compiler's F# 8-mode consumer.
Older language mode is not an older compiler/runtime compatibility proof.

Required delivery acceptance:

- Reconcile actual compiler/Core/reference/runtime assets. Initially pin SDK
  10.0.402 / 11.0.100-rc.2.26504.105 and Core 10.1.402 /
  11.0.101-rc2.26504.105. The new attribute is distributed in net10.0 and both
  netstandard assets; prove any claimed target-10 route through actual restore,
  compilation and execution, not framework labels. Do not add a ninth foundation.
- Compile/run original older/newer library **and a separate consumer assembly**
  using literal and opaque callbacks. Verify results, order/side effects and
  exception propagation. Inspect optimized emitted IL for a hoisted Adapt and
  absence of repeated InvokeFast in the relevant loop; retain an unannotated
  control. Structural evidence proves that transform, not universal performance.
- Reproduce applicable F# 10 and invalid-shape diagnostics with actual compilers.
  Verify imported metadata consumption with the newer compiler at an older
  language mode; document an older-compiler fallback only after testing it.
- Supply complete offline Markdown, canonical snippets, numbered migration,
  applicability and exact evidence. Capture fresh compilation/execution, build
  and pack once, then check discovery **ID/title/goal**, full Markdown,
  applicability and target selection through the real installed stdio server
  offline from an unrelated directory. GA reconciliation remains separate.

Destination proposal: **Agent**, existing **02 - Language and library guidance**
milestone, Backlog, existing Agent/Feature labels where appropriate. **M (3)**
includes library/consumer/IL/diagnostic checks and installed guidance. PIN-981's
FCS memory-host goal does not cover library callback annotations. No matching
delivery appeared in the 319-issue scoped overlap review or current recipes.
Allocate a title prefix only after approval and refreshed overlap. Relate the
approved ticket to PIN-690; retain it as a PIN-959/final language reconciliation
prerequisite. No native workload or account setup is required by this proposal.

## B. Choose a contract for IEEE-decimal function results — M (3), approval required

**Story:** As a developer storing calculated measurements, I want to choose how
to compare and refresh calculated results so a numerical correction does not
silently invalidate saved expectations.

**Outcome:** One complete recipe, proposed ID
`numerics.compare-ieee-decimal-results`, edition/introduction 11.0. A fictional
observatory stores normalized angle/logarithm results and distinguishes input
identity, a deliberately versioned persisted output, and an explicitly chosen
numeric tolerance. RC2 intentionally corrects Decimal32/64/128 transcendental
outputs, especially Decimal128 cancellation near ±1/1; alternate decimal
representations and Log(x, base) also change. No new signature or old-algorithm
switch is established. These functions are **not guaranteed correctly rounded**.

Required delivery acceptance:

- Reconcile exact RC1/RC2 implementations and actual numeric assets before
  selecting representative Acos/Log/SinPi cases. Prove the changed outputs with
  actual releases when teaching that before/after claim; unavailable RC1 artifacts
  block that claim. Retain exact artifact hashes and the actual comparison basis.
- Author original .NET 10/11 consumers for the same comparison/persistence goal.
  Explain that .NET 10 has no framework Decimal32/64/128 transcendental equivalent;
  its valid Math/double route is a different representation, not an emulation of
  old IEEE-decimal results. Do not claim a new System.Decimal behavior.
- Compile/run actual consumers; cover a cancellation-sensitive finite value,
  alternate representations of an exact input, ordinary values, signed/nonfinite
  handling and a deliberately failing stale expected value. Derive any accuracy
  assertion from an independent justified mathematical reference, not from one
  implementation's output. State chosen absolute/relative tolerances and bounds;
  do not prescribe a universal epsilon or promise correct rounding.
- Keep stored-output migration explicit/versioned and reversible on disposable
  data. Do not silently rewrite production records or replace exact identifiers
  with approximate comparison. Explain recomputation versus preserved historical
  values. Show meaningful negative checks for an unacceptable drift.
- Deliver original canonical snippets, complete offline guidance and requirements,
  actual compilation/behavior evidence, and the same packed/installed list/get
  acceptance as A. No benchmark/native matrix. GA reconciliation stays separate.

Proposed **Agent**, existing **02 - Language and library guidance**, Backlog;
**M (3)** includes actual-version qualification, controlled persistence/comparison
cases and installed verification. PIN-736's accumulation/precision recipe does
not cover transcendental outputs; PIN-683's conversion/rounding drafts concern
different operations. No duplicate delivery appeared in the scoped title/full
relevant-owner and current-recipe review. Refresh overlap before approval-based
creation, reserve no prefix, relate to PIN-690 and keep PIN-959/GA prerequisites.
Access to matching RC1 artifacts is a delivery prerequisite, not proof supplied
by this audit. PIN-986 owns the approval decision.

## C. Bind Linux Negotiate authentication to TLS — L (5), approval required

**Story:** As a Linux service developer, I want authentication to check the secure
connection carrying it so credentials cannot be accepted on an unrelated connection.

**Outcome:** One complete recipe, proposed ID
`networking.bind-linux-negotiate-to-tls`, edition/introduction 11.0. A fictional
research-station service obtains the actual TLS endpoint binding and supplies it
with an explicit required protection policy to `NegotiateAuthentication` through
the existing `NegotiateAuthenticationServerOptions.Binding` and `.Policy`.
This is new supported Linux server behavior, not new option declarations.
The old Unix source does not pass the server binding to GSSAPI and reports no OS
extended-protection support. The new source passes it to `gss_accept_sec_context`;
`ExtendedProtectionPolicy.OSSupportsExtendedProtection` includes Linux, while
macOS is explicitly excluded because its native implementation accepts mismatches.

Required delivery acceptance:

- Reconcile actual old/new managed and native artifacts and supported Linux
  GSSAPI implementation, authentication mechanism, TLS/certificate requirements
  and credential/keytab environment. A public property or successful Windows
  HTTP.sys test is insufficient. Do not claim managed-NTLM server support from a
  GSSAPI path, or general Unix/macOS support from a filename.
- Author original .NET 10/11 consumers of the same protection goal. Show the real
  older unsupported-policy behavior and an honest fail-closed/supportable
  alternative; never emulate protection by declaring a successful old exchange
  secure. Keep TLS verification and required policy enabled.
- Compile/run against actual releases in a disposable Linux environment with
  real TLS and GSSAPI credentials. Verify matching bindings authenticate;
  mismatched bindings from another TLS endpoint are rejected; missing bindings,
  unsupported policy/platform and credential failures cannot silently authenticate.
  Record actual status codes, negotiated mechanism, binding lifetimes and native
  dependencies. A fake callback/native shim is not credential-relay protection proof.
- Deliver full original offline guidance and canonical samples, applicability,
  numbered migration and limits, fresh compilation/native behavior evidence and
  the same packed/installed discovery/body/applicability acceptance as A. Keep
  HTTP/2 downgrade R17, browser token binding and HTTP.sys hardening separate.
  No recurring native CI matrix, production credentials or resource publication.

Proposed **Agent**, existing **02 - Language and library guidance**, Backlog;
**L (5)** covers the managed/native authentication setup, real security failure
cases and installed guidance. A usable disposable Linux GSSAPI/TLS environment is
a delivery prerequisite; provision any owner-dependent credentials through the
normal private environment, never the repository. Refresh scoped overlap before
approval-based creation and reserve no prefix. Relate the approved ticket to
PIN-690 and preserve PIN-959/final runtime reconciliation prerequisites. PIN-831
remains unchanged with its explicitly Windows HTTP.sys scope. PIN-986 owns C's
approval, rather than leaving this new obligation with a completed source audit.

## Replay and remaining limits

[Inspect-AutomaticExclusions.ps1](../../../eng/Inspect-AutomaticExclusions.ps1)
downloads and checks the 34 source hashes and both package hashes, then reads
metadata without running inspected assemblies. Prerequisites: PowerShell 7,
HTTPS and the exact installed .NET 10/11 SDK roots/reference pack recorded above.
Use its root parameters for another installation. The following was run from
`C:\Windows` and selects this actual worktree explicitly:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-a63c4143'
& 'C:\Users\pingu\AppData\Local\Microsoft\WindowsApps\Microsoft.PowerShell_8wekyb3d8bbwe\pwsh.exe' -NoProfile -File ./eng/Inspect-AutomaticExclusions.ps1 `
    -OutputPath ./artifacts/automatic-exclusions.replay.json `
    -CompareWith ./docs/planning/audits/automatic-exclusions.assets.json
if ($LASTEXITCODE -ne 0) { throw 'Exclusion evidence replay failed' }
Get-Content -LiteralPath ./docs/planning/audits/automatic-exclusions.md |
    Select-String -Pattern '^\| `|^## [ABC]\.'
```

Expect seven asset records (three absent/four present attributes), exact baseline
match, thirteen row dispositions and three approval-only drafts. A changed source,
package or metadata baseline fails replay; do not overwrite the committed input
to manufacture a match. [Verification and criterion ledger](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-690.md)
distinguishes these results from the unchanged-host package smoke.

No proposed consumer was built or run. No native TLS/profiler/browser/workload,
numerical accuracy, RC1 output, GA or release claim is established. Earlier audit
unknowns remain with their named owners and PIN-986/PIN-959; the partial reversals
cannot be closed as recipe coverage merely by merging this source audit. No MCP
tool, served recipe, dependency, project, CI or update-policy change is made.
