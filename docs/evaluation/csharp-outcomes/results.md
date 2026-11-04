# C# outcome comparison: 4 November 2026

Two fresh standalone Codex CLI 0.166.0 / gpt-6.1-sol / medium attempts completed
under the [frozen protocol](../comparison-protocol.md), without Playbook first.
The [versioned evidence bundle](../evidence/2026-11-04-codex0166/controls/README.md)
retains controls, package, exact prompts, successful preflights, transcripts,
submitted sources, evaluator diagnostics and usage. Neither attempt was coached,
resumed, corrected or replaced. No timeout occurred.

## Separate observations

| Aspect | Without Playbook | With Playbook |
| --- | --- | --- |
| Consultation | No Playbook server or calls | One `list_recipes` and three `get_recipe` calls; `csharp.closed-results` and `json.serialize-closed-results`, edition/target 11.0; JSON detail retrieved twice |
| Correctness | All 13 common oracle assertions pass, including ambiguous JSON rejection | The same 13 assertions pass |
| Closedness | String input rejected with CS1503; external fourth case rejected with CS0509 | The same compiler rejections |
| Missing cases | Required `Match` arguments reject omitted rejection/absence handlers with CS7036; arbitrary switches have no exhaustiveness guarantee | Omitting Rejected yields CS8509; omitting absence compiles and throws `SwitchExpressionException` at runtime. Production switches handle absence explicitly |
| Applicability | net11.0/C# 15.0 pins retained; ordinary struct/custom converter needs no new package or opt-in | Same pins; actual RC3 compiler builds the union. A custom converter preserves case identity because Applied and Unchanged share their Revision property shape |
| Newer API adoption | Custom readonly struct and manual `Match`; no C# 15 union adoption | C# 15 `union`; no structural JSON classifier adoption. The custom converter handles the overlapping property shapes identified by the consulted applicability constraints |
| Unnecessary code | Three nullable backing fields and manual dispatch implement the older representation. No extra dependencies or unrelated application features observed | Union removes that manually maintained representation/dispatch. Explicit JSON validation remains necessary. No extra dependencies or unrelated application features observed |
| Execution seconds | 203.457256 | 235.8594103 |
| Reported input tokens | 251,376 | 781,217 |
| Reported cached input tokens | 222,976 | 745,984 |
| Reported output tokens | 8,017 | 7,846 |
| Reported reasoning output tokens | 1,051 | 902 |

Token fields are the CLI's exposed turn totals, including repeated input across
model/tool requests. Cached input and reasoning are reported separately as exposed;
do not add them to the other fields as if they were disjoint. These are not a
context-window size or a bill. Preparation and evaluator usage are excluded.
The CLI also explicitly exposed `cache_write_input_tokens: 0` in both turn
receipts; capture preserved that field rather than supplying a default zero.

Both submissions preserve the original records and workflow method names/arguments,
keep the project/SDK/language pins, reject malformed/unknown/ambiguous persisted
inputs, preserve all three cases, and round-trip absence as JSON null. Independent
rebuilds also execute their submitted demonstrations: the without submission
reports 62 checks including 27 invalid documents; the with submission demonstrates
all cases, absence and 22 invalid documents. Those different self-check counts are
not used as comparative scores; the common private oracle supplies the 13 assertions.

Both attempts encountered denied `builds.dotnet.microsoft.com` requests through
the CLI build launcher and independently used the pinned SDK's MSBuild host,
bypassing the CLI launcher's network lookup, to build offline. The failures and
subsequent commands remain in the
transcripts. No network exception or corrective prompt was supplied.

## Controls, verification and limits

Both exports use scaffold commit `6d53662cdba54551a74b3ade063c49009c63561c` and
SDK 11.0.100-rc.3.26522.148. The retained package and CLI hashes match the protocol.
Preflight confirms real shell/edit/build/application HTTP loopback, external
HTTP/socket/proxy-bypass denial, protected file reads denied and the intended
skill/MCP inventory in both configurations. Export/config/cache/temp roots are
independent. User/machine PATH comparisons before and after preparation/evaluation
remain unchanged. Authentication and generated build/module caches are excluded
from the durable source bundle; omitted module-cache identity is recorded separately.

Independent scoring used disposable directories outside the repository, preserving
both submissions. An initial evaluator location inside ignored repository artifacts
inherited the root `.editorconfig` and produced analyzer errors unrelated to the
exported consumer. Moving evaluation outside the repository removed that leakage;
no submission was changed. Baseline/reference calibration then gives nine/zero
failures across the 12 calibration assertions. The three malformed-input rejection
assertions accept any exception, so even the unfinished scaffold passes those;
the nine other assertions discriminate that scaffold from the reference. The
ambiguous-input assertion is added as the thirteenth check for both submissions.
The unchanged oracle, [exact ambiguous document](../evidence/2026-11-04-codex0166/controls/ambiguous-input.json), interface-probe sources
and actual diagnostics are retained. The independent submitted-source rebuilds
were forced to avoid stale incremental outputs after replacing evaluator Program.cs.

Total scored time is 439.3166663 seconds (7.322 minutes), below this pair's 20-minute
maximum. Six further 600-second runs would bring the eight-run total to 4039.3166663
seconds, below 4800. This records no execution of PIN-952–954 and allocates no retries.

For an integrity/receipt check from any directory:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-b1556af3'
& ./docs/evaluation/evidence/2026-11-04-codex0166/Verify.ps1
```

Expect matching retained hashes, two distinct sequential context IDs, unchanged
input manifests and the recorded time/consultation/usage fields. This verifies
retained observations; it does not launch another scored attempt or replace
semantic review. The retained scripts are audit material, not instructions to
repeat the one-attempt exercise.

With the same pinned SDK installed, replay the common checks in fresh
temporary copies (no model calls and no changes to submissions):

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-b1556af3'
& ./docs/evaluation/evidence/2026-11-04-codex0166/Replay.ps1
```

Expect 13 behavior assertions and CS1503/CS0509 compiler rejection receipts for
each condition. The script prints its disposable output directory; it launches
no persistent service and preserves PATH. Supply `-Dotnet` only for another
installation of the same pinned SDK. A missing SDK is a replay prerequisite,
not missing evidence for the observed runs.
The original preparation worktree's SDK was removed after the successful parent
and first-reviewer replays. Replay now defaults to the existing local dotnetup
installation and checks the exact SDK version before building; it no longer
depends on another task's disposable worktree.

Pingu's final assessment remains under PIN-955. This one pair establishes these
observations only: no broad claim about model knowledge, general superiority,
performance, GA readiness or release authorization follows.
