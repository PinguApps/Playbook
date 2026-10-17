# Playbook Linear proposal

Proposal awaiting approval; no Linear mutations performed.

Editorial note (17 October 2026): display wording was clarified after publication. The original approved proposal remains at commit b3778cc. These repository edits do not update published Linear issues or change their verified metadata and dependencies.

Destination: **PinguApps → PinguApps (PIN) → Playbook**, explicitly confirmed. Primary source: [SPEC.md](../../SPEC.md), source commit `0a8989814ea11346354464b845c79c15bb71c5e9`. Supporting inventories clarify the agreed scope; they do not add standalone products or features.

## Review scope

Propose **303 new Backlog tickets across 10 new milestones**: 269 Agent and 34 Human. No existing in-project milestones or issues were found, including completed/archived results. No Linear writes have occurred.

Approve the milestone names/order, ticket order and PR granularity, criteria, executor/labels, full-scope estimates, source provenance, overlap decisions and minimal native blockers. This approval would authorize backlog publication only. Implementation, credentials/resource provisioning, merging, releasing and schedule activation remain future work.

The configured estimate scale is T-Shirt: XS=1, S=2, M=3, L=5, XL=8; extended values are disabled. Values below are native Linear estimates, not days. No estimate setting or project date is changed. No assignee, delegate, cycle, issue date or milestone date will be added.

Each Agent ticket is one complete PR boundary sized for one fresh context. Content includes original old/new samples, prose/metadata, actual compilation, focused claim-specific proof and installed-MCP retrieval. Human work is an access, proof, judgement or launch action. Native Agent work is conditional on a real executable route supplied by its stated access prerequisite.

Source audits and GA deltas can reveal additional qualifying recipes. Audit outcomes explicitly propose narrow estimated follow-ons for review; no fictional API or arbitrary-sized catch-all implementation ticket. Every qualifying follow-on remains a publication blocker until delivered. Work requiring a larger fresh context must be split before execution.

First-public publication, even a preview, retains the comprehensive .NET 11 plus exact 8-foundations gate. GA checks compare complete prepared family guidance once actual final artifacts exist; an unavailable GA release is an explicit external wait, never RC-as-GA proof. Tiny Windows/macOS tool smokes stay tiny.

## Milestone order

| New milestone | Tickets | Agent / Human |
| --- | ---: | ---: |
| Installed documentation server | 8 | 8 / 0 |
| Verified authoring baselines | 35 | 23 / 12 |
| Language and library guidance | 48 | 48 / 0 |
| Build and deployment guidance | 13 | 13 / 0 |
| Web application guidance | 70 | 70 / 0 |
| Data access guidance | 48 | 48 / 0 |
| Desktop guidance | 9 | 7 / 2 |
| Mobile and native guidance | 39 | 36 / 3 |
| Clients and release readiness | 23 | 8 / 15 |
| GA reconciliation and stable launch | 10 | 8 / 2 |

Numbers restart at 000 within each milestone. They are display order, not dependencies. Only listed blockers become native blockedBy edges; independent tasks do not wait for milestone completion.

## Overlaps and omissions

- The first installed slice owns the field-backed-property foundation; no duplicate language ticket.
- The synchronous Minimal API and EF named-filter foundations each have one canonical delivery ticket, distinct from .NET 11 integration.
- SDK mobile-test and native app-test rows share one native delivery owner; no duplicate mobile test matrix.
- Web container rows link the shared container recipe, with no second Blazor-specific publishing recipe.
- Shared validation, unions and telemetry explanations remain canonical; separate web integrations retain their own real framework evidence. Crosslinks alone do not create false blockers.
- OpenAPI, grid, list, startup and complex-model/JSON bundles were split into separate goal PRs. Removed-member and obsolete inventories are bounded source audits with actual delivery follow-ons, not claims of completed coverage.
- Owner-only trust, PMC, identity, device/settings/AI/map proof is separate from compiled Agent examples. Proof actions update evidence before publication, avoiding circular sample/proof dependencies.
- The optional manual preview decision never blocks stable. Stable requires complete GA reconciliation, full coverage, compile/package/client/evaluation evidence and owner publication.
- Same-team matches in other projects are prior art only; none is reused, edited or counted as Playbook delivery. No existing in-project item is renamed, moved or closed.

## Initial parallel frontier

Unblocked Agent work:

