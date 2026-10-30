# PinguApps.Playbook specification

Status: synthesis of the shared understanding confirmed by Pingu on 16 October 2026, including all 51 planning decisions. This is the primary scope document for a later to-linear run. It specifies work to deliver, not functionality or verification already completed.

## Problem Statement

Coding agents can produce working .NET code while missing newer capabilities that would make the implementation simpler, clearer or better suited to the application's target release. An agent may repeat an older workaround, suggest an API unavailable in the actual framework or platform, or overlook a required migration. Release notes alone do not consistently connect a developer's goal to a complete implementation with practical constraints.

Pingu wants one installable Playbook covering the .NET ecosystem, rather than separate tools for Blazor, web APIs, desktop and mobile. Its initial comprehensive coverage is the .NET 11 release family wherever a change affects authored code, markup, scripts, configuration or implementation design. A small curated .NET 10 foundation set remains useful alongside that coverage. This addresses a practical guidance gap; no evidence establishes that every agent lacks this knowledge.

The package must work offline after installation, remain small and deterministic, and fit PinguApps repository, coding, verification and NuGet publishing conventions. It must preserve the original Blazor scope, disclose experimental and platform restrictions, and distinguish documented claims from compilation or execution evidence.

## Solution

Publish `PinguApps.Playbook` as a .NET tool with the command `playbook`. A client starts `playbook mcp` as a local stdio MCP server. The server provides two read-only tools: `list_recipes` for concise, filtered discovery and `get_recipe` for complete guidance. The calling agent interprets the developer's request, inspects the consumer project, chooses suitable guidance and makes the code changes.

A recipe answers a developer goal. It explains the older approach, the newer approach, migration steps, applicability, prerequisites, constraints, alternatives where useful, and evidence. Its stable identity is independent of release number; recipe editions preserve guidance for particular target releases. Every discovery/detail call requires an explicit target release or deliberate `latest`, resolved numerically from the installed catalogue. Every response makes relevant runtime, SDK, language, package, platform, rendering and stability requirements visible.

Bundle all essential guidance and examples in the package. Author original explanations and wholly original examples; use the Practical .NET guide as inspiration and baseline coverage, never as copied example material. Generate served snippets from the same canonical samples used for verification. Content updates arrive through package updates.

Prepare .NET 11 GA guidance using current RC evidence. Include genuinely experimental APIs with explicit labels, including Blazor AI. At GA, reconcile final notes, breaking changes and actual artifacts, then correct or extend guidance and prepare an appropriate release. Verification records retain the actual RC or GA baseline used.

## User Stories

### Installation and agent use

1. As a developer, I want to install Playbook as a standard NuGet-distributed .NET tool, so that I can use the same package across my .NET projects.
2. As a developer, I want the command to remain `playbook`, so that setup instructions remain consistent as coverage grows.
3. As a developer, I want one package covering applicable .NET frameworks, so that I do not need a separate guidance server for each application type.
4. As a developer, I want installed guidance to work offline, so that network access is not required to consult a recipe.
5. As a developer, I want guidance requests to need no service account or API key, so that local setup remains straightforward.
6. As a developer, I want recipe updates through ordinary package updates, so that I control when installed guidance changes.
7. As an MCP client, I want to launch `playbook mcp` directly over stdio, so that I can manage the server through the normal local-process lifecycle.
8. As a developer, I want supported tool operation on Windows, Linux and macOS, so that I can use it across development environments.
9. As a developer, I want the tool host to run on .NET 10 and roll forward to .NET 11 when needed, so that consuming .NET 11 guidance does not require an unnecessarily restrictive host installation.
10. As a developer, I want the documentation host to require no desktop or mobile workloads, so that installing guidance does not install sample toolchains.
11. As a developer, I want help and version information from the command line, so that I can confirm installation and identify the installed package.
12. As a developer, I want bare invocation to display help, so that I can discover the MCP launch command.
13. As an MCP client, I want stdout reserved for protocol messages, so that diagnostics do not corrupt communication.
14. As a developer, I want Playbook to provide guidance while my coding agent owns project inspection and edits, so that the server remains predictable and read-only.

### Discovery, editions and complete responses

15. As a coding agent, I want concise recipe summaries before retrieving detail, so that I can select relevant guidance without loading the entire catalogue.
16. As a coding agent, I want optional area filters, so that I can focus on Blazor, WPF or another area without being forced to choose one.
17. As a coding agent, I want several areas to match any requested area, so that I can discover guidance spanning related application types.
18. As a coding agent, I want relevant shared language, library and SDK guidance included with framework filtering, so that a framework choice does not hide applicable cross-cutting improvements.
19. As a coding agent, I want to disable shared-area inclusion explicitly, so that I can request directly tagged framework guidance only.
20. As a coding agent, I want language filters to match demonstrated programming languages, so that an F# request does not unexpectedly return C#-only programming examples.
21. As a coding agent, I want applicable language-neutral configuration recipes included and labelled, so that a programming-language filter does not hide shared build or package configuration.
22. As a coding agent, I want deterministic keyword filtering, so that I can narrow discovery without relying on a hosted search or AI service.
23. As a coding agent, I want different filter types to combine consistently, so that explicit constraints remain meaningful.
24. As a coding agent, I want paged results in stable order, so that I can traverse a large catalogue reliably.
25. As a coding agent, I want discovery metadata to list supported targets, areas and example languages, so that I can form valid requests using the existing tools.
26. As a coding agent, I want to supply a target release on both tools, or deliberately request `latest` for exploratory/theoretical guidance or an unknown target, so that target selection is explicit and its resolved release is visible. Missing, null and blank targets fail helpfully. (PIN-971 supersedes the original omission default.)
27. As a coding agent, I want explicit target selection to preserve release-appropriate guidance, so that a newer package does not silently recommend an incompatible implementation.
28. As a coding agent, I want to distinguish curated .NET 10 coverage from comprehensive .NET 11 coverage, so that an empty result is not mistaken for an exhaustive older-release finding.
29. As a coding agent, I want stable goal-based recipe IDs and release-specific editions, so that references survive catalogue growth.
30. As a coding agent, I want documented and verified newer-package or SDK routes for older supported targets, so that useful conditional approaches are not hidden by introduction release alone.
31. As a coding agent, I want complete recipe content and applicability in the same structured response, so that client representation choices cannot separate essential constraints from instructions.
32. As an MCP client, I want an output schema and a complete JSON text fallback, so that standard structured and text result handling both retain the guidance.
33. As a coding agent, I want helpful errors for unknown IDs, unsupported targets and misspelled filters, so that I can correct a request without silent substitutions.
34. As a coding agent, I want valid zero-match combinations to return successful empty results, so that I can distinguish absence of matches from invalid input. Empty discovery explains the effective target/filters and incomplete coverage; it can suggest an explicit keyword-only retry preserving compatibility constraints, or an offset correction for an empty page beyond a nonzero total. It never retries or widens filters automatically. (PIN-975.)

