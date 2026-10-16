# Manual product evaluation

Status: the small manual with/without comparison is accepted in Round 6. The four candidate tasks and execution budget below are Round 7 proposals. No scaffolds, evaluation checks, or agent runs have been created.

## Proposed tasks

Use four small, independently authored scaffolds, each addressing an accepted recipe goal. Task prompts describe developer outcomes without naming a new API or recipe.

1. **C# outcome modelling.** Represent three unrelated synchronization result types as a type-safe closed set and simplify caller handling. Check valid cases, rejection of unrelated values, and honest handling of missing cases/defaults/nulls. Candidate guidance: C# 15 unions; verify exact compiler diagnostics before freezing checks.
2. **Minimal API asynchronous validation.** Reject invalid or unavailable reservation requests before booking logic runs, declaring validation alongside the model's other rules. Check validation errors, valid handler execution, generated registration, and cancellation propagation. Candidate guidance: the .NET 11 asynchronous validation pipeline; verify actual API/registration against pinned artifacts.
3. **Blazor static grid navigation.** Give two static SSR grids independent shareable sorting/paging state while preserving an existing query-string filter, without adding interactivity. Check direct links, refresh, independent grid state, and retained filter values. Candidate guidance: QuickGrid URL-state and multiple-grid configuration.
4. **Non-web manifest reconciliation.** Produce a deterministic report of matched and unmatched entries from two unique-key manifests without losing either side. Check matches, left-only/right-only entries, and missing-side handling. Candidate guidance: .NET 11 FullJoin; verify the exact operator/asset before freezing checks.

These are proposed task goals, not tested API claims. Scope each scaffold so dependencies and unrelated application setup are already prepared. Freeze the expected behavior and target applicability before running either condition. Candidate API uncertainty is resolved using the accepted sample compilation/evidence process.

## Proposed controls and budget

- One fixed available client/model/version, with recorded SDK/packages, tools, network policy, and execution budget.
- One with/without pair per task: eight runs total, at most ten execution minutes each, capped at 80 execution minutes excluding preparation/review.
- Identical scaffold commits in separate clean contexts/checkouts. Do not expose either run to its pair's output. Alternate which condition runs first across tasks.
- Only Playbook availability and its companion instruction differ. Prepare dependencies equally before running. No corrective follow-ups, retries, or additional model matrix; retain timeouts/failures.
- Check correctness and target applicability separately from adoption of the newer API and unnecessary code. A correct older approach can pass behavior without demonstrating adoption of the newer capability.
- Record consultation, generated result, compilation/behavior checks, unsupported assumptions, time, and token usage where available. Repair Playbook content/tool defects exposed by the exercise before stable launch; do not require Playbook to outperform the baseline on every task.

Keep this a small manual launch exercise, not recurring CI, an automated benchmark platform, or evidence of broad agent/model knowledge. Report actual observations and their limits.
