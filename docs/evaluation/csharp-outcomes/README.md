# C# comparison preparation (PIN-946)

Current comparison controls: [the owner-approved protocol](../comparison-protocol.md) refreshes these scaffolds to RC3. The RC2 pins and export commits below describe the original preparation only; use the refreshed committed revision for the next comparison.

Evaluator material. Never mount this directory, repository history, recipe sources
or either condition's outputs in a run. Export only `samples/OutcomeEvaluation/`
at scaffold commit `80e41846f1a7c43d0f606978aed571e08e09f219` into two independent clean directories. Supply
TASK.md verbatim to both fresh contexts. The baseline intentionally builds but
does not solve the task. This preparation records no agent evaluation results.

## Frozen controls

- SDK 11.0.100-rc.2.26504.105, net11.0, C# 15.0; no NuGet dependencies,
  workloads or external services. The scaffold has its own build/package/feed
  boundary; root analyzers and package settings do not apply after export.
- Prepare restore equally before starting either clock. Use the same Windows
  machine, absolute dotnet executable, shell and file/build tools. Network off
  for both runs; only the with condition gets the same frozen offline Playbook
  package and companion instruction. No repository search outside the export.
- Ten wall-clock minutes each, including compilation and checks. Stop and retain
  timeout/partial output. No retries or corrective prompts. Without first for this
  pair; alternate order for the next task. Other tasks and all eight fresh contexts
  belong to later tickets.
- At execution freeze one identical available client/model/version/settings for
  all eight runs, package version/hash and companion instruction; record elapsed
  time, tokens if exposed, tools and actual network restrictions. Do not claim a
  controlled run if these differ. Client selection is execution preparation, not
  an evaluation result supplied here.

## Private scoring

Copy Oracle.cs over Program.cs in a disposable copy of each submitted solution,
then build and run using the pinned SDK. Keep the submitted original. Score each
assertion separately and keep actual diagnostics; do not repair the submission.
The same checks apply to both conditions. Do not compare JSON text/property order.
Rejection may throw an exception or return a Rejected outcome; an absent result
does not count as rejection. The oracle prints every assertion and a failure total.

Additionally test an ambiguous document in the solution's chosen JSON format
(two case discriminators/payloads), and verify rejection. Review the actual public
result type: attempts to pass an unrelated string/object to Describe or construct
an external fourth case must fail compilation. Generate those probes against the
submitted type, preserving the same SDK/options, and save source and diagnostics.
This adaptation identifies the submitted interface; it must not change scoring.
An object parameter or publicly extensible hierarchy fails closedness even if the
runtime oracle passes. Matching must preserve every case; explicit default/null
handling must not be presented as compiler-enforced exhaustiveness.

Record independently: consultation, behavioral correctness, applicability,
newer-API adoption, unnecessary code, elapsed time, tokens and limitations.
Correct sealed hierarchy/tagged implementations can pass behavior and closedness
without C# 15 adoption. No adoption points are required for correctness.

## Expected guidance and actual compiler boundary

Expected IDs/edition: csharp.closed-results/11.0, csharp.closed-hierarchies/11.0,
json.serialize-closed-results/11.0 and json.infer-closed-polymorphism/11.0.
The latter two apply only to the chosen union/hierarchy serialization route.
The pinned RC2 compiler supports union/closed syntax; the repository's real
ClosedOutcomes sample and Test-ClosedOutcomes.ps1 exercise CS8509 for omitted
cases, CS9382 for an external descendant and SYSLIB1243 for inference on an open
type. These are applicability probes, not compulsory implementation choices.
Default/null remains explicit. Source-generated JSON metadata must include the
selected route; runtime opt-in cannot repair omitted generation. This is RC2
evidence, not a GA claim. PIN-711 and SPEC stories 152,156,157,160 constrain this
task; docs/planning/evaluation-plan.md supplies the shared exercise controls.
