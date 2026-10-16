# MCP contract

Status: core contract, packaging, and discovery/applicability semantics accepted through Round 6; independent package SemVer accepted in Round 7. Not implemented. Experimental APIs remain included with explicit labels. See the [release plan](release-plan.md) for accepted publication policy.

## Catalogue discovery

`list_recipes` has optional `targetVersion`, `areas`, `languages`, and `query` filters, plus `includeShared` (default true) and `offset`/`limit` paging arguments. The earlier proposed preview/experimental status filter is not approved.

Illustrative request shape:

```json
{
  "targetVersion": "11.0",
  "areas": ["blazor", "wpf"],
  "languages": ["csharp"],
  "includeShared": true,
  "query": "validation",
  "offset": 0,
  "limit": 20
}
```

Accepted semantics:

- Omitted target means the newest release represented in the installed catalogue. Return the resolved target and whether it was assumed.
- Omitted/empty area filter means all areas. Multiple requested areas match any of them. Multiple languages similarly match any requested language.
- `languages` matches languages demonstrated by the recipe's examples. Broader API language applicability is recorded separately; a C#-only example does not satisfy an F# filter.
- With an area filter, `includeShared: true` also matches relevant shared language/library/SDK recipes through explicit applicability metadata. False limits area matching to directly tagged areas. Deduplicate shared matches. Without an area filter, all areas remain eligible regardless of this switch.
- Different kinds of filters combine: for example, a recipe must match an area, a requested language, the target, and the keywords when all are supplied.
- Keyword matching is deterministic, case-insensitive, and uses recipe titles, goals, tags, and API names. The agent interprets natural language; the server does not host an AI search system.
- Page size: 20 summaries by default, at most 50. Use stable ordering for identical arguments and return the next offset when more results exist.
- Summaries contain identity, goal, areas, key applicability/status, and enough information to choose detail. Do not put full examples in catalogue responses.
- Include the supported area identifiers in discovery metadata so clients can form valid filters without a third tool.

These are filtering rules, not a claim that an application has been inspected. A caller can request more than one area for shared language/library guidance. Retagging a recipe does not duplicate its content.

Shared inclusion only broadens area matching. It does not bypass target, example-language, keyword, or explicit platform constraints and does not admit unrelated platform guidance.

## Recipe identity and release editions

Use stable, descriptive goal IDs, such as `aspnet-core.validate-requests`, independent of the release number. Exact names are examples, not seeded content.

`get_recipe` accepts an ID and optional `targetVersion`. Select the guidance edition applicable to that target. Preserve older editions as later releases arrive. A .NET 10 recipe can remain applicable to .NET 11 when still appropriate; prefer a newer compatible edition when it supersedes the older approach. Never return an incompatible newer edition for an explicit older target.

Introduction release is separate from target prerequisites. Include a .NET 11-era recipe for an older supported target when the exact SDK/package route is documented and verified. Return those requirements prominently without inferring that the caller has installed them. Do not infer supported language/runtime compatibility merely because syntax compiles. The curated .NET 10 historical backfill remains eight recipes; verified older-target routes for .NET 11-era capabilities are a distinct applicability matter.

## Complete response objects

Use one complete object for each response. A detailed recipe object contains identity, the resolved target, full applicability/prerequisites, API stability, evidence/verification, source references, and the full Markdown body. Narrative sections retain goal, older approach, newer approach, migration, constraints, and alternatives where needed.

Use standard MCP structured content with an output schema, plus the same complete object serialized as a JSON text fallback. Do not put only metadata in structured content while leaving the recipe body exclusively in another representation. The typed C# SDK result pattern supports this without a custom output protocol. [MCP structured content](https://modelcontextprotocol.io/specification/2026-07-28/server/tools#structured-content), [C# SDK guidance](https://csharp.sdk.modelcontextprotocol.io/v2/concepts/tools/tools.html#structured-content).

This accepted design is based on protocol/SDK documentation and client-source inspection, not executed client tests. In particular, the reviewed VS Code source prefers structured content for model input when it exists. Actual T3 Code, Codex CLI, Claude Code, and VS Code versions still need the agreed brief client checks. [VS Code source](https://github.com/microsoft/vscode/blob/main/src/vs/workbench/contrib/mcp/common/mcpLanguageModelToolContribution.ts).

## GA target and RC evidence

Pingu replaced the earlier preview-default proposal with a GA content target. Build for the .NET 11 full release, using what current RCs establish about the intended final contract. At GA, reconcile final notes, migrations, actual SDK/packages, and the coverage register; update recipes and release corrections/additions. This is a maintenance step, not a scheduled automation or authorization to publish now.

Do not present an RC-only transition as the final .NET 11 approach or label RC execution as GA-tested. Round 5 confirms that deliberately experimental APIs remain included, including Blazor AI. Summaries and details must label them experimental; details explain package versions, required opt-ins, and stability limitations. Do not hide them behind an unapproved opt-in filter or call them stable because the runtime has reached GA. Round 7 settles the independent package-version/channel policy in the [release plan](release-plan.md).

## Errors and empty results

Use explicit, machine-readable failures for an unknown recipe ID, unsupported target, or unknown area/language filter. Include supported values or available editions where useful. A known recipe with no applicable edition for an explicit target should say so.

A valid filter combination matching nothing is a successful empty result. Never silently discard a filter or substitute the latest target to manufacture a match.

Exact error codes and transport serialization remain to design within standard MCP behavior; this is the intended caller-visible contract.

## CLI boundary

The accepted public CLI is `playbook mcp`, `playbook --help`, and `playbook --version`. Bare `playbook` shows help and exits. Guidance retrieval stays on the two MCP tools; no separate terminal browsing feature set is planned.

MCP mode reserves stdout for protocol traffic and uses stderr for diagnostics. Help/version output is ordinary terminal output outside MCP mode. Client registration is documented; the executable does not rewrite client/project configuration automatically.

## Windows/macOS smoke limit

Pingu's limit is already settled: one very short packaged-tool smoke per OS. Install the actual package, initialize its stdio server, call list/get, check that the body and applicability are both present, and shut down cleanly. No native UI, emulator, device, or comprehensive recipe test matrix is planned for these jobs.

Core Linux validation and sample build checks remain focused. Round 5 requires examples to compile against their stated SDKs/dependencies, with focused behavior checks where useful rather than a broad native runtime gate. Resolve compilation gaps before an example meets that gate. Smoke checks establish tool behavior, not every example's native runtime behavior. Verification metadata records only work actually performed; compilation does not imply native execution.

## Client checks

One brief real check per T3 Code, Codex CLI, Claude Code, and VS Code Copilot, each on an available supported OS. Confirm the recipe body and applicability both reach the agent, and record actual client/OS versions. These checks supplement the tiny Windows/macOS tool smokes; do not create a four-client-by-three-OS matrix. No such checks have run during planning.