### Content quality, applicability and provenance

35. As a developer, I want recipes organized around outcomes, so that I can select an implementation by the goal I am trying to achieve.
36. As a developer, I want original explanations and examples written from scratch, so that Playbook has its own useful teaching material.
37. As a developer, I want the older and newer approaches to solve the same realistic goal, so that their practical difference is clear.
38. As a developer, I want comparisons against the previous .NET release, so that the migration context is relevant and reproducible.
39. As a developer, I want an honest explanation when no older equivalent existed, so that examples do not invent misleading historical workarounds.
40. As a developer, I want valid older approaches described fairly, so that a newer option is not presented as proof that existing code is broken.
41. As a coding agent, I want explicit runtime, SDK, language, package and platform requirements, so that I can compare the recipe with the consumer's actual environment.
42. As a coding agent, I want framework, provider and rendering restrictions where relevant, so that I avoid applying a technique in the wrong application context.
43. As a developer, I want experimental status and required opt-ins disclosed in summaries and detail, so that I can judge stability before adopting an API.
44. As a developer, I want official references and exact verification baselines, so that I can assess the source and limits of a recipe.
45. As a contributor, I want served snippets extracted from canonical sample sources, so that documentation and verified code do not drift.
46. As a contributor, I want invalid metadata, missing snippets and incompatible edition requirements rejected during verification, so that incomplete content does not enter the package unnoticed.
47. As a maintainer, I want a source-backed disposition register, so that every qualifying release change has coverage, a justified grouping or exclusion, or a visible unresolved finding.

### Original 25 Blazor topics

48. As a Blazor developer, I want immediate client feedback for server-rendered forms using model-derived rules, so that I do not maintain duplicate browser validation logic.
49. As a Blazor developer, I want asynchronous form validation with appropriate progress and cancellation, so that remote checks finish before an edit is accepted.
50. As a Blazor developer, I want localized validation feedback shared with model rules, so that forms and endpoints present consistent messages.
51. As a Blazor developer, I want accessible labels associated with generated form inputs, so that reusable editors do not require fragile manual ID coordination.
52. As a Blazor developer, I want field captions drawn from model display metadata, so that names remain consistent across views.
53. As a Blazor developer, I want validation to cover nested objects and collections through supported generated metadata, so that invalid child values are not missed.
54. As a Blazor developer, I want one-use state after a static-rendered redirect, so that I can show an outcome without maintaining custom redirect storage.
55. As a Blazor developer, I want session-supplied state in static-rendered workflows, so that progress can continue across requests.
56. As a Blazor developer, I want relative navigation that respects the current route and application base, so that nearby links do not require hand-built prefixes.
57. As a Blazor developer, I want explicitly varied partial-page caching for static content, so that reusable sections can be cached without leaking user-specific output.
58. As a Blazor developer, I want to pass supported static fragments across render-mode boundaries, so that I can compose content without inventing an unsupported serialization route.
59. As a Blazor developer, I want environment-specific content through the supported component approach, so that deployment differences are expressed clearly.
60. As a Blazor developer, I want sorting and paging reflected in grid URLs, so that views survive refresh and can be shared.
61. As a Blazor developer, I want row interaction through the grid's supported event surface, so that I do not add unnecessary wrapper markup or event plumbing.
62. As a Blazor developer, I want explicit virtualized-list anchoring guidance, so that list changes preserve the intended visible position.
63. As a Blazor developer, I want guidance for variable-height virtualized content and removable workarounds, so that I can simplify authored list code where the new contract permits it.
64. As a Blazor developer, I want initial positioning and programmatic scrolling for virtualized lists, so that I can reveal a relevant item predictably.
65. As a Blazor developer, I want virtualization to work with a strict Content Security Policy, so that I can remove obsolete exceptions or workarounds when supported.
66. As a Blazor developer, I want automatic pause behavior for hidden interactive-server tabs configured appropriately, so that circuit lifecycle matches the application.
67. As a Blazor developer, I want server-directed circuit pause and continuation guidance, so that the server can manage expensive interactive sessions deliberately.
68. As a Blazor developer, I want browser and reconnection options configured through supported C# APIs, so that startup settings use the intended contract.
69. As a Blazor developer, I want browser-hosted services registered with their real lifecycle constraints, so that background client work starts and stops correctly.
70. As a Blazor developer, I want server-provided browser configuration with clear boundaries, so that client environment settings are supplied without exposing secrets.
71. As a Blazor developer, I want server culture transferred or deliberately overridden for browser execution, so that formatting remains consistent through hydration.
72. As a Blazor developer, I want the .NET Web Worker template explained through an original task, so that computation can move away from rendering.

