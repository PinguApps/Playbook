# PinguApps.Playbook planning brief

Status: discovery and planning. Rounds 1–7 are recorded, targeting the .NET 11 full release and eight selected .NET 10 foundations. RCs supply preparation evidence; GA reconciliation is required. Experimental APIs are included with clear labels. Product, contract, layout, content delivery, release policy/identity, minimal platform/client verification, and bounded manual evaluation are accepted. Weekly release behavior follows Pingu's references without a shipped-path or empty-notes guard. The concrete source/recipe inventory and final contract details remain to finish. Implementation awaits the completed design and Pingu's confirmation of shared understanding.

This brief records the knowledge for the grill-with-docs session, initially captured on 15 October 2026 and updated through Round 7 on 16 October. It separates Pingu's requirements, earlier assistant proposals, reference observations, and decisions still to make.

## Sources

- [Complete reference conversation: Check Guide Against .NET 11 Blazor](t3-thread://v1/372736c9-d15c-463f-a2ea-5c5f880eef79), including the final naming decision. All user and assistant messages were read.
- [Practical .NET field guide](https://lab.practicaldotnet.io/guide). The current index contains 25 topics. Each has problem, demonstration, and implementation stages.
- The reference thread's local research snapshots: `C:\Users\pingu\.t3\scratch\2026-10-15-can-you-please-take-a-880eef79\guide-chapters.json`, `guide-stages.json`, and `release-notes.json`. These preserve 25 chapter pages, 50 demonstration/implementation pages, and the release notes retrieved in that thread. They are research evidence, not committed product content.
- Pingu's current instructions and seven local reference repositories. See [repository findings](reference-repositories.md).

The live index was retrieved successfully over HTTP. Browser inspection twice failed with `Preview automation snapshot timed out after 15000ms.` The browser's interactive demonstrations have not been exercised in this session. Current index links use `/learn/{slug}`; the earlier snapshots use `/guide/{slug}`. Preserve topic identities separately from source URLs.

## Requirements already supplied

### Product and identity

- Help coding agents use .NET 11 capabilities across the .NET ecosystem that their existing knowledge may miss. The original Blazor-only scope has been superseded.
- Explain the developer's goal, the older approach, and the .NET 11 approach.
- Cover everything in .NET 11 that changes the code developers write or how they implement a goal, including Minimal APIs, other ASP.NET Core applications, MVC, Razor Pages, desktop, and mobile. Languages, shared libraries, and data access also belong in the release-wide inventory. Benefits obtained solely from upgrading, with no different authoring approach, remain outside scope.
- Distribute through NuGet as a locally installed .NET tool exposing MCP.
- Project, repository, and package identity: `PinguApps.Playbook`.
- Executable command: `playbook`, without a PinguApps prefix or year suffix.
- Expand the same package in future years. Separate .NET 11/.NET 12 packages and commands were superseded by this decision.
- Use the existing PinguApps repository structure, coding conventions, and infrastructure. Central package management is explicit.
- Closely match PinguApps GitHub Actions, particularly NuGet publishing.
- Use Roslynk as the practical .NET-tool/MCP reference, while keeping PinguApps style and infrastructure.
- Plan the whole package before implementation. Build the shared understanding through grill-with-docs; maintain domain language and record consequential decisions as they settle.

The checkout is currently `V:\Playbook`, on the assigned `feature/planning` branch, with origin `https://github.com/PinguApps/Playbook.git`. The requested repository identity differs from that existing remote slug. Reconcile this during repository setup; no remote rename is part of discovery.

### Direction established in the earlier discussion

The earlier assistant proposed a deterministic documentation server. The calling agent interprets natural language; Playbook need not host an AI model or natural-language processing service. Pingu accepted the direction before moving on to naming.

That proposal used a compact catalogue followed by detailed retrieval, illustrated as `list_features()` and `get_feature(id)`. It bundled metadata and Markdown with the tool and used stdio for local MCP. Search, embeddings, and a natural-language endpoint were considered unnecessary for the initial catalogue. `playbook mcp` was the suggested launch command. These are starting proposals, not a settled public API, storage schema, transport architecture, or complete CLI specification.

Suggested entry content was: goal, applicability, older approach, .NET 11 approach, migration instructions, constraints, and evidence. Suggested applicability included framework version, render mode, and required packages. Suggested evidence included an official source and the version actually verified.

The earlier discussion also identified agent discovery as a product risk: useful guidance has no effect if agents never consult it. Good tool descriptions and companion instructions were proposed. An evaluation comparing realistic tasks with and without Playbook was proposed, but no launch benchmark or acceptance threshold was agreed.

## Content to carry into planning

The original Blazor material below remains required minimum coverage within the expanded all-.NET-11 scope. It is not the complete product inventory or a claim that examples are verified. Build a source-backed inventory across every .NET 11 area and classify each author-facing change. A topic is not necessarily one recipe: some topics may split by goal or applicability.

### The guide's 25 topics

Forms:

1. [Client-side validation for static SSR forms](https://lab.practicaldotnet.io/learn/ssr-client-validation).
2. [Async form validation](https://lab.practicaldotnet.io/learn/async-form-validation).
3. [Validation message localization](https://lab.practicaldotnet.io/learn/validation-localization).
4. [Labels and generated input IDs](https://lab.practicaldotnet.io/learn/label-and-input-ids).
5. [DisplayName](https://lab.practicaldotnet.io/learn/displayname-component).
6. [Supported nested validation with ValidatableType](https://lab.practicaldotnet.io/learn/validatable-type-stable).

Pages and navigation:

7. [TempData for static SSR](https://lab.practicaldotnet.io/learn/tempdata-ssr).
8. [Session-supplied state](https://lab.practicaldotnet.io/learn/supply-parameter-from-session).
9. [Relative navigation](https://lab.practicaldotnet.io/learn/relative-navigation).

Rendering:

10. [CacheView and partial-page caching](https://lab.practicaldotnet.io/learn/cacheview).
11. [Static RenderFragment content across render-mode boundaries](https://lab.practicaldotnet.io/learn/renderfragment-across-render-modes).
12. [Environment-specific content](https://lab.practicaldotnet.io/learn/environment-view).

Grids and lists:

13. [QuickGrid URL sorting and paging](https://lab.practicaldotnet.io/learn/quickgrid-url-paging).
14. [QuickGrid row click handling](https://lab.practicaldotnet.io/learn/quickgrid-onrowclick).
15. [Virtualize anchoring](https://lab.practicaldotnet.io/learn/virtualize-anchor-mode).
16. [Variable-height virtualized items](https://lab.practicaldotnet.io/learn/virtualize-variable-height).
17. [Initial item positioning and scrolling](https://lab.practicaldotnet.io/learn/virtualize-scroll-to-item).
18. [Virtualize with a strict Content Security Policy](https://lab.practicaldotnet.io/learn/virtualize-csp).

Interactive Server:

19. [Automatic circuit pause for hidden tabs](https://lab.practicaldotnet.io/learn/circuit-auto-pause).
20. [Server-initiated circuit pause](https://lab.practicaldotnet.io/learn/server-initiated-circuit-pause).
21. [Browser/reconnection configuration from C#](https://lab.practicaldotnet.io/learn/browser-options).

WebAssembly:

22. [Hosted services in the browser](https://lab.practicaldotnet.io/learn/wasm-hosted-services).
23. [Server-provided environment configuration](https://lab.practicaldotnet.io/learn/wasm-env-vars-config).
24. [Culture flowing from server to browser](https://lab.practicaldotnet.io/learn/wasm-culture-from-server).
25. [The .NET Web Worker template](https://lab.practicaldotnet.io/learn/web-worker-template).

The original code-change criterion still needs to be applied carefully to each entry. For example, variable-height and CSP fixes matter when they let developers remove old workarounds; an upgrade-only benefit does not need its own recipe. Do not drop these supplied topics without discussing that distinction.

### The exact seven additions retained by the reference thread

1. **BasePath**: framework-managed base-path markup. [Microsoft release notes](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11?view=aspnetcore-11.0#new-basepath-component).
2. **NavigationManager.GetUriWithFragment**: construct fragment links through the framework helper. [Microsoft release notes](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11?view=aspnetcore-11.0#geturiwithfragment-extension-method).
3. **IComponentPropertyActivator**: custom population of injected component properties. [Microsoft release notes](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11?view=aspnetcore-11.0#icomponentpropertyactivator).
4. **Expanded authorization metadata**: policy and requirement metadata for Blazor authorization views and route views. [Preview 7 notes](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview7/aspnetcore.md#consistent-authorization-metadata-across-the-stack).
5. **C# unions in Blazor**: component parameters, JS interop, and persisted state. The original thread's preview-language caveat is superseded by RC1 stabilization; reconcile examples against the actual artifacts. [Preview 6 integration](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview6/aspnetcore.md#unions-in-aspnet-core), [Preview 7 Razor syntax](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview7/aspnetcore.md#razor-accepts-literal-attributes-for-union-typed-component-parameters), [RC1 stabilization](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/csharp.md#c-15-stabilizes-unions-and-other-language-features).
6. **WebAssembly Gateway and service defaults**: API proxy configuration and generated client defaults for discovery, resilience, and telemetry. These were deliberately grouped as one of the seven. [Gateway proxy](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview6/aspnetcore.md#blazor-gateway-can-proxy-backend-api-calls), [service defaults](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview4/aspnetcore.md#blazor-webassembly-service-defaults-template).
7. **Blazor AI components**: streaming conversations, tool UI, approval flows, and shared/predictive UI state. This original scope item remains experimental throughout .NET 11, not merely during RCs. Round 5 explicitly retains it with experimental labels. [RC1 notes](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/aspnetcore.md#experimental-blazor-ai-components-for-agentic-user-interfaces), [current status](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11?view=aspnetcore-11.0#experimental-blazor-ai-components-for-agentic-user-interfaces).

### Three expansions, separate from those seven

- **QuickGrid virtualization**: grid-specific examples for `InitialItemIndex`, `ScrollToItemAsync`, `AnchorMode`, and `ItemComparer`. [Preview 7 notes](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview7/aspnetcore.md#quickgrid-supports-initialitemindex-and-scrolltoitemasync), [RC2 stabilization](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/aspnetcore.md#quickgrid-virtualization-apis-are-no-longer-experimental).
- **ConfigureConnection**: demonstrate Interactive Server transport, authentication-expiry, and buffer settings, rather than merely mentioning the API under browser reconnection. [Microsoft release notes](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11?view=aspnetcore-11.0#signalr-configureconnection-for-interactive-server-components).
- **Web Workers**: expand beyond timeout coverage to `InvokeVoidAsync` and creation/invocation cancellation. [Preview 4 notes](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview4/aspnetcore.md#blazor-web-worker-template-updates).

The earlier broad gap list also mentioned authentication refresh, analyzers, MathML, template changes, startup-option compatibility, automatic antiforgery integration, CoreCLR-on-WASM, trimming, telemetry, and toolchain changes. The follow-up narrowed this to the seven above using the code-change criterion. Preserve that refinement rather than restoring the initial broad list as accepted scope. A later concrete migration recipe may justify reconsidering an excluded item.

## Evidence and limits

- The reference thread described .NET 11 RC2 as its research baseline. Release status, API signatures, package versions, and applicability must be checked when examples are authored.
- Reading a guide or release note does not establish that an example compiles or behaves correctly. No product build, package install, MCP handshake, or sample execution has run in this planning session.
- The tool's own host runtime is separate from the frameworks its guidance describes. None of the supplied naming or scope decisions requires the documentation server itself to target `net11.0`.
- Guide demo render modes are examples, not proof that the feature is restricted to that mode. Check actual applicability, including prerendering and Interactive Auto.
- Some older-way demonstrations run emulations inside a .NET 11 app. Do not call these actual .NET 10 integration evidence.
- Worker timeout/cancellation/termination behavior, hosted-service shutdown in a browser, and relative navigation syntax need particular care where the guide and current notes differ. Resolve each against source and executable evidence before publication.
- The earlier thread says QuickGrid anchoring/comparer APIs became non-experimental in RC2; the current documentation inspected during discovery still carries experimental guidance. Record the exact SDK/package combination before classifying stability.
- The guide describes an RC2 shared-options leak between `WithBrowserOptions` and `ConfigureBrowser`. Treat this as version-specific evidence, not permanent framework behavior.
- The guide is inspiration only. Pingu explicitly requires original explanations and totally unique examples written from scratch. Renaming or lightly adapting the guide's examples does not meet that requirement. Source links and attribution remain part of the evidence trail.

## Design tree

### Settled roots

- Identity: `PinguApps.Playbook` and `playbook`.
- Distribution: local .NET tool published through NuGet.
- Initial comprehensive subject: all .NET 11 author-facing changes. Blazor is one area within it.
- Future product: expand the same package.
- Required Blazor subset: all 25 guide topics, seven additions, and three expansions. Preserve this while expanding the release-wide inventory.
- Engineering constraints: PinguApps conventions, central package management, matching GitHub Actions, Roslynk MCP mechanics.

### Round 1: accepted by Pingu

1. **Product boundary.** A small, deterministic, read-only guidance server. The calling agent inspects and edits the user's project. Playbook does not scan projects, run user builds, or apply migrations.
2. **Version safety.** Preserve release-specific guidance with explicit applicability. Round 2 records Pingu's latest-version default when the target is omitted.
3. **Launch completeness.** All 25 guide topics, seven additions, three expansions, and other previously identified refinements form the original required Blazor baseline. The later scope expansion makes this a subset of comprehensive .NET 11 coverage.
4. **Content authorship.** Preserve the goal/older/newer structure. Write original explanations and totally unique examples from scratch; the site is inspiration only.
5. **Evidence standard.** Official citations, exact verified SDK/package versions, compiled examples, appropriate real runtime/browser checks, and verification of the packed tool. Distinguish preview status, source claims, and actual execution evidence. Round 3 narrows Windows/macOS CI to tiny tool smoke checks; Round 5 requires example compilation with focused behavior checks where useful rather than a broad native runtime gate.
6. **Repository baseline.** CodeStyle.Analyzers for structure, Template's generated application for current style/analyzer consumption, BlazorSitemap for modern NuGet workflows and packed-consumer verification, RazorStyle for PinguApps tool packaging, and Roslynk for MCP mechanics.
7. **Release automation.** Manual initial release, then the existing weekly publishing pattern. Pingu delegated selection of a time offset from all existing PinguApps organization publishers. The organization-wide audit selected **Sunday 18:17 UTC**, cron **`17 18 * * 0`**. See [schedule evidence](publishing-schedule.md). This decision approves the plan, not a release now.
8. **Supported environments.** Windows/Linux/macOS, with setup documentation and real client smoke checks for T3 Code, Codex CLI, Claude Code, and VS Code Copilot. The exact client/platform verification matrix remains to decide.

### Fact-finding completed after Round 1

- Read-only GitHub audit of all 35 PinguApps organization repositories, their default-branch workflows, enabled state, and sampled publication history. Selected Sunday 18:17 UTC; see [audit](publishing-schedule.md). No other organizations or personal repositories were inspected.
- Bounded primary-source recheck identified the additional candidates below. Candidate findings do not automatically expand the scope.
- The local machine has .NET SDK 10.0.402 and no .NET 11 SDK installed. The MCP SDK's published package supports older frameworks, so guidance for .NET 11 does not itself require a .NET 11 host. Choose the tool runtime separately from sample runtimes; no SDK installation has been performed.

### Blazor migration additions and refinement

The CSRF and navigation migrations below were accepted in Round 2 where they meet the authoring criterion. Telemetry remains a possible refinement within the existing service-defaults topic.

- **Blazor SSR CSRF migration.** The earlier broad list excluded automatic protection as an upgrade benefit. Microsoft's current migration guidance also identifies explicit changes to startup/form authoring, which makes a concrete migration entry worth reconsidering. Any example must cover when token-based protection remains necessary and prove the intended protection in real requests. [Migration guidance](https://learn.microsoft.com/en-us/aspnet/core/migration/100-to-110?view=aspnetcore-11.0#automatic-csrf-protection).
- **CSP-friendly navigation scripts.** The .NET 11 template removes inline navigation JavaScript. An original migration example could replace inline handlers with an external module while following PinguApps conventions. The published template uses `NavMenu.razor.js`; the selected PinguApps analyzer forbids that filename convention. Our example should preserve the behavior using the approved external-module organization. This is template migration guidance, not a new API that was impossible in .NET 10. [Microsoft migration recipe](https://learn.microsoft.com/en-us/aspnet/core/migration/100-to-110?view=aspnetcore-11.0#adopt-inline-js-event-handler-removed-from-the-navmenu-component), [local analyzer rule](../../../CodeStyle.Analyzers/README.md).
- **WASM component telemetry configuration.** A possible expansion of the approved service-defaults guidance, rather than a separate topic: explicitly enable, subscribe to, and export built-in component metrics/traces. Resolve actual SDK switches and exporter compatibility through executable evidence before presenting it as verified. [Feature notes](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11?view=aspnetcore-11.0#blazor-webassembly-component-metrics-and-tracing), [diagnostic configuration](https://learn.microsoft.com/en-us/aspnet/core/blazor/performance/webassembly-event-pipe-diagnostics?view=aspnetcore-11.0#how-to-observe-metrics-emmited-by-a-webassembly-app).

Nested startup-option syntax belongs within browser configuration. QuickGrid link/CSS and multiple-grid query-parameter migration belong within the approved URL-state topic. Neither needs a new top-level topic. Container-template options and automatic MathML fixes still lack a distinct recipe under the stated code-change criterion.

### Architecture facts consulted

- The official C# SDK documents stdio as a client-launched local process transport. [Transport documentation](https://csharp.sdk.modelcontextprotocol.io/v1/concepts/transports/transports.html).
- The published `ModelContextProtocol` package is 2.2.0 at inspection, includes hosting/DI for non-HTTP servers, and supports frameworks older than .NET 10. HTTP integration is a separate package. This is a researched candidate dependency, not a settled version pin. [NuGet package](https://www.nuget.org/packages/ModelContextProtocol).
- Standard .NET runtime selection does not automatically cross major versions. A `net10.0` host needs an explicit runtime policy if it is to run on a machine with only .NET 11 installed. `Major` prefers the requested major when present and can move to a higher one when absent; `LatestMajor` always prefers the highest installed major. Choose and verify the actual packaged policy. [Runtime version selection](https://learn.microsoft.com/en-us/dotnet/core/versions/selection#control-roll-forward-behavior).

### Round 2: accepted, modified, or deferred by Pingu

9. **Local process architecture — selected.** `playbook mcp` runs MCP directly over stdio as a client-launched process. Pingu delegated the choice to our technical recommendation. Each client owns its process; avoid daemon startup, ports, shared-state lifetime, and a separate HTTP service for this documentation workload. The tradeoff is a small process/content copy per client, acceptable for a bundled catalogue.
10. **Offline content delivery — accepted.** Bundle all guidance in the NuGet package. Installed requests run offline without telemetry, API keys, remote content fetches, or a runtime updater. Package updates deliver content updates.
11. **Content unit and language — accepted.** A goal-focused guidance entry is a **recipe**. One recipe answers one developer goal and can use several APIs. Framework feature count does not dictate recipe count.
12. **Initial MCP surface — accepted.** Two tools: `list_recipes` and `get_recipe`, named in Round 3. Catalogue growth does not itself require more tools or a search service. Round 4 settles combination/paging and complete-response behavior.
13. **Target selection — modified by Pingu.** Callers should supply the target release. When omitted, assume the latest version rather than failing or asking for a selection. Round 3 confirms this means the newest release represented in the installed catalogue, with the resolved release returned explicitly. Framework/platform applicability still matters. Do not restore the rejected missing-version error policy.
14. **Host runtime policy — accepted.** A `net10.0` tool host with `Major` roll-forward, verified on .NET 10 and .NET 11. Examples use separately pinned SDKs/workloads. The documentation host does not acquire desktop/mobile workload dependencies merely because it serves those recipes.
15. **Production project boundary — accepted starting point.** One production console/tool project, one Reqnroll-based core test project, and independently buildable sample projects. Round 3 limits Windows/macOS jobs to brief tool smoke checks, without native UI/emulator/device test suites. No separately published core library is planned.
16. **Agent adoption — accepted.** Include a short companion instruction to consult Playbook before applicable .NET implementation work. Expand the earlier Blazor-only wording. Users choose to add those instructions through client setup; server descriptions alone cannot guarantee consultation.
17. **CSRF migration — accepted under the authoring criterion.** Include the original recipe where the .NET 11 approach changes startup/form code and explains retained protections. Do not turn an invisible automatic benefit into a standalone recipe. This candidate has a documented concrete migration.
18. **CSP navigation migration — accepted under the authoring criterion.** Include the original inline-handler-to-external-module example, following PinguApps JavaScript conventions. Its value is changed application markup/script/CSP authoring; label it template migration.
19. **Pre-GA publication policy — initially deferred, settled in Round 7.** Pingu deferred the original question, then accepted manual preview publication with agreed coverage/checks, independent package SemVer, GA reconciliation before stable, and weekly automation only after the first manual stable release. Evidence/status labeling remains required.

### Scope expansion and .NET 10 question

Pingu explicitly locked in all .NET 11 code-changing capabilities, with the same exclusion of invisible performance gains. Prioritization can order the work, but cannot silently narrow this coverage back to selected frameworks or headline features. See [scope ADR](../adr/0003-cover-author-facing-changes-across-dotnet.md).

Pingu asked whether .NET 10 guidance adds enough value to justify the work, then approved a small explicitly selected foundation set in Round 3. This is not approval of a complete .NET 10 backfill. Select high-value carry-forward recipes and prerequisites separately from the comprehensive .NET 11 commitment. We have not measured the target agents' knowledge; release age does not establish that they know an API or can use it correctly.

The [release-wide coverage map](coverage.md) records the researched source families, eligible candidate areas, source conflicts, completeness method, and selected .NET 10 foundations. It is not an exhaustive completed catalogue. The wider scope primarily expands content and verification; the accepted small offline server and two-tool boundary remain.

### Round 3: accepted or modified by Pingu

20. **Curated .NET 10 foundations — accepted.** A small explicitly selected set that removes substantial custom code or supports .NET 11 recipes. Comprehensive .NET 11 remains primary; this is not a complete .NET 10 backfill or evidence of measured agent deficiencies. Round 4 approves the exact eight-recipe shortlist.
21. **Release-family boundary — accepted.** Cover the .NET 11 SDK/runtime/languages and associated first-party framework releases, including ASP.NET Core, EF Core, WPF, WinForms, and MAUI/platform workloads. Other packages and native APIs are documented when a recipe requires them; their independently versioned standalone features require a separate scope decision.
22. **Catalogue contract — accepted and clarified.** Tool names are `list_recipes` and `get_recipe`. Catalogue discovery supports deterministic area/language/target/keyword filtering and pagination. Area filters are optional: callers may choose Blazor, WPF, or another area, or leave area filtering off. No area filter means all areas for the selected/default target, not a forced framework choice. No AI matching service is required.
23. **Precise latest default — accepted.** An omitted target resolves to the newest .NET release covered by the installed package; return that resolved value. Explicit targets retain release-specific guidance, and explicit unsupported targets receive an honest coverage response. Platform/framework/preview constraints still apply.
24. **Applicability dimensions — accepted.** Every individual recipe response identifies what it describes and where it applies: introduction/applicability, target runtime, required SDK/language mode, framework/package versions, platform/provider constraints, and preview/experimental status. Include rendering context where relevant.
25. **Platform verification lanes — narrowed by Pingu.** Windows and macOS get only a very minimal, brief smoke test. Do not add a broad native UI, emulator, device, or platform-feature matrix. Plan one packed-tool install/start/initialize/list/get/shutdown check per OS; keep the established focused Linux checks. A tool smoke does not establish native recipe behavior. Record source review, compilation, and runtime execution separately and report gaps; the earlier broad native-suite proposal is superseded.
26. **Content authoring and sample source — accepted.** Markdown prose, JSON metadata, and snippets drawn from the same independently authored sample files used for build/test verification. Keep one authoritative example source. Round 5 accepts directory layout and embedded/generated packaging. Do not imply that every platform sample has been executed merely because its source is canonical.

### Round 4: accepted or modified by Pingu

The [tool contract](tool-contract.md) records the accepted input/output design. It is not implemented behavior.

27. **Filter combinations and paging — accepted.** Match any requested area, then combine different filter kinds. Example: Blazor or WPF, and C#, and the target/keywords. Area filtering stays optional. Default to 20 summaries per page, maximum 50, with supported area identifiers in discovery metadata.
28. **Stable recipe IDs — accepted.** Goal-based IDs omit release numbers. Preserve release-specific editions under that identity; target selection chooses an applicable edition, including still-relevant .NET 10 guidance on .NET 11.
29. **Complete standard response — accepted.** One structured object contains applicability/metadata and full Markdown, with an output schema and complete JSON text fallback. Use the standard SDK representation; keep required information together.
30. **Experimental-default proposal — rejected/replaced.** Target the full .NET 11 release, preparing content from what current RCs establish about the final contract. At GA, compare final release notes/API behavior, identify omissions/changes, update recipes, and create a corrective/additional release as needed. Round 5 separately confirms inclusion of deliberately experimental APIs with clear labels, including Blazor AI; this does not restore temporary RC-only approaches as the product target. See [GA policy ADR](../adr/0004-target-ga-guidance-using-rc-evidence.md). Package versioning/publication timing is not settled by this content-target decision.
31. **Helpful failures — accepted.** Explicit errors for unknown IDs, unsupported targets, and misspelled filters; successful empty results for valid combinations with no matches. Never quietly relax explicit filters or targets.
32. **Minimal CLI — accepted.** `playbook mcp`, help, and version only; bare `playbook` shows help. No separate human recipe browser or automatic client-configuration editing.
33. **Initial .NET 10 set — accepted.** The eight goals in [coverage](coverage.md#approved-initial-eight-recipes): extension properties, field-backed properties, Minimal API validation, named query filters, outer joins, strict JSON contracts, async LINQ, and file-based apps.

### Round 5: accepted or modified by Pingu

The [implementation plan](implementation-plan.md) records the accepted layout, packaging, comparison baseline, and verification boundaries alongside proposed delivery milestones.

34. **Experimental-at-GA inclusion — modified by Pingu.** Do not exclude experimental APIs. Include them when they meet the authoring criterion and explicitly disclose their experimental status in summaries and detail. Blazor AI remains part of the original seven additions. A GA runtime does not imply a stable API contract; retain exact package/opt-in requirements and evidence.
35. **Repository layout — accepted.** Use `src/`, `tests/`, `content/recipes/<id>/<edition>/`, `samples/`, and `eng/Verify.ps1` with shared root conventions. Keep the single production project and avoid another published library.
36. **Packaged assets — accepted.** Generate the catalogue/rendered snippets during build/pack and embed the resulting guidance in the tool assembly. Installed requests need no sidecar repository files, caller-specific working directory, database, or network.
37. **Older comparison baseline — accepted.** Compare against the previous release: .NET 10 for .NET 11 recipes, and .NET 9 for the selected .NET 10 recipes. Use actual targets/dependencies, state when an older equivalent was unavailable, and do not claim valid old patterns are broken.
38. **Example evidence gate — accepted.** Require code examples to compile against their stated SDKs/dependencies, with focused behavior checks where useful rather than a broad native runtime gate. Windows/macOS automated jobs remain tiny tool smokes. Resolve compilation gaps before examples meet the gate and record actual platform/compilation/runtime evidence without implying compilation proves runtime behavior.

### Round 6: accepted by Pingu

39. **Example-language depth.** Use C# as the normal canonical language for shared framework/library examples, F# for F#-specific capabilities, and VB for any qualifying VB-specific change. Add translations where authoring differs materially rather than triplicating every sample. Language-feature coverage is already in scope; this decides sample depth.
40. **Language-filter meaning.** `languages` matches languages actually demonstrated by the recipe. Record broader supported language applicability separately from example languages, so an F# filter does not unexpectedly return only C# code.
41. **Shared guidance with area filtering.** Choosing Blazor, WPF, or another framework includes relevant shared language/library/SDK recipes as well as framework-specific ones. Add optional `includeShared`, default true; false requests only directly tagged areas. Shared does not bypass target/language/keyword filters or admit unrelated platform recipes. Define eligibility through explicit metadata, not AI inference, and deduplicate results.
42. **Conditional target applicability.** Include a .NET 11-era recipe for an older supported target when the exact SDK/package route is documented and verified. Display the extra prerequisites; do not infer installed SDK/packages or equate introduction release with minimum runtime. This does not expand the eight-recipe .NET 10 historical backfill or permit unsupported language/runtime combinations.
43. **Client verification matrix.** One brief real check per named client on an available supported OS, plus the already agreed Windows/macOS packed-tool smoke checks. Do not create a four-client-by-three-OS matrix. Record actual client/OS versions and verify that both body and applicability reach the agent; missing environments remain visible gaps.
44. **Product evaluation.** A small manual comparison on three to five original tasks, with and without Playbook, before public launch. Span a language change, web, and a non-web area. Record agent/model versions, whether Playbook was consulted, and correctness of the resulting approach. This is separate from CI, does not create another test matrix, and cannot support broad marketing claims from such a small sample. Round 7 accepts four task goals and a bounded execution budget.

### Round 7: accepted or modified by Pingu

The [release plan](release-plan.md) and [evaluation plan](evaluation-plan.md) record these decisions. They settle the package publication policy deferred in Round 2; no publication occurs during planning.

45. **Independent package SemVer — accepted.** First stable `1.0.0`, independent of .NET target releases; previews use `1.0.0-preview.N`. Patch for corrections, minor for compatible recipes/capability additions, and major for incompatible public tool/contract or stable recipe-ID changes. Preserve the reference Release Drafter major/minor/patch mechanism.
46. **Preview and stable release channels — accepted.** Manual preview packages before GA, clearly disclosing RC evidence. First public publication, including a preview, still requires accepted coverage/checks and the manual evaluation; stable additionally waits for GA reconciliation. Weekly stable publishing starts only after the first manual stable release. Check both version syntax and GitHub prerelease flags; the reference gate would otherwise let a preview unlock weekly stable publishing.
47. **Releasable-change guard — rejected; use reference behavior.** Preserve the standard weekly release flow from Pingu's other packages. New `main` commits plus a suitable draft can publish, including docs/CI/tests/maintenance. Do not add a shipped-path, package-difference, or empty-release-note gate. Keep the reference no-new-commits/no-draft safeguards and the independently accepted stable-channel gate.
48. **GitHub versus package identity — accepted.** Retain actual `PinguApps/Playbook` on GitHub, `PinguApps.Playbook` for NuGet/solution/project, and `playbook` for the CLI. This resolves the earlier requested repository naming versus the existing origin before OIDC/link setup. No remote rename is needed.
49. **Evaluation tasks and budget — accepted.** The four original scaffold tasks in the evaluation plan: C# outcomes, Minimal API async validation, static Blazor grid links, and non-web manifest reconciliation. One with/without pair each on the same recorded client/model: eight fresh runs, maximum ten minutes each, at most 80 execution minutes plus preparation/review. No retries or model matrix. Fix Playbook defects exposed by the exercise without requiring a win on every task.

### Dependent branches to explore next

- Release-family and .NET 10 decisions -> complete source register and recipe breakdown. Language-feature coverage, eight foundations, example-language depth, and shared-recipe discovery are accepted.
- Catalogue and applicability -> final schema names/normalization and backwards compatibility. Language-filter meaning, conditional SDK/package routes, shared-area matching, core filtering/paging, stable IDs, complete results, and errors are accepted.
- Latest default -> final target normalization and edition metadata; explicit unsupported targets, retained editions, supersession safety, and package/content version separation are accepted.
- Content source and verification lanes -> metadata validation, snippet generation, native toolchain pinning, minimal CI selection, and evidence records. Layout, embedded content, previous-release comparisons, compile gate, and client matrix are accepted.
- Accepted runtime/project boundary -> exact SDK/package pins, analyzer settings, installable tool artifacts, stdio shutdown/logging, and client setup. The CLI surface is now accepted.
- Publication policy -> concrete trusted-publishing setup and readiness checks; version/channel/identity/reference-weekly behavior are accepted. The selected schedule is accepted but not activated.
- Product value -> freeze checks against actual artifacts for the accepted task goals/budget; actual run evidence comes during implementation/launch preparation.

Update this tree after each answer. Record resolved vocabulary in [GLOSSARY.md](../../GLOSSARY.md). Offer ADRs only for consequential tradeoffs. Begin implementation only after Pingu confirms the shared understanding.
