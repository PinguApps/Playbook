# MCP contract

Status: core contract, packaging, and discovery/applicability semantics accepted through Round 6; independent package SemVer accepted in Round 7. Not implemented. Experimental APIs remain included with explicit labels. See the [release plan](release-plan.md) for accepted publication policy.

## Catalogue discovery

`list_recipes` requires `targetVersion`, with optional `areas`, `languages`, and `query` filters, plus `includeShared` (default true) and `offset`/`limit` paging arguments. PIN-971 supersedes the historical omission default. The earlier proposed preview/experimental status filter is not approved.

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

- Every call requires a concrete supported target release or explicit `latest`; missing, null and blank values fail helpfully with `required_target`. `latest` resolves numerically to the newest target release represented in the installed catalogue, using no network, installed SDK inventory or SDK build number. Echo the original `requestedTarget` and canonical `resolvedTarget`; retain `targetAssumed: false` for existing readers. No successful target is assumed.
- Inspect the application's `TargetFramework`/`TargetFrameworks` and relevant project/`global.json` context before selecting a release. An SDK version is not the application's target release. Repeat `targetVersion` on both tools and each page: calls share no target state. Use `latest` deliberately for exploratory/theoretical requests or an unknown target; it does not establish consumer compatibility. Retrieve full detail and check prerequisites before applying guidance.
- Omitted/empty area filter means all areas. Multiple requested areas match any of them. Multiple languages similarly match any requested language.
- `languages` matches languages demonstrated by the recipe's examples, plus applicable genuinely language-neutral configuration/build recipes under Round 8. Broader API language applicability is recorded separately; a C#-only programming example does not satisfy an F# filter. Label neutral results explicitly. Language-specific project settings/directives are not neutral just because their files contain configuration.
- With an area filter, `includeShared: true` also matches relevant shared language/library/SDK recipes through explicit applicability metadata. False limits area matching to directly tagged areas. Deduplicate shared matches. Without an area filter, all areas remain eligible regardless of this switch.
- Different kinds of filters combine: for example, a recipe must match an area, a requested language, the target, and the keywords when all are supplied.
- Keyword matching is deterministic, case-insensitive, and uses recipe titles, goals, tags, and API names. The agent interprets natural language; the server does not host an AI search system.
- Page size: 20 summaries by default, at most 50. Use stable ordering for identical arguments and return the next offset when more results exist.
- Summaries contain identity, goal, areas, key applicability/status, and enough information to choose detail. Do not put full examples in catalogue responses.
- Include the supported area identifiers in discovery metadata so clients can form valid filters without a third tool.

These are filtering rules, not a claim that an application has been inspected. A caller can request more than one area for shared language/library guidance. Retagging a recipe does not duplicate its content.

Shared inclusion only broadens area matching. It does not bypass target, the accepted language/neutral matching rules, keyword, or explicit platform constraints and does not admit unrelated platform guidance.

## Recipe identity and release editions

Use stable, descriptive goal IDs, such as `aspnet-core.validate-requests`, independent of the release number. Exact names are examples, not seeded content.

`get_recipe` requires an ID and `targetVersion` (a supported release alias or `latest`), independently of any discovery call. Select the guidance edition applicable to that target. Reuse one canonical edition across explicitly declared, source-supported and independently verified targets when its guidance and feature requirements are unchanged. A new target alone does not need a new edition. Add a newer edition for a real guidance or requirements change, preserving historical editions; prefer the newest edition explicitly declaring the requested target when it supersedes older guidance. Never return an incompatible newer edition for an explicit older target.

Introduction release is separate from target prerequisites. Include a .NET 11-era recipe for an older supported target when the exact SDK/package route is documented and verified. Return those requirements prominently without inferring that the caller has installed them. Do not infer supported language/runtime compatibility merely because syntax compiles. The curated .NET 10 historical backfill remains eight recipes; verified older-target routes for .NET 11-era capabilities are a distinct applicability matter.

## Complete response objects

### Requirements and verification baselines

