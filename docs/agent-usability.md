# Bounded agent usability checkpoint

Run manually outside CI after the [isolated T3 setup](t3-verification.md).
This is separate from the four-task [with/without launch evaluation](planning/evaluation-plan.md).
The first [checkpoint receipt](verification/PIN-977.md) records actual outcomes;
a failed scenario is evidence, not permission to coach it into a first-pass success.

The [larger-catalogue extension](agent-usability-larger-catalogue.md) freezes the
second checkpoint's paging/neutral-guidance additions. Its [PIN-978 report](verification/PIN-978.md)
retains three passing scenarios and one failed recovery-filter check.

For the single PIN-1018 recovery follow-up, copy the current
[companion instruction](agent-instructions.md), including its recovery paragraph,
into a new seed revision; retain the earlier seed and receipts. Use the unchanged
D prompt and checks below for one fresh ordinary thread, bounded to five execution
minutes. Record the actual first retry's target, areas, languages and shared
setting against the empty request/suggestion, and any later narrowing separately.
Keep failures without coaching or rerunning. This does not rerun or relabel PIN-978.
See the [follow-up receipt](verification/PIN-1018.md).

## Prepare

1. Record the current source commit and clean production paths. Build/pack the ordinary
   tool with a unique unpublished package version. Install from a local-only feed into
   a fresh slot/cache using the setup guide; preserve earlier slots used by live threads.
   Set `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` for every .NET child. Compare process,
   user and machine PATH before/after; never assign PATH or restore shared settings.
2. Use the owner-authorized dedicated seed/project, never the product checkout.
   `eng/Prepare-AgentUsability.ps1` prepares an independent, package-free consumer,
   explicit empty build/package boundaries and the normal companion instruction.
   It refuses a dirty seed or existing consumer. Its default SDK pin is an exact
   reproducibility baseline, not an inferred product requirement. Review the dependency
   policy before changing that pin or adding packages. No root Renovate permission
   applies to these external, deliberately pinned verification consumers.
3. Build/run the baseline before timing any agent. Inspect and commit only the seed's
   connection, instructions, ignores and consumer. Record its SHA. To repeat, select
   a clean dedicated seed revision without `consumer/` (the original setup seed is
   `766aac805f7f8e9071cb69a2e4890e8d992ad4de`); preserve existing checkouts/work.
   The script does not select branches, commit or remove files.

The first checkpoint used this preparation from an unrelated directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-d85e439b'
& ./eng/Prepare-AgentUsability.ps1 `
  -Seed 'V:\_Scratch\PlaybookVerification' `
  -Playbook 'V:\_Scratch\PlaybookVerification\packages\pin977-1\tool\playbook.exe'