### Seven additions, expansions and accepted migrations

73. As a Blazor developer, I want framework-managed base-path markup, so that hosted application paths are configured without brittle manual duplication.
74. As a Blazor developer, I want fragment links built through the supported navigation helper, so that URI construction follows the framework's rules.
75. As a Blazor library author, I want supported custom population of injected component properties, so that component activation does not depend on private framework internals.
76. As a Blazor developer, I want policy and requirement metadata applied consistently in authorization views and route views, so that the UI uses the intended authorization rules.
77. As a Blazor developer, I want C# unions used correctly in component parameters, JavaScript interop and persisted state, so that these boundaries retain their intended type information.
78. As a Blazor developer, I want a standalone WebAssembly client hosted with gateway routes and service defaults, so that backend access, discovery, resilience and telemetry use the supported integration.
79. As a Blazor developer, I want experimental agent UI guidance covering streamed conversations, tool presentation, approval continuation and shared or predictive state, so that I can build those interfaces with stability limitations visible.
80. As a Blazor developer, I want grid-specific positioning, scrolling, anchoring and comparison guidance, so that QuickGrid virtualization is not confused with list-only examples.
81. As a Blazor developer, I want interactive-server connection transport, authentication-expiry and buffer configuration explained, so that session transport is configured deliberately.
82. As a Blazor developer, I want worker creation and invocation cancellation, timeout and no-result calls covered, so that worker lifecycle and results are handled correctly.
83. As a web developer, I want an original CSRF migration explaining changed startup and form code alongside retained protections, so that simplification does not remove necessary defenses.
84. As a Blazor developer, I want inline navigation handlers replaced with an original external-module approach consistent with PinguApps style, so that navigation supports the intended CSP policy.

### Eight .NET 10 foundations

85. As a C# developer, I want to add computed property-like behavior to a type I cannot edit, so that call sites can use supported extension-member authoring.
86. As a C# developer, I want to validate or normalize property assignments without a manually maintained backing field, so that accessor code is simpler.
87. As a Minimal API developer, I want synchronous model validation before handler execution, so that I can remove repeated input checks while retaining the .NET 10 baseline.
88. As an EF developer, I want to include archived records while retaining a tenant query filter, so that selective filter changes do not disable every query constraint.
89. As a developer, I want outer joins that retain unmatched rows, so that I do not repeat manual left/right join patterns.
90. As a developer, I want strict JSON contract checks at an import boundary, so that structural input validation is distinct from business validation.
91. As a developer, I want to filter and enrich asynchronous streams without buffering them first, so that cancellation and streaming behavior remain explicit.
92. As a C# developer, I want a small repository utility without a throwaway project, so that file-based application support can replace unnecessary scaffolding.

### Wider .NET 11 release-family coverage

93. As a C# developer, I want qualifying C# 15 language changes explained with actual compiler and runtime requirements, so that I adopt supported language constructs.
94. As an F# developer, I want qualifying F# 11 features and migrations demonstrated in F#, so that guidance reflects shipped language and library contracts.
95. As a Visual Basic developer, I want qualifying language, compiler and framework authoring changes covered, so that VB-specific work is not omitted or invented from a presumed new language version.
96. As a developer, I want improved process execution and cancellation APIs connected to realistic utilities, so that I can replace custom child-process plumbing where appropriate.
97. As a developer, I want new JSON, union and streaming serialization capabilities explained with their actual asset and AOT restrictions, so that data contracts behave as intended.
98. As a developer, I want new LINQ and asynchronous collection operations mapped to data-processing goals, so that I can remove unnecessary join or iteration code.
99. As a developer, I want qualifying archive, stream, buffer, text and I/O changes covered, so that I can use new APIs or address concrete compatibility changes.
100. As a developer, I want HTTP, DNS, networking and compression authoring changes covered, so that I can implement supported client and transport behavior.
101. As a developer, I want qualifying cryptography, security, numerics and interoperability changes explained with platform constraints, so that I avoid unsupported or misleading substitutions.
102. As a service developer, I want qualifying configuration, hosting and Extensions changes connected to authored code, so that I can update service behavior rather than read unrelated optimization headlines.
103. As a developer, I want qualifying runtime-facing APIs and explicit compilation or deployment choices explained, so that I can make implementation decisions with their tradeoffs visible.
104. As a maintainer, I want SDK, project, testing-runner and MSBuild changes connected to build goals, so that configuration migrations are covered alongside application APIs.
105. As a package author or consumer, I want qualifying NuGet configuration and dependency-management changes covered, so that I can update package workflows using supported options.
106. As a developer, I want qualifying container publishing and deployment-artifact changes covered, so that I can author the required deployment configuration correctly.
107. As a Minimal API developer, I want new validation, binding and response-authoring capabilities covered, so that endpoint code uses supported .NET 11 patterns.
108. As an MVC or Razor Pages developer, I want qualifying framework and migration guidance covered, so that non-Blazor server-rendered applications benefit from Playbook.
109. As a web developer, I want new authentication and authorization authoring requirements explained with their real browser and stability constraints, so that security integration is based on available APIs.
110. As a SignalR developer, I want qualifying cancellation and hub-call contracts covered, so that client/server work has the intended lifecycle.
111. As an API developer, I want qualifying OpenAPI and response-description changes covered, so that generated contracts match endpoint behavior.
112. As a web developer, I want qualifying hosting, caching and HTTP configuration changes covered, so that middleware and endpoint integration remain correct.
113. As a developer, I want gRPC and other release-family areas reviewed even when they yield no distinct recipe, so that absence of a recipe is a documented audit outcome.
114. As an EF developer, I want new model, complex-type and relationship capabilities connected to modelling goals, so that schema and application structure use the supported contracts.
115. As an EF developer, I want query translation and provider-specific limitations covered, so that a supported in-memory operation is not assumed to translate everywhere.
116. As a SQL Server developer, I want qualifying JSON, full-text and vector authoring guidance with database prerequisites, so that features are used against compatible provider and server versions.
117. As a Cosmos developer, I want qualifying data access and persistence migrations explained, so that source changes and stored-document consequences are considered together.
118. As an EF maintainer, I want migration tooling, design-time and dependency changes covered, so that generated artifacts and commands use the intended build inputs.
119. As a WPF developer, I want concrete compatibility and authoring changes covered without invented features, so that guidance remains useful even when a release has few qualifying changes.
120. As a WinForms developer, I want qualifying rendering, layout, custom-control and presentation APIs explained, so that I can replace manual UI plumbing where appropriate.
121. As a Visual Basic WinForms developer, I want Application Framework and Designer-specific authoring demonstrated in VB, so that C# startup examples do not misrepresent my application model.
122. As a MAUI developer, I want qualifying XAML, source-generation, UI, navigation and integration changes covered, so that cross-platform authoring uses supported .NET 11 patterns.
123. As an Android developer, I want qualifying workload, manifest, build and compatibility migrations covered, so that native application configuration is correct.
124. As an Apple-platform developer, I want qualifying lifecycle, deployment, architecture, manifest and workload migrations covered, so that native applications use the required platform toolchain correctly.
125. As a MAUI developer, I want first-party template integrations to identify required external dependencies, so that integration guidance does not silently become unrelated third-party framework coverage.

