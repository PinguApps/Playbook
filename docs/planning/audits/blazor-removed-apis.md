# Removed Blazor APIs: bounded reconciliation and delivery proposal

PIN-691 / `WEB_W18B` / `WEB-089`, reviewed **20 October 2026**. Source/API audit complete for the pinned baselines below. **Aggregate recipe delivery remains outstanding.** R1–R6 and R9 now have the focused deliveries below; audit completion itself does not establish comprehensive coverage or publication readiness.

## R5/R6 delivery, 28 October 2026

PIN-1150 delivers `blazor.allocate-web-renderer-identity`, edition 11.0;
PIN-1151 delivers `blazor.preserve-wasm-navigation-state`, edition 11.0.
Original paired Planetarium and MineralArchive WASM consumers compile against
actual .NET 10/11 dependencies. [Real browser observations](../../verification/blazor-renderer-navigation.md)
establish separate renderer attachment/dispatch and committed URI/history-state
sequences through intercepted links, Back and Forward. The old init assignment is
inert, the four-argument constructor has no ID parameter, and the allocation hook
uses only static state during base construction. BL0006 and internal browser
protocol risk remain explicit in discovery and detail. Neither notification
overload is recommended to applications; the old adapter is compile-only history.
Ordinary installed-package stdio inspection verifies both complete offline bodies.
R7–R9, comprehensive PIN-959 and actual-GA PIN-964 remain outstanding.

## R1/R2 delivery, 28 October 2026

PIN-1146 delivers `blazor.preserve-route-matching/11.0`; PIN-1147 delivers
`blazor.validate-with-scoped-services/11.0`. Both include original .NET 10/11 RC2
consumers, fresh compilation and actual HTTP/Chromium/scoped-DI execution records,
plus installed ordinary-tool stdio retrieval. ResourceAsset remains an unchanged
optional-parameter correction inside R1. The aggregate identity and existing
editions are preserved; R7–R9 and GA/comprehensive gates remain open.

## R3/R4 delivery, 28 October 2026

PIN-986 approved the nine follow-ons ([decision record](../audit-proposal-review.md)).
PIN-1148 delivers `blazor.bound-upload-read`, edition 11.0; PIN-1149 delivers
`blazor.create-typed-event-callback`, edition 11.0. These goal IDs leave the
`blazor.replace-removed-api` aggregate identity and all existing editions unchanged.
Both include complete original .NET 10/11 samples, fresh actual compilation,
[real browser observations](../../verification/blazor-streams-callbacks.md) and
installed ordinary-tool offline inspection. R3 removes dead options while
preserving the bounded/cancellable consumer; R4 compiles all twenty mappings
and executes representative synchronous/asynchronous component callbacks.
The stream-options rename and segment/buffer/timeout equivalence remain factual
gaps, not implemented APIs. R7–R9, PIN-959 and actual-GA PIN-964 gates remain
outstanding; no whole-family completion is claimed. The original proposal text
below is retained as audit history rather than rewritten as delivery evidence.

## R9 delivery, 28 October 2026

PIN-1154 delivers `blazor.resume-token-acquisition/11.0`, combining result
construction and URL/options consumption. Original HydrophoneArchive .NET 10/11
consumers compile with exact matching authentication packages. Selected
`behavior.evidence.json` records beside the samples retain real WASM browser
success, interaction, both exception overloads and no-options fallback execution.
The provider boundary is controlled; no external token issuance, protected API
response or recording download is claimed. The old four-argument no-options
result actually fails in .NET 10's legacy-URL fallback; RC2 uses the current URL.
Supported .NET 10 interaction remains demonstrated. Installed ordinary-tool
inspection verifies the complete canonical recipe. R7–R8, aggregate WEB-089,
PIN-959 and actual-GA PIN-964 remain open.

## Boundary and provenance

