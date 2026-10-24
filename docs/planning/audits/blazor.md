# Blazor and Razor authoring coverage: PIN-708

PIN-804 delivers the actually available streaming subset through `blazor.stream-agent-conversation/11.0`, using pinned Components.AI **0.1.0-preview.1.26466.103**, the real Interactive Server renderer and a clearly scripted IChatClient. PIN-808 delivers `web.protect-browser-form-posts/11.0`: real RC2 requests require MVC `[RequireAntiforgeryToken]`; legacy MVC and automatic Razor Pages filters still require tokens. [Original samples, source reconciliation, real browser/HTTP and installed offline evidence](../../verification/PIN-804-808.md) preserve these limits. Later AI tools/restore and GA reconciliation remain separate work.

PIN-801/802/803 deliver the three BreakwaterLab 11.0 editions for explicit
component property activation, no-result interop intent and direct interactive
MathML. The [original comparison and actual browser/installed-tool evidence](../../verification/PIN-801-803.md)
retain .NET 10's valid adapter/void/JS-owned formula paths. The custom hook is
new; InvokeVoidAsync already existed. The old dynamically inserted MathML
control demonstrates the HTML-namespace limitation. Trimming, general concurrency
and GA support remain unverified.

PIN-800 delivers `blazor.preload-needed-resources/11.0` (WEB-082/WEB_W16C).
The [original TernLedger proof](../../verification/PIN-800.md) separates explicit
hints from framework defaults and reconciles the serviced .NET 10/RC2 guard.

Reviewed **22 October 2026**. This closes a bounded artifact/source reconciliation,
not recipe delivery. At audit time every qualifying goal remained **delivery outstanding**.
Subsequent delivery is linked per goal below; the audit itself did not change served
recipes, MCP contracts, sample evidence or publication-completeness flags.

PIN-797 delivers `blazor.observe-browser-components/11.0` with original
LoomActivity .NET 10/11 WASM consumers and an official local OTLP collector.
Both releases expose public static AddComponentsMetrics/AddComponentsTracing;
the older registration remains valid. The .NET 11 host adds conditional automatic
registration, while its SDK's public diagnostics property defaults the three
runtime switches. [Actual compilation, browser receipt and correlation evidence](../../verification/PIN-797.md)
keeps source review, subscription and export/delivery distinct. This preserves
the shared `tracing.configure-source-rules/11.0` recipe and the eight foundations.

## Boundary and exact evidence

