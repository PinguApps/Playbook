# Agent usability initial evidence

**21 October 2026. Four first-pass scenarios passed; 255.721 seconds summed
execution (4m 15.721s), no timeout, coaching or corrective rerun.** This is one
client/model checkpoint, not launch evaluation, comprehensive coverage, GA proof
or a model comparison. [Repeat instructions and frozen prompts](../agent-usability.md).

## Candidate and scope

- Production source: `e89df1b0ce950acdc10fe4112a78f7bab9cf1b4c` (fresh `origin/main`).
  Production source/content/build configuration was unchanged when packed.
- Installed local-only package: `PinguApps.Playbook` **1.0.0-preview.977.1**;
  `--version` returned `1.0.0-preview.977.1+e89df1b0ce950acdc10fe4112a78f7bab9cf1b4c`.
  SHA-256 **068501b507e4e4ba8f099f855b15c2816c1e4e8d247f15443bb0a05cd0441327**.
  Artifact: `artifacts/pin977/feed/PinguApps.Playbook.1.0.0-preview.977.1.nupkg`.
  New install/cache slot: `V:\_Scratch\PlaybookVerification\packages\pin977-1`.
  Earlier PIN-976 slots and existing sessions were preserved.
- Real installed-server Inspector discovery (`latest`, page size 50) returned
  **49 distinct ordinary persistent recipe IDs**, one edition per ID, no fixture
  IDs and no next page. There are **44 stable-status and 5 experimental-status**
  recipes; experimental guidance still has stable identity. The
  [complete installed inventory](agent-usability-initial.catalogue.json) records ID/title/goal,
  introduction, supported targets and stability. Generated catalogue identity/count
  agrees. “Stable ID” is not a claim of stable API status.
- The seed plus PIN-711–721 delivered **26 goals**, rather than the expected
  roughly twelve: seed 1; closed C#/JSON outcomes 4; collection authoring 3;
  enclosing-loop control 1; interface helpers 1; unsafe boundaries 1; F# models 3;
  F# API boundaries 4; F# diagnostics 2; F# async 3; enum migration 1; process
  capture/lifetime 2. Subsequent process/buffer/Unicode/binary work adds 12 and
  JSON workflows add 11. No recipes were manufactured or removed to reach a count.
  This delivery drift does not replace the later PIN-978 checkpoint.

## Environment and isolation

T3 environment `PINGU-SERVER`, `e161a58f-5feb-4993-b10f-5dc13775bd35`, app/server
`0.1.1-nightly.20261019.3348`, Windows x64; Codex CLI **0.163.0**; PowerShell
**7.6.7**. Live `orchestrator_capabilities` and `t3_thread_configuration` selected
instance **codex**, model **gpt-6.1-sol**, **high** reasoning, **default** service
tier, full-access/default modes for every scenario. No other model was tested.

The PIN-976 owner-authorized project is
`mcp:cb660864-7bf7-43f9-b205-1a54c5e763cf`, seed
`V:\_Scratch\PlaybookVerification`. Preparation committed only its local MCP
connection, companion instruction and independent consumer at
**0afb73afe4d96a82a1d2751ed7417600d3dd3f17**. Every launch explicitly selected
that local SHA with `startFromOrigin:false`, its own branch/worktree and the
same model options. Durable state confirms ordinary top-level threads,
`parentThreadId:null`, one run each and no inherited conversation.

The consumer has empty explicit build/package boundaries, no PackageReferences,
`net10.0`, C# `14.0`, warnings as errors and exact SDK baseline
**11.0.100-rc.2.26504.105**, roll-forward disabled. Actual compilation used the
separate SDK 11 executable; execution used system .NET **10.0.13**. The unchanged
baseline passed constructor and setter normalization, blank rejection through
both paths, and preservation of the prior value after rejected assignment.
No workload, experimental opt-in, installer or persistent environment change was
needed. Preparation/build/install and post-run process/user/machine PATH comparisons
matched; shared settings were not restored from snapshots.

## Actual first-pass results

Each evidence JSON retains the original prompt, exact thread/run/item IDs,
complete actual MCP inputs/outputs, answers and file changes. Tool outputs were
recovered without truncation through T3's durable activity API. Timings below
are actual run start-to-completion, including all agent reads/builds; preparation
and coordinator inspection are excluded. No run approached the five-minute cap.

1. **A — PASS, 75.928s.** [Thread](t3-thread:/v1/mcp:3e714e16-41e4-428a-a855-e171a3b4ee1f),
   [full evidence](agent-usability-initial.a.json), `verify/pin977-a`.
   Positions **4/6** show discovery `query=field`, target `10.0`, then full
   property detail with the same target. Both resolve `10.0`, edition `10.0`.
   The answer distinguishes the optional SDK 11 route, separate alternatives,
   baseline versions and RC/GA limits. Only `consumer/Program.cs` changed:
   compiler-managed `field` storage replaces the explicit field; constructor
   validation and the `UNASSIGNED` initializer remain. Agent before/after builds
   and checks passed. Independent coordinator build/run also passed, zero
   warnings/errors, with project/global/build/package files unchanged.
