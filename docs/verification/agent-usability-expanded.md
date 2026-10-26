# Agent usability expanded evidence

**25 October 2026: A/B/C pass; D fails the frozen filter-preservation check while
its generated consumer passes.** Four first-pass runs took **316.328 seconds
summed execution** (5m 16.328s), with no timeout, coaching or corrective rerun.
All finished within 86 seconds. Concurrent execution is not a performance result.
[Frozen prompts and checks](../agent-usability-larger-catalogue.md) extend the
[existing runbook](../agent-usability.md), outside CI.

## Candidate and growth

Ordinary installed local-only package **1.0.0-preview.978.1** was built from
**6f351aa2b5483376d38d5482b4af04bf823f549d**. `--version` returned that exact
package version plus commit. SHA-256:
**c16b30dd670221309f1ea193db95bd38817a63c2f4d5e0010b187d395be96104**.
Artifact: `artifacts/PinguApps.Playbook.1.0.0-preview.978.1.nupkg`;
fresh retained install/cache/home slot:
`V:\_Scratch\PlaybookVerification\packages\pin978-1`.
Production/sample/build inputs were committed and unchanged when packed.
Later documentation does not change this frozen candidate.

Real installed-server discovery from `C:\Windows\Temp` enumerated **171 distinct
ordinary persistent IDs**, excluding editions and unpublished fixtures, across
four pages (50/50/50/21). API stability statuses are **157 stable, 13 experimental
and 1 preview**; stable identity is separate from API stability. There are five
explicitly neutral goals. [Full catalogue](agent-usability-expanded.catalogue.json) and
[candidate/delta receipt](agent-usability-expanded.candidate.json) retain all IDs and descriptions.