Each applicability route has a `usage` (`primary` or `optional-alternative`) and a `requirements` object. Alternatives are separate choices, not cumulative prerequisites. `conditional` retains its release-routing meaning independently of usage. Put the normal route first. The route's `target` is the selected application release; `introducedIn` identifies when the capability arrived.

Runtime, SDK and language requirements have `name`, `version`, `match` and `reason`. Packages and workloads use arrays of the same objects. `match: release-family` means the named major.minor family and its supported servicing/artifact versions, including RC artifacts where explicitly disclosed. It is neither a minimum nor permission to use later families. `match: exact` means that specific version or language mode is necessary for the declared route. A reason and declared official route sources must justify the condition independently of a successful build. Do not turn a test pin into an exact requirement or infer an open-ended minimum. This contract does not resolve versions or inspect a consumer.

Frameworks, package assets, providers, platforms, rendering modes, prerequisites, constraints, experimental opt-ins and stability limitations remain explicit inside `requirements`. The property recipe uses .NET 10 runtime/SDK family requirements and C# 14 mode; the newer SDK route is an optional alternative retaining the older target. Exact SDK pins are not part of these family requirements. .NET 11 routes disclose RC-only execution and pending GA reconciliation independently of stable C# syntax.

`evidenceSamples` links routes to the existing complete `evidence` records in detail, labelled `role: verification-baseline`. Those records retain exact target/SDK/language, measured runtime output, dependencies/workloads/native versions, commands, input hashes, source identity and freshness. Discovery carries requirements and evidence links, not duplicate evidence records. A baseline reports what ran; it does not establish minimum versions or broaden supported targets. No freshness or evidence-identity rules are relaxed. Structured content, its generated output schema and the complete JSON-text fallback expose the same distinction.

Generation rejects legacy prose requirements, malformed family pins, missing justification and SDK/language requirements contradicting linked baselines. Source review still establishes support: structural checks cannot prove prose, package assets or provider/platform claims. Replacing flat applicability fields with `requirements` is an incompatible (`major`) contract change under the accepted SemVer policy. Development packages remain unpublished candidates.

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

## Technical defaults for implementation

These fill in routine details of the accepted contract without adding another tool or service:

- Initially support consumer targets `10.0` (curated foundations and verified conditional routes) and `11.0` (the comprehensive release-family commitment). A .NET 9 comparison sample does not add .NET 9 catalogue support. Discovery must disclose curated versus comprehensive coverage; empty results do not prove an older release has no eligible capabilities.
- Normalize `10`/`10.0`/`net10.0` and corresponding .NET 11 aliases to the canonical release. Return a helpful invalid/unsupported-target response for patch/RC strings or platform-qualified TFMs, explaining how to choose the target release and inspect platform prerequisites. Never silently drop an explicit platform/RC condition and claim it was checked.
- Treat blank keyword input as no keyword filter; match all whitespace-separated terms case-insensitively against the accepted title/goal/tag/API fields. Keep ordering ordinal by stable recipe ID before paging. Reject invalid negative offsets and out-of-range page limits rather than silently changing them.
- Return supported targets, area identifiers, example-language identifiers, requested/resolved target, `targetAssumed: false`, total matches and next offset with discovery. All metadata describes installed content, not a probed consumer environment.
- Keep applicability conditions explicit per verified route. A newer SDK/package route is not unconditional runtime compatibility, and future target releases are not inferred from an open-ended minimum version.
- Use the standard SDK tool/output-schema/error mechanisms and read-only annotations. Preserve protocol-only stdout and stderr diagnostics. Exact DTOs/internal error-code names are implementation details, with public compatibility governed by the accepted SemVer policy.

Round 8 accepts applicable genuinely language-neutral configuration/build recipes as an explicit exception to strict example-language matching. No additional toggle or MCP tool is needed.

Requiring `targetVersion` is an incompatible public contract change, classified `major` under the existing independent package SemVer policy. Existing clients must repeat a concrete release or deliberate `latest` on both tools. Development packages remain unpublished `1.0.0-preview.N` candidates; this change neither publishes a package nor bypasses the first-publication gates.
