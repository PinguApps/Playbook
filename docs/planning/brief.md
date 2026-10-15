# PinguApps.Playbook planning brief

Status: discovery and planning. No implementation has been approved.

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

These are the required planning inputs. They are not yet a claim that every topic has a verified example or a settled launch scope. A topic is not necessarily one final entry: some topics may split by goal or applicability.

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
- The guide is a reference, not an agreed redistribution source. Content authorship, attribution, and reuse policy remain open.

## Design tree

### Settled roots

- Identity: `PinguApps.Playbook` and `playbook`.
- Distribution: local .NET tool published through NuGet.
- Initial subject: .NET 11 Blazor coding guidance.
- Future product: expand the same package.
- Reference inputs: all 25 guide topics, seven additions, and three expansions.
- Engineering constraints: PinguApps conventions, central package management, matching GitHub Actions, Roslynk MCP mechanics.

### Round 1: independent decisions awaiting Pingu

1. **Product boundary.** Recommend a read-only guidance server; the calling agent inspects and edits the user's project. Playbook itself does not scan projects, run builds, or apply migrations.
2. **Version safety.** Recommend explicit target-release applicability so agents working on .NET 11 do not receive recommendations requiring .NET 12. The package can expand while preserving guidance for earlier targets; defaults and support lifetime remain to decide.
3. **Launch completeness.** Recommend making the initial release cover all 25 guide topics, seven additions, and three expansions. Topic splitting and overlaps can change the final entry count.
4. **Content authorship.** Recommend original explanations and examples informed by the guide and official sources, with attribution and links, rather than redistributing the guide's prose/code as the package content.
5. **Evidence standard.** Recommend official citations, exact verified SDK/package versions, compiled examples, and real runtime/browser checks for behavior compilation cannot prove. Keep preview and experimental status visible, and distinguish source claims from executable evidence.
6. **Repository baseline.** Recommend CodeStyle.Analyzers for structure, Template's generated application for current style/analyzer consumption, BlazorSitemap for modern NuGet workflows and packed-consumer verification, RazorStyle for PinguApps tool packaging, and Roslynk for MCP mechanics.
7. **Release automation.** The supplied package workflows include weekly publication of release drafts. Recommend matching that automation after Pingu explicitly authorizes the first release, while leaving the initial release manual. This is a proposal, not release authorization.
8. **Supported environments.** Recommend Windows/Linux/macOS and initial setup documentation plus real client smoke checks for T3 Code, Codex CLI, Claude Code, and VS Code Copilot. This does not yet specify testing every client on every OS. Narrow this if launch effort should be smaller.

### Dependent branches to explore next

- Product boundary -> tool versus resource surface, discovery, result contracts, errors, server instructions, companion instructions, offline behavior, and extensibility.
- Version safety -> target-version selection, retention/support lifetime, feature evolution, applicability and stability metadata, content identifiers, backwards compatibility, and package versioning.
- Launch coverage and evidence -> entry granularity, sample organization, valid older/newer comparisons, test matrix, source conflicts, maintenance, contribution workflow, and product evaluation.
- Repository baseline -> exact solution/projects, SDK and runtime support, analyzer settings, Reqnroll scenarios, packaging metadata, dependencies, and actual artifact checks.
- Release automation -> initial version, trusted-publishing setup, repository identity, draft categories, prereleases, scheduled publishing activation, and release readiness checks.
- Supported environments -> installation methods, MCP registration, command behavior, process lifecycle, logging, cancellation, troubleshooting, and platform/client acceptance checks.

Update this tree after each answer. Record resolved vocabulary in [GLOSSARY.md](../../GLOSSARY.md). Offer ADRs only for consequential tradeoffs. Begin implementation only after Pingu confirms the shared understanding.