Compared with the [first checkpoint's 49 IDs](agent-usability-initial.catalogue.json), **122 were
added and none removed**. The expected roughly 50 goals was exceeded by real
delivery, rather than manufactured identities. The specified PIN-722–734 and
PIN-772–796 readiness cohorts and PIN-977 were Done in the verified team/project;
their actual delivered receipts/content confirmed readiness. Growth also includes
later library/SDK/web deliveries and this PR's two new goals. The nine currently
discoverable target-10 IDs include declared conditional guidance; they are not
a claim that the eight selected historical foundations changed scope.

## Environment and isolation

T3 `PINGU-SERVER`, environment `e161a58f-5feb-4993-b10f-5dc13775bd35`, Windows x64,
server **0.1.2-nightly.20261022.3541**, Codex CLI **0.163.0**, PowerShell **7.6.7**.
Live capabilities and every launch selected instance **codex**, model
**gpt-6.1-sol**, **high** reasoning / **default** service tier, full-access/default
modes. Model/options match PIN-977; the T3 version and content differ. No
uncontrolled between-checkpoint difference establishes improvement or causation.

The owner-authorized verification project remains
`mcp:cb660864-7bf7-43f9-b205-1a54c5e763cf`. Dedicated seed
`V:\_Scratch\PlaybookVerification` gained branch `verify/pin978-seed`, commit
**495ce309c103789332277efe7eb8a0eeb84b0092**, changing only its local MCP executable
path. The previous seed branch, consumer, package slots and old threads were
preserved. The unchanged independent consumer builds with SDK
**11.0.100-rc.2.26504.105**, targets **net10.0 / C# 14.0**, and executes on
**.NET 10.0.13**. Its baseline assertions passed before launch.

Each ordinary top-level `t3_thread_launch` explicitly selected that seed SHA,
`startFromOrigin:false`, its own worktree/branch and the same model options.
Durable state confirms `parentThreadId:null`, one run per scenario, no inherited
answers and actual `playbook.list_recipes`/`get_recipe` calls in every thread.
No owner-only access problem occurred. No direct protocol test substitutes for
these agent decisions.

## First-pass outcomes

Each linked raw receipt contains complete recovered actual MCP inputs/outputs,
observable commands, generated changes and answers, with exact run/item IDs.
Reasoning items are excluded. Each run is terminal and retained.

1. **A — PASS, 78.030s.** [Thread](t3-thread:/v1/mcp:068c4a45-406c-421a-86f5-579d71550c06),
   [receipt](agent-usability-expanded.a.json), `verify/pin978-a`.
   Positions **5/8** use target `10`, language C#, page size 3 and offsets 0/3;
   the second offset equals the returned continuation, with filters unchanged.
   The comparison distinguishes extension properties from stored-value validation.
   Position **9** retrieves full property detail before position **13** edits.
   The answer selects the optional SDK 11 route while preserving net10/C#14,
   initializer and constructor validation. Agent before/after and independent
   coordinator builds/runtime assertions pass; configuration remains unchanged.
2. **B — PASS, 85.540s.** [Thread](t3-thread:/v1/mcp:f7092c7c-7d3d-41a7-8b8e-ebb71deb06bc),
   [receipt](agent-usability-expanded.b.json), `verify/pin978-b`.
   Deliberate `latest` consistently resolves 11.0. The union query finds five
   neighbouring goals; a broad source query returns fourteen candidates.
   Positions **6/7** retrieve full C# result/JSON guidance. Position **8** combines
   MSBuild with C#/F# and returns genuinely neutral goals; **10** retrieves the
   item-glob detail. The answer distinguishes patterns from expanded files and
   explicitly reports source-only uncompiled proof, unknown consumer compatibility,
   and RC/GA limits. Checkout is clean. Illustrative snippets were not compiled;
   the frozen prose-only case does not require that and no execution is claimed.
3. **C — PASS, 79.203s.** [Thread](t3-thread:/v1/mcp:16a3bd70-74a7-48c8-a85f-8664a3c6eed5),
   [receipt](agent-usability-expanded.c.json), `verify/pin978-c`.
   Positions **5–7** inspect empty target-10 keywords and broader compatible
   discovery. **9/10** deliberately explore target 11; **12/13** retrieve both
   closed-result and closed-hierarchy detail. The answer compares genuine
   runtime/language/inheritance requirements with the fixed consumer, rejects
   both native routes, and recommends the older typed wrapper or deferral.
   It does not use absence alone as impossibility evidence. Checkout is clean;
   no generated implementation required compilation.
4. **D — FAIL: strict recovery preservation, 73.555s.**
   [Thread](t3-thread:/v1/mcp:37db6b37-ebe7-4709-8aa6-e21fdb159757),
   [receipt](agent-usability-expanded.d.json), `verify/pin978-d`.
   Position **6** uses the exact frozen phrase and target `10`, gets zero matches,
   incomplete coverage and a correct suggested no-keyword retry retaining empty
   area/language sets, shared inclusion, offset 0 and limit 20. At **8** the agent
   instead supplies `areas:["csharp"]`, `languages:["csharp"]`, limit 50 and no
   keywords. Target is retained and compatibility is narrowed, not broadened,
   but area/language filters changed: this fails the frozen check permitting only
   keyword/page correction. Position **9** still retrieves full compatible detail
   before the scoped edit. Agent and independent builds/assertions pass with
   unchanged configuration. Successful code does not turn the recovery result
   into a pass. The final answer does not explicitly discuss that filter change.

The unresolved observation is tracked as
[PIN-1018 — 00-013 - Preserve selection filters during agent search recovery](https://linear.app/pinguapps/issue/PIN-1018),
**Backlog, S (2)**, native milestone 00 and related to PIN-978. It covers a concise
companion instruction and one bounded real follow-up, not a server API change or
another full checkpoint. Scoped overlap review read PIN-975 and PIN-675 fully:
the server already suggests the correct retry, while the companion paragraph
does not explicitly explain the two separate search steps. Their completed
content/status/estimates remain unchanged; incidental service-created relation
links were removed to retain only the intended PIN-978 relation.

No other regression or unresolved PIN-977 product defect was demonstrated. B's
first broad source query did not find item-glob guidance; a targeted discovery
did. That is retained search behavior, not an invented defect. PIN-1018 does not
block publishing this completed checkpoint report; its unresolved status prevents
claiming all four agent scenarios passed.

## Sizes and independent verification

[Metrics](agent-usability-expanded.metrics.json) measure UTF-8 bytes of compact reserialized actual
tool **output**, excluding protocol framing, duplicate text fallback and full
activity size. Provider token usage is not exposed and remains unavailable.
A's two discovery pages/detail measure **5,297 / 4,973 / 38,229** bytes. B's broad
source discovery is **22,267** bytes; its three detail outputs are
**27,340 / 27,658 / 17,672**. D's empty/retry/detail outputs are
**1,401 / 4,208 / 38,229**. The receipt retains every other measured output.

[Independent consumer evidence](agent-usability-expanded.consumer.json) confirms A/D build with
zero warnings/errors and execute constructor/setter normalization, blank rejection
and retained-value assertions. Only their `consumer/Program.cs` files differ;
all project/global/build/package/connection/instruction files are unchanged.
B/C have clean worktrees. Process/user/machine PATH comparisons match.

Retain the frozen first-pass outcomes independently of later consumer checks.