### Verification, contribution and client setup

126. As a maintainer, I want focused behavior scenarios organized using PinguApps test conventions, so that verification checks meaningful outcomes rather than internal methods.
127. As a maintainer, I want installation and real MCP calls tested against the packed tool, so that source-project tests cannot hide packaging failures.
128. As a maintainer, I want actual .NET 10 hosting and .NET 11 roll-forward checked, so that runtime compatibility is established through delivered artifacts.
129. As a maintainer, I want one brief packed-tool smoke on Windows and one on macOS, so that basic platform confidence does not require a broad native test matrix.
130. As a contributor, I want locally testable original examples to compile against their declared targets and dependencies, with explicit exclusions for unavailable native platforms, so that evidence remains honest.
131. As a native-content contributor, I want feasible authoring checks on the current Windows machine and existing Docker, with unavailable native checks explicitly labelled untested, so that additional machines and hardware are not prerequisites.
132. As a maintainer, I want native compilation evidence refreshed when relevant inputs change, so that unchanged examples can reuse proof without treating stale builds as current.
133. As a developer, I want source review, compilation and runtime execution distinguished, so that a successful build is not presented as proof of native behavior.
134. As a maintainer, I want focused real behavior checks where claims require them, so that mocks or generated SQL alone are not misreported as integration evidence.
135. As a developer, I want T3 Code, Codex CLI, Claude Code and VS Code Copilot setup documented and checked on actual versions, so that I can connect an agent with reliable instructions.
136. As a maintainer, I want one real check per named client rather than a client-by-OS matrix, so that compatibility evidence stays proportionate.
137. As a developer, I want a companion instruction that tells my agent when to consult Playbook, so that available guidance is more likely to be used.
138. As a contributor, I want contribution and evidence requirements documented, so that new recipes meet the same originality, applicability and verification standards.
139. As a developer, I want offline-complete recipes and installation troubleshooting, so that missing repository files or external implementation steps do not prevent use.

### Repository, publishing and release stewardship

140. As a maintainer, I want PinguApps structure, analyzers, formatting and centralized package management, so that this repository fits the existing portfolio.
141. As a maintainer, I want the established six GitHub Actions roles reused, so that validation, labels, drafts and publishing behave consistently with other packages.
142. As a maintainer, I want modern NuGet OIDC trusted publishing, so that package publication follows the accepted PinguApps reference.
143. As a maintainer, I want the package tested and published without repacking, so that the released artifact is the artifact whose behavior was verified.
144. As a maintainer, I want package SemVer independent of .NET target versions, so that content and contract changes have meaningful package versions.
145. As a maintainer, I want manual pre-GA preview releases with their RC evidence disclosed, so that early publication does not imply GA verification.
146. As a maintainer, I want stable publication to wait for GA reconciliation, so that final release guidance reflects the shipped framework.
147. As a maintainer, I want weekly stable publishing enabled only after the first manual stable release, so that a preview cannot unlock the stable release channel accidentally.
148. As a maintainer, I want Sunday 18:17 UTC publication rechecked against PinguApps schedules before activation, so that the chosen slot remains separated from other package publishers.
149. As a maintainer, I want the standard any-new-main-commit weekly behavior, so that docs, CI, tests and maintenance changes follow the same release flow as other packages.
150. As a maintainer, I want the existing GitHub repository identity retained, so that package setup does not require an unnecessary repository rename.
151. As a project owner, I want release readiness and human-owned launch actions made explicit, so that credentials, environment access and publication are not assumed to have happened.

### Bounded product evaluation and honest completion

