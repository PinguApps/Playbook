# Native deployment-artifact migrations: PIN-687

Reviewed **22 October 2026**. This source-only audit resolves the three assigned
findings. Two additional delivery proposals await backlog approval. It creates no
recipe and does not complete comprehensive coverage or permit publication.

Delivery update, 29 October 2026: proposal A is implemented by
[PIN-1064](https://linear.app/pinguapps/issue/PIN-1064), stable
`deployment.control-runtime-overrides`, edition 11.0. Its original
TidalDiagnostics .NET 10/11 consumers carry current compilation and actual
isolated hostfxr/runtime execution records. SDK Release defaults, explicit dev
generation/copy, regeneration and a fresh production artifact allowlist are
measured rather than inferred from source. The historical proposal below remains
the source of its bounded acceptance; Unix delivery, unsupported static-library
availability and PIN-959/PIN-963's comprehensive/actual-GA gates remain separate.

## Boundary and provenance

[PIN-687](https://linear.app/pinguapps/issue/PIN-687), planning key `SH_Q07`,
inventory `migration.deploy-native-artifacts`; `SPEC.md` at
`0a8989814ea11346354464b845c79c15bb71c5e9`, stories 47, 103, 106, 158 and 159;
approved proposal `b3778cc`. Consulted the historical spec/proposal, current spec,
planning README, coverage/implementation/verification plans, glossary, ADRs
0001–0004, contribution/dependency policies, neighboring runtime, SDK and hosting
audits, and the actual assembly recipes, samples and evidence.

Root PLAN.md, CONTEXT.md and CONTEXT-MAP.md are absent. Historical planning claims
that implementation has not begun are stale: the tool and assembly guidance now
exist. No explicit target provenance pointer is unavailable. The optional Linear
shared-notes call returned `McpServerError: Tool get_agent_context not found`.

Linear boundary: PinguApps team `62f80270-b641-4fa3-b60c-46be4c4df2c8`, Playbook
project `b22380c7-6fde-49f4-8826-e5631dc5df7a`. An archived-inclusive, paginated
overlap query returned 317 issues, all checked against both IDs. Full PIN-742,
PIN-685 and PIN-959 were read after scoped discovery; PIN-742's handoff comment
was also read. PIN-742 prevents duplicate caller delivery; PIN-685 distinguishes
application configuration providers from runtime-host settings; PIN-959 keeps
unpublished qualifying goals visible. None of these related issues is changed.
PIN-687 has no native blocker. Its saved M (3) estimate is retained for this
bounded audit, source replay, coverage reconciliation and proposal preparation.

## Source evidence and limits

The [21-file manifest](native-deployment.sources.json) pins full-file SHA-256
hashes and immutable source URLs:

- `dotnet/docs` `f0021fde66c59559fa1ba3c9a044148738f12faf`: the complete compatibility
  index, all three full mitigation articles, runtime-configuration guidance and
  Native AOT overview, native-library and cross-compilation prerequisites.
- `dotnet/core` `ffed591b0fb8f9aab50ecc28f2c765049fb28d8f`: canonical runtime
  Preview 3, 6, 7, RC1 and RC2 files, cross-checked for the assigned changes and
  later supersession. Preview 3 records the prefix migration; Preview 7 records
  caller support and the stack-disabled failure. Absence of a repeated headline
  in later notes is not evidence that a migration was reverted.
- `dotnet/dotnet` `4099f83977f4d29604a9aeb971fb0a7bc1d5f699` /
  `820230c386db5b0106c5430b3e5aee92a7826bf5`: .NET 10 / .NET 11 RC2 implementations
  of `runtime_config_t`, native-output targets and CoreCLR/Native AOT
  `Assembly.GetCallingAssembly`. These source baselines match the repository's
  10.0.13 / 11.0.0-rc.2.26504.105 comparison family; they are not binary tests.

Read the full mitigation text, including introduction, old/new behavior,
recommendation and affected APIs, then inspected the relevant implementation
branches. The index explicitly remains incomplete. This is a bounded deployment
audit, not a full runtime/SDK API diff. No new consumer was compiled or executed;
source evidence must not be relabelled as native execution or GA verification.

## Dispositions

| Finding / introduction | Exact scope and authoring consequence | Outcome |
| --- | --- | --- |
| [Runtime configuration precedence][config-note], Preview 6 | A duplicate `runtimeOptions.configProperties` key in `<app>.runtimeconfig.dev.json` now wins over `<app>.runtimeconfig.json`. In both source baselines `ensure_parsed` reads the dev file first. The old `parse_opts` overwrites existing keys; the new version skips a key already present. Remove unintended dev duplicates or keep intentional overrides separate from production artifacts. The change is in the host, not a new managed API. | **Included → proposal A.** No delivered recipe covers this goal. This is not Microsoft.Extensions.Configuration provider order, appsettings.json, launch-profile precedence or a Native AOT runtime-setting contract. |
| [Unix Native AOT library prefix][prefix-note], Preview 3 | New targets apply `lib` when `UseNativeLibPrefix != false`, the output is non-executable and the target OS is not Windows. `NativeBinary` composes prefix + TargetName + extension. Migrate exact-filename copy/load/link scripts or deliberately set the public opt-out to false. Executables and Windows libraries are outside this default-prefix change. | **Included → proposal B.** Shared-library delivery needs actual Unix publication and consumption. Static `.a` naming is source-confirmed but supported static-library publication is **unresolved**; see below. No unavailable API is invented. |
| [Caller assembly with stack traces disabled][caller-note], Preview 7 | .NET 11 Native AOT can inspect stack-trace data for the caller. Both newer Native AOT and CoreCLR implementations throw NotSupportedException when stack traces are unavailable. Older CoreCLR lacks that guard; older Native AOT normally throws PlatformNotSupportedException. Enable stack traces, remove the lookup or handle an explicit unavailable result. | **Grouped → delivered PIN-742**, stable `reflection.inspect-loaded-metadata`, with `hosting.report-assembly-location` for the distinct location goal. Existing original samples and separate Windows native evidence cover the assigned case. No additional caller recipe or new ID is proposed. |

### Host configuration is not application configuration

`host-config-10/11` in the manifest confirm the same-key change only. Their
separate framework/roll-forward and probing-path branches do not establish a
universal precedence reversal. `ensure_dev_config_parsed` accepts an absent dev
file and comments that it is never bundled into a single-file app; only an
on-disk dev file is processed. This does not prove every publish command excludes
every manually supplied dev sidecar. Proposal A must inspect actual artifacts.

Keep generated configuration distinct from its authoring inputs:
runtimeconfig.template.json and RuntimeHostConfigurationOption items contribute
to generated runtimeconfig.json, which a later build can overwrite. Do not teach
editing that generated file as a durable source change. The general guide also
documents per-setting environment precedence since .NET 9; this audit does not
claim a single global ordering across all environment, host and AppContext paths.
Do not export test overrides to user or machine scope.

### Native naming has a support boundary

The [native-library guide][library-guide] officially supports shared libraries
only; it warns that static libraries are unsupported and may require a source
build. Thus the compatibility note's `.a` example and the `NativeLib=Static`
target branch establish naming behavior, not a supported SDK static-library
recipe. Preserve that distinction in proposal B, with a visible unresolved
static route for final source reconciliation rather than an invented workaround.

The older target uses internal `_UseNativeLibPrefix == true`; the newer target
uses public `UseNativeLibPrefix != false`. Existing custom builds could already
produce prefixed outputs. Do not call the old internal property a supported new
opt-in, or rename an already-prefixed artifact blindly. The default target branch
adds its prefix to TargetName; scripts and custom names need artifact inspection.

Native AOT does not support cross-OS compilation: Windows selecting a linux RID
does not establish Linux publication. Use a Linux environment (including a
suitable WSL/container/VM) with compiler/linker and C/zlib development inputs, or
a Mac with matching Xcode command-line tools for macOS. Record exact versions,
RID, architecture and native-library consumers. A Windows native executable smoke
cannot verify Unix `.so`/`.dylib` names. Static archives and iOS/Android/workload
composition are not silently added to a desktop Unix verification claim.

### Existing caller coverage and source qualifications

Reviewed the actual [recipe](../../../content/recipes/reflection.inspect-loaded-metadata/11.0/recipe.md),
DeploymentInspection comparisons, shared checks, both deployment.evidence.json
records and [PIN-742–744 report](../../verification/assembly-deployments.md).
All recorded input hashes in both deployment records match current files. Their
saved Windows x64 outputs cover:

| Publication | .NET 10.0.13 | .NET 11 RC2 |
| --- | --- | --- |
| Single-file CoreCLR, traces enabled | Caller returned; physical location unavailable | Caller returned; physical location unavailable |
| Native AOT, traces enabled | PlatformNotSupportedException fallback | Caller returned |
| Native AOT, traces disabled | PlatformNotSupportedException fallback | NotSupportedException fallback |
| CoreCLR, traces disabled | Caller returned | NotSupportedException fallback |

This audit checks saved evidence and source alignment; it does not rerun those
publications. The existing fallback catches NotSupportedException, including its
PlatformNotSupportedException subclass. Existing guidance already warns about
inlining/tail calls, excludes security identity, suggests explicit identifiers and
separates metadata-only loading from Native AOT caller inspection.

Both Native AOT source baselines contain a legacy
`Switch.System.Reflection.Assembly.SimulatedCallingAssembly` branch returning the
entry assembly before the normal behavior. This qualifies the note's word
"always": that opt-in is simulated entry identity, not real caller inspection or
a recommended mitigation. No source contradiction requires changing the existing
ordinary-route recipe. The selected new Native AOT implementation can also fall
back to the immediate frame, and can return null without usable assembly data;
do not promise a stable caller across arbitrary optimized execution paths.

Invisible AOT compiler/dispatch speedups are **excluded** as standalone goals:
upgrading alone obtains them. Explicit runtime-async, diagnostics and native
interop authoring retain their existing family owners; they are not new scope
for this three-finding audit. GA sources/artifacts and unsupported static-library
availability remain reconciliation work, not waived coverage.

## Approval-only delivery proposals

No new Linear issues or recipes are created. Both proposals belong in existing
**03 - Build and deployment guidance**, `7b3876e4-5f75-4b54-a375-cc598059679d`,
inside the team/project boundary above, normally Backlog, related to PIN-687 and
blocking PIN-959's comprehensive publication check. Native estimate convention is
S=2, M=3, L=5. Existing highest allocated title prefix is 03-012; recheck all
published and approved-but-unpublished proposals before numbering new tickets.
A/B are local draft labels, not allocated issue numbers. No milestone/name
changes are proposed.

Common acceptance for each proposal:

1. Write wholly original fictional-domain .NET 10/11 comparisons, offline-complete
   prose/metadata and canonical snippets. Keep actual feature requirements separate
   from exact verification baselines. Use SDK 10.0.402 / 11.0.100-rc.2.26504.105 and
   runtime/native packs 10.0.13 / 11.0.0-rc.2.26504.105 as the proposed measured pins;
   inspect and record actual restored assets. Apply dependency policy to isolated
   sample projects. Do not infer a down-level route from an SDK/package version.
2. Compile and run actual consumers for every claimed route; record commands,
   exact artifacts/toolchains, output and fresh input hashes. Missing platform
   access or behavior evidence blocks that delivery PR. A mock, property
   evaluation, renamed placeholder or no-op does not satisfy execution acceptance.
3. Build/pack and install the real tool; inspect explicit-target list/get from an
   unrelated directory, including ID/title/goal, complete rendered Markdown,
   applicability and evidence. Check canonical snippet identity and offline
   retrieval with the existing harness. Update the disposition register and
   preserve RC/GA and unverified-platform limits. No recurring native CI matrix.

### A — Preserve production runtime settings while allowing development overrides

Proposed goal ID `deployment.control-runtime-overrides`; estimate **M (3)** for
host-version isolation, generated artifacts, configuration cases and installed
guidance. No duplicate appeared in the scoped overlap search.

An original tidal-station report prints its selected diagnostic mode. Author the
production setting through supported source inputs, deliberately add a different
dev-sidecar value and show the same goal on both real host releases.

Specific acceptance:

- Pin and identify the actual hostfxr/runtime selected; a net10 target rolled onto
  an 11 host is not the old-host comparison. Use a harmless app-specific
  AppContext data key to observe the merged value, not a timing/performance proxy.
- With both files containing that key, demonstrate production wins on 10 and dev
  wins on 11; with the duplicate removed or the sidecar absent, production wins
  on both. Include a nonduplicate key to prove ordinary merging still works.
- Inspect build and publish directories, including an intentionally supplied
  sidecar; demonstrate that the chosen production artifact set cannot apply the
  development override. Document any dev-file generation/copy controls only after
  evaluating and running the actual SDK path. No universal publish-exclusion,
  Native AOT sidecar or application-provider precedence claim.
- Exercise durable source-input regeneration rather than only hand-editing a
  generated output. Use process-local settings and preserve all PATH scopes and
  persistent environment variables. A workload-free supported CoreCLR host is
  sufficient; no native compiler or external service is required.

### B — Keep Unix native-library packaging and callers aligned with output names

Proposed goal ID `deployment.consume-native-library-artifacts`; estimate **L (5)**
for two Unix environments, actual native consumers, default/opt-out controls,
original samples and installed guidance. No duplicate appeared in the scoped
overlap search; PIN-742's executable caller probe does not cover native libraries.

An original tide-window library exports a deterministic calculation using
UnmanagedCallersOnly with an explicit EntryPoint. A tiny C consumer and packaging
script consume the library produced by each release, rather than predicting its
filename from the managed assembly name alone.

Specific acceptance:

- Publish real shared libraries on Linux and macOS with matched SDK/native packs,
  Clang/linker, OS development inputs and explicit RIDs. Capture compiler/OS/SDK
  versions and exact artifacts. For macOS establish Mac/Xcode access before
  implementation; Windows-to-Unix RID selection is not a substitute. Keep
  Linux/macOS authoring checks outside routine desktop tool-smoke CI.
- For each environment verify old default, new default and new
  UseNativeLibPrefix=false. Expect unprefixed old/default and new opt-out names,
  and lib-prefixed new default `.so`/`.dylib` names with ordinary TargetName.
  An exact-old-filename packaging control must fail against the new default;
  migrated scripts must select the actual intended library without double-prefixing.
- Compile/link or load with the real native consumer, call the export and verify
  its numeric result for all three cases on both environments. Check exported
  symbols and packaged contents. Do not unload a Native AOT shared library:
  unloading is unsupported. Confirm executables/Windows are outside the prefix
  condition without claiming unexecuted platform behavior.
- State the static-library limitation explicitly. The source-confirmed `.a`
  naming branch remains an unresolved supported-delivery route, not a compiling
  consumer claim. Reconcile official support/source-build requirements at GA;
  if a supported additional authored goal emerges, draft it for approval before
  expanding this ticket. Do not silently drop the static finding from coverage.

## Verification and handoff

All three assigned findings have an included or grouped owner. A/B retain original
sample, real behavior, native access and publication obligations. Closing PIN-687
does not assert that A/B are approved, implemented or delivered. PIN-959 must
continue to see these gaps after the audit issue closes.

PowerShell 7 source replay below was exercised from `C:\Windows`; it downloads
each complete immutable file to a temporary file, checks its hash and leaves the
worktree unchanged. No SDK setup, build or execution of downloaded code occurs.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-ea2003dd'
$ErrorActionPreference = 'Stop'
$manifest = Get-Content -Raw -LiteralPath './docs/planning/audits/native-deployment.sources.json' | ConvertFrom-Json
$download = [IO.Path]::GetTempFileName()
try {
    foreach ($source in $manifest.sources) {
        Invoke-WebRequest -Uri $source.url -OutFile $download
        $actual = (Get-FileHash -LiteralPath $download -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($actual -cne $source.sha256) { throw "Source mismatch: $($source.id)" }
    }
    "PASS: $($manifest.sources.Count) immutable source hashes."
}
finally { Remove-Item -LiteralPath $download }
```

Expected: `PASS: 21 immutable source hashes.` Network access to GitHub raw content
is required. A mismatch or failed download is a failure, not source-review proof.
Local audit links, manifest identities, saved caller-evidence input hashes and
whitespace are checked separately. Ordinary repository package smoke results and
the final CI/review audit are recorded in the PR/Linear handoff. No MCP tool,
served recipe, project, dependency or workflow changes are made here.

[config-note]: https://learn.microsoft.com/en-us/dotnet/core/compatibility/deployment/11/runtimeconfigdev-configproperties-precedence
[prefix-note]: https://learn.microsoft.com/en-us/dotnet/core/compatibility/interop/11/nativeaot-lib-prefix
[caller-note]: https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/11/assembly-getcallingassembly-stacktracesupport-disabled
[library-guide]: https://github.com/dotnet/docs/blob/f0021fde66c59559fa1ba3c9a044148738f12faf/docs/core/deploying/native-aot/libraries.md