```

This preparation is intentionally once per clean seed revision; do not rerun it
against the retained prepared seed. Build using the absolute SDK 11 executable
from `consumer/`; run the built DLL using the system .NET 10 executable. Record
actual SDK/runtime versions and baseline assertions. Do not run an installer or
change the user's persistent environment to satisfy a scenario.

## Freeze four initial prompts

The seed's AGENTS.md supplies the normal Playbook companion consultation workflow
and local build instructions. Prompts below contain no recipe IDs, tool argument
objects or expected answers. Freeze them and the checks before launch. Do not put
the coordinating report, expected tool calls or passing answers in the test seed.

### A — application target versus build SDK

> Simplify StockLabel's property implementation in consumer/ by removing its explicit backing-field declaration while preserving its existing normalization and validation behaviour. Keep the application's existing target, language and build configuration. Consult the connected Playbook guidance as instructed, implement the change and verify the consumer. Briefly explain applicability and evidence limits.

Pass: real discovery/detail use the application target consistently; full detail
precedes implementation; the SDK 11 optional route is understood without treating
both routes as cumulative or exact verification pins as required installations.
The explicit field disappears; constructor/assignment/rejection/retained-value
checks pass on net10.0. No target/language/SDK upgrade.

### B — theoretical guidance without a target

> For a separate application whose target framework has not been chosen, explain how to represent three unrelated synchronization results as one type-safe closed set and persist it as JSON. This is exploratory guidance only: consult the connected Playbook, explain prerequisites and limitations, and do not edit this checkout or choose a target framework for that application.

Pass: deliberate catalogue-latest discovery/detail, target echoed consistently;
relevant full detail retrieved; unknown compatibility and RC/GA limits disclosed;
no files changed and no consumer compatibility claim. No compilation is required
for prose-only guidance; do not count illustrative prose as executed code.

### C — incompatible prerequisites

> Assess whether this consumer's current configuration can represent three unrelated synchronization result records as one language-enforced closed result type. Consult the connected Playbook. Keep the current target framework, language version and SDK pin; do not enable experimental features. Give a recommendation grounded in the consumer and retrieved requirements. Do not edit files.

Pass: real MCP consultation; project/SDK/language inspected; genuine incompatible
language/target prerequisites identified without quietly adopting latest or
changing configuration. Detail is required before recommending application of a
route. An unavailable recipe on this target may be reported honestly with coverage
limits; absence alone does not establish an impossible capability.

### D — keyword recovery

> Simplify StockLabel's normalization and validation property in consumer/ while preserving behaviour and all existing project constraints. Consult the connected Playbook; start your discovery using my phrase "tangerine warehouse property normalization". Implement and verify an appropriate simplification if guidance supports it. Briefly explain how you found and checked the guidance.

Pass: an actual empty keyword result; caller-controlled retry preserves its
original target and every non-keyword compatibility filter, with only query/page
correction as appropriate; detail before implementation; no exhaustive-absence
claim, invented results or upgrades. Build/run the same assertions as A.

## Launch, bound and inspect

- Discover `orchestrator_capabilities` and `t3_thread_configuration`; record one live
  client/provider/model/options for all scenarios. Record the app/client and provider
  versions where available. Do not add a native/client/model matrix.
- Launch each with `t3_thread_launch`, the dedicated project ID, explicit model
  selection and `workspaceStrategy:{type:"worktree",baseRef:"<prepared-seed-SHA>",
  branch:"verify/unique-scenario",startFromOrigin:false}`. Use the frozen prompt
  as `message`. These must be ordinary top-level threads, with no inherited answers.
  Retain each returned thread/run ID immediately. Inspect thread listings after an
  uncertain launch; never blindly retry and create duplicate work.
- Start the five-minute execution clock when the run starts; use bounded status
  waits and inspect durable activity. Stop a still-running scenario with
  `t3_thread_interrupt` before five minutes, allowing time for interruption. Retain
  the timeout/partial evidence. Sum actual execution durations, at most 20 minutes;
  preparation, owner access and coordinator inspection are outside that budget.
- Verify actual `playbook.list_recipes`/`get_recipe` activity and complete returned
  Markdown/applicability in each. If unavailable, stop dependent checks and report
  setup blocked; shell/Inspector/mock calls do not satisfy the real-agent criterion.
- Read `t3_thread_read(view:"activity")`, follow `nextPosition`, and recover each
  truncated item's `itemId`/`nextTextOffset` until complete. Save real arguments,
  results, final answer, generated diff, timings and tool output sizes. Distinguish
  serialized activity size from protocol payload size; record token usage only
  when exposed. Compile/run generated consumer code independently. Check unchanged
  project/global/build files and persistent PATH after the runs.
- Save outcome per criterion, including failures/timeouts and unverified claims.
  Link justified narrow product/setup defects after a scoped Linear overlap search;
  leave them unresolved and keep failed scenarios failed. No corrective prompt or
  rerun belongs to this first-pass checkpoint. Keep original worktrees and durable
  activity while evidence is under review; archive/clean only those test resources
  later using the guarded [setup cleanup](t3-verification.md#cleanup).
