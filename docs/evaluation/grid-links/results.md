# Static grid links comparison

Two fresh Codex CLI 0.166.0 / gpt-6.1-sol / medium attempts completed under the
[protocol](../comparison-protocol.md), without Playbook first. Neither attempt
was coached, resumed, repaired or retried. The [evidence bundle](../evidence/2026-11-04-codex0166/grid/README.md)
retains actual prompts, configs, preflights, transcripts, submissions and checks.

| Observation | Without Playbook | With Playbook |
| --- | --- | --- |
| Consultation | No Playbook server/calls | One list_recipes and one get_recipe: blazor.share-grid-view, target/edition 11.0 |
| Frozen HTTP oracle | 15 passes, one false negative | 12 passes, two false negatives; two dependent assertions skipped |
| Focused browser checks | Pass with JavaScript disabled; selects the requested sort column | Pass with JavaScript disabled; selects the requested sort column and accepts an omitted first-page default |
| Applicability | Frozen net11.0/C# 15.0/QuickGrid RC3 pins preserved; manual tables and query links | Same pins; real QuickGrid query-state support, PaginationState, QueryParameterNameOptions and Paginator |
| Newer API adoption | None | Independent QueryParameterNameOptions and PaginationState per grid |
| Unnecessary code | Manual query parsing, sorting, paging and table rendering implement the older approach; no extra dependencies | Framework restores sorting/paging; explicit heading anchors remain necessary in static rendering |
| Execution seconds | 261.8750946 | 140.0253117 |
| Reported input tokens | 435943 | 460164 |
| Reported cached input tokens | 408960 | 427136 |
| Reported output tokens | 9144 | 4295 |
| Reported reasoning output tokens | 1530 | 520 |
| Reported cache-write input tokens | 0 | 0 |

Usage fields overlap and include repeated input across requests; these are the
CLI's exposed values, not disjoint billing totals or context-window sizes.
Preparation and evaluator usage are excluded. Both agents encountered a blocked
SDK build-launcher download and used the pinned local MSBuild host to build offline.
Their failures and subsequent checks remain in the transcripts.

The unchanged frozen oracle has two selection limitations exposed by these real
submissions. Its first north.direction=desc anchor also switches from units to
code, but its expected rows assume units remain selected. Both submissions offer
the correct units-descending anchor too. QuickGrid's first-page link removes the
default north.page key rather than emitting north.page=1, so the oracle does not
recognize that valid link in the with submission. Raw failures are retained;
neither the oracle nor either submission was repaired to manufacture a pass.

The supplementary browser check chooses the requested sort column and accepts
an omitted first-page default. Chrome 156.0.8078.4, with script execution disabled
through CDP, passes direct asymmetric bookmarks, refresh, ordinary links preserving
filter spaces and the other grid, real browser back/forward, both sort columns and
directions, and next/previous on both grids. These checks run on disposable copies.
The browser harness initially selected Chrome's toolbar target; setup failures
were diagnosed before the successful checks, without further model attempts.
The frozen oracle calibration gives 16 reference passes and seven unfinished
scaffold failures. Independent submitted-source builds have zero warnings/errors.

Pingu approved SDK relocation before either scored launch. All 5,467 frozen ZIP
files match the installed SDK; its archive hash, the CLI hash and retained package
hash match the protocol. The exact-hash package was extracted without rebuilding.
Both final MXC preflights passed shell/edit/build/loopback, protected-read denial
and external HTTP/direct-socket/proxy-bypass denial. Host reachability controls
succeeded. Actual tools/configs are retained; both list the same native helper
tools, with only Playbook list/get added to the with condition. Dependency caches
match. User/machine PATH remains unchanged. No authentication is retained.

This pair used 401.9004063 seconds. Together with the preceding two pairs, six
attempts used 1179.2255161 seconds; two remaining 600-second attempts would total
2379.2255161, below 4800. PIN-954 execution and Pingu's final PIN-955 assessment
remain separate. These observations establish no broad model-knowledge,
performance, superiority, GA-readiness or release claim. RC3 evidence remains RC3.

From any directory, verify retained evidence or replay checks without model calls:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-1e371751'
& ./docs/evaluation/evidence/2026-11-04-codex0166/grid/Verify.ps1
& ./docs/evaluation/evidence/2026-11-04-codex0166/grid/Replay.ps1
```

Expect matching hashes, distinct sequential contexts and budgets; replay retains
the one/two known frozen-oracle failures and reports BROWSER_PASS for each condition.
Requires the same pinned SDK, Node at the recorded path and the recorded Chromium
installation. Replay owns/stops its application and browser processes and prints
its disposable output path. Missing replay tools are prerequisites, not new model runs.
