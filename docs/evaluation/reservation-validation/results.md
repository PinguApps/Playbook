# Reservation validation comparison

Two fresh Codex CLI 0.166.0 / gpt-6.1-sol / medium attempts completed under the
[protocol](../comparison-protocol.md), with Playbook first. The
[evidence bundle](../evidence/2026-11-04-codex0166/reservation/README.md) retains
prompts, configs, successful preflights, transcripts, unchanged submissions,
private oracle receipts, usage and hashes. Neither attempt was corrected or retried.

The original SDK preparation directory had been removed. After the with launch,
the parent discovered that using the installed RC3 SDK at a different path violated
the frozen absolute-path control. Pingu authorized an amendment conditional on
identity verification before the without launch. The original SDK ZIP matches
the frozen SHA-512 and all 5,467 archive files match the installed counterparts.
Additional installed versions do not override the exact global.json pin. The
exact-hash Playbook package was extracted without rebuilding; actual SDK/server
paths are retained. This post-launch amendment is part of the evidence's limits.

| Observation | With Playbook | Without Playbook |
| --- | --- | --- |
| Consultation | One list_recipes and one get_recipe; minimal-api.await-model-rules, target/edition 11.0 | No Playbook calls/server |
| Correctness | All 15 private HTTP assertions pass | All 15 pass |
| Applicability | net11.0/C# 15.0 pins retained; RC3 ASP.NET Core assets supply async validation without an extra package | Same pins/assets; ordinary awaited model method and endpoint filter |
| Newer API | AsyncValidationAttribute and AddValidation; generated endpoint validation | No async-attribute/generated-pipeline adoption |
| Unnecessary code | No redundant logic observed; the custom availability attribute declares the required rule, and the framework maps errors | Manual DataAnnotations collection, grouping and field-error mapping plus endpoint adapter; this implements the older approach, not a correctness failure |
| Execution seconds | 198.5460576 | 139.4623859 |
| Reported input tokens | 650590 | 236624 |
| Reported cached input tokens | 589312 | 200576 |
| Reported output tokens | 6180 | 4187 |
| Reported reasoning output tokens | 916 | 517 |
| Reported cache-write input tokens | 0 | 0 |

The CLI reports repeated turn input, including cached input, and reasoning output
as exposed fields. They are not disjoint billing totals or context-window sizes.
Preparation/evaluator usage is excluded. No combined score or required Playbook win
is inferred from these observations.

Both submissions declare the awaited availability rule alongside the model's
other validation rules. Independent builds have zero warnings/errors. The unchanged
private oracle exercises required Code, guest limits, field-keyed errors,
unavailable rejection, held valid/invalid requests before release, exactly one
valid acceptance afterwards, request cancellation and no booking after canceled
checks are released. All 15 assertions pass for each. Calibration gives zero
reference failures and 15 unfinished-scaffold failures. Source inspection confirms
the availability service, control routes, pins and task are preserved; neither
adds a dependency. The without filter calls the model method before the handler;
the with registration uses the actual RC3 generated validation route.

Both attempts encountered a blocked SDK build-launcher network request and used
the pinned SDK's local MSBuild to build offline. The with attempt also reported
blocked temporary-log cleanup. Failures remain retained. The parent scored disposable
copies without repairing submissions. The submitted build.log and its projection
are empty; the attempted build commands remain in the transcript, while the
independent oracle/replay builds supply the retained compiler results.
RC3 execution is not GA evidence.

Both MXC preflights passed shell/edit/build/application-loopback checks; actual
repository, evaluator, paired-output, user-config and package reads were denied,
as were external HTTP, direct sockets and proxy bypass. Host reachability controls
succeeded. Actual configs/tool inventory are retained; native helper tools appear
in both, with only Playbook list/get added to the with context. User/machine PATH
remains unchanged. Authentication and generated bin/obj/module caches are excluded
from capture; necessary original source and diagnostics remain retained.

This pair used 338.0084435 seconds. Including the C# pair gives 777.3251098 seconds;
four remaining 600-second runs would yield 3177.3251098, below the 4800-second cap.
PIN-953–954 execution is outside this change. Pingu owns the final assessment in
PIN-955; these bounded observations establish no broad model-knowledge,
performance, general superiority, GA-readiness or release claim.

From any starting directory, verify retained hashes, fresh single-turn contexts,
order, budgets, prompts and frozen inputs:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-6b45d19b'
& ./docs/evaluation/evidence/2026-11-04-codex0166/reservation/Verify.ps1
```

Expect matching hashes and two distinct sequential contexts. Replay the unchanged
HTTP oracle in disposable copies with the same pinned SDK installed:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-6b45d19b'
& ./docs/evaluation/evidence/2026-11-04-codex0166/reservation/Replay.ps1
```

Expect 15 passing assertions per condition and `Oracle: 0 failed` twice. These
commands launch no model calls or replacement scored attempts. Replay owns and
stops its application processes; its temporary output is printed for inspection.
