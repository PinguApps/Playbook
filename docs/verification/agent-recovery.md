# Agent recovery evidence

**25 October 2026: one fresh ordinary T3 first pass passed in 88.113 seconds.**
The companion instruction now separates the first suggested retry, preserving
selection constraints, from any later explained narrowing. Server code, recovery
metadata, tools, recipe identities and matching semantics are unchanged.

## Historical observation remains a failure

[PIN-978 D](agent-usability-expanded.md), its
[original thread](t3-thread:/v1/mcp:37db6b37-ebe7-4709-8aa6-e21fdb159757) and
[complete original receipt](agent-usability-expanded.d.json) retain the frozen first-pass result:

- Position 6: `list_recipes({"query":"tangerine warehouse property normalization","targetVersion":"10"})`
  returned total 0, incomplete coverage and a correct suggestion retaining target
  `10`, empty areas/languages, shared inclusion, offset 0 and limit 20, with null query.
- Position 8: `list_recipes({"targetVersion":"10","areas":["csharp"],"languages":["csharp"],"limit":50})`
  instead returned total 2: `csharp.extend-uneditable-types` and
  `csharp.validate-property-assignment`. This C# search was compatible and narrower;
  it changed selection filters and therefore **failed the frozen strict preservation
  check**. It was not a replay of the suggestion. The answer did not explain the change.
- Position 9: `get_recipe({"id":"csharp.validate-property-assignment","targetVersion":"10"})`
  returned complete property Markdown and both target-10 applicability routes before
  editing. Generated code and independent consumer assertions passed. Those passes
  do not change the failed recovery result.

PIN-975 already supplies the correct deterministic server suggestion. PIN-675's
normal companion instruction lacked the explicit two-step recovery guidance.
Neither completed issue nor the historical PIN-978 report/receipt is rewritten.

## Frozen follow-up and isolated candidate

The [bounded runbook](../agent-usability.md) supplies the unchanged D prompt:

> Simplify StockLabel's normalization and validation property in consumer/ while preserving behaviour and all existing project constraints. Consult the connected Playbook; start your discovery using my phrase "tangerine warehouse property normalization". Implement and verify an appropriate simplification if guidance supports it. Briefly explain how you found and checked the guidance.

Before launch, retain the original checks: actual empty keyword result; the first
retry preserves target, areas, languages and shared inclusion, changing only query
or paging as suggested; any subsequent narrowing is separately explained; complete
detail precedes application; no exhaustive-absence claim or configuration upgrade;
independently passing constructor/assignment/rejection/retained-value assertions.
One ordinary top-level thread, five execution minutes maximum, no coaching, rerun,
model/OS matrix, schedule or new test platform. A failure would remain a failure.

The candidate is ordinary local-only package **1.0.0-preview.1018.1**, source
**d033963b13b611a6fd2844b25406b0c1f959ab74**, SHA-256
**8cd91d296e5e6a5eb54524bd557975a7d48ac53d24bfc3f8b4afd7df3fb321a0**.
`playbook --version` returned the package version plus that commit. Build/pack used
SDK 10.0.402. Installed from the local-only feed into a fresh tool/cache/home slot
`V:\_Scratch\PlaybookVerification\packages\pin1018-1`; earlier slots remain intact.

The existing owner-authorized verification project is
`mcp:cb660864-7bf7-43f9-b205-1a54c5e763cf`. Seed revision
**ee45bc0b1bdffec1b85e62b63734fe3273d9c38c**, branch `verify/pin1018-seed`, changes
only its Playbook executable and adds the canonical companion recovery paragraph.
The unchanged package-free consumer still pins SDK **11.0.100-rc.2.26504.105**,
targets **net10.0 / C# 14.0**, and runs on .NET **10.0.13**. Its baseline passed.
The normal preparation helper and seed recovery wording match the companion text.

Live capabilities/configuration selected **codex / gpt-6.1-sol / high reasoning /
default service tier**, full-access/default modes, on Windows x64. These model
options match PIN-978. No controlled comparison or causal improvement is claimed.

## Actual decisions and independent verification

[Fresh thread](t3-thread:/v1/mcp:4c90b62b-cae0-4993-9cd1-45b73b5870d7),
[complete receipt](agent-recovery.agent.json), branch `verify/pin1018-recovery`, worktree
`V:\_worktrees\PlaybookVerification\verify-pin1018-recovery`.
Durable state confirms `parentThreadId:null`, one terminal run, no inherited
answers. Execution was **12:44:20.054–12:45:48.167 UTC**, **88.113 seconds**.
Complete tool results, commands, generated diff and answer are retained; truncated
items were recovered with T3 continuations. Reasoning is excluded; each dynamic
activity entry points to its single full `toolCalls` record by position.

1. Position **5**, actual `list_recipes` input:

   ```json
   {"targetVersion":"10.0","query":"tangerine warehouse property normalization"}
   ```

   Total 0; `no_matches`, both incomplete coverage disclosures. Effective areas
   and languages are empty; `includeShared=true`. The suggested request is exactly
   the input used at position 8 below.
2. Position **8**, actual first retry:

   ```json
   {"targetVersion":"10.0","areas":[],"languages":[],"query":null,"includeShared":true,"offset":0,"limit":20}
   ```

   Independently compared equal to the complete returned suggestion. Only keywords
   were removed; original target selection, both filter sets, shared inclusion and
   default paging were retained. Total **9**, no next page: extension/property,
   F# compiler-service memory, malformed JSON contracts, two LINQ goals, NuGet client
   hosts and two file-app goals. No subsequent narrower search was needed or observed.
3. Position **10**, actual detail request:

   ```json
   {"id":"csharp.validate-property-assignment","targetVersion":"10.0"}
   ```

   Full **6,286-character Markdown** and both .NET 10 routes were received before
   position **16** edited the consumer. The agent distinguished the primary SDK 10
   route from its optional SDK 11 route and disclosed RC evidence and incomplete
   coverage. Its explanation accurately described keyword-only recovery.
4. Positions **14/17** build/run before and after. The agent removed the explicit
   backing field, retained the sentinel initializer and constructor validation,
   and wrote normalization through `field`. Independent build/run also passes:
   zero warnings/errors; constructor/assignment normalization, blank rejection and
   retained previous value. Only `consumer/Program.cs` differs; all project,
   build/package/global files, instructions and connection are unchanged.

Coordinator process/user/machine PATH comparisons match; the agent's three saved
PATH snapshots also compare equal. No persistent environment variable was changed.
Token usage was not exposed. This one successful observation does not establish
reliable recovery across models, clients or operating systems. A later separate
narrowing was not exercised; its distinction is explicit in the instruction and
the preserved historical failure, not claimed as newly observed behavior.
