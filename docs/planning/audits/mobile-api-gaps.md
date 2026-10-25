# MAUI and platform API gaps: PIN-694

Reviewed **25 October 2026**, retaining the inventory's **.NET 11 RC2 preparation
baseline**. This is the assigned source/API audit, not delivery of native recipes,
GA reconciliation or a complete catalogue of Apple/Android OS APIs.

## Evidence and authority

The [manifest](mobile-api-gaps.sources.json) pins **183 complete official files**
by immutable URL and SHA-256: all **142 shipped/unshipped PublicAPI files** in
MAUI's RC2 tree, all nine Preview 1–7/RC1/RC2 mobile notes, and the implementation,
prior-source and test files needed for the named gaps. Inspecting all those lists
does not imply that every unrelated MAUI member is classified here. The bounded
register below reconciles the seven inventory rows assigned to PIN-694.

- MAUI RC2 rebuild `11.0.100-rc.2.26513.2`:
  **d32b79928709cf53b671336dabd0caef5a679fc2**.
- MAUI .NET 10 GA source `10.0.0`:
  **9d881e94b1c56692e40a16262897677a3bb46fcb**.
- Canonical core notes: **60871330fce2b3da383461be027ebdf3ce30f8fb**.
- Apple published RC2 `dotnet-11.0.1xx-rc2-12212`:
  **a75b11fe9cdbd66a6ddb8bfea120d006c1ec1e8d**. The two immediate pre-change
  Apple sources are separately pinned; these establish annotation changes, not a
  .NET 10 app compilation result.

[Inspect-MobileApiGaps.ps1](../../../eng/Inspect-MobileApiGaps.ps1) verifies all
source hashes and **21 source assertions**, then downloads and hashes the four
actual `Microsoft.{iOS,tvOS,MacCatalyst,macOS}.Ref.net11.0_27.0` packages at
**27.0.12212-net11-rc.2**. It reads reference metadata with PEReader; it never
executes inspected assemblies. The source assertions make the critical
distinctions replayable; passing them is not a behavior test.

Source IDs below refer to the manifest. `~` in a PublicAPI line is nullable-oblivious
notation, not an obsoletion marker. `*REMOVED*` is an analyzer removal record,
not a callable member. Shipped/unshipped lists are source contracts, not proof
that every declaration exists in every published target asset.

## Named API dispositions