[PIN-708](https://linear.app/pinguapps/issue/PIN-708), planning key `API_BLAZOR`,
implements the audit in SPEC.md at `0a8989814ea11346354464b845c79c15bb71c5e9`
(stories 47, 48, 73, 79–84, 158–160), approved proposal `b3778cc`.
ADRs 0001–0004 retain one offline, read-only product, all qualifying authored
changes and experimental APIs, with GA-targeted guidance prepared from RC evidence.
The web inventory was a headline/source baseline, explicitly not a complete API diff.

The [replay script](../../../eng/Inspect-BlazorApis.ps1),
[artifact inventory](blazor.assets.json), [source pins](blazor.sources.json) and
[ownership rules](blazor.dispositions.json) extend that baseline mechanically:

- Previous: **SDK 10.0.402**, reference packs **10.0.13**, `net10.0`;
  ASP.NET source `0ef4bbfa3291b306a21e5001cb0491277bdd35bc`.
- Current: **SDK 11.0.100-rc.2.26504.105**, reference packs
  **11.0.0-rc.2.26504.105**, `net11.0`;
  ASP.NET source `4a70870818bf5662f21b0c28f9a8c267dccb77f0`.
- Inspect all public/protected declarations in the framework Components,
  Razor/Runtime and JSInterop assemblies, actual QuickGrid/EF adapter,
  WASM/auth/server/JSInterop.WASM packages and new AutoPause package.
  Include types, interfaces, generic variance/constraints, constructors, methods,
  accessors, properties, fields/constants, events, optional defaults, modifiers,
  custom/nullable/obsolete/experimental attributes and forwarders.
- Inspect actual SDK Razor compiler and Core MSBuild task assemblies as separate
  public tooling surfaces. Compare their SDK targets/props, package build assets
  and actual SDK-bundled Blazor Web/WASM/worker/service-defaults template inputs.
  Retain hashes for binaries/scripts and per-file added/removed text lines.
  Framework Tasks scheduling support is cross-referenced to PIN-707.
- Package versions normally match each framework baseline. **Components.AI is
  11.0.0-preview.7.26425.128**, not RC2: its only compile asset is `net11.0`,
  depending on Components.Web **11.0.0-rc.1.26425.128** and
  Microsoft.Extensions.AI.Abstractions **10.6.0**. Its repository commit is
  `dotnet/dotnet@3551975be08744f0418857c5bed8ab1545c5dd47`.
  Razor.Language **6.0.0** and DI.Abstractions **10.0.0** are metadata-resolution
  helpers, not new .NET 11 capabilities or evidence of a restored application.
- Gateway and Gateway.Cli are actual RC2 packages, with build/tool assets rather
  than an ordinary `lib/net11.0` application contract. The manifest preserves
  package repository commits, dependencies, hashes and tool-file hashes;
  no invented Gateway `lib` API comparison stands in for its manifest/CLI contract.

All declarations are read with MetadataLoadContext; inspected code is never
executed. This is a comparison of the stated artifacts, not every historical TFM,
RID, private/internal type or independently versioned product. The tooling text
diff is an ordinal **line-membership** diff, not a statement-order semantic diff;
paired file hashes retain changes in order/duplicates. Reconstruct the pinned
archives when reviewing complete changed files. Binary/compiler-generated metadata
differences are not automatically new callable operations.

The 60 immutable source files include **all nine** canonical ASP.NET release
snapshots (Preview 1–7, RC1–2), cumulative Blazor/Hybrid/localization/performance
includes and their root, the complete ASP.NET breaking-change directory/table,
migration root/Blazor include, .NET 11 compatibility/custom-obsoletion and global
SYSLIB indexes, ASPDEPR index, paired analyzer descriptors and focused implementation
sources. Source revisions: AspNetCore.Docs
`700188d8bdbc582411c4102f38afb90a59a5c91b`, dotnet/docs
`705acf6d28963fdb60e49ddd1c5437b3e0419a66`, dotnet/core
`ffed591b0fb8f9aab50ecc28f2c765049fb28d8f`. Each path, URL, SHA-256 and heading
inventory is saved. Root tables are boundaries, not substitutes for their articles.

## API and canonical-source reconciliation

Every raw API addition/removal must match an explicit ownership rule or replay
fails. Accessor/overload/enum/record-generated members group by developer goal;
counts are raw declarations, not recipe counts. The following also accounts for
source-only scripts, defaults, markup, manifests and removable workarounds.

| Family / actual change | Authored impact and disposition | Accountable delivery |
| --- | --- | --- |
| Label, DisplayName, InputBase metadata | Accessible IDs/captions; same model metadata, not separate accessor recipes. | Grouped: PIN-773. |
| Static SSR client rules, DisableClientValidation, ClientValidationProvider, ClientValidationRule/IClientValidationRuleProvider | Built-in immediate/nested validation and localized rules stay assigned; authored custom rule adapters have distinct server-metadata/browser wiring. | PIN-772/PIN-775; uncovered custom adapter **B06**. |
| EditContext.ValidateAsync, async field validators/progress/faults, ValidationRequestedEventArgs, Validate obsoletion | Await field and submit rules, cancellation and stale results; **Validate() is newly obsolete with the default diagnostic**, outside SYSLIB's custom-ID list. | Grouped: PIN-774; shared validation semantics PIN-748/PIN-706. |
| ITempData, supply-from-session/temp-data, cookie/provider options; template status/redirect messages | One-use redirect versus persistent session; early cookies for streaming and fail-fast missing-session setup. RC2 bound TempData values write back: consume with copy then clear. Interactive/no-HttpContext behavior differs. | Grouped: PIN-776; two original 11.0 recipes and actual .NET 10/11 comparison/HTTP/installed proof in [PIN-776](../../verification/PIN-776.md); GA pending; no additional persistence recipe per overload. |
| CacheView, CacheBehavior/CacheCondition/CacheVaryBy and built-in annotations | Boundary variation/expiration/HybridCache stays assigned. Authoring a reusable child with live holes or refusal is a distinct contract; cached parameters are replayed unchanged, not refreshed. AuthorizeView requires user variation; AntiforgeryToken/HeadOutlet stay live. | PIN-780 delivered boundary/storage policy with [real local provider proof](../../verification/PIN-780.md); **B07** retains reusable child contracts. |
| BasePath, RelativeToCurrentUri, GetUriWithFragment, NavLink | Base versus current-route navigation and fragment handling. Preview GetUriWithHash/EnvironmentBoundary spellings are not final APIs. BasePath moved to Components in RC2. | Delivered: PIN-777 three 11.0 editions with real .NET 10/RC2 non-root navigation and fragment checks; [evidence](../../verification/PIN-777.md). EnvironmentView delivered by PIN-795 as blazor.show-environment-specific-content/11.0; [server/client evidence](../../verification/PIN-795.md). |
| Virtualize anchor/identity/index/scroll APIs, variable heights, CSP spacer attributes | Explicit anchoring/reveal/CSP and removing existing compensation qualify. Automatic sizing alone does not add a recipe. Default overscan is Virtualize 15 versus QuickGrid 3, not a universal grid change. | PIN-785 initial/programmatic reveal delivered with original .NET 10/11 samples and real delayed-provider/visible-target proof; [evidence](../../verification/PIN-785.md). PIN-786 restrictive-style CSP delivered with original old/new consumers, real policy enforcement controls and visible distant rows; [evidence](../../verification/PIN-786.md). PIN-784 delivers anchor/identity/variable-height guidance with [real browser proof](../../verification/PIN-784.md). |
| QuickGrid URL state, QueryParameterNameOptions, row callbacks, virtualization forwarding | URL sorting/page isolation and link/aria-disabled CSS migration, row interaction and virtualized position. RC2 AnchorMode/ItemComparer metadata has no ASP0030 attribute. | PIN-781 URL state delivered with immutable Title keys/custom visible headers (no dedicated ColumnId), two-grid static/interactive proof and original .NET 10/11 comparisons; [evidence](../../verification/PIN-781.md). PIN-782 retains row-action ownership. PIN-783 delivers grid positioning, stable identity and Start anchoring with [real comparison evidence](../../verification/PIN-783.md). |
| Circuit.RequestCircuitPauseAsync and AutoPause package | Task<bool> reports request acceptance, not completed persistence; optional cancellation and hidden-tab delay need real browser proof. | Grouped: PIN-787, delivered with [native browser/state/resource and host lifecycle proof](../../verification/PIN-787.md). |
| BrowserOptions/ConfigureBrowser and three option groups | Server-supplied startup/reconnect/DOM/WASM policy. Final groups are InteractiveServer, StaticServer, InteractiveWebAssembly and static GetBrowserOptions(HttpContext). | PIN-788 delivered original .NET 10/RC2 comparisons, actual generated options/reconnect and separate-request sharing reproduction; [bounded evidence](../../verification/PIN-788.md). Separate JS startup shape delivered by PIN-789 with original .NET 10/RC2 Server/WASM legacy and Web App consumers; nested circuit/webAssembly and retained flat legacy compatibility have [real browser evidence](../../verification/PIN-789.md). The serialized server group is not the JavaScript circuit group. Configuration/culture remain PIN-793/PIN-794. |
| ServerComponentsEndpointOptions.ConfigureConnection | Auth expiry, transport and buffers; no invented safe universal values. | Included: PIN-790; `blazor.control-circuit-transport`, LockhouseSessions old/new real circuit proof; [evidence](../../verification/PIN-790.md). |
| WASM hosted services, runtime environment configuration, culture transfer, component metrics | Register browser work; distinguish shutdown from guaranteed browser unload. Environment data is visible to the client. Culture/config/OTel opt-ins require actual build/runtime evidence. | PIN-791 delivered original .NET 10/RC2 WASM startup/cancellation/owned-disposal proof and offline guidance; [evidence](../../verification/PIN-791.md). PIN-793/PIN-794/PIN-797 retain their separate scopes. |
| Actual web-worker template + generated WebWorkerClient/JS | New blazorwebworker identity; create/invoke/void, timeout/cancellation and termination. Void calls are awaited operations, not proof that errors or timeouts disappear. | Grouped: PIN-792. |
| Gateway/Gateway.Cli build targets, endpoint/development manifests, SPA fallback, proxy/service-defaults inputs | Replace DevServer; route actual backend traffic and standalone fallback, remove obsolete inspectUri/UseWebAssemblyDebugging (ASPDEPR011). Generated service defaults are not proof that Aspire orchestration has run. | Grouped: PIN-796. |
| IComponentPropertyActivator | Keyed/unkeyed injection and activation lifetime, distinct from cascading subscriptions. | Grouped: PIN-801. |
| MathML rendering, NavMenu external JS, custom event collision, removed implicit enhanced-nav preload | Authored markup/JS or removable exception qualifies despite small/no API delta. | PIN-799 delivered blazor.rename-custom-event/11.0 with original old/new native-browser proof and installed offline retrieval; [evidence](../../verification/PIN-799.md). NavMenu delivered: PIN-798 original BrackenMenu comparisons, both real module paths and strict CSP controls; [evidence](../../verification/PIN-798.md). PIN-803/PIN-800 remain separately assigned. |
| RenderFragment<T> contravariance | Reuse a base-item renderer for a derived-item slot; does **not** serialize generic delegates across rendering modes. | **B03**, separate from PIN-778's non-generic static fragment boundary. |
| AssetPath/AcceptsAssetPath/AssetPathAttributes, compiler metadata | Mark component parameters/custom HTML attributes for compiler asset expansion instead of scattering explicit lookup. | **B01**. PropertyMetadata.AcceptsStringLiteral delivered with [PIN-779's union-valued markup](../../verification/PIN-779.md); AssetPath remains B01. |
| CascadingParameterSubscription/TryAddCascadingValueSupplier<TAttribute> | Attribute-selected scoped suppliers, notification and disposal, rather than component property injection. ComponentState is a framework-facing seam. | **B02**; acknowledge BL0006 where emitted, no blanket stable/AOT guarantee. |
| RenderTreeBuilder.SetAttributeValue | Experimental **ASP0032**, indexed pending attribute-frame replacement; not an ordinary application rendering recommendation. | **B04**, advanced renderer authoring only. |
| CircuitOptions.JsonTypeInfoResolvers | Experimental **ASPNETCORE9004**, EditorBrowsable Never. RemoteJSRuntime and protected-browser-storage source insert resolvers in registration order before reflection fallback. | **B05**, no blanket circuit protocol or NativeAOT compatibility claim. |
| InputFile protected virtual Dispose(bool) | New subclass cleanup hook; existing public IDisposable behavior remains. | **B15**. |
| Razor compiler document constructors/output flags/GetCSharpDocument | Optional parameters preserve some source calls but change CLR constructor identity; custom compiled tool hosts must rebuild against matching tooling. | **B12**. |
| Intermediate-node CloneNode overrides | Authored custom Razor transformations can copy nodes; not a guarantee of deep-copying every mutable descendant. | **B13**. |
| SdkRazorGenerate Assemblies/UseSourceGenerator; SdkRazorTagHelper.UseSourceGenerator | Advanced build-task source-generator path; default remains false. `_RazorToolUseSourceGenerator` is an internal SDK switch, not a stable public application property. | **B14** for direct task authors; internal default alone excluded. |
| Razor task TaskEnvironment/IMultiThreadableTask and scheduling attributes | Built-in task scheduling is automatic; custom task concurrency contracts already assigned. | Grouped with PIN-707; no Razor-specific duplicate. |
| Razor language 11/default task TFM, net10-only embedded validation attributes | SDK/runtime selection and removal of embedded validation aliases/suppressions on net11. Keep net10 path separate. | PIN-772/PIN-748; SDK pin/dependency boundary PIN-707. |
| Razor runtime compilation AddRazorRuntimeCompilation overloads | Actual RC2 overloads gain ASPDEPR003; runtime behavior is not removed. | Grouped: PIN-840. |
| Removed Blazor APIs | All removals and factual replacement limitations retain the existing finite audit and nine proposals. | [PIN-691 R1–R9](blazor-removed-apis.md); no duplicate analysis/delivery claims. |
| BindConverter/JSInterop StringSyntax, LayoutAttribute trimming annotation, state-machine/nullability metadata | No new algorithm/operation. Respect formatting/trimming constraints; changed generated state-machine names do not remove methods. | Excluded as standalone recipes; exact declarations retained. |
| Razor.Runtime SetOutputContentAsync; EF QuickGrid adapter; JSInterop.WASM | First is generated state-machine metadata only; remaining selected public surfaces have zero differences. | No separate goal inferred. |
| Hybrid, gRPC, non-Blazor MVC/HTTP/security/OpenAPI entries in the complete sources | Hybrid note yields no new independent goal here. Non-Blazor public API reconciliation, including the Preview-7 Grpc.Swagger removal, belongs to PIN-709; do not turn its lack of a Blazor recipe into a no-change claim. | Existing web inventory; PIN-709, desktop/Hybrid owners where relevant. |

Pure automatic release fixes/performance (trimming size, HTTP3 scheduling,
allocation, default bug fixes) and contributor lists are excluded as independent
goals. Existing authored workarounds must still be removed/tested under their
assigned goal: list anchoring, session cookies, CSRF, render-fragment handling,
startup and grid behavior. Emscripten/workload configuration belongs to the
SDK/native publishing audit PIN-707, not a new Blazor recipe here. Identity schema,
passkey/auth UI and EF-generated template edits remain with existing web/data
owners; generated template demonstration rewrites do not each justify a recipe.

## Analyzer and obsolete ownership; retained drift

Paired descriptor sources mechanically show **BL0001–BL0009 → BL0001–BL0016**;
framework ASP descriptor IDs remain **26 → 26** (help-link construction refactor,
not 26 new diagnostics). Actual analyzer binary hashes are saved. Descriptor/source
review is not a claim that all diagnostics executed on a real Razor consumer.

- BL0010 → PIN-802 (void JS calls). BL0011 → PIN-784: pinned analyzer source still
  warns on ItemsProvider without ItemComparer, while Preview-7 notes say a comparer
  is no longer required. Keep the source/runtime distinction and reproduce the
  actual SDK/analyzer combination before prescribing a suppression or equality policy.
- BL0012 → **B08** (redundant StateHasChanged); BL0013 → PIN-813 (subscribe to
  changing authentication state); BL0014 → **B09** (loop capture); BL0015 → **B10**
  (public JS-invokable methods); BL0016 → **B11** (guard JS interop failures).
  BL0016's source tests for an enclosing try with catch, not an IsBrowser guard
  or proof that the chosen catch handles the real failure.
- ASPDEPR011 → PIN-796; ASPDEPR003 → PIN-840. The ASPDEPR overview lists only
  ASPDEPR002 and explicitly says it is incomplete: it cannot replace artifact
  enumeration. Historical Router.NotFound/LocationChangingContext and old auth
  obsoletions are retained, not newly invented .NET 11 recipes. Compiler
  RequiredMembers/RefStructs obsolete-error compatibility markers are not user
  deprecations. ASP0032/ASPNETCORE9004 are experimental opt-ins, not obsolete IDs.
- The complete .NET 11 custom-obsolete table has SYSLIB0064/0066, neither a new
  Blazor-owned entry; cryptography/Extensions retain PIN-686/PIN-746 ownership.
  The global SYSLIB/EXTOBS index remains PIN-689's cross-family enumeration.
  Default-diagnostic EditContext.Validate is owned here through PIN-774.
- Culture drift: the cumulative page still shows public
  WebAssemblyComponentsOptions.UseCultureFromServer, removed during Preview 7.
  PIN-794 delivered `Components:UseCultureFromServer`/localization with original
  .NET 10 manual and RC2 persisted-culture consumers. Real browser runs prove
  both culture names, aligned and independent-client policies, and actual
  satellite loading before rendering. The emitted RC2 browser configuration
  has no applicationCulture in these samples; initial en-US is explicit JS
  startup, while aligned en-GB/fr-FR comes from persistent state. See
  [exact proof and limits](../../verification/PIN-794.md).
- AI drift: the downloaded preview-7 assembly has UIAgent, ChatPage, content/rich
  text and handler primitives, **not** FunctionInvocationContentBlock,
  UIActionBlock, FunctionApprovalBlock, ActivityContentBlock/ActivityHandler,
  ToolBlock/ToolParameter/ToolResult, UIAgent<TState>/AgentState<TState>,
  IConversationThread, RestoreAsync or RegisterUIAction. Current documentation
  demonstrates these later contracts. There is no generator DLL in this package
  supplying the absent tool attributes. PIN-805–807 remain explicit availability
  gaps; adding AGUI.Client does not invent missing Components.AI types. PIN-804
  can verify the actually shipped streaming/rich-text subset, with experimental
  status disclosed even though the inspected types lack ExperimentalAttribute.
  Durable restart/restore is a distinct uncovered source goal, **B16**, held for
  a real artifact. No newer package, remote model or live AG-UI proof is assumed.
- Browser-options sharing, worker cancellation versus termination, final GA
  behavior, rendering-mode restrictions, custom-cascade framework seams and
  experimental hooks require the focused delivery checks below. This audit has
  no browser/provider execution and does not resolve those facts by assumption.

## Approval-only recipe ticket drafts

**No new Linear issues or recipes are created.** B keys are local draft references,
not allocated native issue numbers or published IDs. Existing scoped tickets were
searched before proposing these independent gaps. Proposed destination is the
existing **04 - Web application guidance** milestone; allocate native prefixes
only after backlog approval. Native scale is S=2, M=3, L=5, as saved on scoped
issues. Sizes include original samples, tests, documentation and package evidence.

All drafts require these common acceptance criteria in their eventual single PR:

1. Author wholly original fictional-domain .NET 10/C#14 and .NET 11/C#15 examples
   of the same goal, with the exact SDK/reference baselines above. Use the actual
   package/tooling pins and locked dependencies needed by that goal. SDK compiler
   hosts must use the matching SDK's compiler/Roslyn/task dependency set, not the
   unrelated Razor.Language 6.0 metadata helper. No ninth .NET 10 foundation.
2. Publish complete original offline Markdown/metadata, canonical sample snippets,
   official source references and exact target/package/rendering/platform/stability
   prerequisites. State an unavailable older equivalent honestly. Separate test
   pins from supported consumer requirements and RC proof from GA proof.
3. Build both real consumers, capture fresh compilation evidence, and execute the
   specified behavior checks; mocks/source review do not satisfy browser/renderer
   claims. Observe failing/previous behavior before the migration where applicable.
4. Build/pack/install the ordinary Playbook through the existing isolated feed/cache
   harness. From an unrelated directory over real stdio, call list_recipes with
   explicit targetVersion 11.0, inspect ID/title/**goal**, then get_recipe for the
   same goal/target. Assert full offline Markdown, canonical snippets, applicability,
   experimental opt-ins and matching structured/JSON-text evidence. No new CI matrix.
5. Update the disposition register; missing artifacts/compilation/behavior/Human
   proof block that delivery PR and publication. Audit closure never substitutes
   for these checks. Proposed IDs remain subject to review.

### B01. Expand asset paths through reusable markup — M (3)

Proposed `blazor.expand-component-asset-paths`. A fictional maritime museum's
image component marks a Parameter with AssetPath, and a custom element declares
AcceptsAssetPath on the compiler-discovered public AssetPathAttributes class.
Compare explicit old asset lookup with compiled expansion. Verify actual generated
C#, published/fingerprinted asset URLs, a non-root base path and real HTTP asset
retrieval; include a normal unmarked string control. Pin framework plus SDK Razor
compiler, inspect Restore/publish manifests, and prove arbitrary strings/unknown
assets are not silently rewritten. M covers compiler, manifest and request evidence.

### B02. Supply scoped values through a custom cascading attribute — M (3)

Proposed `blazor.supply-attribute-selected-state`. A fictional archive supplies
two same-typed collection contexts selected by an original attribute. Compare
explicit named cascades with TryAddCascadingValueSupplier<TAttribute> and an original
subscription. Verify selection, independent DI scopes, real rendered updates after
notifications and exactly-once disposal when a subscriber leaves. Inspect and
acknowledge actual BL0006 exposure around ComponentState; no blanket suppression,
thread-safety, trimming or NativeAOT promise. M covers renderer/lifetime integration.

### B03. Reuse a base-item rendering fragment for derived items — S (2)

Proposed `blazor.reuse-contravariant-item-fragments`. A fictional observatory reuses
a RenderFragment<Observation> for SpecialObservation. Old code uses an original
typed adapter; new code assigns the delegate through contravariance. Compile and
render actual derived content, including a null policy, and reject the unsafe
reverse assignment. Keep generic fragments within a compatible render context;
this does not extend PIN-778's cross-mode serialization. S covers one delegate rule.

### B04. Replace a staged renderer attribute deliberately — M (3)

Proposed experimental `blazor.update-staged-render-attribute`. An original custom
renderer for a fictional gallery replaces one pending attribute after building
frames. Compare an old original rebuild path with SetAttributeValue. Require
ASP0032 and any BL0006 acknowledgements localized to the deliberate framework
boundary. Execute real frame/render output checks, null/replacement behavior and
the documented wrong-frame/out-of-range failures; do not mutate an already
submitted render batch. M covers a constrained advanced renderer, not application
DOM mutation or a supported stable rendering abstraction.

### B05. Choose metadata for application-owned circuit JSON — M (3)

Proposed experimental `blazor.configure-circuit-json-metadata`. A fictional
planetarium sends one app-owned record through real Interactive Server JS interop
and protected browser storage. Old code uses the supported reflection fallback;
new code registers generated metadata in CircuitOptions.JsonTypeInfoResolvers.
Require ASPNETCORE9004, real server/browser execution and Data Protection. Verify
resolver order, one unhandled type's fallback, round-trip values and scope isolation.
Do not replace framework converters or infer full NativeAOT/circuit-protocol
support. M is bounded to those two demonstrated application-payload paths.

### B06. Connect a custom server form rule to browser validation — M (3)

Proposed `blazor.export-custom-client-validation-rules`. A fictional garden validates
an accession-code shape. Compare an original duplicate client rule with an authored
IClientValidationRuleProvider/ClientValidationProvider integration and matching JS.
Use real static SSR enhanced/plain forms and actual client feedback; prove the
server rejects a bypassed invalid POST, and distinguish disabled-client-validation
controls. Keep async business rules with PIN-774 and shared validation with PIN-748.
M covers a single custom rule, metadata mapping and real requests/browser checks.

### B07. Declare how a reusable child behaves inside a cached section — M (3)

Proposed `blazor.declare-component-cache-safety`. A fictional library's request-aware
badge uses CacheBehavior/CacheCondition. Compare an old explicit live boundary with
Rerender, Throw and a matching vary-by condition. Execute repeated real SSR requests
for two users; count lifecycle execution, demonstrate captured parameter replay,
live output, refusal without required variation and no cross-user leakage. Reuse
PIN-780's boundary/expiration explanation rather than reauthor it. M covers one
reusable child contract; HybridCache provider behavior remains separate.

### B08. Let component events schedule their own rerender — S (2)

Proposed `blazor.remove-redundant-render-notifications`. A fictional ticket display
compares redundant StateHasChanged in framework-managed events/lifecycle with its
removal. Pin the RC2 analyzer; prove BL0012 on the actual old consumer and absence
afterward, then exercise real UI updates. Retain an external timer callback that
still dispatches a necessary render. S includes the positive/negative distinction,
not a blanket instruction to remove every notification.

### B09. Bind each repeated action to its own item — S (2)

Proposed `blazor.capture-loop-items-safely`. A fictional aquarium's numbered feeding
controls compare a captured for iterator with an original per-iteration local.
Prove BL0014 on the actual generated consumer and its removal; click first/last
buttons in a real browser and assert distinct selected items. Retain safe foreach
behavior as a control. S covers the new diagnostic and one realistic closure bug.

### B10. Expose an intended JavaScript callback with the right visibility — S (2)

Proposed `blazor.expose-js-invokable-callbacks`. A fictional instrument panel calls
one .NET callback from JS. Prove BL0015 on a non-public JSInvokable method, make the
intended method public and call it through real JS with its declared identifier.
Dispose any DotNetObjectReference; show that this is interop visibility, not an
authorization boundary. S covers a bounded callback/diagnostic migration.

### B11. Handle JavaScript invocation failures without hiding application errors — M (3)

Proposed `blazor.handle-js-invocation-failures`. A fictional reading room invokes
a browser module. Prove BL0016 before and after a deliberately scoped try/catch;
exercise successful calls, an actual JS exception and an Interactive Server
disconnect/disposal race. Assert the chosen recovery and that unrelated failures
remain visible. Analyzer silence alone proves neither safe lifecycle timing nor
correct exception recovery. M includes the real race/renderer behavior.

### B12. Keep a custom Razor tool constructing and reading generated documents — M (3)

Proposed `razor.migrate-generated-document-contracts`. An original fictional catalogue
tool processes Razor source through the actual SDK compiler. Pin each SDK's compiler
and Roslyn dependency set. Compare the old constructor calls with the new optional
diagnostic/declaration/stub choices; rebuild and execute the tool, asserting generated
text, diagnostics and source mappings for a valid and invalid input. Test a previous
compiled constructor consumer against the new tooling to distinguish source-compatible
optional arguments from ABI compatibility. M covers one custom tool, not all IDE hosts.

### B13. Copy intermediate Razor nodes before a custom transformation — M (3)

Proposed `razor.copy-intermediate-nodes`. A fictional map renderer transforms an
original custom intermediate node. Use matching compiler artifacts and an original
old manual-copy alternative; implement/use the new CloneNode hook where appropriate.
Run the real Razor pipeline and prove which fields/annotations/children remain
shared versus copied, original-node preservation and generated output. Do not
advertise deep-cloning without proof. M includes extension-node and pipeline tests.

### B14. Select the hosted generator in an authored Razor build task — M (3)

Proposed `razor.choose-task-generation-path`. A fictional exhibit compiler invokes
the actual SdkRazorGenerate/SdkRazorTagHelper tasks with explicit Assemblies and
UseSourceGenerator. Compare the real older/default path with an explicit new task
invocation, build/run generated output and check invalid input diagnostics and
discovery dependencies. Pin actual tasks, rzc/compiler and MSBuild from each SDK;
record binlogs and generated artifacts. Demonstrate defaults; disclose that the
underscored SDK forwarding property is internal and not a stable app setting.
M covers one bounded advanced task consumer and remains blocked if no usable public
invocation route can be demonstrated.

### B15. Clean up resources owned by an InputFile subclass — S (2)

Proposed `blazor.dispose-input-file-subclass-resources`. A fictional restoration
workbench owns one cancellable upload-status subscription in a derived input.
Compare an original old wrapper lifecycle with overriding Dispose(bool), preserving
base cleanup. Compile both Razor consumers and use a real rendered file input;
remove it and verify callback/subscription cleanup once, with ordinary input behavior
still working. Do not claim cancellation undoes a completed upload. S covers one
extension lifecycle hook, independent of PIN-691's removed stream options.

### B16. Restore a completed agent conversation after restarting its view — M (3), availability blocked

Proposed experimental `blazor.restore-agent-conversation`. This source-qualified
goal is distinct from PIN-804's in-view history. **Prerequisite:** a real pinned
Components.AI artifact exposing IConversationThread/Thread/RestoreAsync and its
actual dependency closure; the reviewed preview-7 package does not. Once available,
a fictional observatory stores completed turns locally, rebuilds a view, explicitly
restores and continues it. Execute the real renderer/storage path; test incomplete
turn exclusion, no duplicate replay and protocol thread/run IDs only if a real
AG-UI loopback is included. Compare an original old app-owned transcript path.
M excludes remote model/provider operations and cannot publish with source-only
or fabricated interfaces. Keep the unavailable fact visible until real proof exists.

## Reproduce and inspect

Requires PowerShell 7, both exact installed SDK/reference roots and public NuGet/raw
GitHub access. The script does not install SDKs, restore projects, execute inspected
assemblies or alter PATH. Root overrides permit equivalent exact installations.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-2ef970e9'
& ./eng/Inspect-BlazorApis.ps1 `
    -OutputPath ./artifacts/blazor.replay.json `
    -CompareWith ./docs/planning/audits/blazor.assets.json
```

Expected: exact artifact/tooling/template/API/disposition replay and all **60**
immutable source hashes pass, followed by raw per-assembly counts. Missing assets,
unowned API declarations, changed evidence or source hashes fail. This block is
tested from an unrelated directory; it is an audit check, not recipe execution.
See [PIN-708 verification and tracker ledger](../../verification/PIN-708.md).