2. **B — PASS, 53.259s.** [Thread](t3-thread:/v1/mcp:c85296a7-5357-4d78-bc66-11beff2090cf),
   [full evidence](agent-usability-initial.b.json), `verify/pin977-b`.
   Positions **4/5/6** discover unions and retrieve both C# outcome and JSON
   detail using deliberate `latest`; every result resolves `11.0`. The answer
   states the application target is unknown, identifies net11/C#15 requirements,
   default/null storage, structural JSON ambiguity and RC-only evidence. It does
   not assert consumer compatibility or select a target. The checkout is clean.
   Its illustrative snippets were not compiled; no consumer code was changed.
3. **C — PASS, 56.919s.** [Thread](t3-thread:/v1/mcp:1d3f8156-c73e-485e-a534-087bf7d78e88),
   [full evidence](agent-usability-initial.c.json), `verify/pin977-c`.
   Positions **5/6** search target `10.0`, encounter an empty union search and
   broaden keywords without changing compatibility. Positions **7–10** explicitly
   inspect `11.0` guidance and the failed `get_recipe` at `10.0`
   (`no_applicable_edition`). Detail is retrieved before the recommendation;
   an identical successful detail call is repeated once. The answer uses actual
   runtime/language prerequisites, not absence alone, to reject a native closed
   union for this fixed net10/C#14 consumer despite its SDK 11. It recommends a
   defensive older wrapper or deferral, without an upgrade or experimental opt-in.
   Both target-specific list/get paths are consistent; the checkout is clean.
4. **D — PASS, 69.615s.** [Thread](t3-thread:/v1/mcp:c57dede3-e9fb-4212-8265-ce61f58489e5),
   [full evidence](agent-usability-initial.d.json), `verify/pin977-d`.
   Position **5** uses the exact frozen phrase and target alias `10`, returning
   `total=0`, incomplete coverage and a safe no-keyword suggestion. At **7** the
   agent chooses a no-keyword retry with the same target and effective area,
   language and shared defaults; it increases page size from 20 to 50. This is
   compatible recovery, not an exact replay of every suggested paging argument.
   Position **8** retrieves full property detail at target `10` before editing.
   It makes the same scoped implementation as A, without an exhaustive-absence
   claim. Agent before/after and independent coordinator builds/checks pass with
   zero warnings/errors; project configuration is unchanged.

No product/setup defect was demonstrated. C's redundant detail retrieval and D's
changed page size are retained observations, not failures or an invented backlog.
No follow-up was created; no related Linear issue was changed. This small sample
does not establish behaviour for explicit non-default recovery filters, F#,
native workloads, other models/clients or all 49 goals. Existing installed
contract tests, rather than these four agents, cover broader filter combinations.

## Sizes and verification

Measured sizes are **UTF-8 bytes of compact reserialized actual tool `output`**,
not MCP framing, duplicated text fallback, provider tokens or full activity bytes.
Token usage is not exposed by these T3 read/run responses and is unavailable.

- A discovery/detail: **2,847 / 38,211**; Markdown **6,286 characters**.
- B discovery/C# detail/JSON detail: **6,409 / 27,103 / 27,421**;
  Markdown **8,359 / 8,572 characters**.
- C discovery/retry/new-target discovery: **1,303 / 5,238 / 6,407**;
  incompatible error output **438**; two detail outputs **27,101** each,
  Markdown **8,359 characters** each.
- D empty/retry/detail: **1,336 / 5,236 / 38,209**;
  Markdown **6,286 characters**.

Completed verification: Release build/pack of the real ordinary tool (zero
warnings/errors), isolated local-feed installation/version/hash, Inspector
2.11.0 tools/list and installed catalogue discovery, four real-agent runs,
independent A/D builds/runtime assertions and unchanged-configuration checks,
B/C clean-worktree checks, and complete receipt inspection. The preparation
helper reproduced the frozen seed from a clean old seed clone when invoked from
`C:\Windows\Temp`; its dirty-seed refusal also passed. Both
`eng/Test-DependencyPolicy.ps1` and `eng/Test-Renovate.ps1` passed: 129 extracted
dependencies, 88 protected sample entries, strict configuration and synthetic
future-package/project/release checks. No root dependency permission changed.

To independently check the changed consumer behaviour again (SDK/runtime already
installed; each command exits):

Expected: two successful builds with zero warnings/errors and twice
`PASS: constructor, assignment, rejection and retained value`. A compiler error,
failed assertion or framework/language/SDK change is a failure. This block was
also checked from an unrelated directory. Keep all four evidence worktrees,
seed and candidate slot during PR review; use the existing guarded cleanup
afterwards. They are isolated test resources, not running product services.
