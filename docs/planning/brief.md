# PinguApps.Playbook planning brief

Status: discovery and planning. Round 1 is accepted; implementation awaits the completed design and Pingu's confirmation of shared understanding.

This brief records the starting knowledge for the grill-with-docs session on 15 October 2026. It separates Pingu's requirements, earlier assistant proposals, reference observations, and decisions still to make.

## Sources

- [Complete reference conversation: Check Guide Against .NET 11 Blazor](t3-thread://v1/372736c9-d15c-463f-a2ea-5c5f880eef79), including the final naming decision. All user and assistant messages were read.
- [Practical .NET field guide](https://lab.practicaldotnet.io/guide). The current index contains 25 topics. Each has problem, demonstration, and implementation stages.
- The reference thread's local research snapshots: `C:\Users\pingu\.t3\scratch\2026-10-15-can-you-please-take-a-880eef79\guide-chapters.json`, `guide-stages.json`, and `release-notes.json`. These preserve 25 chapter pages, 50 demonstration/implementation pages, and the release notes retrieved in that thread. They are research evidence, not committed product content.
- Pingu's current instructions and seven local reference repositories. See [repository findings](reference-repositories.md).

The live index was retrieved successfully over HTTP. Browser inspection twice failed with `Preview automation snapshot timed out after 15000ms.` The browser's interactive demonstrations have not been exercised in this session. Current index links use `/learn/{slug}`; the earlier snapshots use `/guide/{slug}`. Preserve topic identities separately from source URLs.

## Requirements already supplied

### Product and identity

- Help coding agents use .NET 11 Blazor capabilities that their existing knowledge may miss.
- Explain the developer's goal, the older approach, and the .NET 11 approach.
- Focus on changes to code developers write. Unrelated .NET features and benefits obtained solely from upgrading the runtime are outside the stated interest.
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

These are the accepted minimum launch coverage, not a claim that every topic already has a verified example. Additional worthwhile Blazor code changes may be proposed after a fresh check. A topic is not necessarily one final entry: some topics may split by goal or applicability.

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
5. **C# unions in Blazor**: component parameters, JS interop, and persisted state, with preview-language applicability. [Preview 6 integration](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview6/aspnetcore.md#unions-in-aspnet-core), [Preview 7 Razor syntax](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview7/aspnetcore.md#razor-accepts-literal-attributes-for-union-typed-component-parameters).
6. **WebAssembly Gateway and service defaults**: API proxy configuration and generated client defaults for discovery, resilience, and telemetry. These were deliberately grouped as one of the seven. [Gateway proxy](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview6/aspnetcore.md#blazor-gateway-can-proxy-backend-api-calls), [service defaults](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview4/aspnetcore.md#blazor-webassembly-service-defaults-template).
7. **Blazor AI components**: streaming conversations, tool UI, approval flows, and shared/predictive UI state. Preserve the experimental status; do not imply a stable framework API. [RC1 notes](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/aspnetcore.md#experimental-blazor-ai-components-for-agentic-user-interfaces).

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
- Initial subject: .NET 11 Blazor coding guidance.
- Future product: expand the same package.
- Reference inputs: all 25 guide topics, seven additions, and three expansions.
- Engineering constraints: PinguApps conventions, central package management, matching GitHub Actions, Roslynk MCP mechanics.

### Round 1: accepted by Pingu

1. **Product boundary.** A small, deterministic, read-only guidance server. The calling agent inspects and edits the user's project. Playbook does not scan projects, run user builds, or apply migrations.
2. **Version safety.** Preserve release-specific guidance with explicit applicability. Target selection and defaults remain for a later round.
3. **Launch completeness.** All 25 guide topics, seven additions, three expansions, and other previously identified refinements form the minimum baseline. Recheck for worthwhile additions; propose new scope explicitly.
4. **Content authorship.** Preserve the goal/older/newer structure. Write original explanations and totally unique examples from scratch; the site is inspiration only.
5. **Evidence standard.** Official citations, exact verified SDK/package versions, compiled examples, appropriate real runtime/browser checks, and verification of the packed tool. Distinguish preview status, source claims, and actual execution evidence.
6. **Repository baseline.** CodeStyle.Analyzers for structure, Template's generated application for current style/analyzer consumption, BlazorSitemap for modern NuGet workflows and packed-consumer verification, RazorStyle for PinguApps tool packaging, and Roslynk for MCP mechanics.
7. **Release automation.** Manual initial release, then the existing weekly publishing pattern. Pingu delegated selection of a time offset from all existing PinguApps organization publishers. The organization-wide audit selected **Sunday 18:17 UTC**, cron **`17 18 * * 0`**. See [schedule evidence](publishing-schedule.md). This decision approves the plan, not a release now.
8. **Supported environments.** Windows/Linux/macOS, with setup documentation and real client smoke checks for T3 Code, Codex CLI, Claude Code, and VS Code Copilot. The exact client/platform verification matrix remains to decide.

### Fact-finding completed after Round 1

- Read-only GitHub audit of all 35 PinguApps organization repositories, their default-branch workflows, enabled state, and sampled publication history. Selected Sunday 18:17 UTC; see [audit](publishing-schedule.md). No other organizations or personal repositories were inspected.
- Bounded primary-source recheck identified the additional candidates below. Candidate findings do not automatically expand the scope.
- The local machine has .NET SDK 10.0.402 and no .NET 11 SDK installed. The MCP SDK's published package supports older frameworks, so guidance for .NET 11 does not itself require a .NET 11 host. Choose the tool runtime separately from sample runtimes; no SDK installation has been performed.

### Additional content candidates

These are research findings awaiting a scope decision, not additions to the accepted baseline.

- **Blazor SSR CSRF migration.** The earlier broad list excluded automatic protection as an upgrade benefit. Microsoft's current migration guidance also identifies explicit changes to startup/form authoring, which makes a concrete migration entry worth reconsidering. Any example must cover when token-based protection remains necessary and prove the intended protection in real requests. [Migration guidance](https://learn.microsoft.com/en-us/aspnet/core/migration/100-to-110?view=aspnetcore-11.0#automatic-csrf-protection).
- **CSP-friendly navigation scripts.** The .NET 11 template removes inline navigation JavaScript. An original migration example could replace inline handlers with an external module while following PinguApps conventions. The published template uses `NavMenu.razor.js`; the selected PinguApps analyzer forbids that filename convention. Our example should preserve the behavior using the approved external-module organization. This is template migration guidance, not a new API that was impossible in .NET 10. [Microsoft migration recipe](https://learn.microsoft.com/en-us/aspnet/core/migration/100-to-110?view=aspnetcore-11.0#adopt-inline-js-event-handler-removed-from-the-navmenu-component), [local analyzer rule](../../../CodeStyle.Analyzers/README.md).
- **WASM component telemetry configuration.** A possible expansion of the approved service-defaults guidance, rather than a separate topic: explicitly enable, subscribe to, and export built-in component metrics/traces. Resolve actual SDK switches and exporter compatibility through executable evidence before presenting it as verified. [Feature notes](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11?view=aspnetcore-11.0#blazor-webassembly-component-metrics-and-tracing), [diagnostic configuration](https://learn.microsoft.com/en-us/aspnet/core/blazor/performance/webassembly-event-pipe-diagnostics?view=aspnetcore-11.0#how-to-observe-metrics-emmited-by-a-webassembly-app).

Nested startup-option syntax belongs within browser configuration. QuickGrid link/CSS and multiple-grid query-parameter migration belong within the approved URL-state topic. Neither needs a new top-level topic. Container-template options and automatic MathML fixes still lack a distinct recipe under the stated code-change criterion.

### Architecture facts consulted

- The official C# SDK documents stdio as a client-launched local process transport. [Transport documentation](https://csharp.sdk.modelcontextprotocol.io/v1/concepts/transports/transports.html).
- The published `ModelContextProtocol` package is 2.2.0 at inspection, includes hosting/DI for non-HTTP servers, and supports frameworks older than .NET 10. HTTP integration is a separate package. This is a researched candidate dependency, not a settled version pin. [NuGet package](https://www.nuget.org/packages/ModelContextProtocol).
- Standard .NET runtime selection does not automatically cross major versions. A `net10.0` host needs an explicit runtime policy if it is to run on a machine with only .NET 11 installed. `Major` prefers the requested major when present and can move to a higher one when absent; `LatestMajor` always prefers the highest installed major. Choose and verify the actual packaged policy. [Runtime version selection](https://learn.microsoft.com/en-us/dotnet/core/versions/selection#control-roll-forward-behavior).

### Round 2: independent decisions awaiting Pingu

9. **Local process architecture.** Recommend `playbook mcp` running MCP directly over stdio as a client-launched process. A persistent HTTP daemon is unnecessary for the accepted documentation-only boundary.
10. **Offline content delivery.** Recommend bundling all guidance in the NuGet package. Installed requests run offline without telemetry, API keys, remote content fetches, or a runtime updater; package updates deliver content updates.
11. **Content unit and language.** Recommend calling a goal-focused guidance entry a **recipe**, rather than equating entries with framework features. One recipe answers one developer goal and can use multiple APIs. Exact entry count follows the goal breakdown.
12. **Initial MCP surface.** Recommend two tools: a compact catalogue and full detail for one selected entry. Defer public tool names until terminology is settled. No separate search engine, MCP resources, or MCP prompts are needed for the initial scope.
13. **Target selection.** Recommend requiring the caller to supply the application's target .NET release, with a clear response listing supported releases if it is omitted or unsupported. Never silently choose the newest guidance. Exact argument names and accepted version syntax remain to decide.
14. **Host runtime policy.** Recommend a `net10.0` tool host, matching the current repository baseline, with `Major` roll-forward so it can use .NET 11 when .NET 10 is absent. Verify the installed package on both supported runtimes. .NET 11 examples use separately pinned sample SDKs; future runtimes are not claimed tested automatically.
15. **Production project boundary.** Recommend one production console/tool project and one Reqnroll-based test project, plus independently buildable sample projects. No separately published core library is needed for the accepted product.
16. **Agent adoption.** Recommend a short companion instruction telling agents to consult the catalogue before implementing Blazor work. Include it in client setup documentation; users opt into those instructions explicitly. Server descriptions and instructions help discovery but cannot guarantee an agent consults the tool.
17. **Additional CSRF migration recipe.** Recommend adding an original Blazor SSR recipe explaining the applicable .NET 11 startup/form migration and when token protection must remain, with actual request-level checks. This is a deliberate reconsideration of an earlier automatic-benefit exclusion because there is a concrete authored migration.
18. **Additional CSP navigation recipe.** Recommend adding an original inline-handler-to-external-module migration example, following PinguApps JavaScript conventions. Label it template migration, not a newly available JavaScript capability.
19. **Pre-GA publication policy.** Recommend using prerelease package versions for any publication before .NET 11 GA, with stable publication requiring a fresh verification pass against GA SDK/packages. This does not relax the accepted minimum coverage for the first public package or authorize publication now.

### Dependent branches to explore next

- Product boundary -> tool versus resource surface, discovery, result contracts, errors, server instructions, companion instructions, offline behavior, and extensibility.
- Version safety -> target-version selection, retention/support lifetime, feature evolution, applicability and stability metadata, content identifiers, backwards compatibility, and package versioning.
- Launch coverage and evidence -> entry granularity, sample organization, valid older/newer comparisons, test matrix, source conflicts, maintenance, contribution workflow, and product evaluation.
- Repository baseline -> exact solution/projects, SDK and runtime support, analyzer settings, Reqnroll scenarios, packaging metadata, dependencies, and actual artifact checks.
- Release automation -> initial version, trusted-publishing setup, repository identity, draft categories, prereleases, scheduled publishing activation, and release readiness checks.
- Supported environments -> installation methods, MCP registration, command behavior, process lifecycle, logging, cancellation, troubleshooting, and platform/client acceptance checks.

Update this tree after each answer. Record resolved vocabulary in [GLOSSARY.md](../../GLOSSARY.md). Offer ADRs only for consequential tradeoffs. Begin implementation only after Pingu confirms the shared understanding.
