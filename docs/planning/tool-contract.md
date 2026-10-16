# Proposed MCP contract

Status: Round 4 proposal, not implemented. The tool names, optional area filtering, latest-version default, and applicability requirements are already accepted. The refinements below await Pingu's answers.

## Catalogue discovery

`list_recipes` has proposed optional `targetVersion`, `areas`, `languages`, `statuses`, and `query` filters, plus `offset` and `limit` paging arguments.

Proposed example:

```json
{
  "targetVersion": "11.0",
  "areas": ["blazor", "wpf"],
  "languages": ["csharp"],
  "query": "validation",
  "offset": 0,
  "limit": 20
}
```

Proposed semantics:

- Omitted target means the newest release represented in the installed catalogue. Return the resolved target and whether it was assumed.
- Omitted/empty area filter means all areas. Multiple requested areas match any of them. Multiple languages similarly match any requested language.
- Different kinds of filters combine: for example, a recipe must match an area, a requested language, the target, and the keywords when all are supplied.
- Keyword matching is deterministic, case-insensitive, and uses recipe titles, goals, tags, and API names. The agent interprets natural language; the server does not host an AI search system.
- Proposed page size: 20 summaries by default, at most 50. Use stable ordering for identical arguments and return the next offset when more results exist.
- Summaries contain identity, goal, areas, key applicability/status, and enough information to choose detail. Do not put full examples in catalogue responses.
- Include the supported area identifiers in discovery metadata so clients can form valid filters without a third tool.

These are filtering rules, not a claim that an application has been inspected. A caller can request more than one area for shared language/library guidance. Retagging a recipe does not duplicate its content.

## Recipe identity and release editions

Recommend stable, descriptive goal IDs, such as `aspnet-core.validate-requests`, independent of the release number. Exact names are examples, not seeded content.

`get_recipe` accepts an ID and optional `targetVersion`. Select the guidance edition applicable to that target. Preserve older editions as later releases arrive. A .NET 10 recipe can remain applicable to .NET 11 when still appropriate; prefer a newer compatible edition when it supersedes the older approach. Never return an incompatible newer edition for an explicit older target.

## Complete response objects

Recommend one complete object for each response. A detailed recipe object contains identity, the resolved target, full applicability/prerequisites, API stability, evidence/verification, source references, and the full Markdown body. Narrative sections retain goal, older approach, newer approach, migration, constraints, and alternatives where needed.

Use standard MCP structured content with an output schema, plus the same complete object serialized as a JSON text fallback. Do not put only metadata in structured content while leaving the recipe body exclusively in another representation. The typed C# SDK result pattern supports this without a custom output protocol. [MCP structured content](https://modelcontextprotocol.io/specification/2026-07-28/server/tools#structured-content), [C# SDK guidance](https://csharp.sdk.modelcontextprotocol.io/v2/concepts/tools/tools.html#structured-content).

This recommendation is based on protocol/SDK documentation and client-source inspection, not executed client tests. In particular, the reviewed VS Code source prefers structured content for model input when it exists. Actual T3 Code, Codex CLI, Claude Code, and VS Code versions still need the agreed brief client checks. [VS Code source](https://github.com/microsoft/vscode/blob/main/src/vs/workbench/contrib/mcp/common/mcpLanguageModelToolContribution.ts).

## Preview and experimental guidance

Recommend making applicable experimental/preview recipes discoverable by default, clearly labeled, with an optional status filter. Detail includes required preview settings, packages, constraints, and alternatives. A stable API inside an RC framework release is distinct from an explicitly experimental API.

This is documentation retrieval, not authorization to enable an experimental dependency in a user's project. Package prerelease/GA publication policy remains deferred separately.

## Errors and empty results

Recommend explicit, machine-readable failures for an unknown recipe ID, unsupported target, or unknown area/language filter. Include supported values or available editions where useful. A known recipe with no applicable edition for an explicit target should say so.

A valid filter combination matching nothing is a successful empty result. Never silently discard a filter or substitute the latest target to manufacture a match.

Exact error codes and transport serialization remain to design within standard MCP behavior; this is the intended caller-visible contract.

## CLI boundary

Recommend the minimal public CLI: `playbook mcp`, `playbook --help`, and `playbook --version`. Bare `playbook` shows help and exits. Guidance retrieval stays on the two MCP tools rather than adding a separate terminal browsing feature set.

MCP mode reserves stdout for protocol traffic and uses stderr for diagnostics. Help/version output is ordinary terminal output outside MCP mode. Client registration is documented; the executable does not rewrite client/project configuration automatically.

## Windows/macOS smoke limit

Pingu's limit is already settled: one very short packaged-tool smoke per OS. Install the actual package, initialize its stdio server, call list/get, check that the body and applicability are both present, and shut down cleanly. No native UI, emulator, device, or comprehensive recipe test matrix is planned for these jobs.

Core Linux validation and feasible sample build checks remain focused. Smoke checks establish tool behavior, not every example's native runtime behavior. Verification metadata records only work actually performed. The final release gate for examples lacking compile/runtime evidence still needs an explicit decision; the smoke limit alone does not authorize claiming those examples are verified or silently waive the earlier evidence standard.