- [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)
- [Verified authoring baselines / 005 - Resolve remaining F# runtime and compiler-service contracts](#sh-q01)
- [Verified authoring baselines / 006 - Resolve C#/VB compiler migrations and VB coverage](#sh-q02)
- [Verified authoring baselines / 007 - Resolve numeric and parsing compatibility findings](#sh-q03)
- [Verified authoring baselines / 008 - Resolve archive, path, handle and clock migrations](#sh-q04)
- [Verified authoring baselines / 009 - Resolve hosting and configuration failure migrations](#sh-q05)
- [Verified authoring baselines / 010 - Resolve cryptography platform migrations](#sh-q06)
- [Verified authoring baselines / 011 - Resolve native deployment-artifact migrations](#sh-q07)
- [Verified authoring baselines / 012 - Resolve SDK/package hosts and affected-test availability](#sh-q08)
- [Verified authoring baselines / 013 - Map remaining obsolete diagnostics to bounded family owners](#sh-q09)
- [Verified authoring baselines / 014 - Recheck explicit automatic/no-change exclusions](#sh-q10)
- [Verified authoring baselines / 015 - Bound the removed Blazor API migration families](#web-w18b)
- [Verified authoring baselines / 016 - Resolve DBSC release availability and record its disposition](#web-w59)
- [Verified authoring baselines / 017 - Reconcile WindowsDesktop public changes and retained exclusions](#nat-r01)
- [Verified authoring baselines / 018 - Reconcile MAUI and platform API breakage gaps](#nat-r02)
- [Verified authoring baselines / 019 - Pin actual mobile composition and stability status](#nat-r03)
- [Verified authoring baselines / 027 - Reconcile data text numeric and I/O API coverage](#api-dataio)
- [Verified authoring baselines / 028 - Reconcile serialization API coverage](#api-json)
- [Verified authoring baselines / 029 - Reconcile runtime transport and security API coverage](#api-runtime)
- [Verified authoring baselines / 030 - Reconcile Extensions API coverage](#api-extensions)
- [Verified authoring baselines / 031 - Reconcile SDK build and package authoring coverage](#api-build)
- [Verified authoring baselines / 032 - Reconcile Blazor and Razor API coverage](#api-blazor)
- [Verified authoring baselines / 033 - Reconcile non-Blazor web API coverage](#api-web)
- [Verified authoring baselines / 034 - Reconcile EF and first-party provider API coverage](#api-ef)

Initial unblocked Human access/decisions (never labelled Agent):

- [Verified authoring baselines / 000 - Provide an isolated SQL Server verification route](#h-sql)
- [Verified authoring baselines / 001 - Provide an isolated Cosmos verification route](#h-cosmos)
- [Verified authoring baselines / 002 - Confirm a suitable portable native-capability authoring route](#h-special-host)
- [Verified authoring baselines / 004 - Provide controlled Windows web and design-time verification access](#h-windows-web)
- [Verified authoring baselines / 020 - Provide an actual Windows native authoring route](#nat-h-windows-route)
- [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route)
- [Verified authoring baselines / 024 - Provide separate authorized map verification resources](#nat-h-map-resource)
- [Verified authoring baselines / 025 - Provide passkey trust signing and device prerequisites](#nat-h-passkey-resource)
- [Verified authoring baselines / 026 - Provide eligible Apple AI hardware model and access](#nat-h-apple-ai-resource)
- [Clients and release readiness / 012 - Freeze the single-client comparison protocol](#h-eval-protocol)

After the first installed slice and the relevant actual access prerequisites, independent content families can proceed concurrently. Shared descriptions do not add artificial phase gates. Every actual predecessor is earlier in the complete order.

## Complete ticket proposal

## Installed documentation server

**New native milestone.** An installed offline tool serves complete release-aware guidance through the two agreed MCP tools, with focused delivered-artifact checks.

<a id="core-slice"></a>

### 000 - Install and retrieve an original property recipe over stdio MCP

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** A narrow but complete first tracer bullet spans repository conventions, one original recipe, generated assets, packaging and a real installed stdio lifecycle.

**Blockers:** None — independent.

**Delivered outcome:** An installed playbook tool serves one wholly original .NET 10 field-backed-property foundation, including its honest .NET 9 comparison, without the checkout or a network connection.

**Acceptance criteria:**

- [ ] Apply the agreed .slnx/src/tests/shared-MSBuild layout, CPM with version overrides disabled, analyzers, nullable/warnings-as-errors and PinguApps style. Keep one production console project and one Reqnroll test project; samples have independent pins.
- [ ] Package PinguApps.Playbook with command playbook, net10.0 host and Major roll-forward. Bare invocation/help/version work; playbook mcp starts a client-owned stdio process with protocol-only stdout and stderr diagnostics. Require no workloads, accounts, keys or runtime network access.
- [ ] Expose exactly list_recipes/get_recipe using the standard MCP SDK, read-only annotations and output schemas. Return the same complete object in structured content and JSON text fallback, including Markdown, applicability, sources and evidence.
- [ ] Create the minimum agreed extensible recipe/edition schema and JSON/Markdown/canonical-sample build pipeline; extract snippets and embed generated content. The property examples preserve identical normalization/validation and explain recursion/initialization fairly.
- [ ] Compile actual .NET 9/10 comparison samples. A focused Reqnroll scenario installs the actual package from an isolated feed/cache outside the checkout, initializes MCP, discovers tools, retrieves body plus constraints offline and shuts down cleanly. No separately published core library.
- [ ] Basic stable-ID and declared-target retrieval must be driven by embedded metadata so independent recipe PRs can add editions without changing transport or host code. CORE_RELEASE adds the complete normalization, conditional-route and error semantics.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 29, 31, 32, 35, 36, 37, 38, 39, 40, 41, 44, 45, 86, 126, 127, 130, 133, 134, 140, 150, 159. Supporting inventory references: `csharp.validate-property-assignment`.

<a id="core-release"></a>

### 001 - Resolve release editions and expose complete applicability

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Includes implementation, focused verification and evidence for this complete outcome.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Agents receive release-appropriate guidance and explicit requirements without Playbook inspecting their project.

**Acceptance criteria:**

- [ ] Normalize 10/10.0/net10.0 and 11 equivalents; omitted target selects the newest release in the installed catalogue and echoes that it was assumed. Disclose curated .NET 10 versus comprehensive .NET 11 coverage.
- [ ] Preserve stable goal IDs and retained editions; choose a newer compatible edition only when applicable. Separate introduction release from SDK/runtime/language/package-asset/provider/platform/rendering conditions, including verified older-target routes.
- [ ] Return helpful standard machine-readable failures for unknown IDs, unsupported or patch/RC/platform-qualified target strings, and known recipes with no applicable edition. Never silently relax an explicit target.
- [ ] Focused packed-MCP scenarios prove retained editions, assumed disclosure, an explicit incompatible target failure and a documented conditional route; .NET 9 remains comparison-only.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 25, 26, 27, 28, 29, 30, 31, 32, 33, 41, 42, 43, 44, 127.

<a id="core-filter"></a>

### 002 - Discover guidance with deterministic combined filters and paging

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Includes implementation, focused verification and evidence for this complete outcome.

**Blockers:** [Installed documentation server / 001 - Resolve release editions and expose complete applicability](#core-release)

**Delivered outcome:** Agents can narrow a large catalogue predictably while retaining relevant shared guidance.

**Acceptance criteria:**

- [ ] Implement optional areas/languages/query/includeShared/offset/limit. OR within area/language sets; AND between types. Empty areas means all; includeShared defaults true through explicit applicability and can be disabled.
- [ ] Match demonstrated languages plus explicitly applicable genuinely neutral configuration recipes; do not call language-specific source/directives neutral. Shared inclusion does not bypass other constraints or admit unrelated platforms.
- [ ] Require all nonblank case-insensitive whitespace query terms in title/goal/tags/API names. Sort ordinal stable IDs, default limit20/max50, total/next offset, supported target/area/language metadata and concise applicable summaries.
- [ ] Packed-MCP scenarios cover the meaningful combined/shared/neutral rules, stable page traversal, helpful invalid filters/ranges and successful zero matches without a broad implementation-mirroring suite.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 31, 32, 33, 34, 43, 127.

<a id="core-validate"></a>

### 003 - Reject incomplete metadata and drifting served snippets during pack

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Includes implementation, focused verification and evidence for this complete outcome.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Malformed or incomplete recipe sources fail verification before a package is accepted.

**Acceptance criteria:**

- [ ] Validate IDs/editions, source references, explicit prerequisite routes, experimental labels and canonical snippet references during build/pack; reject duplicates, missing sources/snippets and incompatible edition metadata.
- [ ] Generate discovery data and full offline Markdown from the same canonical samples; reject missing essential implementation steps rather than requiring a live reference page.
- [ ] Use a small meaningful invalid-content fixture and a real packed positive case; do not add a separate catalogue service or a broad edge-case test suite.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 4, 35, 36, 41, 42, 43, 44, 45, 46, 47, 130, 138, 139, 159.

<a id="core-evidence"></a>

### 004 - Preserve sample evidence and detect stale verification inputs

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Includes implementation, focused verification and evidence for this complete outcome.

**Blockers:** [Installed documentation server / 003 - Reject incomplete metadata and drifting served snippets during pack](#core-validate)

**Delivered outcome:** Contributors can reuse unchanged compilation evidence while changed inputs visibly require refreshed proof.

**Acceptance criteria:**

- [ ] Record exact source/sample identity, targets/dependencies, commands and SDK/workload/native versions; distinguish official review, compilation, cross-compilation, mocks and real execution.
- [ ] Validate evidence freshness against relevant source/sample/target/dependency/toolchain inputs and surface missing/stale compilation before publication. Native evidence can be supplied from suitable authoring machines; no native recurring matrix.
- [ ] Prove one unchanged evidence record remains valid and one relevant changed input requires refresh, then retrieve the evidence/applicability through the actual packed tool. Targeted Human proof may remain pending in incremental work but blocks release readiness.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 44, 45, 46, 130, 131, 132, 133, 134, 138, 158, 160.

<a id="core-ci"></a>

### 005 - Validate the same packed tool on supported host environments

**New · Agent · L (5) · Labels: Agent, Deployment**

**Estimate rationale:** Real cross-OS artifact consumption, isolated runtime compatibility and reusable/PR workflow integration need focused end-to-end verification.

**Blockers:** [Installed documentation server / 002 - Discover guidance with deterministic combined filters and paging](#core-filter); [Installed documentation server / 004 - Preserve sample evidence and detect stale verification inputs](#core-evidence)

**Delivered outcome:** PR validation establishes the delivered package on Linux and the two agreed tiny desktop smokes.

**Acceptance criteria:**

- [ ] Adapt reusable validation and PR-validation roles from BlazorSitemap/PinguApps with SHA-pinned actions, Blacksmith Linux, superseded-run cancellation and useful artifact/TRX reporting.
- [ ] Pack once and pass the exact identified artifact to focused Linux checks and one brief Windows and one brief macOS install/start/initialize/list/get/shutdown smoke. Isolated feed/cache and unrelated working directory must select the new artifact.
- [ ] Use actual appropriately isolated .NET 10 hosting and .NET 11-only major roll-forward environments; record runtime evidence rather than project settings alone.
- [ ] Measure/report runner time as well as elapsed validation time. Keep native samples/UI/devices/emulators and client-by-OS matrices out of recurring CI.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 8, 9, 10, 126, 127, 128, 129, 140, 141, 143, 160.

<a id="author-docs"></a>

### 006 - Document original recipe contribution and evidence refresh

**New · Agent · M (3) · Labels: Agent, Documentation**

**Estimate rationale:** Includes implementation, focused verification and evidence for this complete outcome.

**Blockers:** [Installed documentation server / 004 - Preserve sample evidence and detect stale verification inputs](#core-evidence)

**Delivered outcome:** A contributor can add a complete recipe using the agreed standards without guessing the verification boundary.

**Acceptance criteria:**

- [ ] Document metadata/editions, canonical snippet extraction, previous-release comparisons, original example/prose requirements, source attribution and runtime/SDK/package/platform/stability distinctions.
- [ ] Show the actual authoring/build/pack workflow, provider/native handoff, freshness rules and meaningful claim-specific evidence. Explain that missing compilation blocks publication and audits do not substitute for recipe delivery.
- [ ] Walk through the seed recipe using the documented commands; keep the host workload-free and native evidence out of broad CI.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 130, 131, 132, 133, 134, 138, 159, 160.

<a id="client-docs"></a>

### 007 - Document offline installation and four MCP client connections

**New · Agent · M (3) · Labels: Agent, Documentation**

**Estimate rationale:** Includes implementation, focused verification and evidence for this complete outcome.

**Blockers:** [Installed documentation server / 001 - Resolve release editions and expose complete applicability](#core-release)

**Delivered outcome:** Developers can install/update the package, connect a supported client and tell their agent when to consult it.

**Acceptance criteria:**

- [ ] Document global/local tool install/update, playbook mcp, help/version, host roll-forward, offline completeness limits and troubleshooting; do not edit client configuration automatically.
- [ ] Provide accurate setup instructions for T3 Code, Codex CLI, Claude Code and VS Code Copilot plus a concise companion consultation instruction.
- [ ] Check documented launch commands against the actual packed tool; leave real client-version evidence to the four explicit checks.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 1, 2, 4, 5, 6, 7, 8, 9, 11, 12, 13, 135, 136, 137, 139.

## Verified authoring baselines

**New native milestone.** Source uncertainties and actual authoring access are explicit, with accountable dispositions and human prerequisites.

<a id="h-sql"></a>

### 000 - Provide an isolated SQL Server verification route

**New · Human · M (3) · Labels: Infrastructure, Platform**

**Estimate rationale:** Owner-controlled resource/access action with version and feature uncertainty.

**Blockers:** None — independent.

**Delivered outcome:** SQL Server-specific recipes can use an authorized compatible database without touching production.

**Acceptance criteria:**

- [ ] Confirm an accessible isolated instance/container and the exact server/provider features and versions needed by the approved SQL recipes, including vector/full-text/JSON prerequisites where used.
- [ ] Supply an agent-executable safe test route and approved credential handling outside tickets/source; record actual capabilities and cleanup ownership. Provisioning is a future owner action, not established access.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 115, 116, 130, 134, 151, 160.

<a id="h-cosmos"></a>

### 001 - Provide an isolated Cosmos verification route

**New · Human · M (3) · Labels: Infrastructure, Platform**

**Estimate rationale:** Owner-controlled provider access and real compatibility checks.

**Blockers:** None — independent.

**Delivered outcome:** Cosmos-specific behavior can be checked against a real authorized supported environment.

**Acceptance criteria:**

- [ ] Confirm an isolated supported account/emulator route, actual API/provider/service capabilities and test-data lifecycle.
- [ ] Provide approved agent-executable access without publishing secrets or using production data; record limitations rather than treating mocks as Cosmos execution.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 115, 117, 130, 134, 151, 160.

<a id="h-special-host"></a>

### 002 - Confirm a suitable portable native-capability authoring route

**New · Human · M (3) · Labels: Platform**

**Estimate rationale:** A bounded owner decision establishing actual special-capability access; per-recipe checks stay in their content tickets.

**Blockers:** None — independent.

**Delivered outcome:** Architecture-specific SIMD, QUIC/TLS and native artifact claims have an actual compilation/execution route where required.

**Acceptance criteria:**

- [ ] Review the assigned current runtime/transport examples and record accessible OS/architecture, supported instructions, native libraries and container engine capabilities.
- [ ] Authorize a suitable existing isolated route or explicitly identify a missing capability and owner handoff. Naming a machine does not prove support; avoid a new hardware/device CI matrix.
- [ ] Make the required command-driven checks agent-executable where supplied. An unavailable route remains a delivery prerequisite, not waived evidence.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 100, 101, 103, 106, 130, 133, 134, 151, 160.

<a id="h-entra"></a>

### 003 - Provide an authorized Entra SQL test identity

**New · Human · M (3) · Labels: Security**

**Estimate rationale:** Owner-controlled identity and authorization action with an actual SQL test route.

**Blockers:** [Verified authoring baselines / 000 - Provide an isolated SQL Server verification route](#h-sql)

**Delivered outcome:** The SQL authentication sample can use a real approved test identity and isolated target.

**Acceptance criteria:**

- [ ] Choose an authorized test identity and supported Entra/SQL route matching the original authentication sample; establish the needed test permissions and noninteractive access where actually supported.
- [ ] Keep credential material outside tickets/source and record exact scope and access limitations. Mocked tokens do not establish real Entra authentication; unavailable owner-only login remains a named Human handoff.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 109, 116, 134, 151, 160.

<a id="h-windows-web"></a>

### 004 - Provide controlled Windows web and design-time verification access

**New · Human · M (3) · Labels: Platform**

**Estimate rationale:** Focused owner access and authorization handoff for three explicitly Windows-only claims.

**Blockers:** None — independent.

**Delivered outcome:** Windows-only HTTP.sys, WSL trust and Visual Studio PMC checks have an explicit real access route.

**Acceptance criteria:**

- [ ] Confirm an isolated Windows/WSL and HTTP.sys HTTPS setup plus actual Visual Studio Package Manager Console availability for the approved examples; record precise tool/OS and permission limits.
- [ ] Provide a genuinely agent-executable path where possible; owner-controlled trust changes and interactive PMC execution remain separate Human proof actions. Do not alter production or owner-wide trust merely to claim access.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 109, 112, 118, 130, 134, 151, 160.

<a id="sh-q01"></a>

### 005 - Resolve remaining F# runtime and compiler-service contracts

**New · Agent · L (5) · Labels: Agent, Research**

**Estimate rationale:** Several unresolved F# contracts

**Blockers:** None — independent.

**Delivered outcome:** Resolve and disposition the assigned official-source findings, preserving any qualifying delivery work: resolve remaining f# runtime and compiler-service contracts.

**Acceptance criteria:**

- [ ] Separate shipped APIs from compiler-test prototypes using exact official source and distributable assets.
- [ ] Identify exact package/assets, AOT/FSI limits and all qualifying delivery slices.
- [ ] For every confirmed additional goal, draft a narrow fully verified delivery follow-on and present it for backlog approval before creation or implementation; source-audit closure does not complete recipe coverage.

**Context:** Independent source-only audit. Record exact source/API evidence and included, grouped, excluded or unresolved outcomes. Existing accepted scope is not narrowed; unavailable APIs are not invented. Any qualifying content remains a delivery/publication obligation.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 47, 94, 158, 159. Supporting inventory references: `fsharp.author-runtime-async-builder`, `fsharp.configure-compiler-service-memory`, `fsharp.aot-and-fsi-lifetimes`.

<a id="sh-q02"></a>

### 006 - Resolve C#/VB compiler migrations and VB coverage

**New · Agent · L (5) · Labels: Agent, Research**

**Estimate rationale:** Compiler-source breadth

**Blockers:** None — independent.

**Delivered outcome:** Resolve and disposition the assigned official-source findings, preserving any qualifying delivery work: resolve c#/vb compiler migrations and vb coverage.

**Acceptance criteria:**

- [ ] Review relevant C#/VB compiler compatibility sources and API changes.
- [ ] Produce VB-native delivery follow-ons or evidenced no-change findings without inventing a language version.
- [ ] For every confirmed additional goal, draft a narrow fully verified delivery follow-on and present it for backlog approval before creation or implementation; source-audit closure does not complete recipe coverage.

**Context:** Independent source-only audit. Record exact source/API evidence and included, grouped, excluded or unresolved outcomes. Existing accepted scope is not narrowed; unavailable APIs are not invented. Any qualifying content remains a delivery/publication obligation.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 47, 93, 95, 158, 159. Supporting inventory references: `vb.release-review`, `languages.compiler-migrations`.

<a id="sh-q03"></a>

### 007 - Resolve numeric and parsing compatibility findings

**New · Agent · L (5) · Labels: Agent, Research**

**Estimate rationale:** Several numeric behavior migrations

**Blockers:** None — independent.

**Delivered outcome:** Resolve and disposition the assigned official-source findings, preserving any qualifying delivery work: resolve numeric and parsing compatibility findings.

**Acceptance criteria:**

- [ ] Review every named parsing, rounding, conversion, tensor and calendar finding against its full official mitigation.
- [ ] Map each authoring change to an existing numeric ticket or a separate narrow delivery proposal.
- [ ] For every confirmed additional goal, draft a narrow fully verified delivery follow-on and present it for backlog approval before creation or implementation; source-audit closure does not complete recipe coverage.

**Context:** Independent source-only audit. Record exact source/API evidence and included, grouped, excluded or unresolved outcomes. Existing accepted scope is not narrowed; unavailable APIs are not invented. Any qualifying content remains a delivery/publication obligation.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 47, 101, 158, 159. Supporting inventory references: `migration.numeric-and-parsing`.

<a id="sh-q04"></a>

### 008 - Resolve archive, path, handle and clock migrations

**New · Agent · L (5) · Labels: Agent, Research**

**Estimate rationale:** Heterogeneous but bounded compatibility notes

**Blockers:** None — independent.

**Delivered outcome:** Resolve and disposition the assigned official-source findings, preserving any qualifying delivery work: resolve archive, path, handle and clock migrations.

**Acceptance criteria:**

- [ ] Audit every listed archive, apphost-name, pipe, clock and custom-Type compatibility entry.
- [ ] Attach established cases to existing tickets and propose separate delivery slices for unrelated qualifying changes.
- [ ] For every confirmed additional goal, draft a narrow fully verified delivery follow-on and present it for backlog approval before creation or implementation; source-audit closure does not complete recipe coverage.

**Context:** Independent source-only audit. Record exact source/API evidence and included, grouped, excluded or unresolved outcomes. Existing accepted scope is not narrowed; unavailable APIs are not invented. Any qualifying content remains a delivery/publication obligation.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 47, 99, 103, 158, 159. Supporting inventory references: `migration.archive-streams`, `migration.paths-handles-and-clocks`.

<a id="sh-q05"></a>

### 009 - Resolve hosting and configuration failure migrations

**New · Agent · M (3) · Labels: Agent, Research**

**Estimate rationale:** Bounded hosting compatibility family

**Blockers:** None — independent.

**Delivered outcome:** Resolve and disposition the assigned official-source findings, preserving any qualifying delivery work: resolve hosting and configuration failure migrations.

**Acceptance criteria:**

- [ ] Establish actual BackgroundService, reload, callback, watcher and asynchronous-options changes from full official notes.
- [ ] Deduplicate with E04/E05 and web hosting work while preserving every uncovered qualifying case.
- [ ] For every confirmed additional goal, draft a narrow fully verified delivery follow-on and present it for backlog approval before creation or implementation; source-audit closure does not complete recipe coverage.

**Context:** Independent source-only audit. Record exact source/API evidence and included, grouped, excluded or unresolved outcomes. Existing accepted scope is not narrowed; unavailable APIs are not invented. Any qualifying content remains a delivery/publication obligation.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 47, 102, 158, 159. Supporting inventory references: `migration.hosting-config-reload`.

<a id="sh-q06"></a>

### 010 - Resolve cryptography platform migrations

**New · Agent · L (5) · Labels: Agent, Research**

**Estimate rationale:** Security and platform uncertainty

**Blockers:** None — independent.

**Delivered outcome:** Resolve and disposition the assigned official-source findings, preserving any qualifying delivery work: resolve cryptography platform migrations.

**Acceptance criteria:**

- [ ] Verify removed APIs, platform implementations, limits and documented alternatives.
- [ ] Allocate distinct authored migrations without blanket diagnostic suppression or unsupported algorithm replacements.
- [ ] For every confirmed additional goal, draft a narrow fully verified delivery follow-on and present it for backlog approval before creation or implementation; source-audit closure does not complete recipe coverage.

**Context:** Independent source-only audit. Record exact source/API evidence and included, grouped, excluded or unresolved outcomes. Existing accepted scope is not narrowed; unavailable APIs are not invented. Any qualifying content remains a delivery/publication obligation.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 47, 101, 158, 159. Supporting inventory references: `migration.crypto-platforms`.

<a id="sh-q07"></a>

### 011 - Resolve native deployment-artifact migrations

**New · Agent · M (3) · Labels: Agent, Research**

**Estimate rationale:** Bounded deployment compatibility family

**Blockers:** None — independent.

**Delivered outcome:** Resolve and disposition the assigned official-source findings, preserving any qualifying delivery work: resolve native deployment-artifact migrations.

**Acceptance criteria:**

- [ ] Audit runtime configuration precedence, NativeAOT naming and caller-assembly changes.
- [ ] Propose original compiling/behavior delivery slices with accurate platform prerequisites.
- [ ] For every confirmed additional goal, draft a narrow fully verified delivery follow-on and present it for backlog approval before creation or implementation; source-audit closure does not complete recipe coverage.

**Context:** Independent source-only audit. Record exact source/API evidence and included, grouped, excluded or unresolved outcomes. Existing accepted scope is not narrowed; unavailable APIs are not invented. Any qualifying content remains a delivery/publication obligation.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 47, 103, 106, 158, 159. Supporting inventory references: `migration.deploy-native-artifacts`.

<a id="sh-q08"></a>

### 012 - Resolve SDK/package hosts and affected-test availability

**New · Agent · L (5) · Labels: Agent, Research**

**Estimate rationale:** Several SDK compatibility routes and unavailable-asset risk

**Blockers:** None — independent.

**Delivered outcome:** Resolve and disposition the assigned official-source findings, preserving any qualifying delivery work: resolve sdk/package hosts and affected-test availability.

**Acceptance criteria:**

- [ ] Reconcile canonical notes, cumulative claims and actual distributable SDK/test assets.
- [ ] Removed or unshipped workflows receive evidenced dispositions; qualifying host, restore, template and test changes receive delivery proposals.
- [ ] For every confirmed additional goal, draft a narrow fully verified delivery follow-on and present it for backlog approval before creation or implementation; source-audit closure does not complete recipe coverage.

**Context:** Independent source-only audit. Record exact source/API evidence and included, grouped, excluded or unresolved outcomes. Existing accepted scope is not narrowed; unavailable APIs are not invented. Any qualifying content remains a delivery/publication obligation.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 47, 104, 105, 158, 159. Supporting inventory references: `migration.sdk-package-hosts`, `sdk.select-affected-tests`.

<a id="sh-q09"></a>

### 013 - Map remaining obsolete diagnostics to bounded family owners

**New · Agent · M (3) · Labels: Agent, Research**

**Estimate rationale:** Bounded index enumeration, deduplication and accountable owner mapping; semantic replacement analysis remains with the family owner.

**Blockers:** None — independent.

**Delivered outcome:** Every relevant official obsolete-index entry has an exact bounded review/delivery owner, with no unknown-sized catch-all recipe task.

**Acceptance criteria:**

- [ ] Enumerate the finite official .NET 11 obsolete/diagnostic index and assign each entry to an existing concrete migration ticket, language/native audit or one of the explicitly bounded API-family audits.
- [ ] Record exact official references and distinguish already covered entries from factual gaps. Do not redo family analysis or hide unrelated authored replacements inside this index task.
- [ ] The assigned family owner must resolve each gap and propose any narrow estimated recipe follow-on. This map can close when ownership is exhaustive; qualifying guidance stays incomplete until delivery and blocks publication.

**Context:** Independent source-only audit. Record exact source/API evidence and included, grouped, excluded or unresolved outcomes. Existing accepted scope is not narrowed; unavailable APIs are not invented. Any qualifying content remains a delivery/publication obligation.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 47, 99, 101, 103, 158, 159. Supporting inventory references: `migration.obsolete-api-index`.

<a id="sh-q10"></a>

### 014 - Recheck explicit automatic/no-change exclusions

**New · Agent · M (3) · Labels: Agent, Research**

**Estimate rationale:** Bounded thirteen-row disposition review

**Blockers:** None — independent.

**Delivered outcome:** Resolve and disposition the assigned official-source findings, preserving any qualifying delivery work: recheck explicit automatic/no-change exclusions.

**Acceptance criteria:**

- [ ] Confirm each assigned exclusion still lacks a qualifying authoring change against the actual release baseline.
- [ ] Record any reversal and its delivery owner rather than silently retaining a stale exclusion.
- [ ] For every confirmed additional goal, draft a narrow fully verified delivery follow-on and present it for backlog approval before creation or implementation; source-audit closure does not complete recipe coverage.

**Context:** Independent source-only audit. Record exact source/API evidence and included, grouped, excluded or unresolved outcomes. Existing accepted scope is not narrowed; unavailable APIs are not invented. Any qualifying content remains a delivery/publication obligation.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 47, 158, 159. Supporting inventory references: `fsharp.collection-compiler-speedups`, `regex.automatic-fixes`, `numerics.automatic-speedups`, `configuration.automatic-watcher-fixes`, `networking.automatic-benefits`, `runtime.automatic-improvements`, `runtime.automatic-wasm-diagnostics`, `runtime.automatic-reliability`, `sdk.cli-build-internals`, `sdk.cli-output-and-fixes`, `msbuild.automatic-improvements`, `nuget.automatic-pack-fixes`, `containers.rc-image-review`.

<a id="web-w18b"></a>

### 015 - Bound the removed Blazor API migration families

**New · Agent · M (3) · Labels: Agent, Research**

**Estimate rationale:** Bounded source/reference-asset reconciliation and concrete PR-sized follow-on design across a finite official removed-member inventory; product sample implementation is explicitly follow-on work.

**Blockers:** None — independent.

**Delivered outcome:** Every removed router, asset, validation, event, rendering and authentication integration family has an exact replacement path and a narrow, sized delivery follow-on; source audit closure does not claim shipped guidance.

**Acceptance criteria:**

- [ ] Read the complete official blazor-obsolete-apis-removed inventory and reconcile exact removed signatures against .NET 10 and current .NET 11 reference assets. Classify each router/assets/validation/events/rendering/auth family as a changed authoring path, unchanged replacement, or factual unresolved case, retaining source provenance.
- [ ] Produce a bounded follow-on proposal for each distinct replacement goal/family requiring authored guidance: title, exact removed/supported APIs, wholly original previous/current consumer scenario, applicability, actual compile/focused behavior proof and installed-MCP acceptance. Size each delivery PR on the native scale; split any XL outcome rather than hide it inside this audit.
- [ ] Keep blazor.replace-removed-api and its included inventory row visibly outstanding until every required follow-on delivers real examples and package retrieval. Audit completion only closes API reconciliation; it does not satisfy recipe coverage, compilation, behavior or publication gates.

**Context:** This is a source/API audit, not a complete recipe delivery ticket. Content commonAcceptance gates apply to every resulting implementation follow-on, not as a claim that this audit shipped them. Source reconciliation can start immediately. No CORE_SLICE dependency is needed to enumerate APIs and propose concrete follow-ons; those delivery tickets will require installed MCP integration and their actual consumer-specific prerequisites. Do not introduce duplicate shared validation or language-concept content when identifying supported replacements. Crosslink the canonical concept; preserve framework integration-specific goals and exact behavior. Root must retain the qualifying delivery follow-ons as outstanding comprehensive-coverage work and create/approve their concrete scope through the normal backlog workflow before implementation.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.replace-removed-api`, `WEB-089`.

<a id="web-w59"></a>

### 016 - Resolve DBSC release availability and record its disposition

**New · Agent · S (2) · Labels: Agent, Research**

**Estimate rationale:** Bounded availability/source-and-artifact audit; runnable recipe work is conditional and must be separately scoped if availability returns.

**Blockers:** None — independent.

**Delivered outcome:** The candidate has an evidence-backed final-release availability decision; no recipe teaches removed RC1 APIs.

**Acceptance criteria:**

- [ ] Reconcile cumulative, RC1, RC2 and current actual package/API artifacts; record that RC2 removed the implementation instead of excluding it merely for being experimental. Establish the current pinned availability disposition without pretending the GA release already exists.
- [ ] If still unavailable, close the source-disposition row as unavailable with official evidence and retain its audit history. If available again, define a separately sized original old/new recipe with actual browser/protocol prerequisites and compile/runtime proof before marking it included.
- [ ] Do not implement a fictional API or assume browser support. This audit may close its current RC availability finding with evidence; GA_WEB must separately recheck the actual final artifacts before stable publication.

**Context:** An unresolved release fact is not a product question for Pingu to answer. This audit can start independently; final closure may await GA artifacts.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 43, 44, 47, 109, 158, 159, 160. Supporting inventory references: `identity.bind-session-to-device`, `WEB-071`.

<a id="nat-r01"></a>

### 017 - Reconcile WindowsDesktop public changes and retained exclusions

**New · Agent · M (3) · Labels: Agent, Research**

**Estimate rationale:** Bounded source/reference/diagnostic audit across desktop milestones; no native execution prerequisite.

**Blockers:** None — independent.

**Delivered outcome:** WindowsDesktop source/API coverage has accountable dispositions.

**Acceptance criteria:**

- [ ] Resolve missing Preview2/3 paths through canonical history and reference/diagnostic evidence.
- [ ] Map qualifying changes to existing narrow slices or propose a separate concrete goal slice; retain justified exclusions without inventing WPF features.
- [ ] Update provenance and exact source conflicts; this audit does not claim sample execution.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 47, 95, 119, 120, 121, 158, 159, 160. Supporting inventory references: `inventory03:082:desktop.complete-public-api-audit`, `inventory03:017:desktop.automatic-reliability-fixes`.

<a id="nat-r02"></a>

### 018 - Reconcile MAUI and platform API breakage gaps

**New · Agent · L (5) · Labels: Agent, Research**

**Estimate rationale:** Cross-workload member/disposition reconciliation beyond release highlights; source conclusions only, with implementation kept out of audit scope.

**Blockers:** None — independent.

**Delivered outcome:** Remaining platform API and excluded-source findings have concrete dispositions.

**Acceptance criteria:**

- [ ] Audit actual API lists for converter culture, screenshot/backend seams, Font/nullable-entry changes and Apple removals.
- [ ] Retain release-family boundaries and justify automatic/contributor-only/native-catalogue exclusions.
- [ ] Attach eligible differences to an existing slice or propose a narrow new slice; removed APIs are not preserved as usable guidance.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 47, 122, 123, 124, 125, 158, 159, 160. Supporting inventory references: `inventory03:078:apple.remove-obsolete-native-bindings`, `inventory03:083:maui.complete-api-and-breakage-audit`, `inventory03:057:maui.automatic-size-and-ui-fixes`, `inventory03:068:android.automatic-build-and-startup-optimizations`, `inventory03:079:apple.build-without-full-xcode`, `inventory03:080:apple.independent-native-feature-catalogue`, `inventory03:081:apple.automatic-runtime-and-build-repairs`.

<a id="nat-r03"></a>

### 019 - Pin actual mobile composition and stability status

**New · Agent · M (3) · Labels: Agent, Research**

**Estimate rationale:** Cross-repository release artifact provenance and status reconciliation, without provisioning or native builds.

**Blockers:** None — independent.

**Delivered outcome:** Affected recipes have traceable artifact versions and honest status labels.

**Acceptance criteria:**

- [ ] Resolve Apple12211/12212 and rebuilt workload-set composition against published manifests/artifacts.
- [ ] Record actual preview/experimental status and cumulative overview conflicts rather than trusting one stale headline.
- [ ] Provide exact supported preparation baseline sets; compilation proof stays in content slices.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 41, 42, 43, 44, 123, 124, 125, 158, 159, 160. Supporting inventory references: `inventory03:085:mobile.resolve-artifact-and-status-baselines`, `inventory03:086:mobile.resolve-overview-lag`.

<a id="nat-h-windows-route"></a>

### 020 - Provide an actual Windows native authoring route

**New · Human · M (3) · Labels: Infrastructure, Platform**

**Estimate rationale:** Owner-controlled native access and tooling uncertainty; includes proving the route, not merely naming a machine.

**Blockers:** None — independent.

**Delivered outcome:** An agent can compile older/current WindowsDesktop and selected MAUI Windows samples through a documented real route.

**Acceptance criteria:**

- [ ] Confirm actual machine/access and authorize any needed changes; do not infer installed WindowsDesktop/MAUI toolchains from the host OS.
- [ ] Demonstrate an agent-executable build and controlled sample-launch route with exact SDK/workload versions and safe sample locations.
- [ ] Document unavailable UI/settings permissions and human handoff; this is not a request to add native CI.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 131, 132, 151, 160.

<a id="nat-h-android-route"></a>

### 021 - Provide an actual Android native authoring route

**New · Human · L (5) · Labels: Infrastructure, Platform**

**Estimate rationale:** Multiple independently versioned toolchains and a real execution handoff; unknown current access must be resolved.

**Blockers:** None — independent.

**Delivered outcome:** An agent can compile older/current Android examples and use the explicitly available controlled runtime route.

**Acceptance criteria:**

- [ ] Confirm accessible machine, supported .NET/workload/JDK/Android SDK composition and authorize required access/installations.
- [ ] Demonstrate agent-executable compilation and controlled adb/emulator/device execution where runtime proof needs it, recording actual limits.
- [ ] Reserve credentials, physical approval and unavailable native interactions for named human actions; no device/CI matrix.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 131, 132, 151, 160.

<a id="nat-h-apple-route"></a>

### 022 - Provide a matched Mac and Xcode authoring route

**New · Human · L (5) · Labels: Infrastructure, Platform**

**Estimate rationale:** Physical/native environment and multiple supported version pairings require owner action and real build-route proof.

**Blockers:** [Verified authoring baselines / 019 - Pin actual mobile composition and stability status](#nat-r03)

**Delivered outcome:** An agent can compile actual Apple examples against supported older/current toolchain combinations.

**Acceptance criteria:**

- [ ] Confirm actual Mac access, supported host OS, matching Xcode and .NET/workload versions; current RC2 requires Xcode27/macOS26.6+, subject to R03 reconciliation.
- [ ] Demonstrate an agent-executable native build/publish route and document any controlled simulator/app execution route, safe paths and missing permissions.
- [ ] Do not treat contributor configure --no-xcode or managed-reference-only compilation as complete app evidence; signing/trust actions remain human.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 124, 130, 131, 151, 160.

<a id="nat-h-evidence-owner"></a>

### 023 - Confirm native evidence handoff and refresh ownership

**New · Human · S (2) · Labels: Infrastructure, Platform**

**Estimate rationale:** Focused ownership/handoff decision with tangible evidence requirements.

**Blockers:** [Installed documentation server / 004 - Preserve sample evidence and detect stale verification inputs](#core-evidence)

**Delivered outcome:** Unavailable human steps and native evidence freshness have a named owner.

**Acceptance criteria:**

- [ ] Record which native actions require the owner and how their logs/artifacts reach the content PR.
- [ ] Use sample/source/input identity, exact commands, target/RID and toolchain versions in the existing evidence format.
- [ ] Reuse unchanged evidence only while relevant inputs remain unchanged; do not create a second CI or evidence system.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 131, 132, 133, 138, 151, 160. Supporting inventory references: `inventory03:087:mobile.verify-native-original-examples`.

<a id="nat-h-map-resource"></a>

### 024 - Provide separate authorized map verification resources

**New · Human · M (3) · Labels: Infrastructure, Platform**

**Estimate rationale:** Owner-controlled account/resource/credential action; availability unknown.

**Blockers:** None — independent.

**Delivered outcome:** Windows map behavior can be checked without reusing unrelated production resources.

**Acceptance criteria:**

- [ ] Choose an appropriate separate Azure Maps resource and approved credential delivery path.
- [ ] Confirm allowed sample/service activity and actual availability; do not place secrets in chat or committed files.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 122, 134, 151, 160. Supporting inventory references: `maui.enable-windows-maps`.

<a id="nat-h-passkey-resource"></a>

### 025 - Provide passkey trust signing and device prerequisites

**New · Human · L (5) · Labels: Infrastructure, Platform**

**Estimate rationale:** Security/trust and signing ownership plus device availability.

**Blockers:** None — independent.

**Delivered outcome:** A supported native ceremony can use a separate relying-party verification setup.

**Acceptance criteria:**

- [ ] Choose actual relying-party/domain/server and supported native target/device, with signing and associated-domain/Digital-Asset-Links setup as applicable.
- [ ] Provide approved access without assuming credentials or modifying production trust configuration.
- [ ] State exactly which platform route can be verified; one route does not establish every platform.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 109, 122, 134, 151, 160. Supporting inventory references: `maui.authenticate-with-platform-passkeys`.

<a id="nat-h-apple-ai-resource"></a>

### 026 - Provide eligible Apple AI hardware model and access

**New · Human · M (3) · Labels: Infrastructure, Platform**

**Estimate rationale:** Hardware/model/owner availability, not package authoring.

**Blockers:** None — independent.

**Delivered outcome:** A specific supported Apple AI execution route exists.

**Acceptance criteria:**

- [ ] Confirm supported hardware/OS/model availability and any owner-controlled capability or entitlement steps.
- [ ] State which compiled sample/API may run and how owner-generated evidence is handed back.
- [ ] Do not assume all devices or native APIs are supported because the MAUI package compiles.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 122, 124, 134, 151, 160. Supporting inventory references: `maui.use-apple-intelligence-abstractions`.

<a id="api-dataio"></a>

### 027 - Reconcile data text numeric and I/O API coverage

**New · Agent · L (5) · Labels: Agent, Research**

**Estimate rationale:** A bounded official source and actual artifact/API diff for one stated family; new recipe implementation is separately sized follow-on work.

**Blockers:** None — independent.

**Delivered outcome:** The remaining collections/LINQ, text/buffers, numerics, files/processes and archives have an accountable authoring-impact inventory beyond release-note headlines.

**Acceptance criteria:**

- [ ] Compare actual .NET 10 and pinned .NET 11 reference/public-tooling assets plus complete canonical new-feature and breaking-change sources for collections/LINQ, text/buffers, numerics, files/processes and archives. Record exact reviewed artifact/source versions; use mechanical diffing to bound review rather than relying on a summary page.
- [ ] Own uncovered public-API differences and obsolete-index entries in this family. Cross-reference already assigned source findings/migration tickets rather than redoing their analysis. Classify authored code/markup/scripts/build configuration/manifests or removable-workaround impact; justify grouping/exclusion and retain unknowns. gRPC or another no-change area may legitimately produce no recipe.
- [ ] For each uncovered qualifying goal propose a narrow fully verified recipe ticket, native-scale estimate, exact prerequisites and real evidence acceptance for review. Audit closure alone does not deliver missing content or permit publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 47, 96, 98, 99, 101, 158, 159, 160.

<a id="api-json"></a>

### 028 - Reconcile serialization API coverage

**New · Agent · M (3) · Labels: Agent, Research**

**Estimate rationale:** A bounded official source and actual artifact/API diff for one stated family; new recipe implementation is separately sized follow-on work.

**Blockers:** None — independent.

**Delivered outcome:** The remaining System.Text.Json and qualifying serialization/data-contract APIs have an accountable authoring-impact inventory beyond release-note headlines.

**Acceptance criteria:**

- [ ] Compare actual .NET 10 and pinned .NET 11 reference/public-tooling assets plus complete canonical new-feature and breaking-change sources for System.Text.Json and qualifying serialization/data-contract APIs. Record exact reviewed artifact/source versions; use mechanical diffing to bound review rather than relying on a summary page.
- [ ] Own uncovered public-API differences and obsolete-index entries in this family. Cross-reference already assigned source findings/migration tickets rather than redoing their analysis. Classify authored code/markup/scripts/build configuration/manifests or removable-workaround impact; justify grouping/exclusion and retain unknowns. gRPC or another no-change area may legitimately produce no recipe.
- [ ] For each uncovered qualifying goal propose a narrow fully verified recipe ticket, native-scale estimate, exact prerequisites and real evidence acceptance for review. Audit closure alone does not deliver missing content or permit publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 47, 97, 158, 159, 160.

<a id="api-runtime"></a>

### 029 - Reconcile runtime transport and security API coverage

**New · Agent · L (5) · Labels: Agent, Research**

**Estimate rationale:** A bounded official source and actual artifact/API diff for one stated family; new recipe implementation is separately sized follow-on work.

**Blockers:** None — independent.

**Delivered outcome:** The remaining runtime/native interop, networking/HTTP/DNS/QUIC/TLS and cryptography have an accountable authoring-impact inventory beyond release-note headlines.

**Acceptance criteria:**

- [ ] Compare actual .NET 10 and pinned .NET 11 reference/public-tooling assets plus complete canonical new-feature and breaking-change sources for runtime/native interop, networking/HTTP/DNS/QUIC/TLS and cryptography. Record exact reviewed artifact/source versions; use mechanical diffing to bound review rather than relying on a summary page.
- [ ] Own uncovered public-API differences and obsolete-index entries in this family. Cross-reference already assigned source findings/migration tickets rather than redoing their analysis. Classify authored code/markup/scripts/build configuration/manifests or removable-workaround impact; justify grouping/exclusion and retain unknowns. gRPC or another no-change area may legitimately produce no recipe.
- [ ] For each uncovered qualifying goal propose a narrow fully verified recipe ticket, native-scale estimate, exact prerequisites and real evidence acceptance for review. Audit closure alone does not deliver missing content or permit publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 47, 100, 101, 103, 158, 159, 160.

<a id="api-extensions"></a>

### 030 - Reconcile Extensions API coverage

**New · Agent · M (3) · Labels: Agent, Research**

**Estimate rationale:** A bounded official source and actual artifact/API diff for one stated family; new recipe implementation is separately sized follow-on work.

**Blockers:** None — independent.

**Delivered outcome:** The remaining configuration, DI/options/hosting, caching, validation and diagnostics have an accountable authoring-impact inventory beyond release-note headlines.

**Acceptance criteria:**

- [ ] Compare actual .NET 10 and pinned .NET 11 reference/public-tooling assets plus complete canonical new-feature and breaking-change sources for configuration, DI/options/hosting, caching, validation and diagnostics. Record exact reviewed artifact/source versions; use mechanical diffing to bound review rather than relying on a summary page.
- [ ] Own uncovered public-API differences and obsolete-index entries in this family. Cross-reference already assigned source findings/migration tickets rather than redoing their analysis. Classify authored code/markup/scripts/build configuration/manifests or removable-workaround impact; justify grouping/exclusion and retain unknowns. gRPC or another no-change area may legitimately produce no recipe.
- [ ] For each uncovered qualifying goal propose a narrow fully verified recipe ticket, native-scale estimate, exact prerequisites and real evidence acceptance for review. Audit closure alone does not deliver missing content or permit publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 47, 102, 158, 159, 160.

<a id="api-build"></a>

### 031 - Reconcile SDK build and package authoring coverage

**New · Agent · L (5) · Labels: Agent, Research**

**Estimate rationale:** A bounded official source and actual artifact/API diff for one stated family; new recipe implementation is separately sized follow-on work.

**Blockers:** None — independent.

**Delivered outcome:** The remaining SDK/MSBuild/NuGet/testing/container authored configuration and relevant public tooling APIs have an accountable authoring-impact inventory beyond release-note headlines.

**Acceptance criteria:**

- [ ] Compare actual .NET 10 and pinned .NET 11 reference/public-tooling assets plus complete canonical new-feature and breaking-change sources for SDK/MSBuild/NuGet/testing/container authored configuration and relevant public tooling APIs. Record exact reviewed artifact/source versions; use mechanical diffing to bound review rather than relying on a summary page.
- [ ] Own uncovered public-API differences and obsolete-index entries in this family. Cross-reference already assigned source findings/migration tickets rather than redoing their analysis. Classify authored code/markup/scripts/build configuration/manifests or removable-workaround impact; justify grouping/exclusion and retain unknowns. gRPC or another no-change area may legitimately produce no recipe.
- [ ] For each uncovered qualifying goal propose a narrow fully verified recipe ticket, native-scale estimate, exact prerequisites and real evidence acceptance for review. Audit closure alone does not deliver missing content or permit publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 47, 104, 105, 106, 158, 159, 160.

<a id="api-blazor"></a>

### 032 - Reconcile Blazor and Razor API coverage

**New · Agent · L (5) · Labels: Agent, Research**

**Estimate rationale:** A bounded official source and actual artifact/API diff for one stated family; new recipe implementation is separately sized follow-on work.

**Blockers:** None — independent.

**Delivered outcome:** The remaining Blazor/Razor/components/QuickGrid/AI/worker/gateway APIs and migrations have an accountable authoring-impact inventory beyond release-note headlines.

**Acceptance criteria:**

- [ ] Compare actual .NET 10 and pinned .NET 11 reference/public-tooling assets plus complete canonical new-feature and breaking-change sources for Blazor/Razor/components/QuickGrid/AI/worker/gateway APIs and migrations. Record exact reviewed artifact/source versions; use mechanical diffing to bound review rather than relying on a summary page.
- [ ] Own uncovered public-API differences and obsolete-index entries in this family. Cross-reference already assigned source findings/migration tickets rather than redoing their analysis. Classify authored code/markup/scripts/build configuration/manifests or removable-workaround impact; justify grouping/exclusion and retain unknowns. gRPC or another no-change area may legitimately produce no recipe.
- [ ] For each uncovered qualifying goal propose a narrow fully verified recipe ticket, native-scale estimate, exact prerequisites and real evidence acceptance for review. Audit closure alone does not deliver missing content or permit publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 47, 48, 73, 79, 80, 81, 82, 83, 84, 158, 159, 160.

<a id="api-web"></a>

### 033 - Reconcile non-Blazor web API coverage

**New · Agent · L (5) · Labels: Agent, Research**

**Estimate rationale:** A bounded official source and actual artifact/API diff for one stated family; new recipe implementation is separately sized follow-on work.

**Blockers:** None — independent.

**Delivered outcome:** The remaining Minimal APIs, MVC/Razor Pages, hosting/HTTP/caching, Identity/auth, SignalR/OpenAPI and gRPC have an accountable authoring-impact inventory beyond release-note headlines.

**Acceptance criteria:**

- [ ] Compare actual .NET 10 and pinned .NET 11 reference/public-tooling assets plus complete canonical new-feature and breaking-change sources for Minimal APIs, MVC/Razor Pages, hosting/HTTP/caching, Identity/auth, SignalR/OpenAPI and gRPC. Record exact reviewed artifact/source versions; use mechanical diffing to bound review rather than relying on a summary page.
- [ ] Own uncovered public-API differences and obsolete-index entries in this family. Cross-reference already assigned source findings/migration tickets rather than redoing their analysis. Classify authored code/markup/scripts/build configuration/manifests or removable-workaround impact; justify grouping/exclusion and retain unknowns. gRPC or another no-change area may legitimately produce no recipe.
- [ ] For each uncovered qualifying goal propose a narrow fully verified recipe ticket, native-scale estimate, exact prerequisites and real evidence acceptance for review. Audit closure alone does not deliver missing content or permit publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 47, 107, 108, 109, 110, 111, 112, 113, 158, 159, 160.

<a id="api-ef"></a>

### 034 - Reconcile EF and first-party provider API coverage

**New · Agent · L (5) · Labels: Agent, Research**

**Estimate rationale:** A bounded official source and actual artifact/API diff for one stated family; new recipe implementation is separately sized follow-on work.

**Blockers:** None — independent.

**Delivered outcome:** The remaining EF model/query/migrations/tooling and SQLServer/SQLite/Cosmos provider APIs have an accountable authoring-impact inventory beyond release-note headlines.

**Acceptance criteria:**

- [ ] Compare actual .NET 10 and pinned .NET 11 reference/public-tooling assets plus complete canonical new-feature and breaking-change sources for EF model/query/migrations/tooling and SQLServer/SQLite/Cosmos provider APIs. Record exact reviewed artifact/source versions; use mechanical diffing to bound review rather than relying on a summary page.
- [ ] Own uncovered public-API differences and obsolete-index entries in this family. Cross-reference already assigned source findings/migration tickets rather than redoing their analysis. Classify authored code/markup/scripts/build configuration/manifests or removable-workaround impact; justify grouping/exclusion and retain unknowns. gRPC or another no-change area may legitimately produce no recipe.
- [ ] For each uncovered qualifying goal propose a narrow fully verified recipe ticket, native-scale estimate, exact prerequisites and real evidence acceptance for review. Audit closure alone does not deliver missing content or permit publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 47, 114, 115, 116, 117, 118, 158, 159, 160.

## Language and library guidance

**New native milestone.** Original verified guidance covers qualifying language, runtime-facing, shared library, service and transport authoring goals.

<a id="sh-l01"></a>

### 000 - Model and persist closed C# outcomes

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Compiler/serializer boundary

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to model and persist closed C# outcomes.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Distinct outcomes survive exhaustive matching and JSON round trips.
- [ ] Null/default cases, closed-hierarchy boundaries and source-generation restrictions are explicit.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 93, 97, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `csharp.closed-results`, `csharp.closed-hierarchies`, `json.serialize-closed-results`, `json.infer-closed-polymorphism`.

<a id="sh-l02"></a>

### 001 - Extend and construct collection-like types

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Three related language contracts and release editions

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to extend and construct collection-like types.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original comparer/capacity and indexer examples preserve intended collection behavior.
- [ ] The .NET 10 extension-property foundation remains selectable independently of .NET 11 additions.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 85, 93, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `csharp.configure-collection-construction`, `csharp.index-existing-types`, `csharp.extend-uneditable-types`.

<a id="sh-l03"></a>

### 002 - Control an enclosing loop directly

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded language construct

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to control an enclosing loop directly.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original older/newer nested traversals produce the same result.
- [ ] Break/continue targets and cleanup behavior are demonstrated.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 93, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `csharp.control-nested-loops`.

<a id="sh-l04"></a>

### 003 - Put implementation helpers on interfaces

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** Bounded compiler declaration/access rules

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to put implementation helpers on interfaces.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] An original consumer uses the permitted static interface helper surface.
- [ ] Helpers do not become implementation obligations or get confused with static virtual members.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 93, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `csharp.static-interface-helpers`.

<a id="sh-l05"></a>

### 004 - Express and check unsafe boundaries

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Unresolved compiler/API interaction and experimental flags

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to express and check unsafe boundaries.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Resolve the conflicting AllowUnsafeBlocks claim against actual supported compiler behavior.
- [ ] Experimental flags, declaration accessibility and caller obligations appear in discovery and detail.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 93, 101, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `csharp.audit-memory-boundaries`, `unsafe.check-accessibility-contract`.

<a id="sh-f01"></a>

### 005 - Construct and serialize F# data models

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Compiler/serializer restrictions

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to construct and serialize F# data models.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original record construction and F# union serialization preserve intended values.
- [ ] Preview gates and reflection-only, trimming, source-generation and AOT restrictions are visible.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 94, 97, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `fsharp.compose-records`, `fsharp.construct-records`, `json.serialize-fsharp-unions`.

<a id="sh-f02"></a>

### 006 - Author clearer F#/.NET API boundaries

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Related API-authoring features with preview uncertainty

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to author clearer F#/.NET API boundaries.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] A small original API demonstrates supported delegate, SRTP-extension and named-call behavior.
- [ ] Documentation output and any supplied named-argument attribute definition are verified rather than presumed shipped.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 94, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `fsharp.pass-methods-as-delegates`, `fsharp.adapt-generic-operations`, `fsharp.require-named-arguments`, `fsharp.reuse-api-documentation`.

<a id="sh-f03"></a>

### 007 - Format F# diagnostics deliberately

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Culture and deployment constraints

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to format F# diagnostics deliberately.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original diagnostics retain the explicitly selected culture.
- [ ] Reflection-free formatting claims are verified separately from ordinary formatting.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 94, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `fsharp.format-without-reflection`, `fsharp.preserve-interpolation-culture`.

<a id="sh-f04"></a>

### 008 - Start, bound and fail F# asynchronous work

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Lifecycle and concurrency behavior

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to start, bound and fail F# asynchronous work.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original bounded work does not start beyond its configured concurrency limit and preserves intended output order.
- [ ] Caller-start semantics, SynchronizationContext restrictions and preserved exception stacks receive focused evidence.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 94, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `fsharp.start-async-on-caller`, `fsharp.bound-async-work`, `fsharp.rethrow-in-computations`.

<a id="sh-f05"></a>

### 009 - Migrate unsupported F# enum operations

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** Focused compiler migration

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to migrate unsupported F# enum operations.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Reproduce the relevant compiler rejection with an independently authored example.
- [ ] A supported representation preserves the original intended flags behavior.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 94, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `fsharp.migrate-enum-flags`.

<a id="sh-b01"></a>

### 010 - Capture and manage a command-line child process

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Related output and lifecycle contracts

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to capture and manage a command-line child process.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] An original utility captures process output and handles cancellation without undocumented custom plumbing.
- [ ] Parent-lifetime and piped-color environment behavior are checked on the declared supported platforms.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 96, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `process.capture-output`, `process.control-lifetime`, `console.color-piped-output`.

<a id="sh-b02"></a>

### 011 - Control native child-process startup and termination

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Native platform behavior

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to control native child-process startup and termination.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Handle ownership and startup preparation are checked on each declared applicable platform.
- [ ] Termination reporting distinguishes supported POSIX and Windows contracts and failure races.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 96, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `process.control-native-handles`, `process.prepare-before-start`, `process.inspect-termination`.

<a id="sh-b03"></a>

### 012 - Adapt existing buffers without redundant copying

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Several related buffer/lifetime contracts

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to adapt existing buffers without redundant copying.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Adapted memory, string or sequence data remains correct through the supported stream readers.
- [ ] Buffer ownership/lifetime and source-builder emptying after transfer are demonstrated.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 99, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `io.adapt-existing-buffers`, `text.transfer-builder-content`.

<a id="sh-b04"></a>

### 013 - Process Unicode characters and line boundaries

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Related Unicode and text cases

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to process Unicode characters and line boundaries.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Scalar-aware processing, comparison and invalid-input offsets are correct for meaningful Unicode input.
- [ ] Regex newline choices are checked without attributing automatic fixes to authored APIs.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 99, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `text.handle-unicode-scalars`, `text.compare-characters`, `text.locate-invalid-unicode`, `regex.handle-unicode-newlines`.

<a id="sh-b05"></a>

### 014 - Encode and consume incremental binary data

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Two small binary-data recipes

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to encode and consume incremental binary data.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] An original binary payload round-trips through the declared Base64 form and target assets.
- [ ] Original CBOR processing handles chunk boundaries using supported reader mode and state transitions.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 99, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `text.encode-base64`, `cbor.read-chunks`.

<a id="sh-j01"></a>

### 015 - Stream JSON records and reuse a writer safely

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Streaming and writer lifetimes

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to stream JSON records and reuse a writer safely.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original records remain independently consumable without whole-input buffering.
- [ ] Writer reset, flush and stream ownership are checked against the actual APIs.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 97, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `json.stream-json-lines`, `json.reuse-writer`.

<a id="sh-j02"></a>

### 016 - Resolve generic JSON contracts and typed metadata

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Serializer resolution contracts

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to resolve generic JSON contracts and typed metadata.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original open-generic contracts choose the intended converter or derived form.
- [ ] Typed metadata retrieval handles unsupported or unavailable contracts honestly.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 97, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `json.configure-generic-contracts`, `json.retrieve-typed-metadata`.

<a id="sh-j03"></a>

### 017 - Generate contracts for encapsulated models

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Code generation and constructor rules

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to generate contracts for encapsulated models.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original private-member and constructor examples compile using the declared serializer generation route.
- [ ] Omitted initializers and reference-parameter constructor binding preserve intended state.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 97, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `json.generate-private-contracts`, `json.preserve-omitted-initializers`, `json.bind-reference-constructors`.

<a id="sh-j04"></a>

### 018 - Define JSON conventions and expanded value contracts

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Related contract configuration

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to define JSON conventions and expanded value contracts.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original models demonstrate verified naming and ignore-rule precedence.
- [ ] Supported numeric/collection serialization and binary schema output match their declared contracts and target assets.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 97, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `json.configure-member-conventions`, `json.use-numeric-collection-contracts`, `json.describe-binary-schema`.

<a id="sh-j05"></a>

### 019 - Reject malformed import contracts

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Foundation with serializer-option interactions

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to reject malformed import contracts.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original .NET 9/10 examples distinguish the supported missing, duplicate and additional structural-input cases.
- [ ] Structural contract validation remains separate from business validation.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 90, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `json.reject-malformed-contracts`.

<a id="sh-a01"></a>

### 020 - Control ZIP entry access and protection

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** New archive and security contracts

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to control ZIP entry access and protection.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original entry access and protected archive round trips work with verified supported algorithms.
- [ ] Unsupported modes, password limitations and target requirements are explicit.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 99, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `archive.control-entry-access`, `archive.protect-entries`.

<a id="sh-a02"></a>

### 021 - Detect archive corruption and choose TAR representation

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Related archive compatibility and formats

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to detect archive corruption and choose TAR representation.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Meaningfully corrupted original archive inputs trigger the documented validation behavior.
- [ ] Original TAR examples demonstrate the applicable format and sparse-file decisions.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 99, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `archive.handle-corruption`, `archive.select-tar-format`.

<a id="sh-a03"></a>

### 022 - Compress buffers with the intended format and lifecycle

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Related compression formats and reset contracts

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to compress buffers with the intended format and lifecycle.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original empty and nonempty payloads round-trip through the declared supported APIs.
- [ ] Reset/reuse, formats and package requirements are verified and not inferred from broad package TFMs.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 99, 100, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `compression.handle-empty-payloads`, `compression.process-buffers`, `compression.use-zstandard`.

<a id="sh-n01"></a>

### 023 - Preserve unmatched records during reconciliation

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Multi-edition joins and operator availability

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to preserve unmatched records during reconciliation.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original .NET 10 left/right joins and .NET 11 full joins retain the intended unmatched records.
- [ ] Enumerable, Queryable and Async operator availability is distinguished from provider-specific translation.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 89, 98, 127, 130, 133, 134, 138, 139, 155, 159. Supporting inventory references: `linq.reconcile-all-rows`, `linq.retain-unmatched-rows`.

<a id="sh-n02"></a>

### 024 - Transform an asynchronous stream without buffering

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Streaming foundation

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to transform an asynchronous stream without buffering.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original .NET 9/10 examples observe filtered/enriched results incrementally.
- [ ] Cancellation and any older-package ambiguity are checked using declared actual dependencies.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 91, 98, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `linq.transform-async-stream`.

<a id="sh-n03"></a>

### 025 - Select decimal and complex numeric representations

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Related new numeric type choices

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to select decimal and complex numeric representations.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original calculations demonstrate the intended decimal and complex representations.
- [ ] Precision, conversions and package/target requirements are explicit.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 101, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `numerics.choose-ieee-decimal`, `numerics.select-complex-precision`.

<a id="sh-n04"></a>

### 026 - Preserve numeric parsing and conversion intent

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Numeric conversion and round-trip family

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to preserve numeric parsing and conversion intent.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Representative original values preserve the stated conversion, rounding or bit intent.
- [ ] Malformed-input and numeric-format boundaries receive focused evidence.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 101, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `numerics.preserve-conversion-intent`, `numerics.parse-delimited-values`, `numerics.roundtrip-float-bits`.

<a id="sh-n05"></a>

### 027 - Generate random values through generic numeric APIs

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** Narrow generic numeric API

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to generate random values through generic numeric APIs.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] An original generic generator produces values within the intended supported range.
- [ ] Guidance distinguishes these APIs from cryptographic randomness.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 101, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `numerics.generate-generic-random`.

<a id="sh-n06"></a>

### 028 - Arrange vector lanes and select supported instructions

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Architecture and hardware availability

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 002 - Confirm a suitable portable native-capability authoring route](#h-special-host)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to arrange vector lanes and select supported instructions.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original scalar and supported-vector paths agree on meaningful input.
- [ ] Architecture checks and fallback behavior prevent unsupported instruction use.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 101, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `simd.arrange-lanes`, `simd.select-supported-instructions`.

<a id="sh-n07"></a>

### 029 - Build bit collections and compare values by key

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Small related collection algorithms

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to build bit collections and compare values by key.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original collection operations produce expected bit counts and order.
- [ ] Key-based equality and comparison semantics are explicit and checked.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 98, 99, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `collections.count-and-build-bits`, `collections.compare-by-key`.

<a id="sh-i01"></a>

### 030 - Use supported pipe handles and mobile file locking

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Native I/O and platform evidence

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to use supported pipe handles and mobile file locking.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original pipe operations use verified handle contracts with explicit ownership.
- [ ] Mobile locking migration records actual platform evidence and compatibility-switch limits.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 99, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `io.use-pipe-handles`, `io.migrate-mobile-locking`.

<a id="sh-e01"></a>

### 031 - Inspect assembly metadata across deployment modes

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Deployment and NativeAOT restrictions

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 002 - Confirm a suitable portable native-capability authoring route](#h-special-host)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to inspect assembly metadata across deployment modes.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original introspection reports supported metadata and assembly locations accurately.
- [ ] NativeAOT caller inspection and unavailable-location cases have separately labelled evidence.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 102, 103, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `reflection.inspect-loaded-metadata`, `hosting.report-assembly-location`, `runtime.inspect-nativeaot-callers`.

<a id="sh-e02"></a>

### 032 - Annotate embedded code strings for tooling

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** Narrow syntax metadata API

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to annotate embedded code strings for tooling.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original code-string syntax annotations compile with the declared dependencies.
- [ ] Tooling assistance is not represented as runtime validation.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 99, 103, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `tooling.annotate-code-strings`.

<a id="sh-e03"></a>

### 033 - Describe URI payloads and media content

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** Small content-description APIs

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to describe URI payloads and media content.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original media input uses the supported data-URI and media-type APIs.
- [ ] Provisional URI/MIME grouping targets are consolidated without inventing extra scope or duplicate recipes.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 99, 100, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `uri.identify-data-uris`, `mime.describe-video-content`.

<a id="sh-e04"></a>

### 034 - Bind configuration selectively and await reload work

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Configuration lifecycle contracts

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to bind configuration selectively and await reload work.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Excluded original model properties remain unbound.
- [ ] Asynchronous reload completion and failure follow the verified contract.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 102, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `configuration.exclude-properties`, `configuration.await-change-callback`.

<a id="sh-e05"></a>

### 035 - Validate options with dependencies and async startup

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Options and startup integration

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to validate options with dependencies and async startup.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original DI-backed and asynchronous validators execute through supported startup paths.
- [ ] Synchronous access, startup failure and package availability restrictions are explicit.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 102, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `options.validate-with-di`, `options.validate-asynchronously`.

<a id="sh-e06"></a>

### 036 - Observe caches and select entry options from values

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Two related cache integration contracts

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to observe caches and select entry options from values.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] An original cache workload exposes the declared statistics and metrics.
- [ ] Value-derived entry options and estimated-size units are demonstrated correctly.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 102, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `cache.observe-memory-cache`, `cache.select-options-from-value`.

<a id="sh-e07"></a>

### 037 - Validate data asynchronously with localized rules

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Cross-framework validation contract

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to validate data asynchronously with localized rules.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original validation respects completion, cancellation and configured culture.
- [ ] Shared validation APIs and evidence are distinguished from Blazor or endpoint integration.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 102, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `validation.validate-asynchronously`, `validation.format-localized-rules`.

<a id="sh-e08"></a>

### 038 - Configure tracing, efficient logging and HTTP observations

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Related observability components and asset versions

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to configure tracing, efficient logging and HTTP observations.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original instrumentation produces the intended records and supported HTTP observations.
- [ ] Exact package versions, generic logging and analyzer guidance are reconciled without adding unrelated independently versioned coverage.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 100, 102, 104, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `tracing.configure-source-rules`, `logging.generate-generic-messages`, `metrics.read-observable-http-data`, `sdk.analyzer-guided-logging`.

<a id="sh-k01"></a>

### 039 - Exchange, compare and wrap key material

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Security correctness and algorithm availability

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to exchange, compare and wrap key material.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original key operations round-trip and use fixed-time comparison as claimed.
- [ ] New unpadded wrapping is distinguished from older padded KWP with platform/asset requirements.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 101, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `crypto.exchange-x25519`, `crypto.compare-fixed-secret`, `crypto.wrap-keys`.

<a id="sh-k02"></a>

### 040 - Use supported post-quantum hybrid and CMS contracts

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Experimental and platform-sensitive cryptography

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to use supported post-quantum hybrid and CMS contracts.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Supported original operations receive actual round-trip evidence.
- [ ] Experimental labels, support checks and platform differences are visible in summary and detail.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 101, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `crypto.encapsulate-hybrid-secrets`, `crypto.encrypt-cms-with-mlkem`.

<a id="sh-k03"></a>

### 041 - Drive TLS handshakes through caller-owned I/O

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Low-level experimental transport state

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 002 - Confirm a suitable portable native-capability authoring route](#h-special-host)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to drive TLS handshakes through caller-owned I/O.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] An original handshake exchanges actual data successfully through the supported API.
- [ ] Buffer ownership, state transitions and experimental diagnostics are explicit.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 100, 101, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `tls.drive-handshake-io`.

<a id="sh-t01"></a>

### 042 - Configure HTTP request compression and connection capacity

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Real protocol and connection-pool integration

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to configure HTTP request compression and connection capacity.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] A real controlled HTTP exchange verifies original configured compression and connection behavior.
- [ ] Protocol/server support and initial stream or retirement limits are documented.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 100, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `http.compress-request-body`, `http.retire-connections`, `http.bound-initial-h2-streams`.

<a id="sh-t02"></a>

### 043 - Resolve DNS service records

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Real resolver evidence

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to resolve DNS service records.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] A controlled service-record lookup returns the expected original record fields.
- [ ] Resolver/platform requirements and relevant failure behavior are explicit.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 100, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `dns.resolve-service-records`.

<a id="sh-t03"></a>

### 044 - Prioritize QUIC streams

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Native transport prerequisites

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 002 - Confirm a suitable portable native-capability authoring route](#h-special-host)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to prioritize QUIC streams.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] A supported real connection exercises the original stream-priority configuration.
- [ ] Support detection and actual priority limitations are explicit.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 100, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `quic.prioritize-streams`.

<a id="sh-r01"></a>

### 045 - Configure crash evidence and hardware requirements

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Runtime/platform evidence

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to configure crash evidence and hardware requirements.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original configuration produces or checks the declared crash evidence.
- [ ] Minimum-hardware consequences and unsupported environments are accurately documented.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 103, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `runtime.configure-crash-reporting`, `runtime.audit-hardware-baseline`.

<a id="sh-r02"></a>

### 046 - Choose runtime-async code generation deliberately

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Compiler/runtime coupling

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to choose runtime-async code generation deliberately.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Declared original compiler/runtime settings are checked against actual artifacts.
- [ ] Live diagnostics, exception-stack behavior and removed switches are distinguished.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 103, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `runtime.choose-async-codegen`.

<a id="sh-r03"></a>

### 047 - Link native code into CoreCLR WebAssembly

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Native WebAssembly toolchain

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 002 - Confirm a suitable portable native-capability authoring route](#h-special-host)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to link native code into CoreCLR WebAssembly.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] An original native function links and is invoked through the documented CoreCLR WebAssembly route.
- [ ] Emscripten, workload and runtime requirements are recorded without assuming availability.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 103, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `runtime.link-native-wasm-code`.

## Build and deployment guidance

**New native milestone.** Original verified guidance covers qualifying SDK, build, package and deployment authoring goals.

<a id="sh-s01"></a>

### 000 - Grow a file-based repository utility

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Release editions and conditional SDK routes

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to grow a file-based repository utility.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original .NET 9/10 foundation and newer include/reference routes are independently reproducible.
- [ ] Serviced SDK support and inherited shared-build/central-package behavior are explicit.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 92, 104, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `sdk.compose-file-apps`, `sdk.configure-file-directives`, `sdk.run-file-utility`.

<a id="sh-s02"></a>

### 001 - Target solution, formatting and launch operations precisely

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Related CLI authoring

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to target solution, formatting and launch operations precisely.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original repository commands affect only intended inputs.
- [ ] CLI, MSBuild and environment argument boundaries are demonstrated accurately.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 104, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `sdk.filter-solutions`, `sdk.format-targeted-code`, `sdk.pass-launch-settings`.

<a id="sh-s03"></a>

### 002 - Configure inner-loop app hosts and preview scaffolding

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Related developer workflow

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to configure inner-loop app hosts and preview scaffolding.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original watch, build and template commands select the intended configuration.
- [ ] SDK, template and device prerequisites are recorded rather than assumed.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 104, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `sdk.watch-app-hosts-and-devices`, `sdk.keep-build-configuration-explicit`, `sdk.install-preview-templates`.

<a id="sh-s04"></a>

### 003 - Select and create test-runner projects

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Several test-runner contracts

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to select and create test-runner projects.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original test projects execute through their declared supported runner.
- [ ] MTP/VSTest option differences and project selection are explicit.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 104, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `sdk.configure-test-runs`, `sdk.choose-test-projects`, `sdk.create-test-runner-projects`.

<a id="sh-s06"></a>

### 004 - Package tools and consume single-file output correctly

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Real package and deployment-artifact boundaries

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to package tools and consume single-file output correctly.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Delivered original sample artifacts install and run as declared.
- [ ] Changed single-file output layout and tool packaging requirements are demonstrated.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 104, 106, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `sdk.migrate-single-file-output`, `sdk.package-tools-correctly`.

<a id="sh-s07"></a>

### 005 - Declare static asset groups including TypeScript assets

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Build and publish artifacts

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to declare static asset groups including TypeScript assets.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] An original project publishes the intended asset group.
- [ ] TypeScript-specific inclusion is demonstrated using actual supported SDK behavior.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 104, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `sdk.declare-static-asset-groups`, `sdk.include-typescript-assets`.

<a id="sh-s08"></a>

### 006 - Publish reproducible multi-architecture containers

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Engine and image/manifest artifact evidence

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 002 - Confirm a suitable portable native-capability authoring route](#h-special-host)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to publish reproducible multi-architecture containers.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original images and manifests use the selected supported engine and architectures.
- [ ] Container reproducibility inputs are checked separately from NuGet package timestamps.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 106, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `sdk.select-local-container-engine`, `sdk.publish-reproducible-containers`, `sdk.publish-multiarch-with-podman`.

<a id="sh-s09"></a>

### 007 - Configure CLI telemetry explicitly

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** Narrow SDK configuration

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to configure CLI telemetry explicitly.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original configuration uses verified supported telemetry switches.
- [ ] Scope, defaults and affected commands are accurately described.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 104, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `sdk.configure-cli-telemetry`.

<a id="sh-m01"></a>

### 008 - Produce build archives and inspect item globs

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Related custom-build tasks and inputs

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to produce build archives and inspect item globs.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] An original custom build creates and extracts expected artifacts.
- [ ] Reported unevaluated item globs are distinguished from expanded file lists and Framework MSBuild availability.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 104, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `msbuild.archive-build-artifacts`, `msbuild.inspect-item-globs`.

<a id="sh-m02"></a>

### 009 - Migrate custom MSBuild XML and feature switches

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Build compatibility

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to migrate custom MSBuild XML and feature switches.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original trusted XML migration works with the current DTD default.
- [ ] Retired wave switches and applicable diagnostics are reproduced accurately.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 104, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `msbuild.migrate-xml-inputs`, `msbuild.retire-old-wave-optouts`.

<a id="sh-p01"></a>

### 010 - Select dependencies with cooldown and vulnerability updates

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Package-resolution behavior

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to select dependencies with cooldown and vulnerability updates.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Controlled package metadata demonstrates original cooldown and vulnerability-update behavior.
- [ ] Cooldown is not described as universal restore exclusion or unrelated warning suppression.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 105, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `nuget.delay-new-package-selection`, `nuget.update-vulnerable-dependencies`.

<a id="sh-p02"></a>

### 011 - Produce deterministic package timestamps

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One delivered-artifact property

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to produce deterministic package timestamps.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Repeated original package builds demonstrate the declared timestamp behavior.
- [ ] Opt-in configuration and reproducibility limits are explicit.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 105, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `nuget.make-package-timestamps-reproducible`.

<a id="sh-p03"></a>

### 012 - Retarget a NuGet client-library host

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Client asset and host compatibility

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to retarget a NuGet client-library host.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] An original small client-library host compiles and runs against the declared assets.
- [ ] Client-library host requirements are distinguished from ordinary consuming application TFMs.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 105, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `nuget.migrate-client-library-hosts`.

## Web application guidance

**New native milestone.** Original verified Blazor and wider ASP.NET Core guidance preserves the entire accepted baseline and experimental additions.

<a id="web-w01"></a>

### 000 - Immediate and complete form validation guidance

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Give a server-rendered editor immediate feedback; Validate the full editable object graph.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Replace duplicate browser rules with model-derived client validation. Configure generated validation coverage and opt-outs; remove obsolete suppression.
- [ ] Exercise valid and invalid SSR forms in a real browser with enhanced and plain posts; show server validation still rejects bypassed client checks. Submit nested objects and collection entries through a real validation consumer; show declared generation/opt-out boundaries and removal of obsolete suppression.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.check-input-before-post, validation.cover-nested-models; explicitly preserve these applicability constraints: Blazor static SSR; enhanced/plain forms; server remains authoritative. `Microsoft.Extensions.Validation`; nested objects/collections; stable attributes.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 53, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.check-input-before-post`, `validation.cover-nested-models`, `WEB-001`, `WEB-006`, `WEB-057`.

<a id="web-w02"></a>

### 001 - Accessible model-driven field captions

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Connect accessible captions to reusable editors; Keep a field's visible name consistent across views.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Use `Label` and generated input IDs instead of manual coordination. Render `DisplayName` from model metadata.
- [ ] Inspect rendered label/input associations and repeated-editor ID uniqueness in a real browser. Render display metadata precedence and a localized resource caption; compare the .NET 10 manual metadata path.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.associate-generated-inputs, blazor.reuse-field-captions; explicitly preserve these applicability constraints: Blazor forms; nested and separate labels; check repeated-instance uniqueness. Blazor; `[Display]` wins over `[DisplayName]`; localized resources supported.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 51, 52, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.associate-generated-inputs`, `blazor.reuse-field-captions`, `WEB-004`, `WEB-005`.

<a id="web-w03"></a>

### 002 - Awaited validation for forms and endpoints

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Finish remote checks before accepting an edit; Check remote business constraints before entering a handler.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Author async attributes/object rules or cancellable field validators. Register async validation; author async attributes/object validators.
- [ ] Demonstrate awaited submit validation and cancellable field validation with a real loopback asynchronous rule; no paid remote service is required. Send real valid/invalid requests through the endpoint and prove an awaited rule completes before the handler runs.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.await-form-rules, minimal-api.await-model-rules; explicitly preserve these applicability constraints: Blazor all render modes; field progress/cancellation differ from submit validation. `Extensions.Validation`; shared DataAnnotations API prerequisite.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 49, 107, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.await-form-rules`, `minimal-api.await-model-rules`, `WEB-002`, `WEB-055`.

<a id="web-w04"></a>

### 003 - Localized validation feedback in Blazor and endpoints

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Return the same localized feedback in forms and endpoints.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Register localization/validation; author resources and custom formatting.
- [ ] Verify localized errors in real Blazor and Minimal API consumers; exercise custom ValidationAttribute.FormatMessage and final resource conventions.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for validation.translate-model-feedback; explicitly preserve these applicability constraints: Shared validation, Blazor and Minimal APIs; RC2 formatter correction above.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 50, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `validation.translate-model-feedback`, `WEB-003`, `WEB-057`.

<a id="web-w05"></a>

### 004 - Author SSR redirect and session state workflows

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Carry a one-use outcome into the next page; Continue a server-rendered multi-step task.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Use TempData instead of bespoke redirect state. Register session services/middleware and bind session-supplied values.
- [ ] Follow a real redirect and second read to prove one-use state; exercise streaming/header timing. Run a real multi-step session across reads and distinguish persistence from one-use TempData; verify middleware and streaming ordering.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.show-once-after-redirect, blazor.retain-session-progress; explicitly preserve these applicability constraints: Static SSR; cookie/header timing matters under streaming. Static SSR; values survive reads; distinguish TempData; streaming constraints.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 54, 55, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.show-once-after-redirect`, `blazor.retain-session-progress`, `WEB-007`, `WEB-008`, `WEB-042`.

<a id="web-w06"></a>

### 005 - Base-aware Blazor navigation

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Move between nearby routes without rebuilding their prefix; Keep base markup aligned with deployment location; Link to a section without manual URI manipulation.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Configure relative navigation on links and imperative navigation. Use framework base-path rendering instead of hard-coded markup. Use fragment helper respecting existing/base URI.
- [ ] Navigate nested routes under a non-root application path using declarative and imperative navigation. Serve at a non-root base path and verify generated base markup and route loading; use the RC2-correct namespace. Follow generated fragment links with existing fragments and non-root bases in a real browser.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.navigate-within-current-path, blazor.render-application-base, blazor.build-section-link; explicitly preserve these applicability constraints: Blazor routing; distinguish current URI and application base. Blazor Web App; RC2 namespace correction. Blazor navigation.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 56, 73, 74, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.navigate-within-current-path`, `blazor.render-application-base`, `blazor.build-section-link`, `WEB-009`, `WEB-026`, `WEB-027`.

<a id="web-w07a"></a>

### 006 - Place server-rendered content inside an interactive boundary

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded authoring/migration change, with original old/new consumer, focused real proof and packed-MCP delivery; no broad platform matrix.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Place server-rendered content inside an interactive boundary.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Pass captured non-generic render fragments instead of extra wrapper components.
- [ ] Render static child content across the interactive boundary and verify actual supported non-generic fragment restrictions; avoid implying arbitrary delegate serialization.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.project-static-child-content; explicitly preserve these applicability constraints: Cross-render-mode static content; not delegate serialization or arbitrary live callbacks.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 58, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.project-static-child-content`, `WEB-011`, `WEB-049`.

<a id="web-w07b"></a>

### 007 - Offer a single parameter for alternative UI values

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Offer a single parameter for alternative UI values.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Author union parameters, literal attributes, interop/persisted contracts.
- [ ] Compile literal and bound union-valued component parameters with the final C#15 contract; verify JSON/persistence limitations with a real component.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.accept-alternative-parameter-shapes; explicitly preserve these applicability constraints: C#15; Blazor/Razor; distinguish static fragments and JSON-compatible values.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 77, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.accept-alternative-parameter-shapes`, `WEB-030`.

<a id="web-w08"></a>

### 008 - Safe static-subtree caching

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Reuse an expensive page section across requests.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Author `CacheView` boundaries and variation/invalidation policy.
- [ ] Verify repeated-request cache hits, variation and invalidation against actual rendered content; prove user-specific markup does not leak across cache keys.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.cache-static-subtree; explicitly preserve these applicability constraints: Static SSR only; cache hits skip child lifecycle; isolate user-specific markup.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 57, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.cache-static-subtree`, `WEB-010`.

<a id="web-w09a"></a>

### 009 - Author URL-restored QuickGrid state

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** One focused authoring goal with original previous/current samples, actual consumer behavior and installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Restore a sorted page of results from its URL.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Adopt URL state, per-grid query names, link styles and stable column identifiers.
- [ ] Restore URL paging/sorting for two grids and verify independent query namespaces, stable column IDs and migrated link markup.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.share-grid-view; preserve these applicability constraints: QuickGrid; static SSR/interactive; own `PaginationState` per grid.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 60, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.share-grid-view`, `WEB-013`, `WEB-095`.

<a id="web-w09b"></a>

### 010 - Interactive QuickGrid row selection

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded original API/declaration migration with actual previous/current consumer proof and installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Open an item when its result row is selected.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Attach the row callback rather than custom row-event plumbing.
- [ ] Trigger the actual interactive row event and verify the chosen item; show applicability excludes an inert static event handler.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.act-on-selected-row; preserve these applicability constraints: QuickGrid `OnRowClick`; interactive handler required.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 61, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.act-on-selected-row`, `WEB-014`.

<a id="web-w09c"></a>

### 011 - Virtualized QuickGrid row positioning

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** One focused authoring goal with original previous/current samples, actual consumer behavior and installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Retain position and reveal rows in a virtualized grid.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Cover grid-specific initial index, scrolling, anchor and comparer parameters.
- [ ] Exercise rendered QuickGrid initial/programmatic scrolling and anchoring; reconcile RC2 removal of ASP0030.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.reveal-grid-row; preserve these applicability constraints: QuickGrid `Virtualize=true`; rendered instance; RC2 removes `ASP0030`.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 80, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.reveal-grid-row`, `WEB-036`.

<a id="web-w10a"></a>

### 012 - Anchoring for changing variable-height lists

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** One goal with real browser/provider evidence, release/API or database-feature reconciliation and original previous/current samples; includes installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Keep a changing feed attached to its chosen edge.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Set anchoring and identity comparison explicitly.
- [ ] Prepend, append and expand variable-height items in a real rendered list; verify anchoring, identity comparison and deliberate overscan.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.hold-list-edge; preserve these applicability constraints: Interactive `Virtualize`; prepend/append behavior; browser/layout differences.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 62, 63, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.hold-list-edge`, `WEB-015`, `WEB-016`.

<a id="web-w10b"></a>

### 013 - Virtual-list item reveal

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** One focused authoring goal with original previous/current samples, actual consumer behavior and installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Bring an off-screen result into view.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Use initial index and programmatic scrolling.
- [ ] Exercise initial and programmatic virtual-item reveal with real item-provider timing.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.reveal-virtual-item; preserve these applicability constraints: Interactive rendered `Virtualize`; index bounds and provider timing.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 64, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.reveal-virtual-item`, `WEB-017`.

<a id="web-w10c"></a>

### 014 - Restrictive-CSP virtual lists

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** One goal with real browser/provider evidence, release/API or database-feature reconciliation and original previous/current samples; includes installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Keep a long list usable under a restrictive style policy.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Remove virtualization-specific CSP exceptions/workarounds when adopting new rendering.
- [ ] Run the real virtualized list under restrictive style CSP without the old virtualization exception and inspect CSP failures.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.virtualize-under-csp; preserve these applicability constraints: `Virtualize`; source-reviewed migration, browser evidence still needed.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 65, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.virtualize-under-csp`, `WEB-018`.

<a id="web-w11"></a>

### 015 - Idle and maintenance circuit pause workflows

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Release an inactive browser tab's server resources; Ask active sessions to pause before maintenance.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Register opt-in automatic circuit pause and delay. Request pause from server lifecycle code and handle acceptance.
- [ ] Observe actual idle circuit pause/resume with persisted state and verify lifetime prerequisites. Request pause from real server lifecycle code; demonstrate acceptance and distinguish cancellation from completed shutdown.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.pause-idle-tab, blazor.drain-active-circuits; explicitly preserve these applicability constraints: Interactive Server; `Components.Server.AutoPause`; persistence/resumption prerequisites. Interactive Server; cancellation of request is not guaranteed completed shutdown.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 66, 67, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.pause-idle-tab`, `blazor.drain-active-circuits`, `WEB-019`, `WEB-020`.

<a id="web-w12a"></a>

### 016 - Server-configured Blazor browser startup

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** One goal with real browser/provider evidence, release/API or database-feature reconciliation and original previous/current samples; includes installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Apply consistent reconnect and startup policy from C#.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Replace startup-JS configuration with endpoint/component browser options.
- [ ] Verify actual generated browser options and reconnect behavior using final RC1 names and static lookup; reproduce any sharing leak before claiming a restriction.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.configure-browser-from-server; preserve these applicability constraints: Server/WASM/Auto contexts; final RC1 names; options leak unverified.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 68, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.configure-browser-from-server`, `WEB-021`.

<a id="web-w12b"></a>

### 017 - Unified Blazor startup script options

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** One focused authoring goal with original previous/current samples, actual consumer behavior and installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Reuse startup configuration across host styles.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Adopt nested `Blazor.start` options; older format remains supported.
- [ ] Run representative legacy-script and Web App startup paths with the nested options contract, while stating older options remain supported.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.unify-startup-script-options; preserve these applicability constraints: Server/WASM legacy scripts and Web App script.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.unify-startup-script-options`, `WEB-039`.

<a id="web-w12c"></a>

### 018 - Deliberate Blazor circuit transport policy

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** One goal with real browser/provider evidence, release/API or database-feature reconciliation and original previous/current samples; includes installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Set session transport and expiry limits deliberately.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Configure transport, auth-expiry and buffer policy via `ConfigureConnection`.
- [ ] Exercise supported transport/auth-expiry/buffer configuration on a real circuit connection; distinguish transport policy from browser reconnect options.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.control-circuit-transport; preserve these applicability constraints: Interactive Server/SignalR; distinct from browser reconnect options.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 81, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.control-circuit-transport`, `WEB-037`.

<a id="web-w13a"></a>

### 019 - Run periodic browser work within application lifetime

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Run periodic browser work within application lifetime.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Register hosted services rather than ad hoc startup tasks.
- [ ] Run a real WASM hosted service and observe start/cancellation/shutdown within browser application lifetime; make no durable-background claim.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.run-browser-background-service; explicitly preserve these applicability constraints: WASM browser lifetime; not durable work after tab/process termination.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 69, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.run-browser-background-service`, `WEB-022`.

<a id="web-w13b"></a>

### 020 - Move long-running browser work away from rendering

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Move long-running browser work away from rendering.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Adopt generated worker client and manage its lifecycle.
- [ ] Generate the actual blazorwebworker artifact and demonstrate work, void calls, creation/invocation cancellation and timeout; distinguish cancellation from termination.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.offload-browser-computation; explicitly preserve these applicability constraints: WASM/CSR; current `blazorwebworker` template; see E03.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 72, 82, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.offload-browser-computation`, `WEB-025`, `WEB-038`.

<a id="web-w14a"></a>

### 021 - Reuse one browser build across environments

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Reuse one browser build across environments.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Supply public configuration through browser options/configuration providers.
- [ ] Serve one browser build with two public environment configurations and verify source precedence and duplicate-provider migration.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.supply-browser-configuration; explicitly preserve these applicability constraints: WASM; environment-source precedence and removal of duplicate registration.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 70, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.supply-browser-configuration`, `WEB-023`, `WEB-098`.

<a id="web-w14b"></a>

### 022 - Keep formatting consistent through hydration

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Keep formatting consistent through hydration.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Use server culture transfer or explicit independent-client opt-out.
- [ ] Verify prerendered-to-hydrated culture and satellite loading in a real browser, including explicit independent-client opt-out.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.align-server-browser-culture; explicitly preserve these applicability constraints: Prerendered WASM in Blazor Web Apps; satellite assembly timing.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 71, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.align-server-browser-culture`, `WEB-024`.

<a id="web-w14c"></a>

### 023 - Present environment-appropriate application chrome

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded authoring/migration change, with original old/new consumer, focused real proof and packed-MCP delivery; no broad platform matrix.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Present environment-appropriate application chrome.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Use `EnvironmentView` instead of repeated environment services/conditionals.
- [ ] Render include/exclude behavior under server/client environments and state that hidden chrome is not authorization.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.show-environment-specific-content; explicitly preserve these applicability constraints: Server/client environments; case-insensitive include/exclude; not access control.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 59, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.show-environment-specific-content`, `WEB-012`.

<a id="web-w15a"></a>

### 024 - Serve a standalone client with configured backend routes

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Serve a standalone client with configured backend routes.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Adopt Gateway, SPA fallback, proxy routes and service-defaults wiring.
- [ ] Run a standalone WASM client through Gateway fallback/proxy routes and generated service defaults against local real backend endpoints; verify Aspire prerequisites only when used.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.host-browser-client-with-backends; explicitly preserve these applicability constraints: WASM; `Components.Gateway`, generated service-defaults library; Aspire dependency when used.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 78, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.host-browser-client-with-backends`, `WEB-031`.

<a id="web-w15b"></a>

### 025 - Collect component activity from the browser

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Collect component activity from the browser.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Enable runtime metrics/tracing and configure collection/export.
- [ ] Collect actual browser component metrics/activity using supported runtime switches and an observable local collector; distinguish subscription/export evidence.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.observe-browser-components; explicitly preserve these applicability constraints: WASM; public switches/package requirements; link shared telemetry guidance.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.observe-browser-components`, `WEB-045`, `WEB-048`.

<a id="web-w16a"></a>

### 026 - Operate navigation without an inline script exception

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Operate navigation without an inline script exception.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Move toggle behavior to collocated JS; remove matching unsafe hash.
- [ ] Operate real navigation after moving behavior to collocated JS and removing the matching inline exception; verify applicable module paths.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.toggle-navigation-with-csp; explicitly preserve these applicability constraints: Accepted NavMenu migration; module path differs with client layout.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 84, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.toggle-navigation-with-csp`, `WEB-040`.

<a id="web-w16b"></a>

### 027 - Keep a custom event distinct from its browser trigger

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded authoring/migration change, with original old/new consumer, focused real proof and packed-MCP delivery; no broad platform matrix.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Keep a custom event distinct from its browser trigger.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Rename JS registration and matching Razor event metadata.
- [ ] Reproduce the colliding browser/custom event name on the new framework and verify the renamed JS/Razor pair dispatches correctly.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.rename-custom-event; explicitly preserve these applicability constraints: Browser event-name collision now throws.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.rename-custom-event`, `WEB-081`.

<a id="web-w16c"></a>

### 028 - Retain deliberate loading hints during enhanced navigation

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Retain deliberate loading hints during enhanced navigation.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Author preload/modulepreload hints instead of relying on implicit preloads.
- [ ] Observe enhanced-navigation network behavior with deliberately authored preload/modulepreload hints; avoid invented universal preload requirements.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.preload-needed-resources; explicitly preserve these applicability constraints: Blazor enhanced navigation; only resources actually needed.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.preload-needed-resources`, `WEB-082`.

<a id="web-w17"></a>

### 029 - Supply component dependencies through a custom activation policy

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Supply component dependencies through a custom activation policy.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Implement/register component property activation.
- [ ] Run custom component activation with actual keyed/unkeyed injection; document concurrency/trimming support only to the extent actually verified.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.customize-injected-properties; explicitly preserve these applicability constraints: Advanced DI; keyed services, concurrency, trimming need verification.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 75, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.customize-injected-properties`, `WEB-028`.

<a id="web-w18a"></a>

### 030 - Express a JavaScript call that has no result

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded authoring/migration change, with original old/new consumer, focused real proof and packed-MCP delivery; no broad platform matrix.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Express a JavaScript call that has no result.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Follow analyzer to use `InvokeVoidAsync` rather than discard a generic result.
- [ ] Compile and invoke the no-result JS call through InvokeVoidAsync; show the supported previous-release equivalent without implying a new runtime capability.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.use-void-interop-intent; explicitly preserve these applicability constraints: Blazor JS interop; focused source authoring, not analyzer internals.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.use-void-interop-intent`, `WEB-044`.

<a id="web-w18c"></a>

### 031 - Display interactive mathematical notation without custom DOM creation

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded authoring/migration change, with original old/new consumer, focused real proof and packed-MCP delivery; no broad platform matrix.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Display interactive mathematical notation without custom DOM creation.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Author MathML directly in interactive components.
- [ ] Render interactive MathML in a real browser and inspect the correct namespace and update behavior.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.render-mathematical-markup; explicitly preserve these applicability constraints: Blazor; correct namespaces replace workaround.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.render-mathematical-markup`, `WEB-043`.

<a id="web-w19"></a>

### 032 - Experimental AI conversation streaming

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Present incremental agent output with conversation history.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Compose agent boundary/message UI and content-block renderers.
- [ ] Exercise incremental content blocks and retained conversation history with a real Components.AI renderer and observable chat stream; label experimental package and any scripted chat source honestly.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.stream-agent-conversation; explicitly preserve these applicability constraints: **Experimental throughout .NET 11** `Components.AI`; `IChatClient`.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 79, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.stream-agent-conversation`, `WEB-032`.

<a id="web-w20"></a>

### 033 - Experimental AI tool rendering

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Show tool activity inside the conversation.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Register frontend tool declarations and custom tool/progress rendering.
- [ ] Exercise real tool-call/progress rendering and frontend declarations; use an actual loopback AG-UI flow where the feature requires it and state remote-provider evidence limits.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.render-agent-tools; explicitly preserve these applicability constraints: Same experimental package; remote tool/event workflows require AG-UI dependencies.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 79, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.render-agent-tools`, `WEB-033`.

<a id="web-w21"></a>

### 034 - Experimental AI approval continuation

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Let a person approve a pending agent operation.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Author approval UI and continuation wiring.
- [ ] Demonstrate approval and denial controlling actual continuation of a pending tool operation; a cosmetic button alone does not satisfy the goal.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.approve-agent-action; explicitly preserve these applicability constraints: Same experimental package; approval flow must affect actual tool continuation.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 79, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.approve-agent-action`, `WEB-034`.

<a id="web-w22"></a>

### 035 - Experimental AI and UI state synchronization

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Keep local UI and agent state coordinated.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Author typed/shared/predictive state bindings.
- [ ] Demonstrate actual typed/shared/predictive state updates through the applicable AG-UI path, with explicit reconciliation and experimental prerequisites.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for blazor.synchronize-agent-view-state; explicitly preserve these applicability constraints: Same experimental package; AG-UI required for remote state protocol.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 79, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `blazor.synchronize-agent-view-state`, `WEB-035`.

<a id="web-w23"></a>

### 036 - Simplify form protection without opening cross-origin writes

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Simplify form protection without opening cross-origin writes.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Adjust antiforgery startup and CORS/endpoint policy while preserving required token defense.
- [ ] Send real same-origin and cross-origin form posts across applicable SSR/Minimal API/MVC paths; prove the required rejection/token-retention exceptions and document CORS policy.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for web.protect-browser-form-posts; explicitly preserve these applicability constraints: Blazor SSR, form-bound Minimal APIs, opted-in MVC/Razor Pages; JSON endpoints unchanged; older browsers/additional-data/compliance exceptions.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 83, 109, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `web.protect-browser-form-posts`, `WEB-041`, `WEB-083`.

<a id="web-w24"></a>

### 037 - Shared authorization requirement metadata

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Apply one requirement across endpoints and component views.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Author policy/requirement metadata consistently across the stack.
- [ ] Send real authorized/denied requests and render matching component views with the same requirement metadata; cover legacy non-endpoint MVC restriction.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for authorization.share-policy-metadata; explicitly preserve these applicability constraints: Blazor views/routes, MVC, SignalR; legacy non-endpoint MVC path differs.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 76, 109, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `authorization.share-policy-metadata`, `WEB-029`, `WEB-069`.

<a id="web-w25a"></a>

### 038 - Verify expiry and lockout without waiting

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded authoring/migration change, with original old/new consumer, focused real proof and packed-MCP delivery; no broad platform matrix.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Verify expiry and lockout without waiting.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Inject a controlled `TimeProvider`.
- [ ] Drive a real Identity expiry or lockout path with controlled TimeProvider, rather than a mocked time-sensitive service.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for identity.control-test-clock; explicitly preserve these applicability constraints: ASP.NET Core Identity; time-sensitive tests and services.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 109, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `identity.control-test-clock`, `WEB-066`.

<a id="web-w25b"></a>

### 039 - Give a saved authenticator an intelligible name

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Give a saved authenticator an intelligible name.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Extend template authenticator mappings and fallback rename UX.
- [ ] Exercise actual template/custom authenticator-name mapping and rename fallback without claiming physical authenticator interoperability from a simulated WebAuthn test.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for identity.name-passkey-authenticator; explicitly preserve these applicability constraints: Identity passkey UI; automatic built-in names alone excluded.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 109, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `identity.name-passkey-authenticator`, `WEB-067`.

<a id="web-w25c"></a>

### 040 - Present a valid outcome when passkey sign-in is disallowed

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Present a valid outcome when passkey sign-in is disallowed.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Handle lockout/unconfirmed account results in custom UI.
- [ ] Exercise real Identity lockout/unconfirmed-account results through the custom sign-in UI; preserve shared pre-sign-in policy.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for identity.handle-passkey-denial; explicitly preserve these applicability constraints: Identity; no bypass of shared pre-sign-in policy.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 109, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `identity.handle-passkey-denial`, `WEB-094`.

<a id="web-w26"></a>

### 041 - Live identity refresh for SignalR and Blazor

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Renew identity without dropping a live connection; Refresh user-specific content when session claims change.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Opt hub into refresh, approve/reject identity, handle client refresh events. Subscribe/reload using authentication-state notifications instead of forced reload.
- [ ] Refresh identity over a real hub connection with accept/reject and client refresh events; cover .NET/TypeScript and negotiate-redirection constraints without assuming Azure SignalR support. Observe actual authentication-state notification and user-specific UI refresh on an interactive server circuit.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for signalr.refresh-live-credentials, blazor.react-to-refreshed-identity; explicitly preserve these applicability constraints: .NET client and TypeScript variants; RC1 final names. Interactive Server; automatic propagation is only context.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 110, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `signalr.refresh-live-credentials`, `blazor.react-to-refreshed-identity`, `WEB-047`, `WEB-050`, `WEB-051`.

<a id="web-w27"></a>

### 042 - Stop an ordinary hub operation when its caller abandons it

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Stop an ordinary hub operation when its caller abandons it.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Pass cancellation through `InvokeAsync` into hub work.
- [ ] Cancel a real non-streaming hub invocation and observe server cancellation; verify required paired client/server capabilities.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for signalr.cancel-active-operation; explicitly preserve these applicability constraints: Non-streaming invocation; matched new client/server capabilities.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 110, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `signalr.cancel-active-operation`, `WEB-052`.

<a id="web-w28a"></a>

### 043 - Return a consistent failure before endpoint execution

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded authoring/migration change, with original old/new consumer, focused real proof and packed-MCP delivery; no broad platform matrix.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Return a consistent failure before endpoint execution.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Inspect failed binding in endpoint filters.
- [ ] Send malformed real endpoint requests and verify shaped binding failure occurs before handler execution.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for minimal-api.shape-binding-error; explicitly preserve these applicability constraints: Set `ThrowOnBadRequest=false` in Development; handler need not run.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 107, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `minimal-api.shape-binding-error`, `WEB-053`.

<a id="web-w28b"></a>

### 044 - Bypass irrelevant middleware for a deliberately standalone response

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Bypass irrelevant middleware for a deliberately standalone response.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Apply `[ShortCircuit]` on controller/action or Minimal API.
- [ ] Send real requests through an instrumented middleware pipeline and verify only deliberately safe endpoints bypass the intended middleware.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for web.short-circuit-simple-endpoint; explicitly preserve these applicability constraints: Must not require skipped authentication/CORS/middleware. Existing convention retained.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 107, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `web.short-circuit-simple-endpoint`, `WEB-056`.

<a id="web-w29a"></a>

### 045 - Call a secured file-based endpoint during development

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded authoring/migration change, with original old/new consumer, focused real proof and packed-MCP delivery; no broad platform matrix.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Call a secured file-based endpoint during development.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Generate development JWT using the file selector.
- [ ] Generate a development JWT with the actual file selector and call a secured file-based endpoint; do not present this as production token issuance.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for web.test-file-app-authentication; explicitly preserve these applicability constraints: `dotnet user-jwts --file`; development only; share file-app setup.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 104, 125, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `web.test-file-app-authentication`, `WEB-068`.

<a id="web-w29b"></a>

### 046 - Use one trust operation for Windows and WSL

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 004 - Provide controlled Windows web and design-time verification access](#h-windows-web)

**Delivered outcome:** Use one trust operation for Windows and WSL.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Author original previous/current Windows/WSL HTTPS examples and compile/check the applicable developer-certificate command against actual supported artifacts.
- [ ] Explain the single trust-operation change and exact Windows/WSL prerequisites. Mark owner-controlled real trust execution pending H_WSL_PROOF; this Agent PR does not change owner-wide trust or claim that certificate generation proves connectivity.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 104, 125, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `web.trust-wsl-development-certificate`, `WEB-073`.

<a id="web-w29c"></a>

### 047 - Start an agent-facing web tool from a bundled template

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Start an agent-facing web tool from a bundled template.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Generate and extend `mcpserver` without separately installing template package.
- [ ] Generate the SDK-bundled mcpserver template and execute its real advertised transport/tool interaction with an MCP client; this example is separate from Playbook product-host implementation.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for web.scaffold-mcp-host; explicitly preserve these applicability constraints: SDK-bundled template; MCP dependency versions verified when authored.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 104, 125, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `web.scaffold-mcp-host`, `WEB-076`.

<a id="web-w30"></a>

### 048 - Describe a body that has several valid shapes

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Describe a body that has several valid shapes.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Use native unions and JSON classifiers instead of parallel DTO routing.
- [ ] Send real supported and invalid union-shaped payloads and verify serialized behavior against the final C#15/JSON contract.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for web.accept-union-payloads; explicitly preserve these applicability constraints: Minimal API/MVC bodies and SignalR JSON protocol; not route/query/header/form binding or MessagePack/Newtonsoft.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 107, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `web.accept-union-payloads`, `WEB-054`.

<a id="web-w31a"></a>

### 049 - Accurate OpenAPI download responses

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** One focused authoring goal with original previous/current samples, actual consumer behavior and installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Generate an accurate contract for a file response.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Supply file-result/content metadata instead of schema patches.
- [ ] Generate the actual OpenAPI document for a real download endpoint and verify binary/content metadata against its HTTP response.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for openapi.describe-download; preserve these applicability constraints: Minimal API/MVC; binary response and result-type include files grouped.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 111, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `openapi.describe-download`, `WEB-058`.

<a id="web-w31b"></a>

### 050 - Author HTTP QUERY request contracts

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** One focused authoring goal with original previous/current samples, actual consumer behavior and installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Publish a large safe-search request contract.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Map HTTP QUERY and generate appropriate operation representation.
- [ ] Invoke the actual QUERY endpoint and inspect generated 3.2 operation versus older-version extension; state client/proposal support limits.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for openapi.describe-body-query; preserve these applicability constraints: QUERY proposal/tool support; 3.2 native operation versus earlier extension.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 111, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `openapi.describe-body-query`, `WEB-060`.

<a id="web-w31c"></a>

### 051 - Multi-shape OpenAPI response contracts

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** One focused authoring goal with original previous/current samples, actual consumer behavior and installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Preserve all supported media types and shapes.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Declare multiple response variants; clean stale declarations.
- [ ] Generate and inspect all media/schema variants, including same-media anyOf; run a representative compatible consumer rather than claiming all generators support them.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for openapi.describe-response-variants; preserve these applicability constraints: Minimal API/MVC; same media type can yield `anyOf`; client-generation verification.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 111, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `openapi.describe-response-variants`, `WEB-061`, `WEB-065`, `WEB-092`.

<a id="web-w31d"></a>

### 052 - Typed SSE OpenAPI contracts

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** One focused authoring goal with original previous/current samples, actual consumer behavior and installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Generate a typed streaming event contract.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Return typed SSE results and preserve per-event schema.
- [ ] Read a real SSE stream and inspect OpenAPI3.2 itemSchema for typed events; distinguish raw async-enumerable JSON.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for openapi.describe-event-stream; preserve these applicability constraints: OpenAPI 3.2 `itemSchema`; raw async enumerable is JSON, not SSE.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 111, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `openapi.describe-event-stream`, `WEB-062`.

<a id="web-w32a"></a>

### 053 - Compatible OpenAPI document versions

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** One focused authoring goal with original previous/current samples, actual consumer behavior and installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Produce a document the downstream generator understands.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Choose OpenAPI 3.2 or explicitly retain earlier version.
- [ ] Generate supported document versions and validate a representative downstream consumer for the selected contract; distinguish OpenAPI spec version from OpenAPI.NET API migration.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for openapi.select-contract-version; preserve these applicability constraints: 3.2 default; consumers may lag; OpenAPI.NET source API is separately migrated.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 111, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `openapi.select-contract-version`, `WEB-059`, `WEB-092`.

<a id="web-w32b"></a>

### 054 - Environment-specific OpenAPI generation

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** One focused authoring goal with original previous/current samples, actual consumer behavior and installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Build a contract using the intended app configuration.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Set build-time generation environment.
- [ ] Build actual documents under two environments and verify expected endpoint/transformer differences with the supported MSBuild property.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for openapi.generate-for-environment; preserve these applicability constraints: `ApiDescription.Server`; endpoint/transformer environment differences.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 111, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `openapi.generate-for-environment`, `WEB-063`.

<a id="web-w32c"></a>

### 055 - Contract deprecation from source metadata

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded original API/declaration migration with actual previous/current consumer proof and installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Mark retired endpoints and fields without a custom transformer.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Author `[Obsolete]`; remove redundant deprecation transformers.
- [ ] Generate operation/schema/property deprecation metadata from original Obsolete declarations and verify redundant transformer removal.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for openapi.retire-contract-member; preserve these applicability constraints: Operations/schema types/properties.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 111, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `openapi.retire-contract-member`, `WEB-064`.

<a id="web-w32d"></a>

### 056 - Supported OpenAPI transformer migrations

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** One focused authoring goal with original previous/current samples, actual consumer behavior and installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Keep custom contract transformations compiling.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Update OpenAPI.NET3 interfaces, references and extension value construction.
- [ ] Compile and run original custom transformers against the new interfaces/reference/extension-value API and inspect generated output.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for openapi.migrate-transformer-model; preserve these applicability constraints: ASP.NET transitive dependency migration, not standalone OpenAPI.NET catalogue.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 111, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `openapi.migrate-transformer-model`, `WEB-087`.

<a id="web-w32e"></a>

### 057 - Stable OpenAPI server URL composition

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded original API/declaration migration with actual previous/current consumer proof and installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Keep generated client URLs and fixtures consistent.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Update URL composition/snapshot expectations or explicit document transform.
- [ ] Generate documents with empty/non-empty PathBase and verify original URL composition fixtures; do not claim every client breaks.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for openapi.retain-server-url-contract; preserve these applicability constraints: Empty `PathBase` removes trailing slash; do not claim all clients break.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 111, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `openapi.retain-server-url-contract`, `WEB-093`.

<a id="web-w33a"></a>

### 058 - Record handshake failures and inspect ClientHello

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Record handshake failures and inspect ClientHello.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Move obsolete callback to connection middleware; inspect handshake exception.
- [ ] Run a loopback TLS handshake/failure and inspect ClientHello/handshake exception through supported connection middleware.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for hosting.inspect-tls-handshake; explicitly preserve these applicability constraints: Kestrel `UseTlsClientHelloListener`/`ITlsHandshakeFeature`.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 112, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `hosting.inspect-tls-handshake`, `WEB-077`.

<a id="web-w33b"></a>

### 059 - Expose or require transport binding for authenticated requests

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 004 - Provide controlled Windows web and design-time verification access](#h-windows-web)

**Delivered outcome:** Expose or require transport binding for authenticated requests.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Read channel binding; configure HTTP.sys hardening explicitly.
- [ ] Verify exposed channel binding and configured HTTP.sys hardening in an actual supported Windows HTTPS environment; state Kestrel/IIS differences.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for hosting.enforce-channel-binding; explicitly preserve these applicability constraints: HTTPS; Windows HTTP.sys `Legacy`/`Medium`/`Strict`; Kestrel/IIS feature differences.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 112, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `hosting.enforce-channel-binding`, `WEB-070`, `WEB-078`, `WEB-085`.

<a id="web-w33c"></a>

### 060 - Make authored clients and proxy adapters accepted by the server

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Make authored clients and proxy adapters accepted by the server.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Remove connection-specific H2/H3 headers and ambiguous H1 framing.
- [ ] Send actual malformed and corrected protocol requests and observe Kestrel rejection/acceptance; no invented compatibility switch.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for hosting.send-compliant-forwarded-requests; explicitly preserve these applicability constraints: Kestrel strict protocol compliance; no server compatibility switch invented.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 112, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `hosting.send-compliant-forwarded-requests`, `WEB-079`, `WEB-086`.

<a id="web-w34"></a>

### 061 - Collect framework traces without duplicate enrichment

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Collect framework traces without duplicate enrichment.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Subscribe native activity source; migrate enrichers/exporters and dependencies.
- [ ] Capture actual request activities with the final source/tags and migrated enrichment/export pipeline; show switch limitations and absence of duplicate instrumentation.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for web.collect-native-http-traces; explicitly preserve these applicability constraints: OTel tags/route/status changes; suppress switch does not revert all changes.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 112, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `web.collect-native-http-traces`, `WEB-074`, `WEB-085`.

<a id="web-w35a"></a>

### 062 - Select output-cache behavior from current tenant policy

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Select output-cache behavior from current tenant policy.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Implement output-cache policy provider instead of fixed registrations.
- [ ] Send real requests with changing tenant policy and observe actual cache selection/hit behavior through IOutputCachePolicyProvider.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for caching.resolve-policy-dynamically; explicitly preserve these applicability constraints: `IOutputCachePolicyProvider`; policy loading/selection semantics.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 112, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `caching.resolve-policy-dynamically`, `WEB-072`.

<a id="web-w35b"></a>

### 063 - Select compression and request decoding policy

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Select compression and request decoding policy.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Configure middleware/provider policy for zstd and interoperability.
- [ ] Exercise real response negotiation and request decompression for supported/unsupported encodings; separate explicit policy from automatic defaults.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for web.negotiate-zstandard-content; explicitly preserve these applicability constraints: Response compression/request decompression; automatic default alone excluded.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 112, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `web.negotiate-zstandard-content`, `WEB-075`, `WEB-096`.

<a id="web-w35c"></a>

### 064 - Retain concurrency limits after obsolete middleware removal

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Retain concurrency limits after obsolete middleware removal.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Replace old middleware/queue registration with rate limiting.
- [ ] Reproduce a saturation/queue scenario using the supported rate-limiter replacement and verify admission/rejection under real concurrent requests.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for web.migrate-concurrency-middleware; explicitly preserve these applicability constraints: Old package may temporarily be retained; no new .NET 11 version.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 112, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `web.migrate-concurrency-middleware`, `WEB-084`.

<a id="web-w36a"></a>

### 065 - Start MVC without removed compatibility APIs

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded authoring/migration change, with original old/new consumer, focused real proof and packed-MCP delivery; no broad platform matrix.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Start MVC without removed compatibility APIs.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Remove compatibility-version registrations/types.
- [ ] Compile and start a real MVC consumer after removing removed compatibility types/registration; exercise a representative request.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for mvc.remove-compatibility-shims; explicitly preserve these applicability constraints: MVC source/binary migration.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 108, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `mvc.remove-compatibility-shims`, `WEB-080`, `WEB-088`.

<a id="web-w36b"></a>

### 066 - Keep a customized image helper constructing correctly

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded authoring/migration change, with original old/new consumer, focused real proof and packed-MCP delivery; no broad platform matrix.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Keep a customized image helper constructing correctly.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Replace removed constructor and inert properties.
- [ ] Compile a real derived ImageTagHelper using the supported construction path and inspect generated markup.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for razor.update-image-helper-extension; explicitly preserve these applicability constraints: MVC/Razor Pages Tag Helpers.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 108, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `razor.update-image-helper-extension`, `WEB-090`.

<a id="web-w36c"></a>

### 067 - Identify reflected properties through the supported overload

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded authoring/migration change, with original old/new consumer, focused real proof and packed-MCP delivery; no broad platform matrix.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Identify reflected properties through the supported overload.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Replace removed string/type overload with property metadata.
- [ ] Compile and execute a real custom metadata provider using reflected property identity and verify model metadata.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for mvc.update-property-metadata-identity; explicitly preserve these applicability constraints: Custom model metadata providers; recompile dependents.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 108, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `mvc.update-property-metadata-identity`, `WEB-091`.

<a id="web-w36d"></a>

### 068 - Build views without newly obsolete registration calls

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded authoring/migration change, with original old/new consumer, focused real proof and packed-MCP delivery; no broad platform matrix.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Build views without newly obsolete registration calls.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Remove extra runtime-compilation overloads; use build-time compilation/Hot Reload.
- [ ] Compile and render MVC/Razor Pages after removing obsolete registration; show the supported build-time/Hot Reload authoring choice.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for razor.replace-runtime-compilation-registration; explicitly preserve these applicability constraints: MVC/Razor Pages; `ASPDEPR003`; runtime behavior unchanged.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 108, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `razor.replace-runtime-compilation-registration`, `WEB-099`.

<a id="foundation-api"></a>

### 069 - Validate Minimal API requests before handler execution

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Foundation with real endpoint evidence

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to validate Minimal API requests before handler execution.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original .NET 9/10 synchronous examples reject invalid input before the handler executes.
- [ ] The later asynchronous edition and business validation remain distinct.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 87, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `aspnet-core.validate-requests`.

## Data access guidance

**New native milestone.** Original verified EF, SQL Server, SQLite and Cosmos guidance records real provider applicability and evidence.

<a id="web-w37a"></a>

### 000 - Restore database authentication after driver dependency changes

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 003 - Provide an authorized Entra SQL test identity](#h-entra)

**Delivered outcome:** Restore database authentication after driver dependency changes.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Add SqlClient Azure extension explicitly; replace obsolete password mode where used.
- [ ] Verify direct Azure-auth extension dependencies in a real SQL consumer and distributed-cache configuration where applicable; actual Entra authentication requires owner-supplied authorized test identity, not a mocked token.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for sql.retain-entra-authentication; explicitly preserve these applicability constraints: EF SQL Server and SQL distributed cache; shared dependency-migration goal.
- [ ] If actual Entra login requires an owner-held session, preserve that portion as pending H_ENTRA_PROOF. The later Human action supplies real authentication evidence; direct references and ordinary SQL sample compilation remain this PR's responsibility.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 116, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `sql.retain-entra-authentication`, `WEB-097`, `WEB-138`.

<a id="web-w37b"></a>

### 001 - Keep generated SQL compatible with the actual database

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 000 - Provide an isolated SQL Server verification route](#h-sql)

**Delivered outcome:** Keep generated SQL compatible with the actual database.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Set provider compatibility explicitly below the new 160 default.
- [ ] Execute real queries on supplied supported lower/newer compatibility configurations and show provider configuration does not upgrade the database.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for sql.set-supported-compatibility; explicitly preserve these applicability constraints: SQL Server 2019 or older/lower configured compatibility.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 116, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `sql.set-supported-compatibility`, `WEB-118`, `WEB-142`.

<a id="web-w38a"></a>

### 002 - Inherited complex-value relational mappings

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** One goal with real browser/provider evidence, release/API or database-feature reconciliation and original previous/current samples; includes installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 000 - Provide an isolated SQL Server verification route](#h-sql)

**Delivered outcome:** Persist nested values in a split inheritance model.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Combine complex/JSON mappings with TPT/TPC.
- [ ] Apply an original mapping and round-trip complex/JSON values with TPT/TPC on a supplied real relational provider supporting that path.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for ef.map-nested-values-with-inheritance; preserve these applicability constraints: EF11 relational; verify provider support for actual JSON path.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 114, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `ef.map-nested-values-with-inheritance`, `WEB-100`, `WEB-104`.

<a id="web-w38b"></a>

### 003 - Chained nested-scalar EF configuration

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** One focused authoring goal with original previous/current samples, actual consumer behavior and installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Configure a deeply nested scalar without intermediate builders.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Use chained member lambda configuration.
- [ ] Build the actual model, create schema and round-trip a chained nested scalar configuration; state collection limitations.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for ef.configure-nested-scalar; preserve these applicability constraints: Complex properties; not arbitrary collections.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 114, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `ef.configure-nested-scalar`, `WEB-102`.

<a id="web-w38c"></a>

### 004 - Complex-value keys and lookup indexes

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** One goal with real browser/provider evidence, release/API or database-feature reconciliation and original previous/current samples; includes installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 000 - Provide an isolated SQL Server verification route](#h-sql)

**Delivered outcome:** Use an embedded value in identity or lookup indexes.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Define keys/indexes over complex scalar paths and JSON index paths.
- [ ] Inspect generated keys/indexes and run real identity/lookup behavior for supported complex paths; verify provider-specific JSON index support separately.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for ef.key-nested-value; preserve these applicability constraints: Provider-specific; keys exclude complex collections.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 114, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `ef.key-nested-value`, `WEB-103`.

<a id="web-w39a"></a>

### 005 - Map embedded sequences without a deprecated synthetic identity

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Map embedded sequences without a deprecated synthetic identity.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Migrate to complex collections or define an explicit non-shadow key.
- [ ] Compare actual old/new JSON collection models and round-trip original data under an explicit complex-collection or keyed-owned migration; identify schema/data consequences.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for ef.replace-ordinal-json-ownership; explicitly preserve these applicability constraints: Owned JSON collections without key; schema/data migration reviewed separately.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 114, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `ef.replace-ordinal-json-ownership`, `WEB-150`.

<a id="web-w39b"></a>

### 006 - Make scalar-versus-collection mapping unambiguous

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded authoring/migration change, with original old/new consumer, focused real proof and packed-MCP delivery; no broad platform matrix.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Make scalar-versus-collection mapping unambiguous.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Use PrimitiveCollection instead of Property where convention is overridden.
- [ ] Build the model and round-trip a deliberately ambiguous primitive collection configuration, including an applicable byte-array case.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for ef.configure-primitive-sequence; explicitly preserve these applicability constraints: Primitive collection configuration, including ambiguous byte arrays.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 114, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `ef.configure-primitive-sequence`, `WEB-151`.

<a id="web-w40a"></a>

### 007 - Query related data from each group's selected item

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Query related data from each group's selected item.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Compose navigation/joins over top-per-group result instead of client workaround.
- [ ] Execute the composed top-per-group/navigation query on a real provider and compare expected records; SQL text alone is insufficient.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for ef.compose-group-winner; explicitly preserve these applicability constraints: Relational translation; supported shapes require provider evidence.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 115, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `ef.compose-group-winner`, `WEB-105`, `WEB-106`, `WEB-156`.

<a id="web-w40b"></a>

### 008 - Preserve both sides of a database reconciliation

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 000 - Provide an isolated SQL Server verification route](#h-sql)

**Delivered outcome:** Preserve both sides of a database reconciliation.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Use translated `FullJoin`.
- [ ] Execute real FullJoin reconciliation and verify left-only/right-only/matched outcomes on a supporting provider; no SQLite support assumption.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for ef.reconcile-unmatched-records; explicitly preserve these applicability constraints: EF11 relational provider supporting full outer join; not blanket SQLite promise.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 115, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `ef.reconcile-unmatched-records`, `WEB-107`.

<a id="web-w40c"></a>

### 009 - Fetch the record associated with a minimum or maximum key

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Fetch the record associated with a minimum or maximum key.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Use translated MinBy/MaxBy and async equivalents.
- [ ] Execute actual MinBy/MaxBy queries including async form and deterministic declared tie behavior on a supporting provider.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for ef.select-record-at-extreme; explicitly preserve these applicability constraints: Query/provider ordering and tie semantics must be explicit.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 115, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `ef.select-record-at-extreme`, `WEB-108`.

<a id="web-w40d"></a>

### 010 - Use EF's asynchronous query adapter intentionally

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded authoring/migration change, with original old/new consumer, focused real proof and packed-MCP delivery; no broad platform matrix.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Use EF's asynchronous query adapter intentionally.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Replace analyzer-detected ToAsyncEnumerable with AsAsyncEnumerable.
- [ ] Consume a real EF asynchronous query and observe actual command execution/disposal when replacing analyzer-detected adapter use.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for ef.stream-query-without-reexecution; explicitly preserve these applicability constraints: EF query behavior; not shared LINQ package migration alone.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 115, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `ef.stream-query-without-reexecution`, `WEB-135`.

<a id="web-w41a"></a>

### 011 - Author SQL JSON path-existence queries

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** One focused authoring goal with original previous/current samples, actual consumer behavior and installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 000 - Provide an isolated SQL Server verification route](#h-sql)

**Delivered outcome:** Filter documents by the presence of a nested property.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Use `JsonPathExists` rather than hand-written SQL.
- [ ] Execute actual presence queries on supplied SQL Server 2022+ and distinguish absent paths from null values.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for sql.test-json-path; preserve these applicability constraints: SQL Server 2022+ JSON path existence.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 116, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `sql.test-json-path`, `WEB-109`.

<a id="web-w41b"></a>

### 012 - Native SQL JSON containment queries

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** One goal with real browser/provider evidence, release/API or database-feature reconciliation and original previous/current samples; includes installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 000 - Provide an isolated SQL Server verification route](#h-sql)

**Delivered outcome:** Search JSON content using native database operations.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Set appropriate compatibility and use JsonContains/path options.
- [ ] Execute actual JSON membership/path queries on supplied SQL Server 2025/170 with null/path cases and exact final feature status.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for sql.query-json-membership; preserve these applicability constraints: SQL Server 2025/170; nullable cases can retain older translation.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 116, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `sql.query-json-membership`, `WEB-114`.

<a id="web-w41c"></a>

### 013 - Author SQL JSON path indexes

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** One goal with real browser/provider evidence, release/API or database-feature reconciliation and original previous/current samples; includes installed MCP delivery.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 000 - Provide an isolated SQL Server verification route](#h-sql)

**Delivered outcome:** Maintain a database index over selected document paths.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Model/scaffold JSON indexes.
- [ ] Apply and inspect the modeled JSON index on a supplied database release supporting the final feature; verify query behavior and label preview/experimental status accurately.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for sql.index-json-path; preserve these applicability constraints: SQL Server JSON-index feature support; validate database release/status.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 116, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `sql.index-json-path`, `WEB-115`.

<a id="web-w42"></a>

### 014 - Indexed vector search and explicit vector loading

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 000 - Provide an isolated SQL Server verification route](#h-sql)

**Delivered outcome:** Rank approximate neighbors through an indexed query; Read back embeddings when entity materialization skips them.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Create vector index and compose `VectorSearch`/approximation. Project vector columns explicitly.
- [ ] Apply the actual vector index and execute exact/approximate searches on supplied SQL Server 2025; verify approximation semantics and explicit experimental status. Read a persisted vector through explicit projection and verify default entity projection omits it; no assumed future opt-in API.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for sql.search-vector-index, sql.read-vector-explicitly; explicitly preserve these applicability constraints: **Experimental SQL Server feature/API**; SQL Server 2025; exact versus approximate semantics. SqlVector properties no longer loaded by default; follow final opt-in availability.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 116, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `sql.search-vector-index`, `sql.read-vector-explicitly`, `WEB-110`, `WEB-111`, `WEB-146`.

<a id="web-w43"></a>

### 015 - Managed full-text indexing and ranked search

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 000 - Provide an isolated SQL Server verification route](#h-sql)

**Delivered outcome:** Manage searchable text and retrieve ranked matches.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Model full-text catalog/index and query table-valued search functions.
- [ ] Create original full-text catalog/index migrations and execute table-valued ranked search on a supplied server with full-text installed.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for sql.manage-ranked-text-search; explicitly preserve these applicability constraints: SQL Server full-text installation; migrations and rank result types.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 116, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `sql.manage-ranked-text-search`, `WEB-112`.

<a id="web-w44"></a>

### 016 - Combined lexical and vector ranking

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 000 - Provide an isolated SQL Server verification route](#h-sql)

**Delivered outcome:** Merge lexical and vector relevance in one query.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Compose supported hybrid ranking rather than disconnected client ranking.
- [ ] Execute actual combined lexical/vector ranking on supplied supported capabilities; identify rank combination semantics and experimental vector prerequisites.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for sql.combine-semantic-and-text-search; explicitly preserve these applicability constraints: SQL Server feature requirements; experimental vector side remains explicit.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 116, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `sql.combine-semantic-and-text-search`, `WEB-113`.

<a id="web-w45a"></a>

### 017 - Read row validity through ordinary entity properties

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 000 - Provide an isolated SQL Server verification route](#h-sql)

**Delivered outcome:** Read row validity through ordinary entity properties.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Map period columns to CLR properties instead of shadow lookups.
- [ ] Round-trip and query temporal rows with CLR period properties; verify database generation and blocked user updates.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for sql.expose-temporal-boundaries; explicitly preserve these applicability constraints: SQL Server temporal; database-generated and not user-updatable.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 116, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `sql.expose-temporal-boundaries`, `WEB-116`.

<a id="web-w45b"></a>

### 018 - Perform time conversion and bucketing on the server

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 000 - Provide an isolated SQL Server verification route](#h-sql)

**Delivered outcome:** Perform time conversion and bucketing on the server.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Use newly translated offset members/constructors and DateTrunc.
- [ ] Execute newly translated time expressions on supplied supported server versions and verify server-local versus client-local semantics.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for sql.query-offset-and-date-parts; explicitly preserve these applicability constraints: SQL Server function/version constraints; server-local differs from client-local time.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 116, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `sql.query-offset-and-date-parts`, `WEB-117`.

<a id="web-w46a"></a>

### 019 - Return an ordered textual summary per group

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Return an ordered textual summary per group.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Order within translated string aggregation.
- [ ] Execute ordered aggregation on actual SQLite3.44+ using the declared native provider and inspect deterministic results.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for sqlite.order-concatenated-values; explicitly preserve these applicability constraints: SQLite 3.44+; replaces client evaluation/workaround.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 116, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `sqlite.order-concatenated-values`, `WEB-119`.

<a id="web-w46b"></a>

### 020 - Bind a full-width unsigned integer without custom encoding

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded authoring/migration change, with original old/new consumer, focused real proof and packed-MCP delivery; no broad platform matrix.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Bind a full-width unsigned integer without custom encoding.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Pass UInt128 parameter using ordered padded representation.
- [ ] Bind full-width UInt128 values to real SQLite and inspect the padded ordered storage; do not claim unsupported reader UInt128 retrieval.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for sqlite.bind-large-unsigned-value; explicitly preserve these applicability constraints: Microsoft.Data.Sqlite; reader UInt128 retrieval not yet supported.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 116, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `sqlite.bind-large-unsigned-value`, `WEB-120`.

<a id="web-w46c"></a>

### 021 - Keep low-level SQLite access on a supported framework

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Keep low-level SQLite access on a supported framework.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Upgrade target or retain compatible earlier provider deliberately.
- [ ] Build/run the supported Microsoft.Data.Sqlite11 net10 consumer and a documented earlier-dependency alternative; state unsupported legacy platform paths without claiming unavailable native proof.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for sqlite.retain-supported-runtime; explicitly preserve these applicability constraints: Microsoft.Data.Sqlite11 targets net10 only; separate from EF11 net11 requirement.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 116, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `sqlite.retain-supported-runtime`, `WEB-153`, `WEB-155`.

<a id="web-w46d"></a>

### 022 - Restore native SQLite initialization after bundle retirement

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Restore native SQLite initialization after bundle retirement.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Select provider/initialization matching existing native/encryption needs.
- [ ] Run actual native SQLite initialization using an appropriate supported provider/bundle path; preserve declared native/encryption constraints without choosing a paid vendor by default.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for sqlite.replace-retired-native-bundle; explicitly preserve these applicability constraints: SQLitePCLRaw external dependency; paid/encryption alternatives are not default product choices.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 116, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `sqlite.replace-retired-native-bundle`, `WEB-154`.

<a id="web-w47a"></a>

### 023 - Treat nested document content as values rather than entities

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 001 - Provide an isolated Cosmos verification route](#h-cosmos)

**Delivered outcome:** Treat nested document content as values rather than entities.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Adopt Cosmos complex properties/collections and value semantics.
- [ ] Round-trip real Cosmos complex properties/collections and verify document shape/value semantics on supplied supported isolated resources.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for cosmos.map-embedded-values; explicitly preserve these applicability constraints: Cosmos EF11; includes repeated Complex types section.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 117, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `cosmos.map-embedded-values`, `WEB-101`.

<a id="web-w47b"></a>

### 024 - Provision only the document indexes the workload needs

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 001 - Provide an isolated Cosmos verification route](#h-cosmos)

**Delivered outcome:** Provision only the document indexes the workload needs.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Model automatic/path/composite/vector/full-text policy.
- [ ] Apply and inspect actual path/composite/vector/full-text indexing policy for the supported supplied container capabilities.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for cosmos.configure-index-policy; explicitly preserve these applicability constraints: Cosmos account/container capabilities; nested complex/collection paths.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 117, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `cosmos.configure-index-policy`, `WEB-121`.

<a id="web-w48a"></a>

### 025 - Make the scope of a multi-document save explicit

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 001 - Provide an isolated Cosmos verification route](#h-cosmos)

**Delivered outcome:** Make the scope of a multi-document save explicit.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Select Auto/Never/Always transaction behavior.
- [ ] Exercise actual multi-document saves within and across applicable partition/container limits; observe Auto/Never/Always outcomes.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for cosmos.choose-save-atomicity; explicitly preserve these applicability constraints: Container+partition batch limits; Auto is not global atomicity.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 117, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `cosmos.choose-save-atomicity`, `WEB-122`.

<a id="web-w48b"></a>

### 026 - Increase ingestion concurrency through supported configuration

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 001 - Provide an isolated Cosmos verification route](#h-cosmos)

**Delivered outcome:** Increase ingestion concurrency through supported configuration.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Opt into bulk execution.
- [ ] Run real opted-in ingestion and verify persistence/error behavior; make no unmeasured throughput promise or atomicity assumption.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for cosmos.enable-bulk-writes; explicitly preserve these applicability constraints: Cosmos; throughput/atomicity tradeoff rather than automatic speed claim.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 117, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `cosmos.enable-bulk-writes`, `WEB-123`.

<a id="web-w48c"></a>

### 027 - Preserve read-your-writes across application instances

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 001 - Provide an isolated Cosmos verification route](#h-cosmos)

**Delivered outcome:** Preserve read-your-writes across application instances.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Enable manual token mode and propagate tokens.
- [ ] Propagate actual owner-scoped session tokens across independent clients and verify read-your-writes behavior under supported consistency.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for cosmos.carry-session-consistency; explicitly preserve these applicability constraints: Cosmos session consistency; caller-bound token ownership.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 117, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `cosmos.carry-session-consistency`, `WEB-124`.

<a id="web-w49"></a>

### 028 - Author Cosmos raw-document and unmapped-field migration

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 001 - Provide an isolated Cosmos verification route](#h-cosmos)

**Delivered outcome:** Avoid losing fields owned by another writer; Access raw JSON without removed shadow state.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Map required fields or use direct document API for round-trip ownership. Replace `__jObject` access with direct CosmosClient.
- [ ] Reproduce unmapped-field removal with old/new EF behavior on a real isolated document; verify the explicit mapping/direct-client preservation strategy and data ownership. Replace real __jObject usage with CosmosClient and verify actual raw-document read/write ownership.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for cosmos.preserve-document-fields, cosmos.read-raw-document; explicitly preserve these applicability constraints: Unmapped fields no longer preserved on EF save; potential data-loss migration. System.Text.Json internal rewrite; explicit raw-data ownership.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 117, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `cosmos.preserve-document-fields`, `cosmos.read-raw-document`, `WEB-125`, `WEB-136`, `WEB-141`.

<a id="web-w50"></a>

### 029 - Keep existing documents reachable after key escaping changes

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 001 - Provide an isolated Cosmos verification route](#h-cosmos)

**Delivered outcome:** Keep existing documents reachable after key escaping changes.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Plan key/data migration or bounded compatibility switch.
- [ ] Create original collision/escaping fixtures, verify old/new actual document reachability and a safe migration/compatibility route; explicitly retain the old switch's collision defect warning.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for cosmos.retain-document-key-identity; explicitly preserve these applicability constraints: Composite/discriminator IDs; old switch retains collision defect.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 117, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `cosmos.retain-document-key-identity`, `WEB-139`.

<a id="web-w51a"></a>

### 030 - Decide what an absent document path contributes to a result

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 001 - Provide an isolated Cosmos verification route](#h-cosmos)

**Delivered outcome:** Decide what an absent document path contributes to a result.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Use IsDefined or CoalesceUndefined.
- [ ] Execute real absent-path versus JSON-null projections and verify IsDefined/CoalesceUndefined behavior.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for cosmos.handle-missing-projection; explicitly preserve these applicability constraints: Cosmos undefined now throws consistently.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 117, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `cosmos.handle-missing-projection`, `WEB-126`, `WEB-140`.

<a id="web-w51b"></a>

### 031 - Handle an empty document collection through collection semantics

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 001 - Provide an isolated Cosmos verification route](#h-cosmos)

**Delivered outcome:** Handle an empty document collection through collection semantics.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Remove null-as-empty workaround where affected.
- [ ] Round-trip empty owned collections and compare actual old/new materialization, distinguishing optional references.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for cosmos.treat-empty-owned-sequence; explicitly preserve these applicability constraints: Owned collections now initialize empty; optional references differ.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 117, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `cosmos.treat-empty-owned-sequence`, `WEB-147`.

<a id="web-w51c"></a>

### 032 - Keep numeric projections from silently changing business results

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 001 - Provide an isolated Cosmos verification route](#h-cosmos)

**Delivered outcome:** Keep numeric projections from silently changing business results.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Apply explicit rounding when former rounding is required.
- [ ] Execute real type-specific floating-to-fixed projections and verify truncation versus explicit rounding of original business fixtures.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for cosmos.preserve-rounding-intent; explicitly preserve these applicability constraints: Floating-to-fixed materialization now truncates; verify type-specific semantics.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 117, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `cosmos.preserve-rounding-intent`, `WEB-149`.

<a id="web-w52a"></a>

### 033 - Complete document queries and saves without removed synchronous paths

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 001 - Provide an isolated Cosmos verification route](#h-cosmos)

**Delivered outcome:** Complete document queries and saves without removed synchronous paths.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Replace sync I/O calls; remove obsolete opt-in assumption.
- [ ] Exercise actual async query/save and reproduce removed synchronous behavior; state no opt-in restores Cosmos sync I/O.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for cosmos.use-async-io; explicitly preserve these applicability constraints: Cosmos; no switch restores sync in EF11.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 117, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `cosmos.use-async-io`, `WEB-137`.

<a id="web-w52b"></a>

### 034 - Query type identity using its correct model name

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded authoring/migration change, with original old/new consumer, focused real proof and packed-MCP delivery; no broad platform matrix.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 001 - Provide an isolated Cosmos verification route](#h-cosmos)

**Delivered outcome:** Query type identity using its correct model name.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Update model-property references while retaining stored JSON name.
- [ ] Inspect the actual model Discriminator and persisted $type and execute a corrected query without renaming existing JSON data.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for cosmos.reference-model-discriminator; explicitly preserve these applicability constraints: Model `Discriminator` versus stored `$type`; no automatic data rename.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 117, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `cosmos.reference-model-discriminator`, `WEB-148`.

<a id="web-w53a"></a>

### 035 - Reconcile competing migration histories before merge

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Reconcile competing migration histories before merge.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Resolve snapshot migration-ID conflict by re-scaffolding an appropriate branch.
- [ ] Create original competing migration histories and resolve them through actual scaffolding; verify coherent snapshot/latest-ID output.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for ef.resolve-migration-branch-divergence; explicitly preserve these applicability constraints: Team migration workflow; do not silently accept both histories.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 118, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `ef.resolve-migration-branch-divergence`, `WEB-128`.

<a id="web-w53b"></a>

### 036 - Add and apply a migration in one command

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 000 - Provide an isolated SQL Server verification route](#h-sql)

**Delivered outcome:** Add and apply a migration in one command.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Use new database-update add option while retaining generated files.
- [ ] Run the actual combined CLI/PMC flow against a supplied isolated database and verify both generated files and applied schema.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for ef.scaffold-and-apply-migration; explicitly preserve these applicability constraints: dotnet-ef/PMC; live schema mutation must be explicit in actual consumer workflow.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 118, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `ef.scaffold-and-apply-migration`, `WEB-129`.

<a id="web-w53c"></a>

### 037 - Undo local migration scaffolding without a reachable database

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Undo local migration scaffolding without a reachable database.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Use offline removal or explicit connection selection.
- [ ] Execute offline removal with an unreachable database and inspect resulting files; verify force/connection-selection limitations without implying applied-state checks.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for ef.remove-unapplied-migration-offline; explicitly preserve these applicability constraints: Offline excludes force; applied state is not checked offline.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 118, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `ef.remove-unapplied-migration-offline`, `WEB-130`.

<a id="web-w54a"></a>

### 038 - Keep a modeled relationship without creating its database constraint

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Keep a modeled relationship without creating its database constraint.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Exclude constraint from migrations explicitly.
- [ ] Apply a real migration and inspect absent constraint plus retained model relationship/index; state changed database integrity ownership.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for ef.omit-database-foreign-key; explicitly preserve these applicability constraints: Relational; relationship/index retained; data integrity responsibility changes.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 114, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `ef.omit-database-foreign-key`, `WEB-127`.

<a id="web-w54b"></a>

### 039 - Distinguish intentional schema ownership from missing migrations

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded authoring/migration change, with original old/new consumer, focused real proof and packed-MCP delivery; no broad platform matrix.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Distinguish intentional schema ownership from missing migrations.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Remove unwanted migrate call or intentionally configure diagnostic behavior.
- [ ] Reproduce the real no-migrations exception and verify intentional schema-owner alternative or documented diagnostic configuration.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for ef.handle-empty-migration-assembly; explicitly preserve these applicability constraints: Migrate/MigrateAsync now throw when no migrations found.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 118, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `ef.handle-empty-migration-assembly`, `WEB-143`.

<a id="web-w55a"></a>

### 040 - Share predictable design-time command defaults

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Share predictable design-time command defaults.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Author dotnet-ef JSON config and targeted command overrides.
- [ ] Execute real dotnet-ef commands under config-file directory/relative-path and explicit override scenarios.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for ef.configure-migration-commands; explicitly preserve these applicability constraints: Directory lookup and relative paths; CLI overrides file.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 118, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `ef.configure-migration-commands`, `WEB-131`.

<a id="web-w55b"></a>

### 041 - Run a design-time operation using an already-current build

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 004 - Provide controlled Windows web and design-time verification access](#h-windows-web)

**Delivered outcome:** Run a design-time operation using an already-current build.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Author and compile the original EF design-time consumer and accurate supported PMC NoBuild command, preserving explicit current-build and stale-artifact constraints.
- [ ] Mark actual interactive Visual Studio PMC execution pending H_PMC_PROOF; do not substitute a CLI emulation for that product-specific evidence.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 114, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `ef.reuse-current-build-in-pmc`, `WEB-132`.

<a id="web-w55c"></a>

### 042 - Restore design-time services previously supplied transitively

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded authoring/migration change, with original old/new consumer, focused real proof and packed-MCP delivery; no broad platform matrix.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Restore design-time services previously supplied transitively.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Add direct Design reference where actually required.
- [ ] Reproduce a real design-time consumer needing Design services and verify the explicit dependency restores it without adding needless references elsewhere.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for ef.reference-design-services; explicitly preserve these applicability constraints: Tools/Tasks no longer depend on Design.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 118, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `ef.reference-design-services`, `WEB-145`.

<a id="web-w56a"></a>

### 043 - Apply a command consistently to several context models

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** A focused original goal or closely related sample set requiring real consumer/request/CLI execution, declared prerequisites and packed-MCP integration.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Apply a command consistently to several context models.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Use context wildcard only on supported commands.
- [ ] Run actual wildcard-supported commands over two original contexts and verify each intended database/script target.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for ef.operate-across-contexts; explicitly preserve these applicability constraints: list/script/update/drop; multiple target databases and generated scripts.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 118, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `ef.operate-across-contexts`, `WEB-133`.

<a id="web-w56b"></a>

### 044 - Pipe only generated SQL into a deployment artifact

**New · Agent · S (2) · Labels: Agent, Feature**

**Estimate rationale:** One bounded authoring/migration change, with original old/new consumer, focused real proof and packed-MCP delivery; no broad platform matrix.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Pipe only generated SQL into a deployment artifact.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Remove log filtering; separate stdout payload/stderr diagnostics.
- [ ] Capture real CLI stdout/stderr separately and verify the artifact contains only executable SQL without log filtering.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for ef.capture-clean-migration-script; explicitly preserve these applicability constraints: dotnet-ef scripts; change authored shell pipeline.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 118, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `ef.capture-clean-migration-script`, `WEB-134`.

<a id="web-w57"></a>

### 045 - Stage-specific generated EF artifacts

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Generate models and precompiled queries at the intended stage.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Replace EFOptimizeContext with stage-specific properties.
- [ ] Run actual build/publish generation with the supported stage-specific properties and inspect generated model/query artifacts; limit precompilation/AOT claims to tested paths.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for ef.configure-generated-model-stage; explicitly preserve these applicability constraints: Build/publish/AOT; do not assume all query shapes support precompilation.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 118, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `ef.configure-generated-model-stage`, `WEB-144`.

<a id="web-w58"></a>

### 046 - Split-query concurrency recovery

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Full original old/new authoring plus real browser/provider/concurrent behavior or multiple tightly related contracts; package/MCP proof and release reconciliation add uncertainty. Split further if actual work reaches XL.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 000 - Provide an isolated SQL Server verification route](#h-sql)

**Delivered outcome:** Retry or isolate a split read when concurrent writes disrupt it.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Handle new query-concurrency exception or use snapshot/serializable/single query.
- [ ] Coordinate real concurrent writes between split query commands to reproduce the new exception and verify the documented retry/isolation/single-query mitigation on the actual provider.
- [ ] Publish original canonical old/new samples, metadata and full Markdown prose for ef.recover-split-query-consistency; explicitly preserve these applicability constraints: Relational; transaction/provider guarantees differ; no blanket consistency claim.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 115, 126, 127, 130, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `ef.recover-split-query-consistency`, `WEB-152`.

<a id="foundation-ef"></a>

### 047 - Include archived records while retaining tenant filtering

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Foundation with real provider evidence

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Deliver original, offline-complete, verified recipe guidance enabling developers to include archived records while retaining tenant filtering.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Original EF 9/10 examples selectively disable the archival query filter.
- [ ] Real provider evidence confirms the tenant filter remains without claiming authorization or write protection.

**Context:** Full scope includes source reconciliation, original samples and prose, actual compilation, focused claim-specific checks and installed offline MCP retrieval. No resource access is assumed.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 41, 43, 44, 45, 46, 47, 88, 127, 130, 133, 134, 138, 139, 159. Supporting inventory references: `ef.include-archived-with-tenant-filter`.

## Desktop guidance

**New native milestone.** Original WindowsDesktop guidance and targeted human proof preserve the actual C#/VB application and platform contracts.

<a id="nat-d01"></a>

### 000 - Restore intentional WPF clipboard error handling

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Small API surface, but native clipboard failure evidence and older-build comparison matter. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 020 - Provide an actual Windows native authoring route](#nat-h-windows-route)

**Delivered outcome:** A WPF developer can choose the correct retrieval-error contract.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Distinguish default empty-result handling from the exception opt-in; never present this as a WinForms API.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 119, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:001:wpf.restore-clipboard-error-handling`.

<a id="nat-d02"></a>

### 001 - Adopt modern WinForms appearance deliberately

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** One screen spans ambient defaults, renderer restrictions and layout changes. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 020 - Provide an actual Windows native authoring route](#nat-h-windows-route)

**Delivered outcome:** A settings screen chooses modern rendering without losing compatibility.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Cover inheritance, editable padding, composite-control fallbacks, toggle constraints and preview-only TreeView migration.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 120, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:002:winforms.adopt-modern-rendering`, `inventory03:003:winforms.adopt-modern-rendering`, `inventory03:010:winforms.present-toggle-options`, `inventory03:011:winforms.customize-small-appearance-details`, `inventory03:016:winforms.adopt-modern-rendering`.

<a id="nat-d03"></a>

### 002 - Make custom WinForms rendering follow Windows preferences

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Protected hooks, lifetime ownership and dynamic system settings require focused native evidence. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 020 - Provide an actual Windows native authoring route](#nat-h-windows-route)

**Delivered outcome:** A custom control responds correctly to renderer and accessibility changes.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Use control-lifetime notifications and protected change-impact/effective-mode hooks; preserve High Contrast semantics.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 120, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:004:winforms.author-adaptive-control-rendering`, `inventory03:005:winforms.follow-system-visual-settings`.

<a id="nat-d04"></a>

### 003 - Configure initial form reveal through C# and VB startup

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Two genuine startup authoring models, with different framework defaults. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 020 - Provide an actual Windows native authoring route](#nat-h-windows-route)

**Delivered outcome:** C# and VB applications deliberately select initial presentation.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Use original VB ApplicationEvents startup and distinguish automatic, deferred and splash exceptions.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 95, 120, 121, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:006:winforms.control-initial-form-reveal`, `inventory03:007:winforms.configure-vb-application-defaults`.

<a id="nat-d05"></a>

### 004 - Batch nested control mutations with disposable scopes

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Focused layout/painting integration and custom hook behavior. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 020 - Provide an actual Windows native authoring route](#nat-h-windows-route)

**Delivered outcome:** Nested UI updates use a single exception-safe mutation scope.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Acquire the correct traversal scope before ambient mutations and keep low-level begin/end infrastructure out of ordinary user code.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 120, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:008:winforms.batch-control-mutations`, `inventory03:009:winforms.batch-control-mutations`.

<a id="nat-d06"></a>

### 005 - Replace manual fullscreen presentation management

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Window restoration and power requests need native integration evidence. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 020 - Provide an actual Windows native authoring route](#nat-h-windows-route)

**Delivered outcome:** An application owns and restores fullscreen presentation safely.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Verify ownership/state restoration and explicitly separate presentation from Windows security lockdown.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 120, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:012:winforms.manage-fullscreen-experience`.

<a id="nat-d07"></a>

### 006 - Preserve C# and VB WinForms Designer compatibility

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** One diagnostic/Designer seam includes materially different VB and C# syntax. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 020 - Provide an actual Windows native authoring route](#nat-h-windows-route)

**Delivered outcome:** Generated forms remain compatible with shipped Designer diagnostics.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Fix explicit initialization, VB WithEvents/Handles and deliberate getter allocations without blanket suppression.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 95, 120, 121, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:013:winforms.preserve-designer-roundtripping`, `inventory03:014:winforms.wire-vb-designer-events`, `inventory03:015:winforms.return-intentional-property-values`.

<a id="nat-h-desktop-settings-proof"></a>

### 007 - Check custom rendering across owner-controlled Windows settings

**New · Human · S (2) · Labels: Testing, Platform**

**Estimate rationale:** Human-owned system setting change and visual judgement, tightly bounded.

**Blockers:** [Desktop guidance / 002 - Make custom WinForms rendering follow Windows preferences](#nat-d03)

**Delivered outcome:** The adaptive rendering claim is checked against real accessibility settings.

**Acceptance criteria:**

- [ ] Use D03's original sample for a small deliberate change of relevant Windows preference/High Contrast state.
- [ ] Record actual notification/rendering outcomes and restore the owner's settings afterward.
- [ ] Attach focused evidence without claiming a broad visual regression suite.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 120, 133, 134, 151, 160. Supporting inventory references: `winforms.author-adaptive-control-rendering`, `winforms.follow-system-visual-settings`.

<a id="nat-h-fullscreen-proof"></a>

### 008 - Check fullscreen restoration and idle behavior in the sample

**New · Human · S (2) · Labels: Testing, Platform**

**Estimate rationale:** Owner-visible fullscreen/power behavior and judgement, not application authoring.

**Blockers:** [Desktop guidance / 005 - Replace manual fullscreen presentation management](#nat-d06)

**Delivered outcome:** The fullscreen recipe's native presentation/power claims are grounded.

**Acceptance criteria:**

- [ ] Run D06's compiled sample in an owner-approved session and check entry/exit/state restoration plus the specific claimed idle request.
- [ ] Record actual OS/session limitations and restore sample-created changes.
- [ ] Keep Windows lockdown configuration outside the action and do not affect unrelated running applications.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 120, 133, 134, 151, 160. Supporting inventory references: `winforms.manage-fullscreen-experience`.

## Mobile and native guidance

**New native milestone.** Original MAUI, Android and Apple guidance has actual native authoring evidence and explicit targeted human proof.

<a id="nat-m01"></a>

### 000 - Migrate ordinary MAUI XAML to SourceGen

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Generated-code prerequisites and fallback diagnostics must match actual artifacts. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 020 - Provide an actual Windows native authoring route](#nat-h-windows-route); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route)

**Delivered outcome:** A page uses the supported generator and namespace model.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Preserve necessary explicit mappings and distinguish generator migration from automatic performance gains.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 122, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:018:maui.choose-xaml-generation`, `inventory03:019:maui.simplify-xaml-namespaces`.

<a id="nat-m02"></a>

### 001 - Author preview C# logic and expressions in XAML

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Preview syntax, generated output and expression diagnostics interact. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 020 - Provide an actual Windows native authoring route](#nat-h-windows-route); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route)

**Delivered outcome:** View-local logic uses the actual preview SourceGen contracts.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Cover x:Code, namespace-qualified expressions and attached-property access with exact opt-ins and inflator limits.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 122, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:020:maui.author-view-local-csharp-in-xaml`, `inventory03:021:maui.bind-xaml-csharp-expressions`.

<a id="nat-m03"></a>

### 002 - Build trim-safe relative and reference bindings

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** A debug managed build cannot establish trimming/AOT integration. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route)

**Delivered outcome:** Bindings compile and retain required members in the relevant publish path.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Show compiled ancestor/reference paths, unresolved-name errors and documented runtime fallbacks in the appropriate trimmed output.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 122, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:022:maui.author-trim-safe-relative-bindings`.

<a id="nat-m04"></a>

### 003 - Use native XAML Hot Reload from the CLI

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Interactive workflow plus version-specific linker and preview-engine behavior. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route); [Verified authoring baselines / 022 - Provide a matched Mac and Xcode authoring route](#nat-h-apple-route)

**Delivered outcome:** The documented CLI workflow updates an existing native app.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Verify target selection, engine defaults/opt-out and current iOS constraints without preserving stale workarounds.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 104, 122, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:023:maui.iterate-with-native-hot-reload`.

<a id="nat-m05"></a>

### 004 - Update visual resources and replace invalid Color inheritance

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Closely related resource authoring includes one source-breaking type change. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 020 - Provide an actual Windows native authoring route](#nat-h-windows-route); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route)

**Delivered outcome:** One visual component updates styles/brushes through supported contracts.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Use invalidation, brush filling and composition; resolve Fill/Color wording against actual package APIs.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 122, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:024:maui.reapply-mutated-style-state`, `inventory03:026:maui.fill-boxes-with-brushes`, `inventory03:054:maui.replace-color-inheritance`.

<a id="nat-m06"></a>

### 005 - Handle a cancellable press-and-hold interaction

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** One interaction goal with gesture and cooperative-task lifecycles. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route)

**Delivered outcome:** A gesture can drive and cancel one awaited animation.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Replace custom gesture plumbing and establish cancellation semantics from actual evidence.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 122, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:025:maui.handle-long-press`, `inventory03:027:maui.cancel-one-animation`.

<a id="nat-m07"></a>

### 006 - Build accessible routed MAUI navigation

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** One navigation screen spans routing, badge/swipe presentation and accessibility contracts. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 020 - Provide an actual Windows native authoring route](#nat-h-windows-route); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route)

**Delivered outcome:** An original navigation flow carries route parameters and understandable action state.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Cover absolute route-template constraints, explicit tint migration and independent spoken labels with platform exceptions.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 122, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:028:maui.navigate-with-path-parameters`, `inventory03:033:maui.show-tab-badges`, `inventory03:034:maui.control-swipe-action-colors`, `inventory03:036:maui.describe-navigation-for-accessibility`.

<a id="nat-m08"></a>

### 007 - Migrate Android Shell off removed compatibility dependencies

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Breaking dependency plus renderer-to-handler migration. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route)

**Delivered outcome:** Android navigation customization uses supported handlers/dependencies.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Keep unaffected compatibility-named Controls.Core types; explicitly map Android portion of shared navigation recipe.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 122, 123, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:029:maui.migrate-navigation-handlers`, `inventory03:032:maui.remove-compatibility-package`.

<a id="nat-m09"></a>

### 008 - Migrate Apple navigation customizations to handlers

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Different page handlers and event timing demand separate Apple integration. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 022 - Provide a matched Mac and Xcode authoring route](#nat-h-apple-route)

**Delivered outcome:** Apple page/navigation customizations respect the new lifecycle.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Cover NavigationPage/TabbedPage/FlyoutPage timing and fallback; distinguish opt-in Apple Shell from Android defaults.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 122, 124, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:029:maui.migrate-navigation-handlers`, `inventory03:030:maui.migrate-navigation-handlers`.

<a id="nat-m10"></a>

### 009 - Migrate custom collection and carousel handlers

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Related handler seam with different Windows and Apple defaults/status. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 020 - Provide an actual Windows native authoring route](#nat-h-windows-route); [Verified authoring baselines / 022 - Provide a matched Mac and Xcode authoring route](#nat-h-apple-route)

**Delivered outcome:** Custom item handlers use the correct supported platform implementation.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Keep Apple Items2 obsoletion separate from Windows CollectionView2 fallback and supported legacy Windows handlers.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 122, 124, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:031:maui.migrate-custom-items-handlers`.

<a id="nat-m11"></a>

### 010 - Customize public Android Material3 handlers

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Native customization must use actually shipped public types. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route)

**Delivered outcome:** An app replaces reflection against internal Material helpers.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Build a minimal handler extension and exclude automatic stock restyling from the authored claim.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 122, 123, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:035:maui.customize-material-handlers`.

<a id="nat-m12"></a>

### 011 - Call .NET from HybridWebView with trimming and AOT

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Generator/interceptor, JSON metadata and native publish evidence. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route)

**Delivered outcome:** Generated JS-to-.NET invocation survives the claimed publish configuration.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Prove generated invocation and JSON paths while stating analyzer/interceptor requirements and legacy-overload limits.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 122, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:037:maui.call-dotnet-from-hybrid-js-with-aot`.

<a id="nat-m13"></a>

### 012 - Cache selected BlazorWebView local assets

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Different control and behavior seam from HybridWebView invocation. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 020 - Provide an actual Windows native authoring route](#nat-h-windows-route)

**Delivered outcome:** A Hybrid host deliberately caches only eligible local content.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Show explicit cache policy with retained no-store and unsupported-request boundaries.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 112, 122, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:038:maui.cache-hybrid-static-assets`.

<a id="nat-m14"></a>

### 013 - Generate C# UI and Avalonia-integrated MAUI projects

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Generated option combinations and third-party package composition need verification. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 020 - Provide an actual Windows native authoring route](#nat-h-windows-route); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route)

**Delivered outcome:** First-party templates produce reproducible supported integration outputs.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Pin generated dependencies/options; retain resource XAML and restrict Avalonia coverage to first-party template integration.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 104, 122, 125, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:039:maui.create-csharp-ui-project`, `inventory03:056:maui.include-avalonia-template-output`.

<a id="nat-m15"></a>

### 014 - Integrate an out-of-tree MAUI backend through first-party contracts

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Wide extension seam, kept to one deliberately small backend demonstration. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 020 - Provide an actual Windows native authoring route](#nat-h-windows-route)

**Delivered outcome:** A minimal backend uses supported service and asset extension seams.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Exercise required asset/SingleProject and public service contracts without teaching a standalone third-party framework.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 122, 125, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:040:maui.build-community-platform-backends`.

<a id="nat-m16"></a>

### 015 - Compose and interact with a dense native map

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Map APIs have significant per-platform capability differences. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route)

**Delivered outcome:** An original map goal uses supported regions, overlays and marker interaction.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Show declarative region/overlays and clustered/custom markers with explicit Android styling and unsupported routes.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 122, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:041:maui.compose-maps-declaratively`, `inventory03:042:maui.interact-with-dense-map-markers`.

<a id="nat-m17"></a>

### 016 - Enable the supported Windows map surface

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Windows-specific service integration with separately owned credentials/runtime evidence. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 020 - Provide an actual Windows native authoring route](#nat-h-windows-route)

**Delivered outcome:** Windows map integration is compiled and its service requirements are explicit.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Compile token registration using a safe placeholder and disclose unsupported Windows capabilities; real service proof is separately human-owned.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 122, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:043:maui.enable-windows-maps`.

<a id="nat-m18"></a>

### 017 - Authenticate through native passkey ceremonies

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Security ceremony, platform trust and capability differences; physical/trust actions are separate. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route); [Verified authoring baselines / 022 - Provide a matched Mac and Xcode authoring route](#nat-h-apple-route); [Verified authoring baselines / 020 - Provide an actual Windows native authoring route](#nat-h-windows-route)

**Delivered outcome:** Native ceremony guidance keeps relying-party verification on the server.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Compile platform integrations and source contracts; make domain/signing/device requirements explicit and link human ceremony evidence.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 109, 122, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:044:maui.authenticate-with-platform-passkeys`.

<a id="nat-m19"></a>

### 018 - Recover interrupted media capture and optionally save it

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Process recreation and per-platform capture behavior need focused native proof. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route); [Verified authoring baselines / 022 - Provide a matched Mac and Xcode authoring route](#nat-h-apple-route)

**Delivered outcome:** Media workflows retain identity across Android recreation and explicit gallery saving.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Persist workflow state, consume/clear/discard recovered Android results and distinguish capture/gallery from picking or no-op routes.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 122, 123, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:045:maui.recover-interrupted-media-picking`, `inventory03:046:maui.save-captured-media-to-gallery`.

<a id="nat-m20"></a>

### 019 - Configure system-bar icons and chrome backgrounds

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Platform-specific appearance semantics are easily conflated. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route); [Verified authoring baselines / 022 - Provide a matched Mac and Xcode authoring route](#nat-h-apple-route)

**Delivered outcome:** The app deliberately selects icon contrast and supported chrome backgrounds.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Keep icon foreground, Android background opt-in and app theme independent and correctly tagged.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 122, 123, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:047:maui.choose-status-bar-appearance`.

<a id="nat-m21"></a>

### 020 - Deliver external activations to one Windows instance

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Native multi-process lifecycle integration, isolated from unrelated running apps. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 020 - Provide an actual Windows native authoring route](#nat-h-windows-route)

**Delivered outcome:** One running app receives redirected external activation.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Use separate sample processes to establish activation/redirection and callback delivery.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 122, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:048:maui.route-single-instance-activations`.

<a id="nat-m22"></a>

### 021 - Use injectable permissions in a location and notification workflow

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Permission abstraction, native grants and coordinate-reference semantics. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route); [Verified authoring baselines / 022 - Provide a matched Mac and Xcode authoring route](#nat-h-apple-route)

**Delivered outcome:** One capability workflow uses supported permission and location contracts.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Cover IPermissions ownership, iOS notification authorization, distance filtering and Android altitude metadata without claiming notification scheduling.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 122, 123, 124, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:049:maui.request-notification-permission`, `inventory03:050:maui.inject-permission-checks`, `inventory03:051:maui.filter-location-updates`.

<a id="nat-m23"></a>

### 022 - Author themed cross-platform app assets

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Asset generation requires actual native build outputs for different pipelines. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route); [Verified authoring baselines / 022 - Provide a matched Mac and Xcode authoring route](#nat-h-apple-route)

**Delivered outcome:** Generated assets match platform, theme and intended sampling.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Build Android monochrome icons, dark splashes, resize quality and platform-conditioned Apple .icon bundles, one item per target.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 122, 123, 124, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:052:maui.author-themed-app-assets`, `inventory03:053:maui.author-themed-app-assets`, `inventory03:077:maui.author-themed-app-assets`.

<a id="nat-m24"></a>

### 023 - Use experimental MAUI Apple Intelligence abstractions

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Experimental package/assets plus hardware/model availability uncertainty. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 022 - Provide a matched Mac and Xcode authoring route](#nat-h-apple-route)

**Delivered outcome:** Shipped MAUI abstractions are usable with honest experimental prerequisites.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Verify shipped APIs/pins and MAUIAI0001 labels; separate compile proof from human hardware/model proof and native API catalogue scope.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 122, 124, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:055:maui.use-apple-intelligence-abstractions`.

<a id="nat-a01"></a>

### 024 - Migrate Android target runtime and toolchain configuration

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Interdependent manifest, platform, runtime and build settings. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route)

**Delivered outcome:** An upgraded Android project builds with intentional runtime/platform choices.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Compile API24/architecture/runtime/JDK choices and intentional targetSdk, including authored trimming/R2R compatibility.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 103, 104, 123, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:058:android.migrate-runtime-and-platform-targets`, `inventory03:064:android.declare-target-sdk-intentionally`, `inventory03:065:android.configure-build-toolchain`, `inventory03:066:android.migrate-runtime-and-platform-targets`.

<a id="nat-a02"></a>

### 025 - Migrate Java peer bindings and native loading

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Binding generation, JNI ordering and obsolete lifecycle APIs share one native library sample. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route)

**Delivered outcome:** An owned binding library uses current typemap and loading contracts.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Establish typemap/package-name/Export/JavaFinalize changes and explicit preload exemptions from real binding artifacts.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 101, 123, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:059:android.control-native-library-preloading`, `inventory03:060:android.migrate-java-peer-binding`.

<a id="nat-a03"></a>

### 026 - Handle Android HTTP faults and cancellation correctly

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Controlled endpoint and real handler evidence needed. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route)

**Delivered outcome:** Android HTTP error handling matches actual handler contracts.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Prove transport/body cancellation and fault shape while retaining inner diagnostics; do not create recipes for automatic fixes.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 100, 123, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:061:android.handle-http-failures-and-cancellation`.

<a id="nat-a04"></a>

### 027 - Preserve R8-sensitive Java names during migration

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Build optimization interacts with runtime reflection/name lookup. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route)

**Delivered outcome:** Optimized packages retain deliberately accessed Java members.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Inspect optimized output and demonstrate precise keep/compatibility choices; exclude unshipped managed rewriting.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 123, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:063:android.preserve-r8-sensitive-members`.

<a id="nat-a05"></a>

### 028 - Declare predictive-back behavior intentionally

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Independent target/platform navigation contract. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route)

**Delivered outcome:** An Android app selects supported back-navigation behavior.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Compile the attribute setting and establish the documented API/target constraints using an original navigation case.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 123, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:062:android.configure-predictive-back`.

<a id="nat-x01"></a>

### 029 - Adopt the required Apple scene lifecycle

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Mandatory startup migration and native app-action callbacks. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 022 - Provide a matched Mac and Xcode authoring route](#nat-h-apple-route)

**Delivered outcome:** Existing Apple/MAUI apps start through supported scene configuration.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Build base/MAUI variants as needed, move launch/actions to scene callbacks, acknowledge completion and preserve single-window defaults.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 122, 124, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:069:apple.adopt-scene-lifecycle`.

<a id="nat-x02"></a>

### 030 - Publish Apple apps with intentional deployment and runtime choices

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** MSBuild evaluation, architecture defaults, registrars and dynamic code interact. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 022 - Provide a matched Mac and Xcode authoring route](#nat-h-apple-route)

**Delivered outcome:** Published architecture/runtime outputs match explicit early build inputs.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Inspect early target/RID selection, universal/host architecture and relevant runtime/registrar/dynamic-code publish outputs.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 103, 104, 124, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:070:apple.pin-deployment-and-architecture`, `inventory03:075:apple.migrate-runtime-dependent-publishing`.

<a id="nat-x03"></a>

### 031 - Configure Apple native HTTP proxy and authentication routes

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Session-scoped proxy selection and HTTPS credential behavior need real handlers. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 022 - Provide a matched Mac and Xcode authoring route](#nat-h-apple-route)

**Delivered outcome:** An Apple HTTP session uses deliberate proxy/auth contracts.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Prove first-destination proxy selection and CONNECT credential limits with controlled endpoints.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 100, 124, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:071:apple.configure-native-http-proxies`.

<a id="nat-x04"></a>

### 032 - Author Apple manifest and framework package metadata

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Closely related packaging outputs, not a broad Apple feature set. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 022 - Provide a matched Mac and Xcode authoring route](#nat-h-apple-route)

**Delivered outcome:** Native app artifacts contain intentional plist/framework metadata.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Inspect typed plist add/override/remove and deliberate framework metadata-preservation output.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 104, 124, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:072:apple.author-plist-from-msbuild`, `inventory03:074:apple.preserve-required-framework-metadata`.

<a id="nat-x05"></a>

### 033 - Bind nullable Objective-C initializers accurately

**New · Agent · M (3) · Labels: Agent, Feature**

**Estimate rationale:** Minimal owned binding requires generated/native compile proof. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 022 - Provide a matched Mac and Xcode authoring route](#nat-h-apple-route)

**Delivered outcome:** Generated C# bindings represent failable native creation correctly.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Compile nullable factory output and consumer handling with exact FactoryMethod/NullAllowed contracts.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 101, 124, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:073:apple.bind-failable-initializers`.

<a id="nat-n01"></a>

### 034 - Capture native .NET diagnostics through supported configuration

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Two workload diagnostic paths; use one deliberate scenario each. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route); [Verified authoring baselines / 022 - Provide a matched Mac and Xcode authoring route](#nat-h-apple-route)

**Delivered outcome:** The recipe distinguishes crash-writer configuration from symbol/archive output.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Cover authored crash-writer and intentional symbol choices without claiming default symbol inclusion proves collection.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 103, 104, 123, 124, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:067:mobile.capture-native-dotnet-diagnostics`, `inventory03:076:mobile.capture-native-dotnet-diagnostics`.

<a id="nat-n02"></a>

### 035 - Run platform tests inside an app process

**New · Agent · L (5) · Labels: Agent, Feature**

**Estimate rationale:** Template generation and in-app execution differ across workload families. Size includes source/API reconciliation, original sample authoring, older/newer compilation, metadata/prose and packed retrieval, not only code edits.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice); [Verified authoring baselines / 021 - Provide an actual Android native authoring route](#nat-h-android-route); [Verified authoring baselines / 022 - Provide a matched Mac and Xcode authoring route](#nat-h-apple-route)

**Delivered outcome:** A developer can use actual native MTP template/invocation contracts.

**Acceptance criteria:**

- [ ] Author wholly original goal-based prose and canonical previous/current-release samples: normally .NET 11 versus .NET 10, and exactly the eight .NET 10 foundations versus .NET 9. Explain a genuinely unavailable older equivalent honestly; do not copy guide examples or disparage valid older approaches.
- [ ] Include complete offline metadata and Markdown, canonical-source snippets, official references and explicit runtime/SDK/language/package-asset/provider/platform/rendering/stability requirements. Label experimental APIs in discovery and detail; documented older-target routes require actual compatible evidence.
- [ ] Compile actual declared older/newer examples and use focused real behavior checks where the claim needs them. Record exact artifacts and distinguish source review, compilation, cross-compilation, mocks and execution; refresh proof after relevant input changes.
- [ ] Use the existing actual packed-tool harness with an isolated feed/cache and unrelated working directory to retrieve the new guidance through real stdio list/get. Check full body, applicability and target edition offline; reuse the harness rather than adding a new OS/client/native test matrix.
- [ ] Update the source-disposition register and preserve goal/edition identity. A missing API, compilation route or required Human proof remains explicit incomplete evidence and blocks publication. No package/account/resource publication or provisioning is authorized by this ticket alone.
- [ ] Verify minimal generated native test apps and supported commands without adding these tests to recurring Playbook CI.
- [ ] Verify the actual packed recipe through list/get; record exact execution limits and identify linked pending Human proof where settings, service, trust or hardware require a separate action.
- [ ] Any separately ticketed Human runtime proof is explicitly pending in this PR, never reported as executed. That later action records the actual result and updates the evidence before the comprehensive publication gate.

**Context:** Native compilation is authoring evidence, not recurring native CI. Agent completion requires a real agent-executable route supplied by the stated access blocker. Separately ticketed owner-only behavior remains explicit pending evidence until its Human proof is recorded; it blocks publication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 104, 123, 124, 126, 127, 130, 131, 132, 133, 134, 138, 139, 158, 159, 160. Supporting inventory references: `inventory03:084:mobile.verify-test-template-contracts`, `sdk.run-mobile-tests`.

<a id="nat-h-map-proof"></a>

### 036 - Verify the original Windows map against the real service

**New · Human · M (3) · Labels: Testing, Platform**

**Estimate rationale:** Credentialed native service interaction with evidence handoff.

**Blockers:** [Mobile and native guidance / 016 - Enable the supported Windows map surface](#nat-m17); [Verified authoring baselines / 024 - Provide separate authorized map verification resources](#nat-h-map-resource)

**Delivered outcome:** The Windows map recipe has focused real service evidence.

**Acceptance criteria:**

- [ ] Use M17's compiled original sample and authorized resource to check its stated supported map/pin claim once.
- [ ] Record Windows/toolchain/service/sample versions and real outcomes; distinguish untested unsupported-feature caveats.
- [ ] Attach evidence to the content record and identify any correction needed before publication; no recurring UI suite.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 122, 133, 134, 151, 160. Supporting inventory references: `maui.enable-windows-maps`.

<a id="nat-h-passkey-proof"></a>

### 037 - Perform the native passkey ceremony and server verification

**New · Human · L (5) · Labels: Testing, Platform**

**Estimate rationale:** Physical ceremony and relying-party security validation cannot be assigned implicitly to an agent.

**Blockers:** [Mobile and native guidance / 017 - Authenticate through native passkey ceremonies](#nat-m18); [Verified authoring baselines / 025 - Provide passkey trust signing and device prerequisites](#nat-h-passkey-resource)

**Delivered outcome:** The passkey recipe has real ceremony evidence and correct trust boundaries.

**Acceptance criteria:**

- [ ] Run the original compiled registration/authentication flow using H_PASSKEY_RESOURCE and acknowledge actual physical/biometric approval.
- [ ] Record server challenge/response verification and native prerequisites without storing secrets or private credential material.
- [ ] Record platform coverage and failures honestly; do not claim IsSupported proves a completed ceremony.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 109, 122, 133, 134, 151, 160. Supporting inventory references: `maui.authenticate-with-platform-passkeys`.

<a id="nat-h-apple-ai-proof"></a>

### 038 - Exercise the experimental MAUI Apple AI sample

**New · Human · M (3) · Labels: Testing, Platform**

**Estimate rationale:** Owner/hardware-controlled execution with a narrow experimental-API claim.

**Blockers:** [Mobile and native guidance / 023 - Use experimental MAUI Apple Intelligence abstractions](#nat-m24); [Verified authoring baselines / 026 - Provide eligible Apple AI hardware model and access](#nat-h-apple-ai-resource)

**Delivered outcome:** Experimental MAUI AI behavior is evidenced on the actual available device.

**Acceptance criteria:**

- [ ] Run M24's original compiled sample on the confirmed eligible route and capture the claimed response/embedding behavior.
- [ ] Retain MAUIAI0001/experimental and model/device limits in evidence and served metadata.
- [ ] Report unsupported/unavailable conditions precisely; do not add native API catalogue scope.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 122, 124, 133, 134, 151, 160. Supporting inventory references: `maui.use-apple-intelligence-abstractions`.

## Clients and release readiness

**New native milestone.** Four client checks, bounded evaluation, standard portfolio workflows and comprehensive pre-publication evidence are reviewable.

<a id="release-meta"></a>

### 000 - Apply portfolio PR labels and release drafting conventions

**New · Agent · M (3) · Labels: Agent, Deployment**

**Estimate rationale:** Includes implementation, focused verification and evidence for this complete outcome.

**Blockers:** [Installed documentation server / 000 - Install and retrieve an original property recipe over stdio MCP](#core-slice)

**Delivered outcome:** Merged changes are categorized and produce the standard independently versioned package draft.

**Acceptance criteria:**

- [ ] Adapt PR template/categories, automatic labels, release-drafter workflow/config and repository metadata to PinguApps/Playbook, retaining existing six-role conventions and pinned actions.
- [ ] Demonstrate category-to-draft behavior and SemVer rules: 1.0.0 first stable; manual 1.0.0-preview.N; patch corrections/minor compatible additions/major breaking public contracts or stable IDs.
- [ ] Retain MIT/license/source attribution, package README and appropriate dependency maintenance settings; do not rename the repository or publish a release.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 140, 141, 144, 145, 150, 159.

<a id="publish-workflow"></a>

### 001 - Publish the verified package through the standard OIDC workflow

**New · Agent · L (5) · Labels: Agent, Deployment**

**Estimate rationale:** Artifact provenance, real workflow composition and OIDC/channel behavior require more than copying YAML.

**Blockers:** [Installed documentation server / 005 - Validate the same packed tool on supported host environments](#core-ci); [Clients and release readiness / 000 - Apply portfolio PR labels and release drafting conventions](#release-meta)

**Delivered outcome:** A release workflow can publish the exact package validated across required lanes using the approved identity.

**Acceptance criteria:**

- [ ] Adapt package-publishing role and Release Drafter version mechanism from the references; stable and manual preview versions must be valid and independently versioned.
- [ ] Carry the exact packed artifact through required checks into publishing with identity/hash evidence and no repack. Use NuGet OIDC trusted publishing and least required workflow permissions.
- [ ] Validate workflow/artifact selection and channel guards without publishing or configuring accounts; provide exact repository/workflow/environment identities for the Human trusted-publisher action.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 141, 142, 143, 144, 145, 146, 151.

<a id="weekly-workflow"></a>

### 002 - Gate the standard weekly stable draft publication flow

**New · Agent · M (3) · Labels: Agent, Deployment**

**Estimate rationale:** Includes implementation, focused verification and evidence for this complete outcome.

**Blockers:** [Clients and release readiness / 001 - Publish the verified package through the standard OIDC workflow](#publish-workflow)

**Delivered outcome:** Weekly automation remains closed until a real stable release exists and then follows portfolio behavior.

**Acceptance criteria:**

- [ ] Adapt the sixth workflow role for Sunday 18:17UTC (17 18 * * 0), kept inactive until the explicit activation action.
- [ ] Require an existing published stable release and suitable stable draft using version syntax plus GitHub prerelease flags; a preview must never unlock weekly stable publication.
- [ ] Preserve no-new-main-commits/no-draft/stable-tag safeguards and any-new-main-commit behavior, including docs/CI/tests/maintenance. Add no shipped-path, package-diff or empty-note guard.
- [ ] Verify meaningful stable/preview/draft cases without publishing or enabling the schedule; reuse the validated publication artifact path.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 141, 144, 147, 148, 149, 151.

<a id="h-oidc"></a>

### 003 - Configure the approved NuGet trusted publisher identity

**New · Human · S (2) · Labels: Security**

**Estimate rationale:** Focused owner-controlled account configuration after the concrete workflow identity is reviewable.

**Blockers:** [Clients and release readiness / 001 - Publish the verified package through the standard OIDC workflow](#publish-workflow)

**Delivered outcome:** The owner has connected NuGet trusted publishing to the reviewed release workflow identity.

**Acceptance criteria:**

- [ ] Use the exact PinguApps/Playbook workflow/environment identity from PUBLISH_WORKFLOW, checking package ownership and NuGet trusted-publisher configuration.
- [ ] Record configuration evidence without tokens/secrets in source or tickets. Do not publish or activate schedules as part of this action.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 142, 151, 160.

<a id="h-client-t3"></a>

### 004 - Check the packaged tool in T3 Code

**New · Human · S (2) · Labels: Testing, Platform**

**Estimate rationale:** One focused authenticated/interactive client check, not a broad test matrix.

**Blockers:** [Installed documentation server / 007 - Document offline installation and four MCP client connections](#client-docs)

**Delivered outcome:** An actual T3 Code agent receives both recipe guidance and its applicability.

**Acceptance criteria:**

- [ ] On one available supported OS, use the documented configuration with the actual packed artifact; record actual client/OS/tool versions.
- [ ] Launch the stdio server and make one brief real discovery/detail check. Confirm Markdown body plus applicability reach the agent and shutdown works; identify needed documentation corrections.
- [ ] Use owner-held client/account access. Do not expand to a client-by-OS matrix or treat source inspection as execution.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 135, 136, 137, 139, 151, 160.

<a id="h-client-codex"></a>

### 005 - Check the packaged tool in Codex CLI

**New · Human · S (2) · Labels: Testing, Platform**

**Estimate rationale:** One focused authenticated/interactive client check, not a broad test matrix.

**Blockers:** [Installed documentation server / 007 - Document offline installation and four MCP client connections](#client-docs)

**Delivered outcome:** An actual Codex CLI agent receives both recipe guidance and its applicability.

**Acceptance criteria:**

- [ ] On one available supported OS, use the documented configuration with the actual packed artifact; record actual client/OS/tool versions.
- [ ] Launch the stdio server and make one brief real discovery/detail check. Confirm Markdown body plus applicability reach the agent and shutdown works; identify needed documentation corrections.
- [ ] Use owner-held client/account access. Do not expand to a client-by-OS matrix or treat source inspection as execution.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 135, 136, 137, 139, 151, 160.

<a id="h-client-claude"></a>

### 006 - Check the packaged tool in Claude Code

**New · Human · S (2) · Labels: Testing, Platform**

**Estimate rationale:** One focused authenticated/interactive client check, not a broad test matrix.

**Blockers:** [Installed documentation server / 007 - Document offline installation and four MCP client connections](#client-docs)

**Delivered outcome:** An actual Claude Code agent receives both recipe guidance and its applicability.

**Acceptance criteria:**

- [ ] On one available supported OS, use the documented configuration with the actual packed artifact; record actual client/OS/tool versions.
- [ ] Launch the stdio server and make one brief real discovery/detail check. Confirm Markdown body plus applicability reach the agent and shutdown works; identify needed documentation corrections.
- [ ] Use owner-held client/account access. Do not expand to a client-by-OS matrix or treat source inspection as execution.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 135, 136, 137, 139, 151, 160.

<a id="h-client-copilot"></a>

### 007 - Check the packaged tool in VS Code Copilot

**New · Human · S (2) · Labels: Testing, Platform**

**Estimate rationale:** One focused authenticated/interactive client check, not a broad test matrix.

**Blockers:** [Installed documentation server / 007 - Document offline installation and four MCP client connections](#client-docs)

**Delivered outcome:** An actual VS Code Copilot agent receives both recipe guidance and its applicability.

**Acceptance criteria:**

- [ ] On one available supported OS, use the documented configuration with the actual packed artifact; record actual client/OS/tool versions.
- [ ] Launch the stdio server and make one brief real discovery/detail check. Confirm Markdown body plus applicability reach the agent and shutdown works; identify needed documentation corrections.
- [ ] Use owner-held client/account access. Do not expand to a client-by-OS matrix or treat source inspection as execution.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 135, 136, 137, 139, 151, 160.

<a id="eval-csharp"></a>

### 008 - Prepare the original C# outcome modelling comparison task

**New · Agent · M (3) · Labels: Agent, Testing**

**Estimate rationale:** Original scaffold, actual artifact applicability and a useful frozen behavior oracle for one bounded task.

**Blockers:** [Language and library guidance / 000 - Model and persist closed C# outcomes](#sh-l01)

**Delivered outcome:** The C# outcome modelling launch task has a small original reproducible scaffold and behavior oracle.

**Acceptance criteria:**

- [ ] Frozen behavior checks distinguish successful and failed closed outcomes; matching, persistence and actual compiler applicability are explicit.
- [ ] Pin the scaffold commit, SDK/package inputs and equal tools/network/ten-minute budgets for both conditions. Prepare expected behaviors privately from the runs; do not require a newer API for an otherwise correct older solution.
- [ ] Record the Playbook recipe applicability expected for this task; preparation is separate from the later eight fresh contexts and does not manufacture evaluation results.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 152, 156, 157, 160.

<a id="eval-api"></a>

### 009 - Prepare the original asynchronous reservation validation comparison task

**New · Agent · M (3) · Labels: Agent, Testing**

**Estimate rationale:** Original scaffold, actual artifact applicability and a useful frozen behavior oracle for one bounded task.

**Blockers:** [Web application guidance / 002 - Awaited validation for forms and endpoints](#web-w03)

**Delivered outcome:** The asynchronous reservation validation launch task has a small original reproducible scaffold and behavior oracle.

**Acceptance criteria:**

- [ ] Frozen behavior checks reject invalid reservations before handler work, including awaited completion/cancellation and the actual supported validation API.
- [ ] Pin the scaffold commit, SDK/package inputs and equal tools/network/ten-minute budgets for both conditions. Prepare expected behaviors privately from the runs; do not require a newer API for an otherwise correct older solution.
- [ ] Record the Playbook recipe applicability expected for this task; preparation is separate from the later eight fresh contexts and does not manufacture evaluation results.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 153, 156, 157, 160.

<a id="eval-grid"></a>

### 010 - Prepare the original independent static Blazor grid links comparison task

**New · Agent · M (3) · Labels: Agent, Testing**

**Estimate rationale:** Original scaffold, actual artifact applicability and a useful frozen behavior oracle for one bounded task.

**Blockers:** [Web application guidance / 009 - Author URL-restored QuickGrid state](#web-w09a)

**Delivered outcome:** The independent static Blazor grid links launch task has a small original reproducible scaffold and behavior oracle.

**Acceptance criteria:**

- [ ] Two grids retain independent sorting/paging links and preserve a filter through refresh/navigation using actual supported static-rendered APIs.
- [ ] Pin the scaffold commit, SDK/package inputs and equal tools/network/ten-minute budgets for both conditions. Prepare expected behaviors privately from the runs; do not require a newer API for an otherwise correct older solution.
- [ ] Record the Playbook recipe applicability expected for this task; preparation is separate from the later eight fresh contexts and does not manufacture evaluation results.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 154, 156, 157, 160.

<a id="eval-manifest"></a>

### 011 - Prepare the original non-web manifest reconciliation comparison task

**New · Agent · M (3) · Labels: Agent, Testing**

**Estimate rationale:** Original scaffold, actual artifact applicability and a useful frozen behavior oracle for one bounded task.

**Blockers:** [Language and library guidance / 023 - Preserve unmatched records during reconciliation](#sh-n01)

**Delivered outcome:** The non-web manifest reconciliation launch task has a small original reproducible scaffold and behavior oracle.

**Acceptance criteria:**

- [ ] Frozen behavior checks preserve unmatched entries on both sides and define duplicate/key/output semantics against the actual available join API.
- [ ] Pin the scaffold commit, SDK/package inputs and equal tools/network/ten-minute budgets for both conditions. Prepare expected behaviors privately from the runs; do not require a newer API for an otherwise correct older solution.
- [ ] Record the Playbook recipe applicability expected for this task; preparation is separate from the later eight fresh contexts and does not manufacture evaluation results.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 155, 156, 157, 160.

<a id="h-eval-protocol"></a>

### 012 - Freeze the single-client comparison protocol

**New · Human · S (2) · Labels: Testing, Platform**

**Estimate rationale:** Focused owner choice and protocol freeze; actual runs are separate tasks.

**Blockers:** None — independent.

**Delivered outcome:** The owner chooses and freezes the one client/model/version and controlled eight-run procedure.

**Acceptance criteria:**

- [ ] Choose one available client/model/version; freeze equal scaffold commits, SDK/packages, tools, network policy and budgets, with only Playbook plus its companion instruction differing.
- [ ] Plan one with/without pair per task, alternating condition order, eight fresh isolated contexts, maximum ten execution minutes each and 80 total excluding preparation/review; retain failures/timeouts with no corrective retries or model matrix.
- [ ] Define evidence capture for consultation, behavior correctness, applicability, newer-API adoption, unnecessary code, elapsed time and token use where available.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 151, 152, 153, 154, 155, 156, 157, 160.

<a id="h-run-csharp"></a>

### 013 - Run the with/without C# outcome modelling pair

**New · Human · M (3) · Labels: Testing, Platform**

**Estimate rationale:** One focused two-run manual action, up to20 execution minutes plus capture/review handoff.

**Blockers:** [Clients and release readiness / 008 - Prepare the original C# outcome modelling comparison task](#eval-csharp); [Clients and release readiness / 012 - Freeze the single-client comparison protocol](#h-eval-protocol)

**Delivered outcome:** Two fresh controlled runs provide launch evidence for C# outcome modelling.

**Acceptance criteria:**

- [ ] Use the frozen protocol and the exact prepared scaffold, with one condition each and the planned alternating order. Maximum ten execution minutes each; keep both contexts isolated.
- [ ] Retain outputs, failures/timeouts, tool consultation, timing and available token evidence without coaching or corrective retries. The total four pairs stay within 80 execution minutes.
- [ ] Make actual results available for owner review; no broad model-knowledge or performance claim follows from this pair.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 152, 156, 157, 160.

<a id="h-run-api"></a>

### 014 - Run the with/without asynchronous reservation validation pair

**New · Human · M (3) · Labels: Testing, Platform**

**Estimate rationale:** One focused two-run manual action, up to20 execution minutes plus capture/review handoff.

**Blockers:** [Clients and release readiness / 009 - Prepare the original asynchronous reservation validation comparison task](#eval-api); [Clients and release readiness / 012 - Freeze the single-client comparison protocol](#h-eval-protocol)

**Delivered outcome:** Two fresh controlled runs provide launch evidence for asynchronous reservation validation.

**Acceptance criteria:**

- [ ] Use the frozen protocol and the exact prepared scaffold, with one condition each and the planned alternating order. Maximum ten execution minutes each; keep both contexts isolated.
- [ ] Retain outputs, failures/timeouts, tool consultation, timing and available token evidence without coaching or corrective retries. The total four pairs stay within 80 execution minutes.
- [ ] Make actual results available for owner review; no broad model-knowledge or performance claim follows from this pair.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 153, 156, 157, 160.

<a id="h-run-grid"></a>

### 015 - Run the with/without independent static Blazor grid links pair

**New · Human · M (3) · Labels: Testing, Platform**

**Estimate rationale:** One focused two-run manual action, up to20 execution minutes plus capture/review handoff.

**Blockers:** [Clients and release readiness / 010 - Prepare the original independent static Blazor grid links comparison task](#eval-grid); [Clients and release readiness / 012 - Freeze the single-client comparison protocol](#h-eval-protocol)

**Delivered outcome:** Two fresh controlled runs provide launch evidence for independent static Blazor grid links.

**Acceptance criteria:**

- [ ] Use the frozen protocol and the exact prepared scaffold, with one condition each and the planned alternating order. Maximum ten execution minutes each; keep both contexts isolated.
- [ ] Retain outputs, failures/timeouts, tool consultation, timing and available token evidence without coaching or corrective retries. The total four pairs stay within 80 execution minutes.
- [ ] Make actual results available for owner review; no broad model-knowledge or performance claim follows from this pair.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 154, 156, 157, 160.

<a id="h-run-manifest"></a>

### 016 - Run the with/without non-web manifest reconciliation pair

**New · Human · M (3) · Labels: Testing, Platform**

**Estimate rationale:** One focused two-run manual action, up to20 execution minutes plus capture/review handoff.

**Blockers:** [Clients and release readiness / 011 - Prepare the original non-web manifest reconciliation comparison task](#eval-manifest); [Clients and release readiness / 012 - Freeze the single-client comparison protocol](#h-eval-protocol)

**Delivered outcome:** Two fresh controlled runs provide launch evidence for non-web manifest reconciliation.

**Acceptance criteria:**

- [ ] Use the frozen protocol and the exact prepared scaffold, with one condition each and the planned alternating order. Maximum ten execution minutes each; keep both contexts isolated.
- [ ] Retain outputs, failures/timeouts, tool consultation, timing and available token evidence without coaching or corrective retries. The total four pairs stay within 80 execution minutes.
- [ ] Make actual results available for owner review; no broad model-knowledge or performance claim follows from this pair.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 155, 156, 157, 160.

<a id="h-eval-review"></a>

### 017 - Review all four comparison results and launch implications

**New · Human · M (3) · Labels: Testing, Platform**

**Estimate rationale:** Focused owner judgement over the bounded eight-run evidence set.

**Blockers:** [Clients and release readiness / 013 - Run the with/without C# outcome modelling pair](#h-run-csharp); [Clients and release readiness / 014 - Run the with/without asynchronous reservation validation pair](#h-run-api); [Clients and release readiness / 015 - Run the with/without independent static Blazor grid links pair](#h-run-grid); [Clients and release readiness / 016 - Run the with/without non-web manifest reconciliation pair](#h-run-manifest)

**Delivered outcome:** The owner assesses actual consultation, correctness and applicability without equating tool use with success.

**Acceptance criteria:**

- [ ] Assess eight retained runs against the frozen behavior oracles; separate consultation, correctness, applicability, newer APIs, extra code and time/tokens where recorded.
- [ ] Accept behaviorally correct older approaches; do not require four Playbook wins or infer broad agent knowledge. Record limits, failures and any concrete product defects.
- [ ] Propose narrow corrective work for actual defects and keep them visible as release blockers until fixed; do not silently reinterpret failures or claim unrun checks.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 152, 153, 154, 155, 156, 157, 158, 160.

<a id="h-wsl-proof"></a>

### 018 - Perform the controlled Windows and WSL certificate-trust check

**New · Human · M (3) · Labels: Testing, Platform**

**Estimate rationale:** Owner-controlled trust mutation and real cross-environment check; the Agent recipe PR provides the prepared example.

**Blockers:** [Web application guidance / 046 - Use one trust operation for Windows and WSL](#web-w29b)

**Delivered outcome:** The authored development-certificate migration has real trust evidence in both supplied environments.

**Acceptance criteria:**

- [ ] Use the original compiled/documented sample with the supplied Windows/WSL development route; perform the owner-controlled trust operation and check actual HTTPS trust from both sides.
- [ ] Record exact command/version/result and any cleanup or approval required; attach the evidence to the recipe and correct overstatements before publication. Do not treat mere certificate generation as trusted connectivity.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 112, 133, 134, 151, 160.

<a id="h-pmc-proof"></a>

### 019 - Verify the no-build command in actual Visual Studio PMC

**New · Human · S (2) · Labels: Testing, Platform**

**Estimate rationale:** One focused actual interactive PMC action using a prepared compiled sample.

**Blockers:** [Data access guidance / 041 - Run a design-time operation using an already-current build](#web-w55b)

**Delivered outcome:** The no-build migration-command recipe has actual interactive design-time evidence.

**Acceptance criteria:**

- [ ] Run the original built sample through the supported NoBuild command in the supplied real Visual Studio Package Manager Console; verify generated/query output and the declared stale-artifact limitation.
- [ ] Record actual Visual Studio/EF/build versions and update the linked recipe evidence. Compilation or CLI emulation does not establish the PMC-specific claim.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 118, 133, 134, 151, 160.

<a id="h-entra-proof"></a>

### 020 - Verify real Entra authentication for the SQL sample

**New · Human · M (3) · Labels: Testing, Security**

**Estimate rationale:** A focused credentialed real-authentication check using a prepared sample.

**Blockers:** [Data access guidance / 000 - Restore database authentication after driver dependency changes](#web-w37a)

**Delivered outcome:** The SQL authentication migration has actual authorized identity evidence.

**Acceptance criteria:**

- [ ] Use the prepared original compiled sample, authorized test identity and isolated SQL route; exercise real supported authentication, including owner-controlled login where required.
- [ ] Record actual driver/extension/provider/identity route and the result, then update linked recipe evidence without storing credentials. A mocked token or successful dependency restore does not prove authentication.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 109, 116, 133, 134, 151, 160.

<a id="coverage-close"></a>

### 021 - Verify comprehensive content and compilation readiness

**New · Agent · L (5) · Labels: Agent, Testing**

**Estimate rationale:** Large but bounded evidence/coverage closure across delivered slices; missing implementation is separate follow-on work, not bundled into this audit.

**Blockers:** [Language and library guidance / 000 - Model and persist closed C# outcomes](#sh-l01); [Language and library guidance / 001 - Extend and construct collection-like types](#sh-l02); [Language and library guidance / 002 - Control an enclosing loop directly](#sh-l03); [Language and library guidance / 003 - Put implementation helpers on interfaces](#sh-l04); [Language and library guidance / 004 - Express and check unsafe boundaries](#sh-l05); [Language and library guidance / 005 - Construct and serialize F# data models](#sh-f01); [Language and library guidance / 006 - Author clearer F#/.NET API boundaries](#sh-f02); [Language and library guidance / 007 - Format F# diagnostics deliberately](#sh-f03); [Language and library guidance / 008 - Start, bound and fail F# asynchronous work](#sh-f04); [Language and library guidance / 009 - Migrate unsupported F# enum operations](#sh-f05); [Language and library guidance / 010 - Capture and manage a command-line child process](#sh-b01); [Language and library guidance / 011 - Control native child-process startup and termination](#sh-b02); [Language and library guidance / 012 - Adapt existing buffers without redundant copying](#sh-b03); [Language and library guidance / 013 - Process Unicode characters and line boundaries](#sh-b04); [Language and library guidance / 014 - Encode and consume incremental binary data](#sh-b05); [Language and library guidance / 015 - Stream JSON records and reuse a writer safely](#sh-j01); [Language and library guidance / 016 - Resolve generic JSON contracts and typed metadata](#sh-j02); [Language and library guidance / 017 - Generate contracts for encapsulated models](#sh-j03); [Language and library guidance / 018 - Define JSON conventions and expanded value contracts](#sh-j04); [Language and library guidance / 019 - Reject malformed import contracts](#sh-j05); [Language and library guidance / 020 - Control ZIP entry access and protection](#sh-a01); [Language and library guidance / 021 - Detect archive corruption and choose TAR representation](#sh-a02); [Language and library guidance / 022 - Compress buffers with the intended format and lifecycle](#sh-a03); [Language and library guidance / 023 - Preserve unmatched records during reconciliation](#sh-n01); [Language and library guidance / 024 - Transform an asynchronous stream without buffering](#sh-n02); [Language and library guidance / 025 - Select decimal and complex numeric representations](#sh-n03); [Language and library guidance / 026 - Preserve numeric parsing and conversion intent](#sh-n04); [Language and library guidance / 027 - Generate random values through generic numeric APIs](#sh-n05); [Language and library guidance / 028 - Arrange vector lanes and select supported instructions](#sh-n06); [Language and library guidance / 029 - Build bit collections and compare values by key](#sh-n07); [Language and library guidance / 030 - Use supported pipe handles and mobile file locking](#sh-i01); [Language and library guidance / 031 - Inspect assembly metadata across deployment modes](#sh-e01); [Language and library guidance / 032 - Annotate embedded code strings for tooling](#sh-e02); [Language and library guidance / 033 - Describe URI payloads and media content](#sh-e03); [Language and library guidance / 034 - Bind configuration selectively and await reload work](#sh-e04); [Language and library guidance / 035 - Validate options with dependencies and async startup](#sh-e05); [Language and library guidance / 036 - Observe caches and select entry options from values](#sh-e06); [Language and library guidance / 037 - Validate data asynchronously with localized rules](#sh-e07); [Language and library guidance / 038 - Configure tracing, efficient logging and HTTP observations](#sh-e08); [Language and library guidance / 039 - Exchange, compare and wrap key material](#sh-k01); [Language and library guidance / 040 - Use supported post-quantum hybrid and CMS contracts](#sh-k02); [Language and library guidance / 041 - Drive TLS handshakes through caller-owned I/O](#sh-k03); [Language and library guidance / 042 - Configure HTTP request compression and connection capacity](#sh-t01); [Language and library guidance / 043 - Resolve DNS service records](#sh-t02); [Language and library guidance / 044 - Prioritize QUIC streams](#sh-t03); [Language and library guidance / 045 - Configure crash evidence and hardware requirements](#sh-r01); [Language and library guidance / 046 - Choose runtime-async code generation deliberately](#sh-r02); [Language and library guidance / 047 - Link native code into CoreCLR WebAssembly](#sh-r03); [Build and deployment guidance / 000 - Grow a file-based repository utility](#sh-s01); [Build and deployment guidance / 001 - Target solution, formatting and launch operations precisely](#sh-s02); [Build and deployment guidance / 002 - Configure inner-loop app hosts and preview scaffolding](#sh-s03); [Build and deployment guidance / 003 - Select and create test-runner projects](#sh-s04); [Build and deployment guidance / 004 - Package tools and consume single-file output correctly](#sh-s06); [Build and deployment guidance / 005 - Declare static asset groups including TypeScript assets](#sh-s07); [Build and deployment guidance / 006 - Publish reproducible multi-architecture containers](#sh-s08); [Build and deployment guidance / 007 - Configure CLI telemetry explicitly](#sh-s09); [Build and deployment guidance / 008 - Produce build archives and inspect item globs](#sh-m01); [Build and deployment guidance / 009 - Migrate custom MSBuild XML and feature switches](#sh-m02); [Build and deployment guidance / 010 - Select dependencies with cooldown and vulnerability updates](#sh-p01); [Build and deployment guidance / 011 - Produce deterministic package timestamps](#sh-p02); [Build and deployment guidance / 012 - Retarget a NuGet client-library host](#sh-p03); [Web application guidance / 000 - Immediate and complete form validation guidance](#web-w01); [Web application guidance / 001 - Accessible model-driven field captions](#web-w02); [Web application guidance / 002 - Awaited validation for forms and endpoints](#web-w03); [Web application guidance / 003 - Localized validation feedback in Blazor and endpoints](#web-w04); [Web application guidance / 004 - Author SSR redirect and session state workflows](#web-w05); [Web application guidance / 005 - Base-aware Blazor navigation](#web-w06); [Web application guidance / 006 - Place server-rendered content inside an interactive boundary](#web-w07a); [Web application guidance / 007 - Offer a single parameter for alternative UI values](#web-w07b); [Web application guidance / 008 - Safe static-subtree caching](#web-w08); [Web application guidance / 009 - Author URL-restored QuickGrid state](#web-w09a); [Web application guidance / 010 - Interactive QuickGrid row selection](#web-w09b); [Web application guidance / 011 - Virtualized QuickGrid row positioning](#web-w09c); [Web application guidance / 012 - Anchoring for changing variable-height lists](#web-w10a); [Web application guidance / 013 - Virtual-list item reveal](#web-w10b); [Web application guidance / 014 - Restrictive-CSP virtual lists](#web-w10c); [Web application guidance / 015 - Idle and maintenance circuit pause workflows](#web-w11); [Web application guidance / 016 - Server-configured Blazor browser startup](#web-w12a); [Web application guidance / 017 - Unified Blazor startup script options](#web-w12b); [Web application guidance / 018 - Deliberate Blazor circuit transport policy](#web-w12c); [Web application guidance / 019 - Run periodic browser work within application lifetime](#web-w13a); [Web application guidance / 020 - Move long-running browser work away from rendering](#web-w13b); [Web application guidance / 021 - Reuse one browser build across environments](#web-w14a); [Web application guidance / 022 - Keep formatting consistent through hydration](#web-w14b); [Web application guidance / 023 - Present environment-appropriate application chrome](#web-w14c); [Web application guidance / 024 - Serve a standalone client with configured backend routes](#web-w15a); [Web application guidance / 025 - Collect component activity from the browser](#web-w15b); [Web application guidance / 026 - Operate navigation without an inline script exception](#web-w16a); [Web application guidance / 027 - Keep a custom event distinct from its browser trigger](#web-w16b); [Web application guidance / 028 - Retain deliberate loading hints during enhanced navigation](#web-w16c); [Web application guidance / 029 - Supply component dependencies through a custom activation policy](#web-w17); [Web application guidance / 030 - Express a JavaScript call that has no result](#web-w18a); [Web application guidance / 031 - Display interactive mathematical notation without custom DOM creation](#web-w18c); [Web application guidance / 032 - Experimental AI conversation streaming](#web-w19); [Web application guidance / 033 - Experimental AI tool rendering](#web-w20); [Web application guidance / 034 - Experimental AI approval continuation](#web-w21); [Web application guidance / 035 - Experimental AI and UI state synchronization](#web-w22); [Web application guidance / 036 - Simplify form protection without opening cross-origin writes](#web-w23); [Web application guidance / 037 - Shared authorization requirement metadata](#web-w24); [Web application guidance / 038 - Verify expiry and lockout without waiting](#web-w25a); [Web application guidance / 039 - Give a saved authenticator an intelligible name](#web-w25b); [Web application guidance / 040 - Present a valid outcome when passkey sign-in is disallowed](#web-w25c); [Web application guidance / 041 - Live identity refresh for SignalR and Blazor](#web-w26); [Web application guidance / 042 - Stop an ordinary hub operation when its caller abandons it](#web-w27); [Web application guidance / 043 - Return a consistent failure before endpoint execution](#web-w28a); [Web application guidance / 044 - Bypass irrelevant middleware for a deliberately standalone response](#web-w28b); [Web application guidance / 045 - Call a secured file-based endpoint during development](#web-w29a); [Web application guidance / 047 - Start an agent-facing web tool from a bundled template](#web-w29c); [Web application guidance / 048 - Describe a body that has several valid shapes](#web-w30); [Web application guidance / 049 - Accurate OpenAPI download responses](#web-w31a); [Web application guidance / 050 - Author HTTP QUERY request contracts](#web-w31b); [Web application guidance / 051 - Multi-shape OpenAPI response contracts](#web-w31c); [Web application guidance / 052 - Typed SSE OpenAPI contracts](#web-w31d); [Web application guidance / 053 - Compatible OpenAPI document versions](#web-w32a); [Web application guidance / 054 - Environment-specific OpenAPI generation](#web-w32b); [Web application guidance / 055 - Contract deprecation from source metadata](#web-w32c); [Web application guidance / 056 - Supported OpenAPI transformer migrations](#web-w32d); [Web application guidance / 057 - Stable OpenAPI server URL composition](#web-w32e); [Web application guidance / 058 - Record handshake failures and inspect ClientHello](#web-w33a); [Web application guidance / 059 - Expose or require transport binding for authenticated requests](#web-w33b); [Web application guidance / 060 - Make authored clients and proxy adapters accepted by the server](#web-w33c); [Web application guidance / 061 - Collect framework traces without duplicate enrichment](#web-w34); [Web application guidance / 062 - Select output-cache behavior from current tenant policy](#web-w35a); [Web application guidance / 063 - Select compression and request decoding policy](#web-w35b); [Web application guidance / 064 - Retain concurrency limits after obsolete middleware removal](#web-w35c); [Web application guidance / 065 - Start MVC without removed compatibility APIs](#web-w36a); [Web application guidance / 066 - Keep a customized image helper constructing correctly](#web-w36b); [Web application guidance / 067 - Identify reflected properties through the supported overload](#web-w36c); [Web application guidance / 068 - Build views without newly obsolete registration calls](#web-w36d); [Data access guidance / 001 - Keep generated SQL compatible with the actual database](#web-w37b); [Data access guidance / 002 - Inherited complex-value relational mappings](#web-w38a); [Data access guidance / 003 - Chained nested-scalar EF configuration](#web-w38b); [Data access guidance / 004 - Complex-value keys and lookup indexes](#web-w38c); [Data access guidance / 005 - Map embedded sequences without a deprecated synthetic identity](#web-w39a); [Data access guidance / 006 - Make scalar-versus-collection mapping unambiguous](#web-w39b); [Data access guidance / 007 - Query related data from each group's selected item](#web-w40a); [Data access guidance / 008 - Preserve both sides of a database reconciliation](#web-w40b); [Data access guidance / 009 - Fetch the record associated with a minimum or maximum key](#web-w40c); [Data access guidance / 010 - Use EF's asynchronous query adapter intentionally](#web-w40d); [Data access guidance / 011 - Author SQL JSON path-existence queries](#web-w41a); [Data access guidance / 012 - Native SQL JSON containment queries](#web-w41b); [Data access guidance / 013 - Author SQL JSON path indexes](#web-w41c); [Data access guidance / 014 - Indexed vector search and explicit vector loading](#web-w42); [Data access guidance / 015 - Managed full-text indexing and ranked search](#web-w43); [Data access guidance / 016 - Combined lexical and vector ranking](#web-w44); [Data access guidance / 017 - Read row validity through ordinary entity properties](#web-w45a); [Data access guidance / 018 - Perform time conversion and bucketing on the server](#web-w45b); [Data access guidance / 019 - Return an ordered textual summary per group](#web-w46a); [Data access guidance / 020 - Bind a full-width unsigned integer without custom encoding](#web-w46b); [Data access guidance / 021 - Keep low-level SQLite access on a supported framework](#web-w46c); [Data access guidance / 022 - Restore native SQLite initialization after bundle retirement](#web-w46d); [Data access guidance / 023 - Treat nested document content as values rather than entities](#web-w47a); [Data access guidance / 024 - Provision only the document indexes the workload needs](#web-w47b); [Data access guidance / 025 - Make the scope of a multi-document save explicit](#web-w48a); [Data access guidance / 026 - Increase ingestion concurrency through supported configuration](#web-w48b); [Data access guidance / 027 - Preserve read-your-writes across application instances](#web-w48c); [Data access guidance / 028 - Author Cosmos raw-document and unmapped-field migration](#web-w49); [Data access guidance / 029 - Keep existing documents reachable after key escaping changes](#web-w50); [Data access guidance / 030 - Decide what an absent document path contributes to a result](#web-w51a); [Data access guidance / 031 - Handle an empty document collection through collection semantics](#web-w51b); [Data access guidance / 032 - Keep numeric projections from silently changing business results](#web-w51c); [Data access guidance / 033 - Complete document queries and saves without removed synchronous paths](#web-w52a); [Data access guidance / 034 - Query type identity using its correct model name](#web-w52b); [Data access guidance / 035 - Reconcile competing migration histories before merge](#web-w53a); [Data access guidance / 036 - Add and apply a migration in one command](#web-w53b); [Data access guidance / 037 - Undo local migration scaffolding without a reachable database](#web-w53c); [Data access guidance / 038 - Keep a modeled relationship without creating its database constraint](#web-w54a); [Data access guidance / 039 - Distinguish intentional schema ownership from missing migrations](#web-w54b); [Data access guidance / 040 - Share predictable design-time command defaults](#web-w55a); [Data access guidance / 042 - Restore design-time services previously supplied transitively](#web-w55c); [Data access guidance / 043 - Apply a command consistently to several context models](#web-w56a); [Data access guidance / 044 - Pipe only generated SQL into a deployment artifact](#web-w56b); [Data access guidance / 045 - Stage-specific generated EF artifacts](#web-w57); [Data access guidance / 046 - Split-query concurrency recovery](#web-w58); [Desktop guidance / 000 - Restore intentional WPF clipboard error handling](#nat-d01); [Desktop guidance / 001 - Adopt modern WinForms appearance deliberately](#nat-d02); [Desktop guidance / 003 - Configure initial form reveal through C# and VB startup](#nat-d04); [Desktop guidance / 004 - Batch nested control mutations with disposable scopes](#nat-d05); [Desktop guidance / 006 - Preserve C# and VB WinForms Designer compatibility](#nat-d07); [Mobile and native guidance / 000 - Migrate ordinary MAUI XAML to SourceGen](#nat-m01); [Mobile and native guidance / 001 - Author preview C# logic and expressions in XAML](#nat-m02); [Mobile and native guidance / 002 - Build trim-safe relative and reference bindings](#nat-m03); [Mobile and native guidance / 003 - Use native XAML Hot Reload from the CLI](#nat-m04); [Mobile and native guidance / 004 - Update visual resources and replace invalid Color inheritance](#nat-m05); [Mobile and native guidance / 005 - Handle a cancellable press-and-hold interaction](#nat-m06); [Mobile and native guidance / 006 - Build accessible routed MAUI navigation](#nat-m07); [Mobile and native guidance / 007 - Migrate Android Shell off removed compatibility dependencies](#nat-m08); [Mobile and native guidance / 008 - Migrate Apple navigation customizations to handlers](#nat-m09); [Mobile and native guidance / 009 - Migrate custom collection and carousel handlers](#nat-m10); [Mobile and native guidance / 010 - Customize public Android Material3 handlers](#nat-m11); [Mobile and native guidance / 011 - Call .NET from HybridWebView with trimming and AOT](#nat-m12); [Mobile and native guidance / 012 - Cache selected BlazorWebView local assets](#nat-m13); [Mobile and native guidance / 013 - Generate C# UI and Avalonia-integrated MAUI projects](#nat-m14); [Mobile and native guidance / 014 - Integrate an out-of-tree MAUI backend through first-party contracts](#nat-m15); [Mobile and native guidance / 015 - Compose and interact with a dense native map](#nat-m16); [Mobile and native guidance / 018 - Recover interrupted media capture and optionally save it](#nat-m19); [Mobile and native guidance / 019 - Configure system-bar icons and chrome backgrounds](#nat-m20); [Mobile and native guidance / 020 - Deliver external activations to one Windows instance](#nat-m21); [Mobile and native guidance / 021 - Use injectable permissions in a location and notification workflow](#nat-m22); [Mobile and native guidance / 022 - Author themed cross-platform app assets](#nat-m23); [Mobile and native guidance / 024 - Migrate Android target runtime and toolchain configuration](#nat-a01); [Mobile and native guidance / 025 - Migrate Java peer bindings and native loading](#nat-a02); [Mobile and native guidance / 026 - Handle Android HTTP faults and cancellation correctly](#nat-a03); [Mobile and native guidance / 027 - Preserve R8-sensitive Java names during migration](#nat-a04); [Mobile and native guidance / 028 - Declare predictive-back behavior intentionally](#nat-a05); [Mobile and native guidance / 029 - Adopt the required Apple scene lifecycle](#nat-x01); [Mobile and native guidance / 030 - Publish Apple apps with intentional deployment and runtime choices](#nat-x02); [Mobile and native guidance / 031 - Configure Apple native HTTP proxy and authentication routes](#nat-x03); [Mobile and native guidance / 032 - Author Apple manifest and framework package metadata](#nat-x04); [Mobile and native guidance / 033 - Bind nullable Objective-C initializers accurately](#nat-x05); [Mobile and native guidance / 034 - Capture native .NET diagnostics through supported configuration](#nat-n01); [Mobile and native guidance / 035 - Run platform tests inside an app process](#nat-n02); [Web application guidance / 069 - Validate Minimal API requests before handler execution](#foundation-api); [Data access guidance / 047 - Include archived records while retaining tenant filtering](#foundation-ef); [Verified authoring baselines / 005 - Resolve remaining F# runtime and compiler-service contracts](#sh-q01); [Verified authoring baselines / 006 - Resolve C#/VB compiler migrations and VB coverage](#sh-q02); [Verified authoring baselines / 007 - Resolve numeric and parsing compatibility findings](#sh-q03); [Verified authoring baselines / 008 - Resolve archive, path, handle and clock migrations](#sh-q04); [Verified authoring baselines / 009 - Resolve hosting and configuration failure migrations](#sh-q05); [Verified authoring baselines / 010 - Resolve cryptography platform migrations](#sh-q06); [Verified authoring baselines / 011 - Resolve native deployment-artifact migrations](#sh-q07); [Verified authoring baselines / 012 - Resolve SDK/package hosts and affected-test availability](#sh-q08); [Verified authoring baselines / 013 - Map remaining obsolete diagnostics to bounded family owners](#sh-q09); [Verified authoring baselines / 014 - Recheck explicit automatic/no-change exclusions](#sh-q10); [Verified authoring baselines / 015 - Bound the removed Blazor API migration families](#web-w18b); [Verified authoring baselines / 016 - Resolve DBSC release availability and record its disposition](#web-w59); [Verified authoring baselines / 017 - Reconcile WindowsDesktop public changes and retained exclusions](#nat-r01); [Verified authoring baselines / 018 - Reconcile MAUI and platform API breakage gaps](#nat-r02); [Verified authoring baselines / 027 - Reconcile data text numeric and I/O API coverage](#api-dataio); [Verified authoring baselines / 028 - Reconcile serialization API coverage](#api-json); [Verified authoring baselines / 029 - Reconcile runtime transport and security API coverage](#api-runtime); [Verified authoring baselines / 030 - Reconcile Extensions API coverage](#api-extensions); [Verified authoring baselines / 031 - Reconcile SDK build and package authoring coverage](#api-build); [Verified authoring baselines / 032 - Reconcile Blazor and Razor API coverage](#api-blazor); [Verified authoring baselines / 033 - Reconcile non-Blazor web API coverage](#api-web); [Verified authoring baselines / 034 - Reconcile EF and first-party provider API coverage](#api-ef); [Clients and release readiness / 018 - Perform the controlled Windows and WSL certificate-trust check](#h-wsl-proof); [Clients and release readiness / 019 - Verify the no-build command in actual Visual Studio PMC](#h-pmc-proof); [Mobile and native guidance / 036 - Verify the original Windows map against the real service](#nat-h-map-proof); [Mobile and native guidance / 037 - Perform the native passkey ceremony and server verification](#nat-h-passkey-proof); [Mobile and native guidance / 038 - Exercise the experimental MAUI Apple AI sample](#nat-h-apple-ai-proof); [Desktop guidance / 007 - Check custom rendering across owner-controlled Windows settings](#nat-h-desktop-settings-proof); [Desktop guidance / 008 - Check fullscreen restoration and idle behavior in the sample](#nat-h-fullscreen-proof); [Clients and release readiness / 020 - Verify real Entra authentication for the SQL sample](#h-entra-proof); [Installed documentation server / 005 - Validate the same packed tool on supported host environments](#core-ci); [Installed documentation server / 006 - Document original recipe contribution and evidence refresh](#author-docs); [Verified authoring baselines / 023 - Confirm native evidence handoff and refresh ownership](#nat-h-evidence-owner)

**Delivered outcome:** The package has accountable coverage of every qualifying release change and the exact eight foundations before any public publication.

**Acceptance criteria:**

- [ ] Reconcile all 406 current disposition rows, the 25+7+3 Blazor baseline and accepted migrations, exact 8 foundations and all delivered source/API-audit findings; grouped/excluded rows need reasons and qualifying unresolved goals need delivered recipes.
- [ ] Verify full offline recipe content, original examples, required actual older/newer compilation and claim-specific evidence/freshness. Confirm targeted Human proofs are complete rather than treating compile/tool smoke as native or provider behavior.
- [ ] New qualifying findings need explicit narrow estimated delivery follow-ons for review; this closure task does not implement an unknown-sized missing corpus or waive the comprehensive gate.
- [ ] Record evidence and actual gaps in a reviewable manifest/report tied to the candidate package; do not claim public/stable readiness from ticket counts alone.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 3, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 73, 79, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 130, 131, 132, 133, 134, 138, 139, 145, 151, 158, 159, 160.

<a id="h-public-readiness"></a>

### 022 - Decide whether to publish an optional pre-GA preview

**New · Human · M (3) · Labels: Operations**

**Estimate rationale:** Focused owner gate over a concrete candidate with optional manual preview decision; not automatic permission to release.

**Blockers:** [Clients and release readiness / 021 - Verify comprehensive content and compilation readiness](#coverage-close); [Clients and release readiness / 017 - Review all four comparison results and launch implications](#h-eval-review); [Clients and release readiness / 003 - Configure the approved NuGet trusted publisher identity](#h-oidc); [Clients and release readiness / 004 - Check the packaged tool in T3 Code](#h-client-t3); [Clients and release readiness / 005 - Check the packaged tool in Codex CLI](#h-client-codex); [Clients and release readiness / 006 - Check the packaged tool in Claude Code](#h-client-claude); [Clients and release readiness / 007 - Check the packaged tool in VS Code Copilot](#h-client-copilot)

**Delivered outcome:** The owner can publish a complete RC-evidenced preview or explicitly choose to wait for stable; no preview is mandatory.

**Acceptance criteria:**

- [ ] Review coverage/compilation/focused package checks, four real client checks, bounded evaluation, artifact identity and trusted-publisher configuration. All gates apply even to a preview.
- [ ] If choosing pre-GA publication, use manual 1.0.0-preview.N with honest RC verification disclosure and the reviewed artifact; no stable-GA claim or weekly unlock. A preview is optional and never a stable-release prerequisite.
- [ ] Record any actual publication and evidence only if explicitly performed by the owner; otherwise record the reviewed channel/readiness decision and remaining GA boundary.
- [ ] This optional preview decision does not block the first stable release. It can close with a recorded decision to wait; missing mandatory public-package gates still cannot be waived.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 144, 145, 146, 147, 151, 156, 157, 160.

## GA reconciliation and stable launch

**New native milestone.** Final shipped release-family guidance is reconciled before the owner publishes stable and activates the checked weekly slot.

<a id="ga-lang"></a>

### 000 - Reconcile GA language and compiler guidance

**New · Agent · L (5) · Labels: Agent, Research**

**Estimate rationale:** A bounded release-family source/artifact reconciliation; actual corrections remain narrow separate delivery work.

**Blockers:** [Language and library guidance / 000 - Model and persist closed C# outcomes](#sh-l01); [Language and library guidance / 001 - Extend and construct collection-like types](#sh-l02); [Language and library guidance / 002 - Control an enclosing loop directly](#sh-l03); [Language and library guidance / 003 - Put implementation helpers on interfaces](#sh-l04); [Language and library guidance / 004 - Express and check unsafe boundaries](#sh-l05); [Language and library guidance / 005 - Construct and serialize F# data models](#sh-f01); [Language and library guidance / 006 - Author clearer F#/.NET API boundaries](#sh-f02); [Language and library guidance / 007 - Format F# diagnostics deliberately](#sh-f03); [Language and library guidance / 008 - Start, bound and fail F# asynchronous work](#sh-f04); [Language and library guidance / 009 - Migrate unsupported F# enum operations](#sh-f05)

**Delivered outcome:** Final release evidence for C#15, F#11 and qualifying VB/compiler changes is reconciled with prepared RC guidance.

**Acceptance criteria:**

- [ ] Wait for actual .NET 11 release-family GA notes, breaking changes and distributable artifacts; compare final API/source and supported dependencies with the retained RC baseline. Do not relabel old RC evidence as GA verification.
- [ ] Resolve availability/stability changes against canonical actual artifacts, retaining deliberate experimental APIs with labels. Link affected existing slices and identify exact corrections/additional goals.
- [ ] Prepare narrow estimated follow-ons for missing authoring work and refreshed evidence; do not close the global stable gate until those are delivered. The audit itself does not implement arbitrary new content.
- [ ] Compare the complete prepared family catalogue after the listed delivery inputs exist. Actual GA artifacts are an external availability condition; if not yet published, record that explicit wait rather than treating RC assets as GA.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 41, 42, 43, 44, 47, 93, 94, 95, 130, 132, 133, 146, 158, 159, 160.

<a id="ga-lib"></a>

### 001 - Reconcile GA runtime and shared-library guidance

**New · Agent · L (5) · Labels: Agent, Research**

**Estimate rationale:** A bounded release-family source/artifact reconciliation; actual corrections remain narrow separate delivery work.

**Blockers:** [Language and library guidance / 010 - Capture and manage a command-line child process](#sh-b01); [Language and library guidance / 011 - Control native child-process startup and termination](#sh-b02); [Language and library guidance / 012 - Adapt existing buffers without redundant copying](#sh-b03); [Language and library guidance / 013 - Process Unicode characters and line boundaries](#sh-b04); [Language and library guidance / 014 - Encode and consume incremental binary data](#sh-b05); [Language and library guidance / 015 - Stream JSON records and reuse a writer safely](#sh-j01); [Language and library guidance / 016 - Resolve generic JSON contracts and typed metadata](#sh-j02); [Language and library guidance / 017 - Generate contracts for encapsulated models](#sh-j03); [Language and library guidance / 018 - Define JSON conventions and expanded value contracts](#sh-j04); [Language and library guidance / 019 - Reject malformed import contracts](#sh-j05); [Language and library guidance / 020 - Control ZIP entry access and protection](#sh-a01); [Language and library guidance / 021 - Detect archive corruption and choose TAR representation](#sh-a02); [Language and library guidance / 022 - Compress buffers with the intended format and lifecycle](#sh-a03); [Language and library guidance / 023 - Preserve unmatched records during reconciliation](#sh-n01); [Language and library guidance / 024 - Transform an asynchronous stream without buffering](#sh-n02); [Language and library guidance / 025 - Select decimal and complex numeric representations](#sh-n03); [Language and library guidance / 026 - Preserve numeric parsing and conversion intent](#sh-n04); [Language and library guidance / 027 - Generate random values through generic numeric APIs](#sh-n05); [Language and library guidance / 028 - Arrange vector lanes and select supported instructions](#sh-n06); [Language and library guidance / 029 - Build bit collections and compare values by key](#sh-n07); [Language and library guidance / 030 - Use supported pipe handles and mobile file locking](#sh-i01); [Language and library guidance / 031 - Inspect assembly metadata across deployment modes](#sh-e01); [Language and library guidance / 032 - Annotate embedded code strings for tooling](#sh-e02); [Language and library guidance / 033 - Describe URI payloads and media content](#sh-e03); [Language and library guidance / 034 - Bind configuration selectively and await reload work](#sh-e04); [Language and library guidance / 035 - Validate options with dependencies and async startup](#sh-e05); [Language and library guidance / 036 - Observe caches and select entry options from values](#sh-e06); [Language and library guidance / 037 - Validate data asynchronously with localized rules](#sh-e07); [Language and library guidance / 038 - Configure tracing, efficient logging and HTTP observations](#sh-e08); [Language and library guidance / 039 - Exchange, compare and wrap key material](#sh-k01); [Language and library guidance / 040 - Use supported post-quantum hybrid and CMS contracts](#sh-k02); [Language and library guidance / 041 - Drive TLS handshakes through caller-owned I/O](#sh-k03); [Language and library guidance / 042 - Configure HTTP request compression and connection capacity](#sh-t01); [Language and library guidance / 043 - Resolve DNS service records](#sh-t02); [Language and library guidance / 044 - Prioritize QUIC streams](#sh-t03); [Language and library guidance / 045 - Configure crash evidence and hardware requirements](#sh-r01); [Language and library guidance / 046 - Choose runtime-async code generation deliberately](#sh-r02); [Language and library guidance / 047 - Link native code into CoreCLR WebAssembly](#sh-r03)

**Delivered outcome:** Final release evidence for runtime-facing/BCL/Extensions/network/crypto/numeric APIs and migrations is reconciled with prepared RC guidance.

**Acceptance criteria:**

- [ ] Wait for actual .NET 11 release-family GA notes, breaking changes and distributable artifacts; compare final API/source and supported dependencies with the retained RC baseline. Do not relabel old RC evidence as GA verification.
- [ ] Resolve availability/stability changes against canonical actual artifacts, retaining deliberate experimental APIs with labels. Link affected existing slices and identify exact corrections/additional goals.
- [ ] Prepare narrow estimated follow-ons for missing authoring work and refreshed evidence; do not close the global stable gate until those are delivered. The audit itself does not implement arbitrary new content.
- [ ] Compare the complete prepared family catalogue after the listed delivery inputs exist. Actual GA artifacts are an external availability condition; if not yet published, record that explicit wait rather than treating RC assets as GA.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 41, 42, 43, 44, 47, 93, 94, 95, 130, 132, 133, 146, 158, 159, 160.

<a id="ga-build"></a>

### 002 - Reconcile GA SDK and package-authoring guidance

**New · Agent · L (5) · Labels: Agent, Research**

**Estimate rationale:** A bounded release-family source/artifact reconciliation; actual corrections remain narrow separate delivery work.

**Blockers:** [Build and deployment guidance / 000 - Grow a file-based repository utility](#sh-s01); [Build and deployment guidance / 001 - Target solution, formatting and launch operations precisely](#sh-s02); [Build and deployment guidance / 002 - Configure inner-loop app hosts and preview scaffolding](#sh-s03); [Build and deployment guidance / 003 - Select and create test-runner projects](#sh-s04); [Build and deployment guidance / 004 - Package tools and consume single-file output correctly](#sh-s06); [Build and deployment guidance / 005 - Declare static asset groups including TypeScript assets](#sh-s07); [Build and deployment guidance / 006 - Publish reproducible multi-architecture containers](#sh-s08); [Build and deployment guidance / 007 - Configure CLI telemetry explicitly](#sh-s09); [Build and deployment guidance / 008 - Produce build archives and inspect item globs](#sh-m01); [Build and deployment guidance / 009 - Migrate custom MSBuild XML and feature switches](#sh-m02); [Build and deployment guidance / 010 - Select dependencies with cooldown and vulnerability updates](#sh-p01); [Build and deployment guidance / 011 - Produce deterministic package timestamps](#sh-p02); [Build and deployment guidance / 012 - Retarget a NuGet client-library host](#sh-p03)

**Delivered outcome:** Final release evidence for SDK, MSBuild, NuGet, tools/testing and container artifacts is reconciled with prepared RC guidance.

**Acceptance criteria:**

- [ ] Wait for actual .NET 11 release-family GA notes, breaking changes and distributable artifacts; compare final API/source and supported dependencies with the retained RC baseline. Do not relabel old RC evidence as GA verification.
- [ ] Resolve availability/stability changes against canonical actual artifacts, retaining deliberate experimental APIs with labels. Link affected existing slices and identify exact corrections/additional goals.
- [ ] Prepare narrow estimated follow-ons for missing authoring work and refreshed evidence; do not close the global stable gate until those are delivered. The audit itself does not implement arbitrary new content.
- [ ] Compare the complete prepared family catalogue after the listed delivery inputs exist. Actual GA artifacts are an external availability condition; if not yet published, record that explicit wait rather than treating RC assets as GA.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 41, 42, 43, 44, 47, 93, 94, 95, 130, 132, 133, 146, 158, 159, 160.

<a id="ga-web"></a>

### 003 - Reconcile GA web framework guidance

**New · Agent · L (5) · Labels: Agent, Research**

**Estimate rationale:** A bounded release-family source/artifact reconciliation; actual corrections remain narrow separate delivery work.

**Blockers:** [Web application guidance / 000 - Immediate and complete form validation guidance](#web-w01); [Web application guidance / 001 - Accessible model-driven field captions](#web-w02); [Web application guidance / 002 - Awaited validation for forms and endpoints](#web-w03); [Web application guidance / 003 - Localized validation feedback in Blazor and endpoints](#web-w04); [Web application guidance / 004 - Author SSR redirect and session state workflows](#web-w05); [Web application guidance / 005 - Base-aware Blazor navigation](#web-w06); [Web application guidance / 006 - Place server-rendered content inside an interactive boundary](#web-w07a); [Web application guidance / 007 - Offer a single parameter for alternative UI values](#web-w07b); [Web application guidance / 008 - Safe static-subtree caching](#web-w08); [Web application guidance / 009 - Author URL-restored QuickGrid state](#web-w09a); [Web application guidance / 010 - Interactive QuickGrid row selection](#web-w09b); [Web application guidance / 011 - Virtualized QuickGrid row positioning](#web-w09c); [Web application guidance / 012 - Anchoring for changing variable-height lists](#web-w10a); [Web application guidance / 013 - Virtual-list item reveal](#web-w10b); [Web application guidance / 014 - Restrictive-CSP virtual lists](#web-w10c); [Web application guidance / 015 - Idle and maintenance circuit pause workflows](#web-w11); [Web application guidance / 016 - Server-configured Blazor browser startup](#web-w12a); [Web application guidance / 017 - Unified Blazor startup script options](#web-w12b); [Web application guidance / 018 - Deliberate Blazor circuit transport policy](#web-w12c); [Web application guidance / 019 - Run periodic browser work within application lifetime](#web-w13a); [Web application guidance / 020 - Move long-running browser work away from rendering](#web-w13b); [Web application guidance / 021 - Reuse one browser build across environments](#web-w14a); [Web application guidance / 022 - Keep formatting consistent through hydration](#web-w14b); [Web application guidance / 023 - Present environment-appropriate application chrome](#web-w14c); [Web application guidance / 024 - Serve a standalone client with configured backend routes](#web-w15a); [Web application guidance / 025 - Collect component activity from the browser](#web-w15b); [Web application guidance / 026 - Operate navigation without an inline script exception](#web-w16a); [Web application guidance / 027 - Keep a custom event distinct from its browser trigger](#web-w16b); [Web application guidance / 028 - Retain deliberate loading hints during enhanced navigation](#web-w16c); [Web application guidance / 029 - Supply component dependencies through a custom activation policy](#web-w17); [Web application guidance / 030 - Express a JavaScript call that has no result](#web-w18a); [Web application guidance / 031 - Display interactive mathematical notation without custom DOM creation](#web-w18c); [Web application guidance / 032 - Experimental AI conversation streaming](#web-w19); [Web application guidance / 033 - Experimental AI tool rendering](#web-w20); [Web application guidance / 034 - Experimental AI approval continuation](#web-w21); [Web application guidance / 035 - Experimental AI and UI state synchronization](#web-w22); [Web application guidance / 036 - Simplify form protection without opening cross-origin writes](#web-w23); [Web application guidance / 037 - Shared authorization requirement metadata](#web-w24); [Web application guidance / 038 - Verify expiry and lockout without waiting](#web-w25a); [Web application guidance / 039 - Give a saved authenticator an intelligible name](#web-w25b); [Web application guidance / 040 - Present a valid outcome when passkey sign-in is disallowed](#web-w25c); [Web application guidance / 041 - Live identity refresh for SignalR and Blazor](#web-w26); [Web application guidance / 042 - Stop an ordinary hub operation when its caller abandons it](#web-w27); [Web application guidance / 043 - Return a consistent failure before endpoint execution](#web-w28a); [Web application guidance / 044 - Bypass irrelevant middleware for a deliberately standalone response](#web-w28b); [Web application guidance / 045 - Call a secured file-based endpoint during development](#web-w29a); [Web application guidance / 046 - Use one trust operation for Windows and WSL](#web-w29b); [Web application guidance / 047 - Start an agent-facing web tool from a bundled template](#web-w29c); [Web application guidance / 048 - Describe a body that has several valid shapes](#web-w30); [Web application guidance / 049 - Accurate OpenAPI download responses](#web-w31a); [Web application guidance / 050 - Author HTTP QUERY request contracts](#web-w31b); [Web application guidance / 051 - Multi-shape OpenAPI response contracts](#web-w31c); [Web application guidance / 052 - Typed SSE OpenAPI contracts](#web-w31d); [Web application guidance / 053 - Compatible OpenAPI document versions](#web-w32a); [Web application guidance / 054 - Environment-specific OpenAPI generation](#web-w32b); [Web application guidance / 055 - Contract deprecation from source metadata](#web-w32c); [Web application guidance / 056 - Supported OpenAPI transformer migrations](#web-w32d); [Web application guidance / 057 - Stable OpenAPI server URL composition](#web-w32e); [Web application guidance / 058 - Record handshake failures and inspect ClientHello](#web-w33a); [Web application guidance / 059 - Expose or require transport binding for authenticated requests](#web-w33b); [Web application guidance / 060 - Make authored clients and proxy adapters accepted by the server](#web-w33c); [Web application guidance / 061 - Collect framework traces without duplicate enrichment](#web-w34); [Web application guidance / 062 - Select output-cache behavior from current tenant policy](#web-w35a); [Web application guidance / 063 - Select compression and request decoding policy](#web-w35b); [Web application guidance / 064 - Retain concurrency limits after obsolete middleware removal](#web-w35c); [Web application guidance / 065 - Start MVC without removed compatibility APIs](#web-w36a); [Web application guidance / 066 - Keep a customized image helper constructing correctly](#web-w36b); [Web application guidance / 067 - Identify reflected properties through the supported overload](#web-w36c); [Web application guidance / 068 - Build views without newly obsolete registration calls](#web-w36d); [Web application guidance / 069 - Validate Minimal API requests before handler execution](#foundation-api)

**Delivered outcome:** Final release evidence for ASP.NET Core, all preserved Blazor baseline/additions, experimental AI, authentication and server contracts is reconciled with prepared RC guidance.

**Acceptance criteria:**

- [ ] Wait for actual .NET 11 release-family GA notes, breaking changes and distributable artifacts; compare final API/source and supported dependencies with the retained RC baseline. Do not relabel old RC evidence as GA verification.
- [ ] Resolve availability/stability changes against canonical actual artifacts, retaining deliberate experimental APIs with labels. Link affected existing slices and identify exact corrections/additional goals.
- [ ] Prepare narrow estimated follow-ons for missing authoring work and refreshed evidence; do not close the global stable gate until those are delivered. The audit itself does not implement arbitrary new content.
- [ ] Compare the complete prepared family catalogue after the listed delivery inputs exist. Actual GA artifacts are an external availability condition; if not yet published, record that explicit wait rather than treating RC assets as GA.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 41, 42, 43, 44, 47, 93, 94, 95, 130, 132, 133, 146, 158, 159, 160.

<a id="ga-data"></a>

### 004 - Reconcile GA EF and provider guidance

**New · Agent · L (5) · Labels: Agent, Research**

**Estimate rationale:** A bounded release-family source/artifact reconciliation; actual corrections remain narrow separate delivery work.

**Blockers:** [Data access guidance / 000 - Restore database authentication after driver dependency changes](#web-w37a); [Data access guidance / 001 - Keep generated SQL compatible with the actual database](#web-w37b); [Data access guidance / 002 - Inherited complex-value relational mappings](#web-w38a); [Data access guidance / 003 - Chained nested-scalar EF configuration](#web-w38b); [Data access guidance / 004 - Complex-value keys and lookup indexes](#web-w38c); [Data access guidance / 005 - Map embedded sequences without a deprecated synthetic identity](#web-w39a); [Data access guidance / 006 - Make scalar-versus-collection mapping unambiguous](#web-w39b); [Data access guidance / 007 - Query related data from each group's selected item](#web-w40a); [Data access guidance / 008 - Preserve both sides of a database reconciliation](#web-w40b); [Data access guidance / 009 - Fetch the record associated with a minimum or maximum key](#web-w40c); [Data access guidance / 010 - Use EF's asynchronous query adapter intentionally](#web-w40d); [Data access guidance / 011 - Author SQL JSON path-existence queries](#web-w41a); [Data access guidance / 012 - Native SQL JSON containment queries](#web-w41b); [Data access guidance / 013 - Author SQL JSON path indexes](#web-w41c); [Data access guidance / 014 - Indexed vector search and explicit vector loading](#web-w42); [Data access guidance / 015 - Managed full-text indexing and ranked search](#web-w43); [Data access guidance / 016 - Combined lexical and vector ranking](#web-w44); [Data access guidance / 017 - Read row validity through ordinary entity properties](#web-w45a); [Data access guidance / 018 - Perform time conversion and bucketing on the server](#web-w45b); [Data access guidance / 019 - Return an ordered textual summary per group](#web-w46a); [Data access guidance / 020 - Bind a full-width unsigned integer without custom encoding](#web-w46b); [Data access guidance / 021 - Keep low-level SQLite access on a supported framework](#web-w46c); [Data access guidance / 022 - Restore native SQLite initialization after bundle retirement](#web-w46d); [Data access guidance / 023 - Treat nested document content as values rather than entities](#web-w47a); [Data access guidance / 024 - Provision only the document indexes the workload needs](#web-w47b); [Data access guidance / 025 - Make the scope of a multi-document save explicit](#web-w48a); [Data access guidance / 026 - Increase ingestion concurrency through supported configuration](#web-w48b); [Data access guidance / 027 - Preserve read-your-writes across application instances](#web-w48c); [Data access guidance / 028 - Author Cosmos raw-document and unmapped-field migration](#web-w49); [Data access guidance / 029 - Keep existing documents reachable after key escaping changes](#web-w50); [Data access guidance / 030 - Decide what an absent document path contributes to a result](#web-w51a); [Data access guidance / 031 - Handle an empty document collection through collection semantics](#web-w51b); [Data access guidance / 032 - Keep numeric projections from silently changing business results](#web-w51c); [Data access guidance / 033 - Complete document queries and saves without removed synchronous paths](#web-w52a); [Data access guidance / 034 - Query type identity using its correct model name](#web-w52b); [Data access guidance / 035 - Reconcile competing migration histories before merge](#web-w53a); [Data access guidance / 036 - Add and apply a migration in one command](#web-w53b); [Data access guidance / 037 - Undo local migration scaffolding without a reachable database](#web-w53c); [Data access guidance / 038 - Keep a modeled relationship without creating its database constraint](#web-w54a); [Data access guidance / 039 - Distinguish intentional schema ownership from missing migrations](#web-w54b); [Data access guidance / 040 - Share predictable design-time command defaults](#web-w55a); [Data access guidance / 041 - Run a design-time operation using an already-current build](#web-w55b); [Data access guidance / 042 - Restore design-time services previously supplied transitively](#web-w55c); [Data access guidance / 043 - Apply a command consistently to several context models](#web-w56a); [Data access guidance / 044 - Pipe only generated SQL into a deployment artifact](#web-w56b); [Data access guidance / 045 - Stage-specific generated EF artifacts](#web-w57); [Data access guidance / 046 - Split-query concurrency recovery](#web-w58); [Data access guidance / 047 - Include archived records while retaining tenant filtering](#foundation-ef)

**Delivered outcome:** Final release evidence for EF11 model/query/migrations and actual provider/service requirements is reconciled with prepared RC guidance.

**Acceptance criteria:**

- [ ] Wait for actual .NET 11 release-family GA notes, breaking changes and distributable artifacts; compare final API/source and supported dependencies with the retained RC baseline. Do not relabel old RC evidence as GA verification.
- [ ] Resolve availability/stability changes against canonical actual artifacts, retaining deliberate experimental APIs with labels. Link affected existing slices and identify exact corrections/additional goals.
- [ ] Prepare narrow estimated follow-ons for missing authoring work and refreshed evidence; do not close the global stable gate until those are delivered. The audit itself does not implement arbitrary new content.
- [ ] Compare the complete prepared family catalogue after the listed delivery inputs exist. Actual GA artifacts are an external availability condition; if not yet published, record that explicit wait rather than treating RC assets as GA.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 41, 42, 43, 44, 47, 93, 94, 95, 130, 132, 133, 146, 158, 159, 160.

<a id="ga-native"></a>

### 005 - Reconcile GA desktop and mobile guidance

**New · Agent · L (5) · Labels: Agent, Research**

**Estimate rationale:** A bounded release-family source/artifact reconciliation; actual corrections remain narrow separate delivery work.

**Blockers:** [Desktop guidance / 000 - Restore intentional WPF clipboard error handling](#nat-d01); [Desktop guidance / 001 - Adopt modern WinForms appearance deliberately](#nat-d02); [Desktop guidance / 003 - Configure initial form reveal through C# and VB startup](#nat-d04); [Desktop guidance / 004 - Batch nested control mutations with disposable scopes](#nat-d05); [Desktop guidance / 006 - Preserve C# and VB WinForms Designer compatibility](#nat-d07); [Mobile and native guidance / 000 - Migrate ordinary MAUI XAML to SourceGen](#nat-m01); [Mobile and native guidance / 001 - Author preview C# logic and expressions in XAML](#nat-m02); [Mobile and native guidance / 002 - Build trim-safe relative and reference bindings](#nat-m03); [Mobile and native guidance / 003 - Use native XAML Hot Reload from the CLI](#nat-m04); [Mobile and native guidance / 004 - Update visual resources and replace invalid Color inheritance](#nat-m05); [Mobile and native guidance / 005 - Handle a cancellable press-and-hold interaction](#nat-m06); [Mobile and native guidance / 006 - Build accessible routed MAUI navigation](#nat-m07); [Mobile and native guidance / 007 - Migrate Android Shell off removed compatibility dependencies](#nat-m08); [Mobile and native guidance / 008 - Migrate Apple navigation customizations to handlers](#nat-m09); [Mobile and native guidance / 009 - Migrate custom collection and carousel handlers](#nat-m10); [Mobile and native guidance / 010 - Customize public Android Material3 handlers](#nat-m11); [Mobile and native guidance / 011 - Call .NET from HybridWebView with trimming and AOT](#nat-m12); [Mobile and native guidance / 012 - Cache selected BlazorWebView local assets](#nat-m13); [Mobile and native guidance / 013 - Generate C# UI and Avalonia-integrated MAUI projects](#nat-m14); [Mobile and native guidance / 014 - Integrate an out-of-tree MAUI backend through first-party contracts](#nat-m15); [Mobile and native guidance / 015 - Compose and interact with a dense native map](#nat-m16); [Mobile and native guidance / 018 - Recover interrupted media capture and optionally save it](#nat-m19); [Mobile and native guidance / 019 - Configure system-bar icons and chrome backgrounds](#nat-m20); [Mobile and native guidance / 020 - Deliver external activations to one Windows instance](#nat-m21); [Mobile and native guidance / 021 - Use injectable permissions in a location and notification workflow](#nat-m22); [Mobile and native guidance / 022 - Author themed cross-platform app assets](#nat-m23); [Mobile and native guidance / 024 - Migrate Android target runtime and toolchain configuration](#nat-a01); [Mobile and native guidance / 025 - Migrate Java peer bindings and native loading](#nat-a02); [Mobile and native guidance / 026 - Handle Android HTTP faults and cancellation correctly](#nat-a03); [Mobile and native guidance / 027 - Preserve R8-sensitive Java names during migration](#nat-a04); [Mobile and native guidance / 028 - Declare predictive-back behavior intentionally](#nat-a05); [Mobile and native guidance / 029 - Adopt the required Apple scene lifecycle](#nat-x01); [Mobile and native guidance / 030 - Publish Apple apps with intentional deployment and runtime choices](#nat-x02); [Mobile and native guidance / 031 - Configure Apple native HTTP proxy and authentication routes](#nat-x03); [Mobile and native guidance / 032 - Author Apple manifest and framework package metadata](#nat-x04); [Mobile and native guidance / 033 - Bind nullable Objective-C initializers accurately](#nat-x05); [Mobile and native guidance / 034 - Capture native .NET diagnostics through supported configuration](#nat-n01); [Mobile and native guidance / 035 - Run platform tests inside an app process](#nat-n02); [Mobile and native guidance / 036 - Verify the original Windows map against the real service](#nat-h-map-proof); [Mobile and native guidance / 037 - Perform the native passkey ceremony and server verification](#nat-h-passkey-proof); [Mobile and native guidance / 038 - Exercise the experimental MAUI Apple AI sample](#nat-h-apple-ai-proof); [Desktop guidance / 007 - Check custom rendering across owner-controlled Windows settings](#nat-h-desktop-settings-proof); [Desktop guidance / 008 - Check fullscreen restoration and idle behavior in the sample](#nat-h-fullscreen-proof)

**Delivered outcome:** Final release evidence for WindowsDesktop, MAUI, Android and Apple workload/API/artifact composition is reconciled with prepared RC guidance.

**Acceptance criteria:**

- [ ] Wait for actual .NET 11 release-family GA notes, breaking changes and distributable artifacts; compare final API/source and supported dependencies with the retained RC baseline. Do not relabel old RC evidence as GA verification.
- [ ] Resolve availability/stability changes against canonical actual artifacts, retaining deliberate experimental APIs with labels. Link affected existing slices and identify exact corrections/additional goals.
- [ ] Prepare narrow estimated follow-ons for missing authoring work and refreshed evidence; do not close the global stable gate until those are delivered. The audit itself does not implement arbitrary new content.
- [ ] Compare the complete prepared family catalogue after the listed delivery inputs exist. Actual GA artifacts are an external availability condition; if not yet published, record that explicit wait rather than treating RC assets as GA.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 41, 42, 43, 44, 47, 93, 94, 95, 130, 132, 133, 146, 158, 159, 160.

<a id="ga-ready"></a>

### 006 - Verify the final GA candidate and refreshed evidence

**New · Agent · L (5) · Labels: Agent, Testing**

**Estimate rationale:** Final delivered-artifact and freshness gate; any discovered product fixes are separately sized work.

**Blockers:** [Clients and release readiness / 021 - Verify comprehensive content and compilation readiness](#coverage-close); [Clients and release readiness / 017 - Review all four comparison results and launch implications](#h-eval-review); [Clients and release readiness / 003 - Configure the approved NuGet trusted publisher identity](#h-oidc); [GA reconciliation and stable launch / 000 - Reconcile GA language and compiler guidance](#ga-lang); [GA reconciliation and stable launch / 001 - Reconcile GA runtime and shared-library guidance](#ga-lib); [GA reconciliation and stable launch / 002 - Reconcile GA SDK and package-authoring guidance](#ga-build); [GA reconciliation and stable launch / 003 - Reconcile GA web framework guidance](#ga-web); [GA reconciliation and stable launch / 004 - Reconcile GA EF and provider guidance](#ga-data); [GA reconciliation and stable launch / 005 - Reconcile GA desktop and mobile guidance](#ga-native)

**Delivered outcome:** A stable candidate is tied to final shipped artifacts and complete current evidence.

**Acceptance criteria:**

- [ ] Confirm all six GA reconciliations and every qualifying correction/follow-on are delivered; refresh only evidence affected by source/target/dependency/toolchain changes.
- [ ] Run the required package validation against the single stable candidate artifact, confirm comprehensive coverage plus 8 foundations and recheck relevant client/product defects. Preserve actual RC versus GA evidence history.
- [ ] Record exact versions, package identity, checks actually run and remaining gaps. Missing compilation or qualifying coverage prevents readiness; no invented broad regression/native matrix.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 44, 47, 128, 129, 130, 131, 132, 133, 134, 143, 146, 151, 158, 159, 160.

<a id="h-stable"></a>

### 007 - Approve and manually publish the first stable package

**New · Human · M (3) · Labels: Deployment, Operations**

**Estimate rationale:** Owner-controlled stable launch and artifact confirmation, not an agent implementation task.

**Blockers:** [GA reconciliation and stable launch / 006 - Verify the final GA candidate and refreshed evidence](#ga-ready); [Clients and release readiness / 004 - Check the packaged tool in T3 Code](#h-client-t3); [Clients and release readiness / 005 - Check the packaged tool in Codex CLI](#h-client-codex); [Clients and release readiness / 006 - Check the packaged tool in Claude Code](#h-client-claude); [Clients and release readiness / 007 - Check the packaged tool in VS Code Copilot](#h-client-copilot)

**Delivered outcome:** The owner releases1.0.0 only after the complete GA candidate is ready.

**Acceptance criteria:**

- [ ] Review the final GA evidence, all four real client checks, evaluation findings and trusted-publisher identity; verify the exact previously validated package is the one published.
- [ ] Manually approve/perform the first stable1.0.0 release with correct SemVer and non-prerelease GitHub metadata. Record actual NuGet/GitHub artifact evidence; leave merging/releasing to Pingu unless separately authorized.
- [ ] No preview or source-audit completion alone unlocks stable publication or weekly scheduling.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 142, 143, 144, 146, 147, 151, 160.

<a id="schedule-recheck"></a>

### 008 - Recheck all PinguApps package publishing schedules

**New · Agent · M (3) · Labels: Agent, Research**

**Estimate rationale:** Organization-wide live workflow audit, bounded to publishing schedules and the chosen activation window.

**Blockers:** [GA reconciliation and stable launch / 007 - Approve and manually publish the first stable package](#h-stable)

**Delivered outcome:** The selected weekly slot is checked against the organization immediately before activation.

**Acceptance criteria:**

- [ ] Read current workflows/schedules across all PinguApps organization repositories, including packages outside the initial local reference list; do not scan personal or other organizations.
- [ ] Confirm Sunday 18:17UTC remains separated from current publishers and record the audit timestamp/evidence. If a clash exists, propose an offset for owner approval before activation.
- [ ] Do not mutate schedules or treat the older planning audit as a fresh result.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 148, 151, 160.

<a id="h-activate"></a>

### 009 - Activate weekly stable draft publication

**New · Human · S (2) · Labels: Operations**

**Estimate rationale:** One focused owner-controlled automation activation after concrete release and schedule evidence.

**Blockers:** [GA reconciliation and stable launch / 008 - Recheck all PinguApps package publishing schedules](#schedule-recheck); [Clients and release readiness / 002 - Gate the standard weekly stable draft publication flow](#weekly-workflow)

**Delivered outcome:** The owner enables the reviewed standard weekly channel after the first real stable release.

**Acceptance criteria:**

- [ ] Use the fresh organization schedule result and reviewed workflow, normally Sunday 18:17UTC. Obtain approval for any changed offset rather than choosing a clashing time.
- [ ] Enable only after actual first stable publication and verify stable release/draft guards and any-new-main-commit portfolio behavior remain intact.
- [ ] Record actual activation/next scheduled run without issuing an extra manual package release; preserve no-new-commits/no-draft safeguards and no rejected path/diff/empty-note guard.

**Source:** `PinguApps/Playbook: SPEC.md @0a8989814ea11346354464b845c79c15bb71c5e9`, User Stories 147, 148, 149, 151, 160.

## Coverage and publication checks

The accompanying [machine-readable proposal](linear-proposal.json) contains every ticket, all 406 unique source-row owners and all 160 story mappings. An included candidate is not automatically a distinct recipe; grouped/excluded/unresolved rows retain their dispositions and owners. Baseline research counts remain 310 included, 44 grouped, 29 excluded and 23 unresolved.

Before publishing: recheck project overlaps; resolve the same confirmed destination and durable existing labels/Backlog state; create milestones then issues in reviewed order. Save every approved estimate natively. After every issue exists, perform a separate exhaustive native blocker pass and reread all estimates, labels, titles, milestone/team/project/state and relations. Creation order and numeric titles are the fallback where native ordering is unavailable. Preserve current project dates.

No production build, recipe compilation, package/client/native check, evaluation run, release or resource provisioning was performed to create this proposal. Validation here covers proposal structure, estimate values, coverage mappings and the dependency graph only.
