# Manifest reconciliation preparation (PIN-949)

Evaluator material: never expose this directory, repository/history, recipes or
paired outputs to a run. Export only samples/ManifestEvaluation at scaffold commit
5ad10da77aba1bf658a2308f7d9d39cae108e2b0 to two independent clean
directories. Supply TASK.md verbatim to each fresh context. The baseline builds
but deliberately returns no rows. Preparation records no agent evaluation results.

## Frozen controls

SDK 11.0.100-rc.2.26504.105, net11.0, C# 15.0. No NuGet dependencies,
workloads, services or provider. The export carries its own build, package and
empty-feed boundary; repository analyzers do not apply. Keep these inputs fixed.
Prepare restore equally before either clock starts. Use the same machine, absolute
dotnet executable, shell and file/build tools. Network off for both conditions;
only the with condition receives the frozen offline Playbook package and companion
instruction. No repository searches outside the export. Never mount evaluator files.

Ten wall-clock minutes per fresh context including builds/checks. Retain timeouts
and partial outputs; no retries or corrective prompts. With condition first for
this fourth task, alternating the preceding without-first grid task. At execution
freeze one identical available client/model/version/settings across all eight
runs, package version/SHA-256 and companion instruction. Record actual tools,
network restrictions, elapsed time and tokens when exposed; differing controls
cannot be called a controlled comparison. Later runs belong to PIN-954.

## Private oracle

Test-Oracle.ps1 copies a submitted export into a disposable directory, replaces
Program.cs with Oracle.cs, and runs the pinned project. Preserve original submissions
and diagnostics; never repair them. Score each printed assertion independently.
The reference switch is only for preparation, never for scoring agent output.
The baseline must build and fail the oracle; Reference.cs must pass all ten checks.

Keys use ordinal case-sensitive equality with no normalization. Each manifest has
unique keys: duplicate keys must throw ArgumentException, including identical
unmatched duplicates. This is an application validation rule: FullJoin itself
produces all matching pairs for duplicates, so the reference validates first.
Rows preserve both quantities, use null for an absent side, retain real zero and
equal matches, and sort by ordinal key rather than join enumeration order.
Null/empty keys, negative quantities and null arrays are outside this bounded task.
The oracle compares typed rows, not formatted JSON. A dictionary or older LINQ
implementation may pass every correctness assertion without newer-API adoption.

## Applicability and scoring

Expected recipe: linq.reconcile-all-rows, edition/target 11.0; in-memory
Enumerable.FullJoin from the framework System.Linq reference/runtime assembly.
No extra package, opt-in, UI, workload or database is needed. Reference.cs was
compiled and executed against the actual pinned RC2 artifact, proving the
comparer/result-selector overload and nullable missing-side handling. This is
RC2 evidence, not GA verification. Queryable/async/provider translation is outside
this task. The existing recipe's previous-release implementation remains valid.

Record consultation (actual list/get calls), behavioral correctness, target
applicability, newer-API adoption, unnecessary code, time, tokens and limitations
separately. Do not award correctness merely for consultation or FullJoin usage.
No comparison outcomes are supplied here. SPEC stories 155–157 and 160, PIN-734,
and docs/planning/evaluation-plan.md constrain this preparation.

From an unrelated PowerShell directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-380759ad'
& ./docs/evaluation/manifest-reconciliation/Test-Oracle.ps1 -Submission ./samples/ManifestEvaluation -Dotnet 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe' -Reference
# Expect ten PASS assertions and zero failures. Omit -Reference to score a submission.
```
