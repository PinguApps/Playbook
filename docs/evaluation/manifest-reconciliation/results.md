# Manifest reconciliation: retained observations

PIN-954 ran the final pair under the [frozen protocol](../comparison-protocol.md),
with Playbook first, then without. Both fresh attempts completed within 600 seconds.
The [evidence bundle](../evidence/2026-11-04-codex0166/manifest/README.md) retains
unchanged submissions, prompts, configs, calls, failures, timing and usage.

## Separate observations

| Observation | With Playbook (run 7) | Without Playbook (run 8) |
| --- | --- | --- |
| Consultation | Successful list_recipes query `join`, then get_recipe `linq.reconcile-all-rows`, target 11, edition 11.0 | No Playbook calls or server |
| Correctness | Build and all 10 frozen assertions pass | Build and all 10 frozen assertions pass |
| Applicability | net11.0 / C# 15; actual RC3 framework FullJoin; no packages or opt-ins | Same target/language/SDK; framework dictionaries and LINQ; no packages or opt-ins |
| Newer API adoption | Enumerable.FullJoin with ordinal comparer and nullable missing-side handling | No FullJoin; Dictionary, Union, OrderBy and TryGetValue |
| Unnecessary code | Retained separate check harness; duplicate validation is necessary because FullJoin permits repeated keys | Two dictionaries support both duplicate rejection and quantity lookup; no unrelated dependencies |
| Elapsed seconds | 157.3195993 | 155.7301164 |
| Input tokens | 429,613 | 259,989 |
| Cached input tokens (included in input) | 404,224 | 228,096 |
| Output tokens | 5,151 | 5,071 |
| Reasoning output tokens | 710 | 835 |
| Termination | Exit 0, no timeout | Exit 0, no timeout |

The unchanged oracle checks matches and both unmatched sides, real zero, empty
inputs, ordinal case/whitespace, input-order independence and duplicate rejection
on either side, including identical unmatched duplicates. The deliberately unfinished
scaffold fails nine assertions; the reference passes all ten. Public Entry, Row and
Compare signatures, original console example, project and pins remain unchanged.
The with submission retains its separate 65-check harness; the without agent reports
112 checks and removes its temporary harness. These self-reports are distinct from
the independently replayed ten-assertion oracle used for both submissions.

Both attempts encountered denied SDK-wrapper requests to builds.dotnet.microsoft.com,
then built with the pinned SDK's local MSBuild.dll. Denials, the baseline's blocked
cleanup commands and autonomous recovery remain in native captures/stderr. No
operator coaching, corrective prompt, replacement attempt or submission repair occurred.
The parent observed an initial preparation restore naming the wrong project
(disposable setup output was not retained); it was corrected before
preflight or scoring and consumed no scored attempt.

## Limits and owner review

All eight retained attempts total **1,492.2752318 seconds** (24.87 minutes), below
4,800 seconds. This pair consumed 313.0497157 seconds. Times include latency,
consultation, failed calls, builds and agent checks; they are not isolated algorithm
timings. Tokens are CLI-exposed cumulative usage, not an independent billing audit.
No separate token cap was imposed. No combined score or required Playbook win applies.

Pingu approved the SDK relocation before either launch; all 5,467 frozen ZIP files
match the installed SDK. Both exact-context preflights passed first. User/machine
PATH stayed unchanged. This is RC3 evidence on one fixed CLI/model, not GA,
general model knowledge, superiority, release or deployment evidence. Pingu's
four-pair assessment and launch implications remain **PIN-955**.