152. As a project owner, I want an original C# outcome-modelling task compared with and without Playbook, so that I can observe whether guidance improves a language implementation.
153. As a project owner, I want an original Minimal API asynchronous-validation task compared with and without Playbook, so that I can observe non-Blazor web value.
154. As a project owner, I want an original static-Blazor-grid link task compared with and without Playbook, so that I can observe correct use of the accepted grid guidance.
155. As a project owner, I want an original non-web manifest-reconciliation task compared with and without Playbook, so that I can observe value outside web UI.
156. As a project owner, I want eight fresh runs on one fixed client/model with equal budgets and recorded outcomes, so that this remains a controlled manual launch exercise.
157. As a project owner, I want consultation, correctness, applicability and newer-API adoption assessed separately, so that using the tool is not mistaken for solving the task.
158. As a maintainer, I want unresolved source and API findings kept visible until resolved, so that comprehensive coverage is not claimed from a headline-only inventory.
159. As a maintainer, I want source attribution and dependency requirements preserved, so that reference reuse and sample prerequisites remain traceable.
160. As a project owner, I want the final report to state what actually ran and what remains unverified, so that release decisions rest on evidence.

## Implementation Decisions

### Product boundary and release coverage

- Playbook is a deterministic, read-only documentation server. The calling agent owns natural-language interpretation, project inspection, code changes and builds. No consumer-project analysis or migration execution belongs in the server.
- Comprehensive initial coverage is the .NET 11 release family: languages, runtime-facing APIs, shared libraries, SDK/build/package tooling, ASP.NET Core, EF Core, WPF, WinForms and MAUI/platform workloads. Include authored markup, scripts, project configuration and manifests where they change an implementation.
- Include performance-oriented APIs when adoption changes authored code. Exclude invisible runtime/library speedups and automatic fixes with no concrete authoring or workaround-removal decision. Do not invent a recipe to populate an area with no qualifying change.
- Preserve all 25 original Blazor topics, the seven explicit additions, three expansions, and accepted CSRF and navigation/CSP migrations. The additions include BasePath, fragment URI construction, component-property activation, expanded authorization metadata, C# unions, the combined WebAssembly Gateway/service-defaults integration, and experimental Blazor AI.
- The historical .NET 10 backfill is exactly the eight approved foundation goals. Query filters are not authorization or write protection; strict JSON structure is not business validation; the validation foundation is synchronous, with asynchronous integration in the .NET 11 edition. Preserve these distinctions in the examples.
- Document independent libraries and native APIs when required by a qualifying recipe. First-party MAUI template integration may explain its Avalonia dependency; standalone Avalonia or unrelated independently versioned product feature coverage is outside scope.
- Target GA guidance using current RC evidence. APIs deliberately experimental after GA remain included and visibly labelled. Do not preserve a temporary preview API as the final approach when canonical later evidence removes or replaces it.
- Maintain one evolving package across yearly releases, retaining release-specific recipe editions. Routine implementation and evidence work must not silently narrow the accepted comprehensive coverage.

### Tool host, CLI and internal modules

- Retain GitHub `PinguApps/Playbook`, NuGet/project/solution identity `PinguApps.Playbook`, and CLI command `playbook`.
- Use one production console/tool project with internal catalogue loading, edition resolution, discovery and retrieval modules. Use one focused Reqnroll core test project and independently buildable original sample projects. Do not publish a separate core library.
- Host on `net10.0` with `Major` roll-forward: prefer .NET 10 when available and permit a higher major when required. Verify actual operation on .NET 10 and .NET 11. Sample SDKs, workloads and native toolchains are independently pinned.
- Start MCP directly over stdio as a client-owned process. Do not inherit Roslynk's persistent HTTP daemon, bridge, ports, workspace state or service lifecycle.
- Public CLI behavior is MCP startup, help and version; bare invocation shows help. MCP stdout contains protocol traffic only, with diagnostics on stderr. Client registration is documented rather than edited automatically.
- Use the standard C# MCP SDK registration, descriptions, schemas, structured content and read-only annotations. Select exact supported dependency pins during implementation and verify the resulting artifact; previously researched versions are not evidence of the delivered package.

### Discovery and release resolution

- Expose exactly `list_recipes` and `get_recipe` initially. Both require `targetVersion`; discovery also accepts optional `areas`, `languages`, `query`, `includeShared`, `offset` and `limit`; detail requires the stable recipe ID.
- Accept supported release aliases or explicit `latest`, resolved numerically to the newest target release represented in the installed catalogue. Missing, null and blank targets fail helpfully. Echo `requestedTarget`, `resolvedTarget` and retained `targetAssumed: false`; no successful target is assumed. Calls are independent and never inherit a discovery target.
- Tool/server and companion instructions tell agents to inspect the application's target framework and relevant project/`global.json` context and repeat the selected release on both tools and every page. The target is not an exact SDK version. Use `latest` deliberately for exploratory/theoretical requests or an unknown target; it uses no network or installed SDK inventory and does not establish consumer compatibility. Check detailed prerequisites before applying guidance.
- Initially recognize .NET 10 and .NET 11 consumer releases. Disclose curated versus comprehensive coverage. .NET 9 comparison examples do not create .NET 9 catalogue support.
- Normalize the agreed major, major/minor and unqualified framework aliases to canonical releases. Reject unsupported patch/RC strings or platform-qualified target inputs helpfully rather than silently discarding explicit constraints. Recipe metadata retains actual platform and artifact requirements.
- Empty or omitted areas means all areas. Multiple areas match any requested area; multiple requested example languages similarly match any requested language. Different filter types combine with each other.
- Match demonstrated programming languages, with an explicit exception for genuinely language-neutral build/configuration recipes applicable to the requested language. Record broader API language applicability separately. Do not relabel language-specific directives or project rules as neutral.
- `includeShared` defaults true and adds relevant shared language/library/SDK guidance through explicit metadata. False limits area matching to directly tagged areas. Without area filtering, all areas remain eligible. Deduplicate results and retain every other applicable filter; shared inclusion does not admit unrelated platform guidance.
- Match nonblank whitespace-separated query terms case-insensitively against titles, goals, tags and API names, requiring all terms. No AI matching, embeddings or semantic search service is needed.
- Default page size is 20 with a maximum of 50. Use stable ordinal recipe-ID ordering, total matches and a next offset. Reject negative offsets and invalid limits rather than changing them silently.
- Discovery returns concise summaries with identity, goal, areas and essential applicability/stability, plus supported target/area/example-language metadata. Full implementations belong in detail, not discovery summaries.
- Unknown IDs, unsupported targets, unknown filter values and known recipes without an applicable edition return helpful machine-readable failures with supported values or available editions where useful. Valid zero-match combinations succeed empty. Never relax explicit filters or targets to manufacture a result.

