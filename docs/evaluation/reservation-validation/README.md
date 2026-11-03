# Reservation validation comparison preparation (PIN-947)

Evaluator material: never expose this directory, the repository/history, recipe
sources or paired outputs to either run. Export only `samples/ReservationEvaluation/`
at commit `64908b8c92ac186b722348371746656b35618568` into independent clean directories.
Give TASK.md verbatim to both fresh contexts. The original scaffold deliberately
accepts invalid input. This preparation creates no agent evaluation results.

## Frozen controls

- SDK 11.0.100-rc.2.26504.105, net11.0, C# 15.0, Web SDK. No NuGet packages,
  workloads, accounts or external services. Isolated build/package/feed files stop
  root inheritance. Use the SDK's aligned ASP.NET Core reference/runtime assets;
  do not add a redundant Microsoft.Extensions.Validation package. Restore equally
  before either clock starts. Keep the SDK archive SHA-512 and installed runtime
  inventory with the eventual run records.
- Same Windows machine, absolute dotnet executable, shell and file/build tools.
  Network off except loopback for both; only the with condition receives the same
  frozen offline Playbook package and companion instruction. No repository access
  outside the export, no evaluator files or corrective follow-ups.
- Ten wall-clock minutes per context including builds/checks. Stop and retain
  partial output/timeouts, with no retry. Run with first for this task, alternating
  the C# task's order. The eight fresh contexts belong to later execution tickets.
- At execution freeze one available client/model/version/settings across all eight
  runs, Playbook version/hash and companion instruction. Record actual restrictions,
  tools, elapsed time and tokens where exposed. Differences invalidate a controlled
  comparison; no model or package version is invented by this preparation.

## Private behavior oracle

Run Test-Oracle.ps1 against a submitted export. It copies source into a disposable
directory, builds with the submission's pinned SDK, starts actual Kestrel on a
loopback ephemeral port and observes the unchanged availability service/handler.
Keep the original submission and console diagnostics. Each assertion is scored
separately: required code, party limits, field errors, unavailable rejection, held
valid/invalid requests unfinished before release, one valid acceptance afterwards,
and client cancellation reaching the rule without later acceptance. An exception,
timeout or setup/build failure is not a pass. Model-level declaration is inspected
separately; controls and service must remain unchanged. Never repair a submission.

`-Reference` replaces only the disposable model with Reference.cs and registers
validation. This is a calibration solution, never an agent run. The actual RC2
API uses AsyncValidationAttribute.IsValidAsync returning Task<ValidationResult?>,
AddValidation and generated metadata for ValidatableType. The synchronous override
throws. This checks generated endpoint registration through HTTP, rather than
calling an attribute directly. Service failure/atomic allocation are outside scope.

An older awaited filter/custom validation adapter can pass behavior; inspect that
its availability rule is declared alongside the model rules. Do not require the
new attribute or generated pipeline for behavioral correctness. Record separately:
consultation, behavior, applicability, newer-API adoption, unnecessary code, elapsed
time, tokens and limitations. A passing reference proves the oracle's positive
route, not broad correctness of every possible implementation.

## Expected guidance

`minimal-api.await-model-rules/11.0` is primary: asynchronous model rules before a
Minimal API handler, field-keyed errors and request cancellation. The shared
`validation.validate-asynchronously/11.0` guidance applies to the underlying
DataAnnotations rule. Blazor form/field guidance is not required for this HTTP-only
task. Inspect actual packaged applicability at execution and retain the response.
RC2 compilation/execution is not GA evidence. PIN-774's real canonical consumers,
SPEC stories 153/156/157/160 and docs/planning/evaluation-plan.md constrain this task.

From PowerShell 7 (supply an intact pinned SDK executable):

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-bdbbc97f'
& ./docs/evaluation/reservation-validation/Test-Oracle.ps1 `
  -Submission 'V:\_worktrees\Playbook\t3-bdbbc97f\samples\ReservationEvaluation' `
  -Dotnet 'V:\_worktrees\Playbook\t3-bdbbc97f\artifacts\sdk\rc2\dotnet.exe' -Reference
```

Expect zero failed assertions with `-Reference`; omitting it must fail against the
unsolved scaffold. For actual scoring omit `-Reference` and point Submission at the
preserved exported submission. Keep evaluator preparation outside both contexts.