- Requirement: [PIN-691](https://linear.app/pinguapps/issue/PIN-691), SPEC.md at `0a8989814ea11346354464b845c79c15bb71c5e9` (stories 35–47, 126–127, 130, 133–134, 138–139, 158–160), approved `WEB_W18B` proposal at `b3778cc`.
- Complete official inventory: [Obsolete Blazor APIs removed, immutable source][article]. Its eleven replacement-table rows and additional `AccessTokenNotAvailableException` behavior statement are all reconciled below. Source revision `28f92a7807fc1f492f6a9739902ff1753d6ef832`; article date 4 June 2026; downloaded bytes SHA-256 `a2273d237563d58d6cf887fe45daa2c6665507156419d18d05a4ef23932e1719`. Read the article as a discovery list; several recommendations conflict with artifacts/source.
- Removal implementation: [aspnetcore#62755][removal], merge `f7bd408686ba9a4d35142f4ec81565b6f24b4bf7`. Its `PublicAPI.Unshipped.txt` tombstones explain the change, but are not proof of the current shipped API. For example, its ResourceAsset tombstone names a two-parameter constructor which still exists in both inspected releases.
- Actual previous baseline: SDK **10.0.402**, `Microsoft.AspNetCore.App.Ref` / `Microsoft.NETCore.App.Ref` **10.0.13**, `net10.0`; ASP.NET source [`0ef4bbfa3291b306a21e5001cb0491277bdd35bc`][source10].
- Actual current baseline: SDK **11.0.100-rc.2.26504.105**, both reference packs **11.0.0-rc.2.26504.105**, `net11.0`; ASP.NET source [`4a70870818bf5662f21b0c28f9a8c267dccb77f0`][source11]. This is RC2 evidence, pending GA reconciliation.
- For each baseline, also inspect the **matching-version** `Microsoft.AspNetCore.Components.WebAssembly` and `.WebAssembly.Authentication` NuGet packages' `lib/net10.0` or `lib/net11.0` compile assets. They are outside the shared framework reference pack. [Saved artifact/API inventory](blazor-removed-apis.assets.json) retains package URLs/SHA-256, inspected assembly SHA-256, exact CLR signatures, access, virtual/static flags, init modifiers and optional defaults. It expands grouped event/type removals mechanically.

The [audit script](../../../eng/Inspect-BlazorRemovedApis.ps1) reads metadata using the SDK's MetadataLoadContext; it never executes the inspected assemblies. Its selected type/member scope is explicit. A missing type means absent from the inspected Components assemblies, not absent from all NuGet products. No whole-framework public API diff is claimed.

## Complete reconciliation

Names below are C# signatures. `Components` means `Microsoft.AspNetCore.Components`; `Forms` means `Microsoft.AspNetCore.Components.Forms`; `Auth` means `Microsoft.AspNetCore.Components.WebAssembly.Authentication`. The saved JSON gives fully qualified CLR spellings and every expanded accessor/overload. Nullable annotations are explained from source; CLR type names alone do not encode them.

1. **Router — changed authoring path, R1.** `Components.Routing.Router.PreferExactMatches: bool { get; set; }` exists in 10.0.13; both accessors are absent in RC2. Remove the parameter/assignment, including `false`; do not replace it with a new switch. .NET 10 already uses exact-match route precedence regardless of this inert setting. [Pinned Router source][router].
2. **Assets — unchanged replacement, no separate delivery PR.** There is **no** `.ctor(string)` in either inspected release. Both have `Components.ResourceAsset(string url, IReadOnlyList<ResourceAssetProperty>? properties = null)`. A one-argument C# call uses that optional second parameter and remains valid. The article's claimed removal does not require changing that consumer call. Explicit properties/null also remain supported. Retain this correction in R1's migration inventory, without inventing another resource-asset recipe. [Previous][asset10] / [current][asset11] source.
3. **Manual data-annotations validation — changed authoring path, R2.** Removed: `Forms.EditContextDataAnnotationsExtensions.AddDataAnnotationsValidation(EditContext) -> EditContext` and `EnableDataAnnotationsValidation(EditContext) -> IDisposable`. Supported in **both** releases: `EnableDataAnnotationsValidation(EditContext, IServiceProvider) -> IDisposable`. Supply the application's actual scoped provider and retain/dispose the subscription. The first method's return is not the replacement subscription. This is Blazor EditContext integration, not new shared validation rules, async validation or object-graph generation. [Pinned implementation][validation].
4. **Remote file stream options — factual unresolved replacement claim, R3.** The public `Forms.RemoteBrowserFileStreamOptions` type, `.ctor()`, `int MaxSegmentSize { get; set; }`, `int MaxBufferSize { get; set; }`, `TimeSpan SegmentFetchTimeout { get; set; }` exist in 10 and are absent in RC2. The containing assembly is **Components.Web**, despite its Forms namespace. The article's proposed `BrowserFileStreamOptions` is not a public type in either inspected Components surface. The old [obsolete message][stream10] points to RemoteJSDataStream defaults, not a rename. Supported consumer boundary in both: `IBrowserFile.OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default) -> Stream`. Remove dead option registration; bound the file size/cancellation at the real consumer. Do not claim this reproduces segment/buffer/timeout knobs or expose internal `BrowserFileStream`. Any such equivalence stays unresolved until separately evidenced.
5. **Typed web events — changed authoring path, R4.** Entire `Components.Web.WebEventCallbackFactoryEventArgsExtensions` type removed. All **20** public static overloads follow these exact two forms for each of the ten types listed below:

   - `EventCallback<E> Create(this EventCallbackFactory factory, object receiver, Action<E> callback)`
   - `EventCallback<E> Create(this EventCallbackFactory factory, object receiver, Func<E, Task> callback)`

   `E` is respectively `ClipboardEventArgs`, `DragEventArgs`, `ErrorEventArgs`, `FocusEventArgs`, `KeyboardEventArgs`, `MouseEventArgs`, `PointerEventArgs`, `ProgressEventArgs`, `TouchEventArgs`, `WheelEventArgs`, all in `Microsoft.AspNetCore.Components.Web`. Use `EventCallbackFactory.Create<E>(object, Action<E>)` or `Create<E>(object, Func<E, Task>)`, present in both releases. Normal inferred `factory.Create(receiver, typedCallback)` may already bind to these instance members; explicit calls to the removed extension class need editing. Preserve receiver/async completion semantics. [Pinned EventCallbackFactory][callback].
6. **Renderer identity — changed authoring path, R5; article recommendation contradicted.** Removed: protected `Components.RenderTree.WebRenderer.RendererId.init` (`set_RendererId(int)` with required `IsExternalInit` modifier). Getter remains. Both releases have the same public constructor `(IServiceProvider, ILoggerFactory, JsonSerializerOptions, Components.Web.Infrastructure.JSComponentInterop)`: **no renderer-ID parameter**. The existing extension hook in both is protected virtual `int GetWebRendererId()`. Source calls it from the base constructor; the .NET 10 init accessor was a no-op. An override must not depend on fields initialized later by the derived constructor. Both compiler probes raise **BL0006** for framework-oriented RenderTree API use: this hook is not a general stable application contract. [Pinned renderer][renderer].
7. **WASM location notification — changed authoring path, R6.** Removed: public static `void Components.WebAssembly.Infrastructure.JSInteropMethods.NotifyLocationChanged(string, bool)`. Retained in both: `void NotifyLocationChanged(string uri, string? state, bool isInterceptedLink)`. It is explicitly **framework use only**, not a supported application navigation seam. Application code should use `NavigationManager.NavigateTo(string, NavigationOptions)` and observe `LocationChanged`; `NavigationOptions.HistoryEntryState` preserves deliberate history state. A custom framework/host adapter must match the framework's JS/managed assets and three-argument protocol, not simulate navigation by directly notifying an internal singleton. [Pinned implementation][location].
8. **Logout — changed authoring path, R7; article's no-op claim contradicted.** Removed entire `Auth.SignOutSessionStateManager`: public constructor `(IJSRuntime)`, virtual `ValueTask SetSignOutState()`, virtual `Task<bool> ValidateSignOutState()`. In .NET 10 these really write/read/remove session storage, and [RemoteAuthenticatorViewCore][authview10] uses them when history state is null. They are not universally no-ops. Supported `Auth.NavigationManagerExtensions.NavigateToLogout(NavigationManager, string logoutPath)` and overload adding `string returnUrl` exist in both releases and supply sign-out history state. Remove old DI/injections/calls while using the supported navigation flow. [RC2 auth view][authview11] rejects a non-null invalid history state but no longer uses the session-storage fallback when state is null; do not claim deletion preserves that old validation, or teach a custom CSRF bypass. [Previous manager][signout10] / [supported navigation][authnavigation].
9. **Custom authentication service — changed authoring path, R8.** Removed constructor on `Auth.RemoteAuthenticationService<TRemoteAuthenticationState, TAccount, TProviderOptions>`:

   `(IJSRuntime jsRuntime, IOptionsSnapshot<RemoteAuthenticationOptions<TProviderOptions>> options, NavigationManager navigation, AccountClaimsPrincipalFactory<TAccount> accountClaimsPrincipalFactory)`.

   Supported constructor in both adds the fifth parameter `ILogger<RemoteAuthenticationService<TRemoteAuthenticationState, TAccount, TProviderOptions>>? logger`. The signature permits null, but the proposed consumer should pass its real DI logger rather than lose diagnostics solely to compile. Preserve the generic type relationships and authentication registration/provider behavior. [Pinned service][authservice].
10. **Token result construction — changed authoring path, R9.** Removed `Auth.AccessTokenResult(AccessTokenResultStatus status, AccessToken token, string redirectUrl)`. Supported in both: `(AccessTokenResultStatus status, AccessToken token, string? interactiveRequestUrl, InteractiveRequestOptions? interactiveRequest)`. Use the actual request options when interaction is needed, with intentional nulls for a noninteractive success path; another argument is not merely padding. [Pinned result][token].
11. **Token result URL — changed authoring path, same R9.** Removed `string? Auth.AccessTokenResult.RedirectUrl { get; }`. Retained `string? InteractiveRequestUrl { get; }` and `InteractiveRequestOptions? InteractionOptions { get; }`. Use both when initiating interactive acquisition through `NavigateToLogin`, retaining scopes/return location and correct `RequiresRedirect` versus successful-token handling. This is the same goal as row 10, not a second recipe/PR.
12. **Token-unavailable exception — unchanged public API, behavior accounted for in R9.** `Auth.AccessTokenNotAvailableException(NavigationManager, AccessTokenResult, IEnumerable<string>?)`, `void Redirect()`, and `void Redirect(Action<InteractiveRequestOptions>)` remain in both assets. It does **not** gain a public `InteractiveRequestUrl` property. Its [implementation][tokenexception] now reads the result's `InteractiveRequestUrl` for the no-options fallback instead of the removed `RedirectUrl`. Existing exception callers normally retain `Redirect()`; custom token-result producers must migrate rows 10–11. Do not confuse this internal implementation change with a removed exception constructor/property.

## Delivery proposals awaiting backlog approval

Each numbered R item below is **one proposed delivery PR**, not an implemented example or a newly created Linear issue. Native scale confirmed by PIN-691 M=3, PIN-772/PIN-959 L=5 and scoped comparable S=2 issues; the approved proposal records XS=1, S=2, M=3, L=5, XL=8. Sizes include original authoring, paired compilation, focused proof, packaging and installed MCP checks. No item is XL; split/review again if actual discovery exceeds L rather than expanding this audit.

Proposed destination is existing **04 - Web application guidance**, in the recorded PinguApps team / Playbook project. On approval, query the complete scoped backlog including archived/completed items, allocate unused `04-DDD` prefixes after its highest number, and save/read back each estimate and milestone. R keys are local proposal references, not fabricated native issue numbers. No milestone, existing title, or related issue was changed here.

All nine PRs must meet these common acceptance criteria:

- Create wholly original previous/current consumer examples for the **same goal**, normally .NET 10/C# 14 versus .NET 11/C# 15 with actual matching ASP.NET assets. Old obsolete calls can be isolated as migration evidence with narrowly explained warnings; preserve a valid supported .NET 10 path too. These migrations do not imply that supported replacements first appeared in .NET 11. Do not add a ninth .NET 10 foundation. No copied or lightly renamed guide demos.
- Compile both actual targets, pin the SDK/dependencies/lock files and capture fresh evidence through the existing authoring workflow. The baselines above are test pins, not exact universal consumer requirements. Keep SDK/runtime/language/package asset/render-mode requirements separate, disclose RC-only proof and pending GA review, and verify any optional older-target package route before declaring it.
- Ship original complete offline prose/metadata and snippets drawn from canonical verified samples. State older/newer steps, limitations, original API/source provenance and claim-specific proof. Reuse the canonical shared validation/language explanation; own the concrete Blazor integration behavior.
- Pack and install the **actual** ordinary tool using the existing isolated-feed/cache harness from an unrelated directory. Over real stdio MCP, call `list_recipes` with explicit `targetVersion="11.0"`, find the approved family identity/title/**goal**, then call `get_recipe` for it with the same target. Assert full original Markdown, applicability, exact canonical snippets, evidence and equal JSON-text/structured responses; verify the package works offline. Do not add OS/client/native matrices. A passing unrelated property recipe is not family coverage.
- Preserve `blazor.replace-removed-api` as the aggregate inventory identity and record each delivered family against `WEB-089`. Finalize any separate goal IDs explicitly during backlog approval; never silently rename the aggregate or mark all its coverage complete when one family ships. Keep the aggregate outstanding until all approved qualifying families, including retained factual caveats, have real examples and package retrieval. Update PIN-959's coverage accounting through the normal scoped workflow.

### R1 — Keep route matching after removing the inert router setting — S (2)

- **APIs:** row 1 removal; supported Router without `PreferExactMatches`. Carry row 2's unchanged `ResourceAsset(string, properties = null)` correction in the migration inventory, with no independent asset feature.
- **Original scenario:** a tidal observatory's route editor opens `/stations/new` and `/stations/{code}`. The .NET 10 app still declares the inert router flag; the .NET 11 app removes it and retains the exact and parameterized page outcomes. A tiny asset consumer compiles the unchanged one-argument constructor in both, with/without explicit properties.
- **Applicability:** Blazor routing; static SSR and interactive route selection must be distinguished. Shared Components APIs, no WASM-auth/provider requirement.
- **Actual proof required:** compile both Razor applications and asset calls; real request/browser checks for the exact route and a station code in each claimed routing mode. Do not claim the old flag controls precedence. Complete common installed-MCP acceptance.
- **Sizing:** small markup migration with fixed route outcomes and a compile-only factual correction.

### R2 — Keep service-backed validation when migrating an EditContext — M (3)

- **APIs:** row 3's two removed methods to `EnableDataAnnotationsValidation(EditContext, IServiceProvider)`, including returned subscription disposal.
- **Original scenario:** an interactive sail-repair job editor has a service-backed validation attribute checking an allowed material catalogue. The obsolete providerless migration fixtures expose their service-resolution limitation; the working provider-aware .NET 10 and current consumers both reject unsupported material and accept a permitted one. Replacing the editor/model disposes the old subscription without duplicate validation messages. Do not pretend the providerless API could already supply the service.
- **Applicability:** manual EditContext + data annotations, scoped DI, interactive component lifecycle. Do not promise static-SSR lifecycle equivalence or nested/generated/async validation.
- **Actual proof required:** paired compilation, real form field/submit checks using the real scoped validation service, and replacement/disposal checks proving one current handler. Crosslink PIN-772's canonical graph validation and shared validation concepts; do not implement their separate rules here. Complete common MCP acceptance.
- **Sizing:** lifecycle/DI regression risk plus original behavior and packaging, bounded to one synchronous form.

### R3 — Bound an uploaded file without obsolete stream options — M (3)

- **APIs:** row 4's complete removed class/member set; supported `IBrowserFile.OpenReadStream(long, CancellationToken)`. Explicitly reject the unverified `BrowserFileStreamOptions` rename and any asserted one-to-one buffer/segment mapping.
- **Original scenario:** an interactive-server estuary survey uploads one instrument log. The old app carries dead stream-options configuration alongside a real size limit; the new app removes the dead registration and preserves the limit and cancellation at the stream consumer.
- **Applicability:** Interactive Server `InputFile`, SignalR/circuit lifetime; file limits differ from message/buffer limits. No disk-path trust, bulk-file service or generic upload platform is added.
- **Actual proof required:** compile old/new apps; use a real browser/file transfer to accept a small log, reject an over-limit log before processing, and cancel an in-flight read. Record measured size/observations; mocks cannot establish circuit transfer. Keep timeout/segment equivalence as an explicit factual gap unless proven. Complete common MCP acceptance.
- **Sizing:** one bounded file consumer and real browser transfer/cancellation. The unsupported recommendation is a caveat, not permission to invent an API.

### R4 — Build typed event callbacks without removed extensions — S (2)

- **APIs:** all twenty row 5 overloads to the two corresponding generic `EventCallbackFactory.Create<E>` instance forms.
- **Original scenario:** a greenhouse valve panel receives a synchronous keyboard selection and an asynchronous pointer confirmation through reusable components. The .NET 10 migration fixture explicitly references the obsolete extension class; the current example uses the generic factory.
- **Applicability:** interactive Blazor component libraries/manual render code; event callbacks require an interactive receiver, not static SSR event execution.
- **Actual proof required:** compile the two callback forms for **all ten E types** against their actual old/new assets (small generated compile fixture is acceptable). Execute representative sync/async callbacks through the component receiver, verifying argument delivery, awaited completion and one state update. A real browser checks keyboard/pointer interaction where UI behavior is claimed. Complete common MCP acceptance; do not author twenty separate recipes.
- **Sizing:** one generic migration pattern with a finite overload check and two representative behaviors.

### R5 — Allocate a custom web renderer's identity through its supported hook — M (3)

- **APIs:** row 6's removed protected init accessor; supported `GetWebRendererId()` override and unchanged four-argument constructor.
- **Original scenario:** a custom planetarium display host allocates separate renderer identities for two independent control surfaces. The previous example exposes the old inert init assignment honestly; supported old/new implementations obtain IDs through the hook and preserve event dispatch separation.
- **Applicability:** specialized `WebRenderer` subclasses, not ordinary component authoring. Preserve framework JS/managed asset alignment and constructor timing; no ID constructor parameter exists in this baseline. Disclose BL0006 and its compatibility risk in discovery/detail. Any necessary diagnostic suppression must be local to the deliberate framework integration, explained and verified; never suppress it across consumer code.
- **Actual proof required:** compile real subclasses for both targets; instantiate through a working JS/browser renderer integration, capture distinct assigned IDs and dispatch an event to each intended surface. A mocked interop call alone is not browser-renderer evidence. Demonstrate the override is safe during base construction. Complete common MCP acceptance.
- **Sizing:** one small custom renderer host; no general renderer framework or new rendering feature.

### R6 — Preserve WebAssembly navigation state through supported application APIs — M (3)

- **APIs:** row 7's removed two-argument framework notification; retained three-argument signature documented only for framework adapters. Application replacement is `NavigationManager.NavigateTo(string, NavigationOptions)`, `HistoryEntryState` and `LocationChanged`.
- **Original scenario:** a mineral archive's WASM shell opens a specimen route with a return-selection history state. The previous custom integration changes browser history and directly notifies the framework; the supported old/current samples navigate through NavigationManager and observe the committed location/state.
- **Applicability:** client-side WASM navigation, framework assets of the same release. Do not recommend calling either JSInteropMethods overload from an app or infer a notification itself performs browser navigation. Custom framework adapter protocol documentation stays explicitly internal-risk guidance.
- **Actual proof required:** compile both actual WASM consumers; real browser forward/back navigation proves URI, history state and observer sequence, including intercepted links. Verify no obsolete notification use in the supported path. Complete common MCP acceptance.
- **Sizing:** one state-preserving navigation flow. This does not implement relative-routing or CSP template work already owned elsewhere.

### R7 — Initiate WebAssembly logout through supported navigation — L (5)

- **APIs:** row 8's removed manager/type members and old injections/registrations to `NavigateToLogout(NavigationManager, string[, string])` with framework auth components.
- **Original scenario:** a wetland permit desk signs out a user and returns to the public permit list. Previous/current consumers retain that outcome while moving from session-storage setup to framework history-state navigation.
- **Applicability:** browser-hosted WASM remote authentication, actual registered provider and auth routes. This is not server cookie logout or authorization enforcement. Keep the .NET 10 fallback and RC2 null/non-null history-state difference explicit.
- **Actual proof required:** paired builds; a real browser plus an isolated local test identity provider verifies sign-out, callback and return URL, valid sign-out state, invalid non-null state and the version-specific no-state behavior. Capture actual request/history/session observations. No external account/provisioning is assumed; a fake provider can prove component branching only and cannot satisfy provider logout claims. Complete common MCP acceptance.
- **Sizing:** largest proposed slice because real browser/provider state and failure paths must be checked. If that fixture grows beyond one provider and one logout flow, split resource/proof scope for approval before implementing; never bury XL work in this audit.

### R8 — Construct a custom remote authentication service with logging — M (3)

- **APIs:** row 9's four-argument constructor to the exact five-argument generic constructor.
- **Original scenario:** an astronomy club's WASM badge portal wraps the remote authentication service to map an account badge claim. The old subclass forwards four services; the current subclass forwards the matching typed logger while preserving account mapping and token-request outcomes.
- **Applicability:** custom `RemoteAuthenticationService<...>` subclasses/factories, matching WASM Authentication package, options and account factory. Ordinary DI consumers need no manual constructor rewrite. No server-side Identity or new authentication system.
- **Actual proof required:** actual old/current compilation; resolve the concrete custom service using real DI/options/logging and exercise its existing account/token path with a controlled local JS auth adapter. Record that adapter as controlled integration, not external-provider login proof. If prose claims external login, require the actual isolated provider/browser check. Complete common MCP acceptance.
- **Sizing:** one constructor/subclass integration with retained account behavior; independent of R7's logout provider setup unless the author chooses a shared fixture.

### R9 — Resume token acquisition without legacy redirect members — M (3)

- **APIs:** rows 10–11's removed constructor/URL getter to the four-argument constructor, `InteractiveRequestUrl`, `InteractionOptions` and supported `NavigateToLogin`; row 12's unchanged exception/Redirect contract and changed fallback.
- **Original scenario:** a hydrophone archive requests access to download a protected recording. An available token proceeds without navigation; a missing grant carries return location/scopes through interactive acquisition. The old custom result producer uses the legacy constructor/property; the current one supplies the real interactive options.
- **Applicability:** WASM `IAccessTokenProvider` consumers/custom result producers, successful versus RequiresRedirect results. No claim that a URL alone grants a token, and no unvalidated return URL handling.
- **Actual proof required:** paired compilation; execute both success and redirect branches, verify scopes/options/return location and unchanged exception call sites, including the parameterless no-options fallback. Use a real browser to inspect history-state navigation; a controlled provider boundary does not prove external token issuance. Any issuance claim needs real provider evidence. Complete common MCP acceptance. Combine construction and URL migration in this PR; neither is complete alone.
- **Sizing:** one bounded token-result integration with two result branches and its exception fallback, separate from custom service construction and logout.

## Outstanding coverage and related work

`WEB-089` stays **included — aggregate delivery outstanding**, under the unchanged `blazor.replace-removed-api` planning ID. R1–R9 were subsequently approved under PIN-986; the R1–R6 deliveries described above establish only their bounded goals. Asset reconciliation adds no separate recipe because the real consumer signature/default is unchanged. The absent stream-options replacement claim remains visibly factual-unresolved; it does not block describing the evidenced consumer boundary with explicit limits.

[PIN-959](https://linear.app/pinguapps/issue/PIN-959) is the comprehensive coverage owner and already requires delivered audit follow-ons before publication; this document is its outstanding family ledger, not a coverage waiver. [PIN-708](https://linear.app/pinguapps/issue/PIN-708) explicitly cross-references assigned migrations, so it should reference this bounded removal audit rather than repeat it. [PIN-772](https://linear.app/pinguapps/issue/PIN-772) owns canonical nested/generated validation; R2 owns only the manual EditContext/service-provider migration. These three related issues were read with verified team/project scope and **were not changed**. No related issue was created. Current native blockers, not milestone order/shared files, determine implementation prerequisites.

The original source spec and approved proposal were resolved. Root PLAN.md, CONTEXT.md and CONTEXT-MAP.md are absent; the current SPEC, planning coverage/tool/verification/release documents, contribution guide, glossary and ADRs 0001–0004 constrain delivery. Historical planning statements that the host does not exist are stale; the checkout has the installed-tool harness and one property recipe. Current explicit-target/concise-discovery rules supersede historical defaults. None of these differences expands this audit into recipe implementation.

## Reproduce and inspect the audit

Requires PowerShell 7 and the pinned SDKs above (no optional workloads). If both SDKs are installed under the normal dotnet root, omit `-Net11Dotnet`; otherwise substitute its executable. The script downloads four pinned WASM packages from NuGet into a new OS temporary directory, hashes them, reads both sets of real assemblies, writes only the requested audit output, and removes its own temporary downloads. It changes no persistent environment variables. Network is needed for this authoring audit; that says nothing about the installed tool's offline behavior.

```powershell
Set-Location V:\_worktrees\Playbook\t3-08b2ae3a
pwsh -NoProfile -File ./eng/Inspect-BlazorRemovedApis.ps1 `
  -Net11Dotnet C:\Users\pingu\.dotnet-playbook-11\dotnet.exe `
  -CompareWith ./docs/planning/audits/blazor-removed-apis.assets.json
$assets = Get-Content ./artifacts/blazor-removed-apis.assets.json -Raw | ConvertFrom-Json
foreach ($baseline in $assets) {
  $baseline | Format-List sdk, version, target
  $baseline.types | Where-Object type -Match 'ResourceAsset|BrowserFileStreamOptions|WebRenderer' |
    ConvertTo-Json -Depth 10
}
```

Expected: `PASS` for both artifact hashes and selected signatures; the same optional ResourceAsset constructor in both; no proposed BrowserFileStreamOptions type in either; RemoteBrowserFileStreamOptions removed; no RC2 RendererId init modifier/setter and no renderer-ID constructor parameter. A hash/signature difference is a failure requiring review, not automatic permission to update the committed evidence. Full command results, focused compiler probes and limitations are recorded in [PIN-691 verification](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-691.md).

[article]: https://github.com/dotnet/AspNetCore.Docs/blob/28f92a7807fc1f492f6a9739902ff1753d6ef832/aspnetcore/breaking-changes/11/blazor-obsolete-apis-removed.md
[removal]: https://github.com/dotnet/aspnetcore/pull/62755
[source10]: https://github.com/dotnet/aspnetcore/tree/0ef4bbfa3291b306a21e5001cb0491277bdd35bc/src/Components
[source11]: https://github.com/dotnet/aspnetcore/tree/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components
[router]: https://github.com/dotnet/aspnetcore/blob/0ef4bbfa3291b306a21e5001cb0491277bdd35bc/src/Components/Components/src/Routing/Router.cs
[asset10]: https://github.com/dotnet/aspnetcore/blob/0ef4bbfa3291b306a21e5001cb0491277bdd35bc/src/Components/Components/src/ResourceAsset.cs
[asset11]: https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Components/src/ResourceAsset.cs
[validation]: https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Forms/src/EditContextDataAnnotationsExtensions.cs
[stream10]: https://github.com/dotnet/aspnetcore/blob/0ef4bbfa3291b306a21e5001cb0491277bdd35bc/src/Components/Web/src/Forms/InputFile/RemoteBrowserFileStreamOptions.cs
[callback]: https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Components/src/EventCallbackFactory.cs
[renderer]: https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Web/src/WebRenderer.cs
[location]: https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/WebAssembly/WebAssembly/src/Infrastructure/JSInteropMethods.cs
[signout10]: https://github.com/dotnet/aspnetcore/blob/0ef4bbfa3291b306a21e5001cb0491277bdd35bc/src/Components/WebAssembly/WebAssembly.Authentication/src/Services/SignOutSessionStateManager.cs
[authview10]: https://github.com/dotnet/aspnetcore/blob/0ef4bbfa3291b306a21e5001cb0491277bdd35bc/src/Components/WebAssembly/WebAssembly.Authentication/src/RemoteAuthenticatorViewCore.cs
[authview11]: https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/WebAssembly/WebAssembly.Authentication/src/RemoteAuthenticatorViewCore.cs
[authnavigation]: https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/WebAssembly/WebAssembly.Authentication/src/NavigationManagerExtensions.cs
[authservice]: https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/WebAssembly/WebAssembly.Authentication/src/Services/RemoteAuthenticationService.cs
[token]: https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/WebAssembly/WebAssembly.Authentication/src/Services/AccessTokenResult.cs
[tokenexception]: https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/WebAssembly/WebAssembly.Authentication/src/Services/AccessTokenNotAvailableException.cs