### Recipe schema and offline content pipeline

- A recipe represents one developer goal and can combine several APIs. Stable IDs omit release numbers; editions retain release-specific guidance. Prefer a newer compatible edition when it supersedes an older approach, without returning incompatible guidance for an explicit older target.
- Separate introduction release from runtime, SDK, language mode, framework, package, provider, platform and rendering prerequisites. Include documented and verified newer-package or SDK routes for older supported targets conditionally, without assuming those prerequisites are installed. Compilation of unsupported language/runtime combinations does not establish supported compatibility.
- Detail is one complete object containing identity, resolved target, applicability/prerequisites, API stability, actual evidence, source references and the full Markdown body. Use an output schema and serialize that same complete object as the JSON text fallback; do not split essential metadata and prose between mutually selected representations.
- Narrative content covers the goal, older/newer implementation, migration, constraints and alternatives where useful. Normal comparisons are .NET 11 against .NET 10, and the eight .NET 10 foundations against .NET 9. Explain unavailable older equivalents and valid older approaches honestly.
- Author JSON metadata and Markdown, drawing snippets from canonical independently authored sample sources. Generate validated discovery data and rendered guidance during build/pack, then embed the results in the tool assembly. Installed use must not depend on the repository, working directory, sidecar content database or network.
- Recipes include the essential code, configuration and instructions offline. References provide provenance and optional further reading, not missing implementation steps. The tool does not bundle every consumer dependency or promise that external services can themselves be used offline.
- Use C# as the canonical language for shared framework/library examples, F# for F#-specific capabilities and VB for qualifying VB-specific changes. Add translations where authoring materially differs rather than maintaining three versions of every example.
- Keep an official-source register with introduction/applicability, authoring impact and included, grouped, excluded or unresolved dispositions. Shared APIs should have canonical explanations and source, with distinct framework integrations linked rather than copied.
- Include experimental labels in discovery and detail, with actual opt-ins, package versions and stability limits. Record source review, compile evidence and runtime execution separately, including the exact artifacts and relevant platforms used.

### Repository conventions and release infrastructure

- Apply CodeStyle.Analyzers structure/shared build conventions, Template-generated application style/analyzer consumption, BlazorSitemap modern publishing/packed-consumer verification, RazorStyle .NET-tool packaging, Upstash behavior-scenario organization, and Roslynk MCP mechanics. Adapt identities and domain-specific infrastructure instead of copying them blindly.
- Use centralized package management, versionless package references, disabled per-package version overrides, shared MSBuild configuration, nullable analysis, deterministic builds, warnings as errors and the selected PinguApps analyzers/formatting. Keep the production host separate from sample toolchain requirements.
- Match current PinguApps authored-code conventions, including explicit types, file-scoped namespaces and the selected analyzer rules. Adapt framework template examples to the approved external JavaScript organization rather than copying a prohibited template naming pattern. Preserve necessary reference-source attribution if substantial code is reused.
- Reuse the six workflow roles: reusable validation, PR validation, PR labels, release drafting, package publishing, and weekly draft publication. Retain SHA-pinned actions, Blacksmith/Linux baseline, release labels/categories, artifact reporting and NuGet OIDC trusted publishing.
- Pack once, verify the real package, pass that exact artifact to required platform checks, and publish it without repacking. Use isolated local-feed/cache verification to establish that the installed package is the newly built artifact.
- Package SemVer is independent of .NET coverage: first stable `1.0.0`, manual `1.0.0-preview.N` before GA if publishing early, patch for corrections, minor for compatible recipe/capability additions, major for incompatible public tool/contract or stable-ID changes. Retain the reference Release Drafter version mechanism.
- First public publication, including a preview, requires agreed comprehensive coverage plus the eight foundations, locally required compilation and focused checks (with explicit native-platform exclusions under the testing policy), packed-tool/client evidence, and the bounded manual evaluation. A preview discloses RC evidence and remaining GA uncertainty; it does not waive these gates. Stable additionally requires GA reconciliation.
- Weekly stable draft publication starts only after the first manual stable release. Require an existing published stable release and a suitable stable draft, checking version syntax and GitHub prerelease flags so a preview cannot unlock the channel.
- Use Sunday 18:17 UTC, cron `17 18 * * 0`, subject to a fresh PinguApps organization-only schedule check before activation. Retain standard any-new-main-commit behavior, including docs, CI, tests and maintenance. Do not add shipped-path, package-difference or empty-release-note guards. Keep reference no-new-commits/no-draft and stable-tag safeguards.
- Merging and manual releasing remain Pingu's responsibility unless explicitly requested. Accepted future weekly automation does not authorize publication, account configuration or schedule activation while producing this specification.