| Finding | Actual contract and authored impact | Disposition and delivery owner |
| --- | --- | --- |
| Binding converter culture, filtered out of Preview 7's highlights (#35821) | Controls Core lists `ConverterCulture` on `Binding`, `MultiBinding` and `Internals.TypedBindingBase`; Xaml lists it on `BindingExtension`. Despite its namespace, TypedBindingBase is public. `binding`, `multibinding` and `typedbinding` use the selected culture in both conversion directions, defaulting to `CurrentUICulture`. `compiled-binding` forwards the markup value into generated bindings; .NET 10 `old-binding` hardcodes `CurrentUICulture`. This configures converter calls, not all parsing: `nullable` still uses `CurrentCulture` for ordinary conversion. | **Eligible, proposal A.** No existing scoped delivery slice explicitly covers converter culture. Do not present this as an automatic locale fix or silently change thread culture. |
| Empty `Entry.Text` bound to a nullable number (#33536, Preview 2) | `nullable` explicitly converts an actual empty string to null for nullable value types; `old-nullable` lacks that branch. `binding-tests` separately covers empty, whitespace, non-nullable numbers and re-entry. Whitespace is not automatically null; a non-nullable property does not become nullable. These tests were read, not executed. A custom converter may still implement deliberate validation or domain rules. | **Partly reverses the automatic exclusion, proposal A.** Plain upgraded binding behavior is automatic; removing an empty-to-null-only workaround is a concrete authoring decision. Retain converters with other responsibilities. |
| `IViewScreenshot` (#34350) | Essentials unshipped lists `CaptureViewAsync(object)` returning a task with nullable `IScreenshotResult`; `old-essentials` lacks the interface. `screenshot-dispatch` resolves **IScreenshot** from the handler's context, then checks capture support and **IViewScreenshot**. A separate IViewScreenshot-only registration is insufficient. Missing handler/view/service/capability yields null. Existing built-in native screenshot paths are distinct from the neutral backend fallback. | **Grouped into [PIN-913](https://linear.app/pinguapps/issue/PIN-913)**, `maui.build-community-platform-backends`. Extend its small backend demonstration with registration, one supported capture and one unsupported/null result; no standalone screenshot tutorial is needed. |
| Alert and gesture backends (#36633/#36655) | Neutral/platform Core lists expose `IAlertManager`, `IAlertManagerSubscription`, `IGesturePlatformManager` and its factory. `alerts` requires idempotent Subscribe and paired Unsubscribe; argument classes in `.Internals` remain public contracts. `gestures` requires a new manager per connection; the framework disposes it. This is authored lifetime/service integration. | **Grouped into PIN-913.** Preserve per-connection disposal and subscriptions; do not teach reflection or a single shared disposable gesture manager. |
| Backend assets, SingleProject, BlazorWebView and OnPlatform (#36653/#36654/#36658/#35901) | `singleproject`, `assets`, `blazor-backend` and the Xaml API lists expose first-party backend selection/asset and service integration. GTK/macOS/WPF names are recognized extension points, not first-party platform implementations. A neutral TFM is not universal native platform support. | **Grouped into PIN-913.** Keep required backend packages explicit; standalone community-framework catalogues remain excluded. Templates keep their separate existing owner. |
| Font readonly / Color immutability (#33824, Preview 2) | The changed type is **Microsoft.Maui.Graphics.Font** (`font`): Name/Weight/StyleType already had private setters in `old-font`. Its readonly declaration adds no public setter-removal migration. **Microsoft.Maui.Font** (`core-font`) is a distinct readonly type. Current `color` is a sealed record class; its generated equality/clone APIs appear in Graphics lists. Old ordinary-class inheritance cannot be retained by deriving from it. | **Font: excluded automatic/metadata residual.** No demonstrated public source break justifies a Font recipe. **Color: existing [PIN-903](https://linear.app/pinguapps/issue/PIN-903)** owns composition, invalidation and brush fills; do not mistake the historical PR's non-sealed record for the RC2 contract. |
| VisualStateGroupList / compatibility and handler removals | The Core API list records the removed IList-returning `GetVisualStateGroups` declaration. The inventory already includes that migration and the separately documented compatibility-package/handler changes. Their presence does not turn all compatibility-named Controls.Core types into removed APIs. | **Existing coverage retained:** PIN-903 for visual-state authoring; Android/Apple navigation and item-handler slices remain separate. This audit does not deliver or close those recipes. |
| Apple `HMError.QuotaExceeded` (#25024, Preview 4) | `apple-src/HomeKit/HMEnums.cs` guards value 106 with `!XAMCORE_5_0`. **Actual RC2 references still contain it** on iOS, tvOS and Mac Catalyst. macOS has no HomeKit.HMError type. Thus removal from Xcode 26.4 native headers is **not** removal from these .NET 11 managed references. There is no established replacement error member. | **Correct the inventory's removal implication.** Retained only as historical/diagnostic compatibility context; do not use this member to demonstrate a current native capability, invent a replacement or advise casting 106 to restore one. No new recipe is justified by the retained enum alone. |
| Apple native result/error nullability (#26464/#26531) | `apple-src/appkit.cs` adds nullable return metadata for WindowWithWindowNumber, main/key/modal windows, current/next events and related AppKit results; its prior source lacks the window-result annotation. `apple-src/corenfc.cs` explicitly allows absent NSError callback values, including NFCQueryNdefStatusCompletionHandler and ReadNdef's nullable message/error; the prior source has a non-nullable error parameter. Callers must handle absence; this is not removal or a guarantee that every error/result pair is non-null. | **Eligible, proposal B.** A narrow consumer-safety migration, separate from [PIN-932](https://linear.app/pinguapps/issue/PIN-932)'s owned failable-initializer generator work. Do not broaden that ticket silently. |

## Retained exclusions and release boundaries

| Original inventory ID | Retained boundary and evidence |
| --- | --- |
| `maui.automatic-size-and-ui-fixes` | Preview 2 TypedBinding allocation/delegate caching, Preview 5 trimming flags and automatic layout/input/Material styling repairs require no separate authored operation. Font readonly remains here. The nullable workaround decision above is explicitly carved out; custom Material handlers, resource composition and generated bindings already have named goals. |
| `android.automatic-build-and-startup-optimizations` | Preview 2 R8 response-file handling, RC1/RC2 assembly-store compression and incremental packaging, stable typemaps and automatic launch/build repairs remain automatic benefits. JNI preload exemptions, runtime/API minimums, R8 keep rules and manifest target-SDK choices remain included in their existing Android slices: authored configuration is not excluded merely because it affects speed. Preview 7's filtered native assembly-store work is not evidence of a required consumer migration. |
| `apple.build-without-full-xcode` | `apple-configure` identifies `--no-xcode`/NO_XCODE as a **dotnet/macios repository contributor** mode. It does not establish an app-project property or replace matched Xcode, native linking, signing or workload compilation. Keep the exclusion; never cite it as evidence that Apple samples built on this Windows machine. |
| `apple.independent-native-feature-catalogue` | Refreshing Xcode bindings for UIKit/AppKit/Metal and other native frameworks does not authorize exhaustive native OS tutorials. Native dependencies and genuine managed binding compatibility changes remain eligible, including proposal B. Apple MAUI AI abstractions retain their separate included/experimental scope. Deprecation of a native symbol is not proof of managed removal. |
| `apple.automatic-runtime-and-build-repairs` | Preview 3/6/RC2 linker, registrar, symbol/package and source-generator repairs remain excluded without a concrete authored migration. Scene lifecycle, publishing/runtime choices, manifest entries, framework metadata and failable factories retain their existing Apple owners. Nullable source-contract corrections are carved out above rather than dismissed as quality fixes. |

The generic [.NET 11 compatibility index](https://learn.microsoft.com/dotnet/core/compatibility/11)
explicitly says it is incomplete. Its lack of an entry cannot negate a source/API
change. The Apple RC2 source tree's four APICompat suppression lists are empty;
these are **not generated API diffs**, and emptiness is not proof that no APIs
changed. The cumulative Apple wiki still contains placeholders and older migration
text; it supplies no replacement for actual RC2 reference evidence.

[PIN-695](https://linear.app/pinguapps/issue/PIN-695) retains the distinct
12211/12212/workload-set and feature-status reconciliation. These four inspected
reference packs do not establish the MAUI rebuild's installed workload composition.
Keep preview labels on XAML C#/incremental reload and platform-specific experimental
publishing where appropriate; a public interface or RC artifact does not prove GA
stability. [PIN-966](https://linear.app/pinguapps/issue/PIN-966) still owns GA
reconciliation. No new toolchain/package pin or native CI lane is introduced here.

## Approval-only delivery proposals

[PIN-986](https://linear.app/pinguapps/issue/PIN-986) owns consolidation, overlap
refresh and Pingu's approval before ticket creation. These are proposed goals,
not new approved recipes or reserved milestone numbers. Existing scoped tickets
PIN-903, PIN-913, PIN-932 and PIN-695 were read in full; they do not already promise
the two outcomes below. No related Linear ticket was changed or created.

### A. Edit nullable values with an explicit converter culture

**Proposed ID:** `maui.edit-nullable-values-with-converter-culture`.
As an app developer, I want an editable value to use the intended language's
format and clear correctly, so users do not retain an old value accidentally.

One original two-way editing workflow compares .NET 10 with the exact .NET 11
MAUI package/platform, selects ConverterCulture in markup and compiled bindings,
and removes only an empty-to-null workaround whose purpose is now built in.
Cover Convert/ConvertBack, MultiBinding's culture route, default CurrentUICulture
versus ordinary CurrentCulture parsing, empty/whitespace/non-nullable distinctions
and preservation of custom validation. Avoid changing global thread culture.

Proposed **Agent**, existing **07 - Desktop and mobile guidance**, **L (5)**:
original older/newer native consumers, culture/clearing behavior, metadata/prose
and installed offline list/get proof. Requires a real native authoring route and
the exact composition from PIN-695, with SourceGen prerequisites recorded rather
than inferred. Verify one platform's edit/clear/re-entry behavior and label its
limits; no device or OS matrix. Refresh overlap before approval-based creation.

### B. Handle absent results and errors from native bindings

**Proposed ID:** `apple.handle-nullable-native-results`.
As an Apple app developer, I want to handle a missing native result or error
safely, so corrected binding annotations do not hide a crash in my app.

Deliver a bounded callback-consumer migration using an original CoreNFC result
handler, with AppKit's nullable window/event lookups documented as the related
source-contract family. Compare actual older/newer binding assets; respect OS
availability and callback nullability. Do not add a native API catalogue, preserve
removed APIs or claim a retained enum makes an absent native feature usable.
PIN-932 remains the separate factory-generation slice.

Proposed **Agent**, existing **07 - Desktop and mobile guidance**, **L (5)**:
older/newer native consumer compilation, null/success/error handling and packed
guidance. Requires the matched Mac/Xcode authoring route and PIN-695 composition;
real NFC/device claims require separately identified Human hardware evidence.
Pure callback checks may prove managed handling only. Approval must settle that
claim/evidence boundary before implementation; do not invent a new provisioning
task or call a mock a device test. Refresh overlap and native estimates at creation.

## Replay and remaining verification

Requires PowerShell 7.2+, HTTPS, no workloads. This command was verified starting
in `C:\Windows`; it downloads evidence into memory and changes no environment:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-95f644c3'
& ./eng/Inspect-MobileApiGaps.ps1
if (-not $?) { throw 'Mobile source/reference audit failed' }
```

Expect three **HMError fields=108; QuotaExceeded=106** lines, macOS fields=0 with
no QuotaExceeded value, then **PASS: 183 pinned sources, 21 source assertions,
four Apple reference packs**. Changed bytes, assertions or enum metadata fail.
No application, upstream unit test, native UI, device, simulator, workload install
or served recipe was built/run by this audit. Reference presence proves the named
declaration, not native support or native behavior. No MCP contract/content changes
are made, so no new guidance retrieval is claimed.

The [criterion and Linear ledger](../../verification/PIN-694.md) records coverage,
source provenance, scoped owner decisions and the verification actually run.
