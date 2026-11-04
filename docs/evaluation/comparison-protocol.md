# Single-client comparison protocol

Pingu approved these decisions for PIN-950 on 4 November 2026. This freezes the
procedure for PIN-951–954; it records no scored runs. Pingu approves the final
assessment under PIN-955. Source: SPEC.md at
`0a8989814ea11346354464b845c79c15bb71c5e9`, stories 151–157 and 160,
and [the evaluation plan](../planning/evaluation-plan.md).

## Fixed agent and inputs

Use standalone Codex CLI **0.166.0**, model **gpt-6.1-sol**, medium reasoning.
The verified Windows x64 executable SHA-256 is
`9D563EE4E0441FF9D59D612013E0460DDF8E5709735C2D9F6B98B7EF8CA4EB42`.
T3 coordinates preparation and capture; its normal child worktrees do not enforce
the required read/network boundaries. Do not use them as the scored runner.

Refresh to the latest available SDK and compatible packages immediately before
the first scored run, including previews needed by these .NET 11 tasks. Revalidate
affected scaffolds, oracles and applicability against actual artifacts. Commit the
new pins and record the resulting full scaffold commit before exporting either
condition. Freeze those inputs for all eight runs; an update arriving midway waits
for a future comparison. No automatic dependency updates or floating pins are added.

The current preparation uses SDK **11.0.100-rc.3.26522.148**, net11.0 and C# 15.0.
GridLinksEvaluation uses QuickGrid **11.0.0-rc.3.26522.148** and its committed lock;
the other three scaffolds have no explicit package dependencies. Their empty
build/targets and central-package boundaries remain isolated from root settings.
This is RC3 evidence, not GA verification. The SDK zip SHA-512 is
`dae0a0450d92b5315034db134259acffe9ec0043d93dced4fb2a9b8700520e87a99005d6845ad80716f4a8f30df24ae5363ca7c9be391797b4f53170e9b2c088`.
Resolve updates from Microsoft's release metadata and the NuGet package index;
record lookup time, URLs, archive hashes and installed runtime inventory.

The refreshed scaffold commit for all four tasks is
`6d53662cdba54551a74b3ade063c49009c63561c`. This exact commit replaces the four
original export commits for the current comparison preparation. Retain/fetch this
PR's head before exporting it in a fresh clone if the PR is squash-merged.

Build one offline Playbook package from one recorded, reviewed repository commit.
Record its version, SHA-256, source commit and installed launch command. Retain
that artifact for all four with-condition runs; do not rebuild between runs.
Retrieve and retain actual list/get responses and applicability from that package.
Earlier recipe compilation records remain observations about their original
artifacts; this refresh does not relabel them as RC3 proof.

The preparation package is `PinguApps.Playbook` **0.1.0-pin950**, built from the
unchanged package/content inputs at reviewed base commit
`b60a5e0e9aad7993ef037e02d0184ffb79720dac`, SHA-256
`9A96E01473994BB854BB93D49C4E8B4CFE90805003DC249D7A7BE60505F2243C`.
It is retained locally at `artifacts/protocol/package/` in the task worktree until
the execution preparation copies it into the durable evidence bundle. A missing
artifact is a preparation blocker: do not silently rebuild and reuse its hash.
If a newer reviewed package is selected before the first run, record its identity
and recheck actual discovery/applicability for all four tasks before freezing it.

## Order and isolation

### PIN-952 relocation amendment

Pingu authorized accepting a verified relocation of identical SDK assets after
the PIN-952 with-condition launch and before its without-condition launch.
The original preparation directories had been removed. All 5,467 files from the
SDK ZIP identified above match the installed files byte for byte under
`C:\Users\pingu\AppData\Local\dotnetup\dotnet`; the ZIP's SHA-512 also matches.
The exact-hash retained Playbook package was extracted without rebuilding.
Actual relocated SDK/server paths, configurations and the SDK identity receipt are
retained in the reservation evidence. Additional installed SDK versions are not
selected by the scaffold's exact global.json pin.

This amendment accepts the recorded relocation for this pair. It does not
authorize a replacement run, change the model or inputs, or remove the requirement
to disclose the post-launch amendment. The original with attempt remains the
sole scored with attempt. Both reservation conditions use the same relocated
configuration and passed their MXC preflights before either scored launch. The
with scored attempt used the installed SDK path before relocation was accepted;
identity verification and Pingu's conditional amendment preceded the without launch.

### PIN-953 relocation amendment

Before either grid scored launch, Pingu approved using the same relocated SDK
accepted for PIN-952: C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe.
Reverify the frozen ZIP and installed file identity, retain actual SDK/server
paths, and pass both exact-context sandbox preflights before scoring. Extract the
same retained exact-hash Playbook package without rebuilding. This extends the
relocation permission to this pair; it changes no model, scaffold, budget or
one-attempt rule. Retain the identity and preflight receipts with grid evidence.

| Runs | Task/export subtree | First | Second |
| --- | --- | --- | --- |
| 1–2 | C# outcomes: samples/OutcomeEvaluation | without | with |
| 3–4 | Reservation validation: samples/ReservationEvaluation | with | without |
| 5–6 | Static grid links: samples/GridLinksEvaluation | without | with |
| 7–8 | Manifest reconciliation: samples/ManifestEvaluation | with | without |

Use one committed revision of each subtree for both conditions. The preparation
READMEs describe the original RC2 captures; use the RC3-updated revision recorded
by this protocol's preparation, or its explicitly revalidated successor. Never
export those older commits while claiming the refreshed SDK/package controls.