## Testing Decisions

1. **Use the delivered package as the primary seam.** The highest product boundary is an installed .NET tool reached through real stdio MCP. Install the actual package using an isolated local feed/cache, initialize the server, discover tools, invoke list/get, assert meaningful complete results, and shut down. Verify operation from an unrelated working directory. This seam tests composition, bundled assets, packaging and transport together.
2. **Keep tests focused on external behavior.** Assert required target schemas and omitted/null/blank rejection, deliberate catalogue-only `latest` resolution, independent list/get target safety, edition choice, combined filters, shared/neutral rules, stable paging, helpful failures and valid empty results. Do not mirror implementation methods, invent a broad edge-case suite or add a separate published library for test access.
3. **Use the accepted core scenario conventions.** One Reqnroll-based test project follows PinguApps behavior-scenario organization and current compatible test tooling. The repository currently has planning documents rather than an implemented test suite; exact runner/package pins are implementation work.
4. **Prefer relevant prior art.** BlazorSitemap demonstrates isolated packed-consumer verification and publishing those same artifacts. Upstash provides behavior-scenario organization. Roslynk demonstrates MCP registration/schema/tool invocation checks but its in-process NullTransport tests do not establish a packed stdio lifecycle. RazorStyle demonstrates tool packaging rather than proof of Playbook's delivered MCP boundary.
5. **Verify real host compatibility.** Exercise the packed tool on .NET 10 and with .NET 11 roll-forward in appropriately isolated runtime environments. Inspect actual behavior rather than inferring compatibility from project settings.
6. **Keep Windows/macOS checks tiny.** Each OS has one brief actual-package install/start/initialize/list/get/shutdown smoke checking both content and applicability. Linux remains the focused core/portable lane. Do not expand native builds, UI, devices or emulators into these jobs or move a broad native matrix into another workflow.
7. **Verify content through its natural artifact boundary.** Validate metadata and referenced sources, IDs/editions, applicability, snippet references and generated content. Essential served snippets must come from the canonical samples, and malformed/missing content must prevent a successful verified package. Compilation of those samples is the evidence seam for code validity; it is not a new consumer-facing module.
8. **Require actual compilation for locally testable examples.** Compile older/newer examples against their stated targets and SDK/package/workload/native baselines. Apply the explicit sample exclusions in the [native testing policy](docs/contributing.md#native-testing-policy) for unavailable platforms; keep their official source review and untested disclosure. Other missing/stale compilation still blocks publication. Do not describe an emulation as an older-runtime build or assume all APIs in a package exist in every target asset.
9. **Use the agreed native testing boundary.** Pingu revised this decision through PIN-699 on 30 October 2026: author on the current Windows machine and existing Docker; retain package CI. Mac/Xcode, Android devices/emulators and other unavailable native hardware are not prerequisites. Record exact sample/source, target, dependency and toolchain evidence for actual checks, and clearly separate excluded, source-only and executed work. Pingu owns feasible interactive approvals and refresh coordination. Reuse evidence only while relevant inputs and conditions remain valid.
10. **Match runtime evidence to claims.** Use focused real request, browser, database or platform checks when a recipe's assertion depends on behavior. Distinguish source review, compilation, cross-compilation, mocks and actual execution. A tool smoke does not establish native recipe behavior, and a query representation alone does not establish execution against a database.
11. **Check each named client once.** T3 Code, Codex CLI, Claude Code and VS Code Copilot each receive one brief real check on an available supported OS. Record versions and verify that the agent receives both Markdown body and applicability. Do not create a four-client-by-three-OS matrix.
12. **Use the agreed manual product exercise.** Prepare four small original scaffolds: C# outcome modelling, Minimal API asynchronous reservation validation, independent static-Blazor-grid links preserving a filter, and non-web manifest reconciliation preserving unmatched entries. Freeze behavior and API applicability against actual artifacts before either condition runs.
13. **Control that exercise's scope.** One with/without pair per task gives eight fresh runs on one fixed client/model/version, at most ten execution minutes each and 80 total execution minutes excluding preparation/review. Keep scaffold commits, SDKs/packages, tools, network policy and budgets equal; only Playbook availability and its companion instruction differ. Alternate condition order, isolate contexts, and retain failures/timeouts without corrective retries or a model matrix.
14. **Review results honestly.** Record consultation, correctness, applicability, newer-API adoption, unnecessary code, elapsed time and token usage where available. A correct older approach can pass behavior. Repair Playbook defects before launch without requiring it to win every comparison or claiming broad agent knowledge from four tasks.
15. **Verify release artifacts and evidence freshness.** All required package lanes consume the same packed artifact that is published. Recheck changed evidence inputs and GA source/API differences before claiming readiness. Record checks actually run, versions and gaps; no compilation, client integration, native behavior or package publication is established by this specification.

## Out of Scope

- Invisible runtime, JIT, GC or library performance benefits and automatic fixes with no changed authoring or removable workaround.
- A complete .NET 10 historical backfill, .NET 9 catalogue support, or automatic coverage of future releases without evidence and release editions.
- Standalone coverage of unrelated independently versioned products, third-party framework feature catalogues, or exhaustive native OS APIs beyond required recipe dependencies.
- Embedded AI models, semantic/vector search infrastructure, hosted documentation services, a persistent HTTP daemon, shared workspaces or project-analysis services.
- Editing consumer projects, executing migrations, building consumer applications, inspecting installed consumer dependencies, or automatically rewriting client configuration from the server.
- Runtime network content retrieval, telemetry, service API keys, background content updating or a separate catalogue database.
- Additional initial MCP tools, a separate human recipe browser, or a broad command-line feature set beyond MCP/help/version.
- Copied or lightly renamed guide demonstrations, code or explanations, invented unavailable APIs, and claims that all older patterns are broken.
- Broad native UI/device/emulator CI, routine native-build matrices, client-by-OS matrices, recurring agent benchmarks or an additional paid benchmark platform.
- Shipped-path, package-difference or empty-release-note guards on standard weekly publishing.
- Dropping qualifying areas to meet an arbitrary recipe count, representing 310 inventory candidates as 310 verified recipes, or treating every story/inventory row as a mandatory separate ticket.
- Account provisioning, secrets collection, external issue creation, Linear destination selection, schedule activation, merging, releasing or tool implementation as part of writing this spec.

## Further Notes

### Authority and provenance

This file is the single primary source for the intended outcome in the future to-linear run. It synthesizes the confirmed scope and decisions rather than reopening the interview. The agreed testing seams have already been checked with Pingu; no new seam confirmation is required to write this file.

Supporting context explains terminology, references and evidence. It does not add unapproved standalone scope. The initial register contains 406 source-disposition rows: 310 included candidates, 44 grouped rows, 29 exclusions and 23 unresolved findings. Those counts are planning evidence, not a finalized recipe or ticket count. Resolve factual availability, compatibility and grouping during delivery before claiming comprehensive coverage.

- [Confirmed plan](docs/planning/README.md) and [complete decision history](docs/planning/brief.md).
- [Domain glossary](GLOSSARY.md) and decisions on [one evolving package](docs/adr/0001-one-playbook-across-dotnet-releases.md), [read-only guidance](docs/adr/0002-read-only-guidance-server.md), [release-family scope](docs/adr/0003-cover-author-facing-changes-across-dotnet.md), and [GA guidance from RC evidence](docs/adr/0004-target-ga-guidance-using-rc-evidence.md).
- [Detailed MCP contract](docs/planning/tool-contract.md), [coverage criteria and exact .NET 10 goals](docs/planning/coverage.md), and [source-disposition inventory](docs/planning/inventory/README.md).
- [Language/library/SDK inventory](docs/planning/inventory/01-languages-libraries-sdk.md), [web/data inventory with all original baseline mappings](docs/planning/inventory/02-web-data.md), and [desktop/mobile inventory](docs/planning/inventory/03-desktop-mobile.md).
- [Reference conventions](docs/planning/reference-repositories.md), [verification boundaries](docs/planning/verification-plan.md), [evaluation controls](docs/planning/evaluation-plan.md), [release policy](docs/planning/release-plan.md), and [organization schedule evidence](docs/planning/publishing-schedule.md).
- [Original reference conversation in T3 Code](t3-thread://v1/372736c9-d15c-463f-a2ea-5c5f880eef79) (requires T3 Code and access to that conversation; unavailable to public GitHub readers) and [Practical .NET guide](https://lab.practicaldotnet.io/guide), used as context and inspiration rather than reusable example material. The accepted decisions are recorded in this specification and the [planning brief](docs/planning/brief.md).

Later confirmed decisions supersede historical proposals: retain the actual GitHub identity stated here despite the earliest ADR's broader repository naming wording; include experimental APIs; require explicit targets or deliberate `latest` on both tools (PIN-971, 18 October 2026, superseding the omitted-target default); and preserve standard weekly publishing rather than the rejected releasable-change guard. Requiring the previously optional target is a breaking public contract change under the existing major-version policy.

### Delivery and later Linear slicing

The current repository contains the MIT license, glossary, ADRs and planning research; it does not yet contain the production tool, samples, tests or workflows. The appropriate first outcome is an installable vertical slice serving an original validated recipe over real MCP. Expand through complete goal slices that include necessary source reconciliation, original sample, metadata/prose, offline packaging and focused evidence. Independent recipe families can proceed concurrently once their actual prerequisites exist; do not turn earlier phase order into invented blanket dependencies.

Treat remaining API/source findings as concrete research and verification work. In particular, reconcile stale cumulative versus canonical RC notes, removed APIs, experimental status, exact package assets and native workload composition. An experimental designation permits inclusion with labels; an unavailable removed API does not become a usable feature because an older page advertises it. Reconcile final GA notes and artifacts before stable publication.

Routine identifiers, DTO shapes, exact supported SDK/package/workload/action pins and fictional example domains are implementation choices within this scope. Unavailable native machines are excluded under the testing policy; their guidance remains explicitly untested. Isolated provider verification resources where behavior requires them, actual client availability and trusted-publishing configuration remain prerequisites for their corresponding required checks or launch actions. Do not assume access, provision resources, or report execution merely from this document.

For the later to-linear invocation, explicitly select this file as the primary source. That skill must confirm the team/project, inspect existing milestones/issues and estimate settings, then propose narrow PR-sized vertical tickets with real dependencies and separately identified human-owned work. Credentials, physical/native environment actions, owner judgement and release approvals must not receive an `Agent` label simply because this is an implementation spec. Review the full ticket proposal before any Linear mutations; do not create tickets, labels, estimates, dates or milestone gates from this file alone.

Preserve this spec as source provenance in later tickets, with the relevant section or numbered stories. One ticket may satisfy several stories; a broad coverage story may require several independently verifiable slices. The full publication gate still applies even when delivery proceeds incrementally. Ticket granularity, overlaps, native milestone assignments and the execution frontier belong to the later approved backlog proposal, not to this synthesis.