Export only the scaffold subtree into eight independent directories outside the
repository, without .git, evaluator files, recipe sources, previous outputs or
shared writable caches. Supply TASK.md verbatim. Each context starts fresh, with
no resume, fork, conversation memory, corrective prompt or exposure to its pair.
Prepare equal dependency caches and restore before starting either clock (locked
mode for the grid). Record hashes of exported files, prompt and resolved packages.

Use the same Windows machine, shell, absolute SDK executable and file/build tools.
Record their versions, hashes, arguments and effective configuration. Preserve
PATH and persistent environment settings. Set DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0
in every .NET child, including isolated CLI homes. Compare saved user/machine PATH
before and after preparation/checks; report unexpected changes without overwriting
shared settings. Keep per-run temporary/build directories independent.

Ignore ambient user configuration and rules; disable inherited instructions,
skills, hooks, plugins, search, apps and unrelated MCP connections. Record the
actual model-visible tool inventory and effective settings in each context.
Only the with condition receives the frozen stdio Playbook server and this exact
additional instruction:

> Before implementing, consult Playbook for guidance relevant to the task and target framework. Apply only guidance whose applicability matches the project.

Everything else, including developer/task instructions and tool permissions, is
equal. Do not name the expected recipe or API in either task prompt. Authentication
is available to the runner's model transport, never exported into the scaffold or
retained evidence. Keep Playbook binaries outside the baseline's readable roots.

## Mandatory preflight before execution

Use Codex's native Windows **MXC** sandbox with a restrictive permission profile.
Allow reads only for the export and necessary platform/SDK/cache paths; allow
writes only for that run's workspace and private temporary/cache locations.
Use the command network proxy with only literal loopback destinations allowed;
deny direct external sockets as well as proxied HTTP. Model transport remains
outside the command sandbox. Never fall back to full access or instruction-only
restrictions. See the official [permissions](https://learn.chatgpt.com/docs/permissions)
and [Windows sandbox](https://learn.chatgpt.com/docs/windows/windows-sandbox) guidance.

Before any scored run, retain successful probes under the exact effective runner
configuration for both conditions:

- A harmless model request succeeds with the frozen model/settings.
- Shell execution, SDK build, application loopback and file editing work.
- External HTTP, direct IP sockets and proxy-bypass attempts fail; a host-side
  control confirms the destination was reachable, so an outage is not a pass.
- Reads of repository, evaluator, other-run output and unrelated user configuration
  fail. Probe actual paths and alternate accessible checkout locations, not merely
  one marker. Baseline cannot read Playbook binaries or recipe sources.
- Only the intended tools/instructions appear; the with condition can consult the
  actual installed package. Stop if the model, configuration or tool list differs.

Preparation observed MXC launch, repository-read denial, loopback success, external
HTTP/direct socket denial and a separate model connectivity success. These are
capability probes, not proof of the final eight-context configuration. Full preflight
must pass after export/configuration and before the execution tickets start clocks.
If it fails, resolve preparation without consuming a scored run; do not claim a
controlled comparison or silently change runner/model. No scored prompt is used
during preflight.

## Budget and stopping

Run sequentially. Start a monotonic wall clock when the runner is launched with
the scored prompt, including model latency, tool calls, compilation and its checks.
Stop at completion or **600 seconds**, whichever comes first; an external supervisor
terminates the runner and all owned child processes at the deadline. Retain partial
files/events and mark timeout. Cap the eight runs at **4,800 seconds** total.
Preparation and subsequent evaluator review are outside this budget. Use identical
model settings without a separate token cap; capture usage if exposed.

There is one attempt per condition per task. Preserve errors, crashes, timeouts and
unsolved submissions. No corrective retries, follow-ups, model matrix or replacement
run. A control failure discovered after launch is retained and explicitly invalidates
the affected comparison; it does not authorize a ninth run.

## Evidence and assessment

Retain a durable, versioned bundle under `docs/evaluation/evidence/<comparison-id>/`
and link it from the final concise results summary. Never mount it in a run. Include
the frozen control manifest, source/export commits, input/config/tool hashes, package
identity, preflight results, run order/IDs, exact prompts, sanitized JSONL transcripts,
tool calls/results, submitted source and diff, elapsed timestamps, termination reason,
available input/output/cached token counts, evaluator sources/outputs and limitations.
Record unavailable usage as unavailable, not zero. Retain necessary original source
and diagnostics, excluding duplicate setup logs, credentials and unrelated personal
data. Ordinary disposable output stays in ignored artifacts/.

Score each assertion independently using the unchanged private oracles in disposable
copies; preserve submissions and never repair them. Apply the task README's interface
probes and manual review too, including JavaScript-disabled browser navigation for
the grid. HTTP replay alone does not prove browser history behaviour. Calibrate
unfinished scaffolds to fail and reference solutions to pass before scoring.

Report each of these separately, without a combined score or required Playbook win:

- **Consultation:** actual list/get calls, returned IDs/edition and applicability;
  distinguish availability from consultation and use.
- **Correctness:** build result, each behaviour assertion, invalid/missing cases,
  unchanged task constraints and failures/timeouts.
- **Applicability:** submitted target, dependencies, opt-ins and actual artifact
  support; identify unsupported assumptions separately from behaviour.
- **Newer-API adoption:** exact API and supported usage, independent of correctness.
  A correct older implementation can pass all behaviour checks.
- **Unnecessary code:** concrete redundant logic/dependencies or unrelated edits,
  with examples; do not penalize necessary code simply for using an older approach.
- **Time/tokens:** actual elapsed execution and exposed usage, with missing metrics
  and accounting limits stated explicitly.

Automated checks establish their observed assertions. Agent-assisted review assesses
qualitative findings; Pingu approves the final assessment. State exactly what ran
and remains unverified. Eight bounded observations on one model do not establish
broad model knowledge, general superiority, GA readiness or release authorization.
