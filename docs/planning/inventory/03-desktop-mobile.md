# Desktop, mobile, and platform workload inventory

PIN-923/PIN-924 delivery: `android.migrate-runtime-and-platform-targets` groups
`android.configure-build-toolchain` and authored trimming/R2R; intentional manifest
behavior remains `android.declare-target-sdk-intentionally`. Java peer and preload
goals retain `android.migrate-java-peer-binding` and
`android.control-native-library-preloading`, all edition11.0. Original CoastalProbe
samples compare .NET10 Mono with the .NET11 CoreCLR route. Exact inputs live in
`samples/CoastalProbe/{sources,artifacts}.json`; the source/reference check is
`eng/Test-CoastalProbe.ps1`. The .NET10 Android36.1.118 build produces the owned
peer library and app; current native build fails NETSDK1147. Current compilation,
optional JNI-library generation/load ordering and both editions' device execution
remain explicitly not-tested under PIN-699. RC2 artifact review does not establish
a compatible RC3 workload composition or GA evidence. PIN-1069 package replacement
and PIN-966 final reconciliation retain their separate scope.

PIN-927/PIN-928 delivery: `android.configure-predictive-back` and
`apple.adopt-scene-lifecycle` retain stable goal IDs with edition11.0. SkuaBack
compares application-attribute and manifest declarations. PuffinScenes/PuffinMaui
compare application startup with scene startup and acknowledged shortcut callbacks.
.NET10 examples compile on Windows; .NET11 native builds fail NETSDK1147.
Exact sources/artifacts and input freshness are retained under
`samples/LifecycleGuidance` and canonical verification records. Device gestures,
Apple launch/action delivery and current native compilation are not-tested under
PIN-699. The actual Android reference includes the property; reference compilation
is separate from native packaging/runtime proof. No GA or Human execution is claimed.

PIN-929/PIN-930 authoring: canonical TernPublish/TernProxy .NET10 examples compile
on Windows; current Apple RC2 handler/build sources are pinned in
samples/AppleRoutes/sources.json. Both current C# samples compile against
their exact RC2 iOS/Mac Catalyst reference assemblies. Current native builds fail NETSDK1147.
Native publish artifacts, universal slices, registrar/runtime execution and Apple
first-destination/CONNECT endpoint checks are explicitly not-tested under PIN-699.
No GA reconciliation or pending Human execution is claimed.

Source review: 16 October 2026, .NET 11 RC2 preparation baseline. These are proposed goal IDs and dispositions within the accepted scope, not published recipes or verification claims. Guidance targets GA; experimental capabilities remain included with labels. Compare the .NET 11 edition with .NET 10, using original examples.

The original 16 October pass recorded 87 disposition lines (before later delivery labels and grouping updates): 63 included goal candidates, eleven grouped subtopics, six excluded families, and seven unresolved items. Counts describe this register, not a final recipe total; some combined goals may split after applicability and sample work.

## Review boundary and evidence

Reviewed the canonical WPF Preview 1–7/RC1/RC2 notes; WinForms Preview 1/4/5/6/7/RC1/RC2 notes; MAUI Preview 1–7/RC1/RC2 notes; current MAUI and desktop overview pages; .NET 11 compatibility index; and current Apple RC2 workload release requirements. The expected WinForms Preview 2/3 raw-note paths returned 404 during the original pass. PIN-693 subsequently reconciled those exact paths through pinned component indexes/history and actual Preview 1/2/3 references/analyzers: both transitions have zero declaration changes. See the [WindowsDesktop audit](../audits/windowsdesktop.md) for the RC2 preparation baseline, source conflicts and retained delivery ownership. Cumulative overviews were used as discovery aids, not authoritative current artifact manifests. Release changelogs and linked PRs identify further audit seams; this is not a completed member-by-member API audit.

Primary source families:

- [WPF RC2][WPF2], [WPF RC1][WPF1], and [WPF overview][WPFOverview]. Earlier milestones use `release-notes/11.0/preview/preview1` through `preview7` in the same dotnet/core tree.
- [WinForms RC2][WF2], [WinForms RC1][WF1], [WinForms Preview 7][WF7], and [WinForms overview][WFOverview]. RC2 consolidates earlier feature contracts and corrects usage details.
- [MAUI Preview 2][M2], [Preview 3][M3], [Preview 4][M4], [Preview 5][M5], [Preview 6][M6], [Preview 7][M7], [RC1][M1], [RC2][M2RC], and [cumulative overview][MOverview]. MAUI notes explicitly cover its Android and Apple workload dependencies.
- [Apple RC2 published workload release][AppleRelease], [Apple known issues][AppleKnown], and [.NET 11 breaking-change index][Breaking]. The index itself says it is incomplete.

All proposed entries below have **source-reviewed evidence only**. No SDK/workload installation, sample compilation, device/emulator/UI execution, or packaged-tool test was performed by this audit. Entries marked included still require exact API/artifact checks and original compiling examples. Applicable .NET 11 SDK, workload set, package versions, platform TFM, native SDK/JDK/Xcode, minimum OS, and stability must accompany each finished recipe. A generic `net11.0` tag is insufficient for these examples.

The tool remains workload-free. Windows/macOS CI retains only the agreed brief install/stdio/list/get/shutdown smoke. This inventory does not create native UI, device, or emulator CI requirements. Compiling a platform example establishes build evidence, not native behavior evidence.

## Windows desktop

Unless stated otherwise, WinForms/WPF entries require Windows Desktop targeting (`net11.0-windows`, with `UseWindowsForms`/`UseWPF`) and Windows for actual UI behavior. Canonical shared examples use C#; runtime API availability in VB does not require duplicate examples unless authoring differs. The VB-specific entries below do have different authoring.

- **Included — `wpf.restore-clipboard-error-handling`.** For applications relying on clipboard retrieval exceptions, configure `Windows.Clipboard.ThrowExceptionsForGetAPIs` before reads and explain the default empty-result contract. This is a WPF `System.Windows.Clipboard` compatibility route, although the change is documented in WinForms RC2. Do not present it as a new WinForms clipboard API or prescribe enabling it universally. [Source][WF2], [implementation PR](https://github.com/dotnet/winforms/pull/15035).
- **Included — `winforms.adopt-modern-rendering`.** Select the application renderer before window creation and retain selected control overrides; distinguish visual-style enablement, `Classic`, `Disabled`, `Net11`, `Latest`, and inheritance. Layout must tolerate changed preferred sizes; use layout containers rather than fixed pixel assumptions. High Contrast constrains the effective renderer. Dark mode is a separate selection. [Source][WF2].
- **Grouped — `winforms.adopt-modern-rendering`: composite controls and editable padding.** Include effective `TextBoxBase.Padding`, RC2 Classic fallbacks for ToolStrip/DataGridView/PropertyGrid, and the two deliberate composite-renderer experiment switches as conditional caveats. Do not imply that assigning Net11 modernizes every control. Their automatic border/RTL/clipping repairs do not each need recipes. [Source][WF2].
- **Included — `winforms.author-adaptive-control-rendering`.** Custom control authors use protected effective-mode/change-impact hooks and base notification paths rather than caching ambient style resolution or treating protected hooks as public queries. Fold accessibility transitions and disposal of rendering resources into this goal. [Source][WF2].
- **Grouped into `winforms.author-adaptive-control-rendering` 11.0 — `winforms.follow-system-visual-settings`.** Read the settings snapshot and handle only relevant changed categories for custom drawing, animation, text scale, focus, and contrast. Prefer control-lifetime notifications within forms; explain static-event lifetime ownership for application services. [Source][WF2].
- **Included — `winforms.control-initial-form-reveal`.** Choose deferred or immediate reveal per application/form instead of hand-hiding startup windows. Distinguish process defaults, resolved form values, dark-mode automatic choice, splash exceptions, and top-level-only behavior. [Source][WF2].
- **Grouped into `winforms.control-initial-form-reveal` 11.0 — `winforms.configure-vb-application-defaults`.** VB Application Framework startup sets renderer/reveal values through `ApplyApplicationDefaultsEventArgs` or the derived application properties. Its Classic compatibility defaults differ from automatic WinForms reveal selection. Needs a VB example in `ApplicationEvents.vb`; do not copy C# startup into framework-managed VB startup. [Source][WF2].
- **Included — `winforms.batch-control-mutations`.** Replace manual nested Begin/End painting/layout pairs with disposable `SuspendPainting` scopes, acquiring the subtree scope before ambient mutations. Explain traversal/filter overloads and exception-safe resumption. [Source][WF2].
- **Grouped — `winforms.batch-control-mutations`: custom controls.** Cover custom core hooks separately from the recommended application extensions. The low-level `ISupportSuspendPainting` begin/end interface is infrastructure, not the standard user-code recipe. [Source][WF2].
- **Included — `winforms.present-toggle-options`.** Select `Appearance.ToggleSwitch` while retaining standard checked/binding/group semantics. Require an effective modern renderer and explain ThreeState, Classic, and High Contrast fallback. [Source][WF2].
- **Included — `winforms.customize-small-appearance-details`.** Author a grid sort glyph color and a print-preview empty/background presentation using their new appearance surface. These may split into separate goals after samples establish usefulness; describe exactly what `PrintPreviewControl.ForeColor` does rather than claiming it recolors printed content. [Source][WF2].
- **Included — `winforms.manage-fullscreen-experience`.** Own/dispose `KioskModeManager`, restore form state, choose focus-local shortcuts/pointer behavior, and request idle-sleep suppression where needed. This manages an application's presentation; Windows lockdown/Assigned Access is separate and is not supplied by this component. [Source][WF1], [RC2 contract][WF2].
- **Included — `winforms.preserve-designer-roundtripping`.** Keep Designer partials explicit and unrolled, move runtime logic to user partials, and repair WFO3000–WFO3010 structures rather than suppressing diagnostics. Includes C# collection expressions and both languages' initialization syntax constraints. New default diagnostics belong to .NET 11+, not NetFx. [Source][WF2].
- **Included — `winforms.wire-vb-designer-events`.** VB Designer-owned members require `WithEvents` with named `Handles` methods in the user partial; distinguish supported local-component wiring from forbidden Designer-member `AddHandler` wiring. Concrete VB-only authoring for WFO3011/3012. [Source][WF2].
- **Included — `winforms.return-intentional-property-values`.** Repair WFO2000 when a getter allocates a fresh reference: decide between stable cached identity and a deliberately creating method without silently changing public semantics. This diagnostic is not limited to Designer files. [Source][WF2].
- **Grouped — `winforms.adopt-modern-rendering`: preview-only TreeView migration.** Existing preview consumers must remove `NodeLeading` and use an appropriate supported sizing property; this is a preview-to-RC migration note, not a new .NET 10-to-11 capability. [Source][WF1].
- **Excluded — `desktop.automatic-reliability-fixes`.** WPF DWM/Fluent/WeakEvent fixes, automatic mixed-mode asset inclusion, Clipboard auto-conversion restoration, and WinForms automatic painting/input/dark-mode/designer repair do not change authored code by themselves. Reconsider only when an actual previous workaround can safely be removed. PIN-693 [retains the named residuals](../audits/windowsdesktop.md#retained-automatic-exclusions-and-exact-source-conflicts), separately preserving the authored clipboard, rendering, scopes and diagnostic decisions. [WPF RC1][WPF1], [WPF RC2][WPF2], [WinForms RC2][WF2].

## MAUI UI, markup, navigation, and integration

MAUI entries target the relevant `net11.0-*` platform TFMs with independently pinned MAUI workload/packages. Android, iOS, Mac Catalyst, and Windows support must be checked per member; MAUI does not establish WPF/WinForms or a universal Linux backend. tvOS/macOS base workloads are distinct from the MAUI supported-platform list. Native behavior checks need the corresponding platform, even where a cross-build is possible.

- **Included — `maui.choose-xaml-generation`.** Migrate from implicit XamlC assumptions to the SourceGen default, understand new build diagnostics and supported fallback configuration, and make source-generation prerequisites explicit for dependent recipes. This is a compatibility/authoring decision; do not make automatic faster loading a standalone recipe. [Preview 1](https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview1/dotnetmaui.md), [RC2][M2RC].
- **Included — `maui.simplify-xaml-namespaces`.** Remove standard root namespace boilerplate where implicit namespaces apply, retaining explicit mappings for ambiguity/custom types. Verify real generated XAML rather than calling all namespaces optional. [Source][M3], [PR](https://github.com/dotnet/maui/pull/33834).
- **Included — `maui.author-view-local-csharp-in-xaml`.** Use SourceGen `x:Code` only with the documented preview opt-in and disclose limitations versus code-behind. This is included despite preview status; exact GA status remains to reconcile. [Source][M4].
- **Included — `maui.bind-xaml-csharp-expressions`.** Author SourceGen C# expressions, namespace-qualified types, and attached-property access with `EnablePreviewFeatures`. XamlC/runtime inflation does not provide the expression path. Include relevant expression diagnostics; do not confuse automatic binding fixes with new syntax. [Source][M2RC], [PR](https://github.com/dotnet/maui/pull/35007).
- **Authored — `maui.author-trim-safe-relative-bindings` (PIN-901), edition11.0.** Original SignalDeck .NET10 typed-C# / .NET11 SourceGen comparison; Windows10 compilation retained, current generated/trimmed output explicitly not-tested under PIN-699. Packed list/get checks cover delivery, not native execution. Supply resolvable ancestor/reference types so SourceGen emits compiled bindings; distinguish unresolved names (compile errors) from known runtime-path fallbacks. `Self` and `TemplatedParent` are not automatically covered by this path. [Source][M7].
- **Authored — `maui.iterate-with-native-hot-reload` (PIN-902), edition11.0, preview.** SignalDeck CLI target/device and live-page walkthrough; current native compilation and live reload explicitly not-tested under PIN-699. Issue25295 records simulator watch fixed from Preview6; do not preserve the Preview4 linker workaround as RC2 advice. Use the Android/iOS `dotnet watch` workflow and the preview incremental XAML engine, with Debug/default and opt-out rules. Treat older iOS `MtouchLink=None` guidance as a version-specific known issue to recheck, not universal GA advice. Device selection and physical-device support belong here where they change the authored development workflow. [Preview 4][M4], [Preview 7][M7], [RC2][M2RC].
- **Authored — `maui.reapply-mutated-style-state` (PIN-903), edition11.0.** BeaconBoard retains the visual tree while reapplying mutated styles/active states, uses inferred VisualStateGroupList and composes named colours. The .NET10 Windows comparison replaces style/state objects. Current native compilation and all UI interaction are explicitly not-tested. [Preview 2][M2], [Preview 3][M3].
- **Authored — `maui.handle-long-press` (PIN-904), edition11.0.** BeaconBoard owns one token-aware awaited scale animation; the documented full hold lifecycle is Apple-only, with Android/Windows completion-only limitations explicit. Apple checks are source-reviewed, not-tested; no hardware handoff is pending. [Source][M3].
- **Grouped — `maui.fill-boxes-with-brushes` into `maui.reapply-mutated-style-state` (PIN-903), edition11.0.** Actual RC2 BoxView.Fill takes Brush and wins over Color in the shape fill; Background is distinct. Original gradient comparison uses Rectangle on .NET10. [Source][M5], [API PR](https://github.com/dotnet/maui/pull/31789).
- **Grouped — `maui.cancel-one-animation` into `maui.handle-long-press` (PIN-904), edition11.0.** Reviewed token-aware ScaleToAsync returns true for cancelled, false for completed; disposal follows await, terminal states/page exit request cancellation. Named per-operation ownership is explicit; no native execution claim. [Source][M5].
- **Included — `maui.navigate-with-path-parameters`.** Register Shell route templates and consume captured values with supported parameter mechanisms instead of hand-building query strings. Include absolute-navigation constraint and trimming implications of the selected receiving mechanism. [Source][M7].
- **Included — `maui.migrate-navigation-handlers`.** Migrate Android Shell and Apple NavigationPage/TabbedPage/FlyoutPage customizations to current handlers or explicitly retain supported renderer fallbacks. NavigationPage on Apple changes event timing: on-screen work belongs in `NavigatedTo`. Keep removed compatibility-package APIs distinct from compatibility-named types still in Controls.Core. [Preview 6][M6], [Preview 7][M7], [RC1][M1].
- **Grouped — `maui.migrate-navigation-handlers`: Apple Shell opt-in.** `UseiOSShellHandler` opts into the complete hierarchy; the compatibility ShellRenderer remains the RC2 default. Do not extrapolate Android's default switch to Apple Shell. [Source][M2RC].
- **Included — `maui.migrate-custom-items-handlers`.** Move Apple CollectionView/CarouselView custom handlers to Items2 implementations; obsolete handler types do not mean the controls are obsolete. Windows CollectionView2 is a separate default/fallback path and its legacy handler is not covered by Apple's obsoletion. [Preview 6][M6], [RC2][M2RC].
- **Included — `maui.remove-compatibility-package`.** Replace explicit `Microsoft.Maui.Controls.Compatibility` package/renderers before upgrading. Do not remove all compatibility-named Controls.Core types by association. [Source][M6].
- **Included — `maui.show-tab-badges`.** Author/update TabbedPage badge text/colors with the new attached properties; record Apple system-color overrides on newer OS versions. [Source][M1].
- **Included — `maui.control-swipe-action-colors`.** Set explicit text/icon colors and themed bindings; restore deliberate tinting for non-font images now retaining their original colors. [Source][M1].
- **Included — `maui.customize-material-handlers`.** Delivered in edition 11.0 by PIN-909. Use public Android Material 3 helper/handler types for custom styling instead of reflection against internals. Automatic stock restyling alone is excluded. [Source][M5].
- **Included — `maui.describe-navigation-for-accessibility`.** Author back-button spoken labels independently from visible titles; include intentionally exposed Windows layout semantics where layout peers should participate in the automation tree. [Preview 5][M5], [Preview 6][M6].
- **Included — `maui.call-dotnet-from-hybrid-js-with-aot`.** Delivered in edition 11.0 by PIN-910. Use the generic invocation target plus generated JSON context; analyzers/interception must remain enabled. Distinguish JS-to-.NET invocation from other JS calls, and avoid claiming the old reflection overload is trim-safe. [Source][M6], [exact mechanism][MOverview].
- **Delivered — `maui.cache-hybrid-static-assets` (edition 11.0).** Select BlazorWebView local content cache headers explicitly, retaining no-store where needed and documenting unsupported request classes/cache boundaries. Shared with the Blazor Hybrid area; not equivalent to enabling arbitrary authenticated-response caching. [Source][M1].
- **Delivered — `maui.create-csharp-ui-project` (edition 11.0).** Use the RC2 C# UI template option while retaining resource XAML and the XAML-only sample-content restriction. No third-party C# UI library is required. [Source][M2RC].
- **Delivered — `maui.build-community-platform-backends` (edition 11.0).** For backend authors, use new first-party alert/gesture/BlazorWebView/asset/SingleProject contracts and recognized `OnPlatform` names. This covers the MAUI extension seam, not full standalone tutorials for every community backend. Verify exact public signatures and supported neutral-TFM behavior before sample authoring. [Source][M7].
- **Included — `maui.compose-maps-declaratively`.** Delivered by `maui.compose-maps-declaratively` edition11.0 with original QuayAtlas comparisons, official source/package review and explicit native not-tested limits. Author initial region/coordinate conversion, overlay visibility/draw order, and map shape interactions without custom native map glue. [Preview 2][M2], [Preview 3][M3].
- **Included — `maui.interact-with-dense-map-markers`.** Delivered by `maui.interact-with-dense-map-markers` edition11.0 using the same canonical QuayAtlas source. Group pin clustering, custom icons, long presses, pin info windows, user-location events, animated region changes, and span computation into an original multi-step goal; split if applicability diverges. Android JSON styling is conditional, not cross-platform. [Source][M3].
- **Included — `maui.enable-windows-maps`.** Delivered by `maui.enable-windows-maps` edition11.0; placeholder registration compiled against actual10/11 Windows assets; exact10.0.110 already contains the Windows handler, native RC3 Windows MAUI unavailable (NETSDK1147). PIN-935 service proof has not run; PIN-699 applies exact not-tested exclusions. RC2 source camera-dispatch mismatch is disclosed rather than treated as supported. Register the Windows Azure Maps token and use the supported map/pin surface. Windows lacks several overlay/location/info-window capabilities; do not let cross-platform samples silently rely on them. Requires Windows App SDK/map-service prerequisites and external credentials for actual service execution. [Source][M5], [PR](https://github.com/dotnet/maui/pull/34138).
- **Included — `maui.authenticate-with-platform-passkeys`.** Delivered as edition 11.0 by PIN-916: original Heron Windows adapter compiles against the actual RC2 asset; server verification remains mandatory. Native host build fails NETSDK1147 and device ceremony is not-tested under PIN-699. PIN-936 remains open, not executed. Configure native trust/signing and exchange server-supplied WebAuthn JSON with the platform APIs. Client ceremony is not server verification. Record Android API34, iOS16, Windows10 1903 and effective Mac Catalyst17 minimum requirements, supported devices, and capability checks. [Source][M7], [current prerequisites][MOverview].
- **Included — `maui.recover-interrupted-media-picking`.** Persist the application workflow and consume/clear/discard Android recovered media after process recreation; ordinary in-process task completion is a different path. Check current complete API set against RC2. [Source][M6], [current set][MOverview].
- **Included — `maui.save-captured-media-to-gallery`.** Set the explicit capture option and platform permissions; distinguish capture from picking, and Android/Apple support from Windows/Tizen no-op behavior. [Source][M7].
- **Included — `maui.choose-status-bar-appearance`.** Combine explicit icon theme and optional Android chrome background integration without confusing foreground icons with bar backgrounds or app theme. Android/iOS icon support and Android-only project opt-in have different applicability. [Preview 7][M7], [RC1][M1].
- **Included — `maui.route-single-instance-activations`.** Handle Windows AppInstance activation and redirection, including external callback delivery to the existing instance. Record Windows-specific registration/lifecycle requirements. [Source][M7].
- **Included — `maui.request-notification-permission`.** Use the common PostNotifications permission on iOS; permission grant is not notification scheduling. [Source][M3].
- **Included — `maui.inject-permission-checks`.** Use `IPermissions`/`Permissions.Current` for replaceable permission services instead of hard-wiring static calls. Verify platform service replacement semantics and avoid turning this content sample into a broad test suite. [Source][M6].
- **Included — `maui.filter-location-updates`.** Set movement distance and consume altitude/reference-system metadata correctly where Android MSL reporting changed. Distinguish source units and per-platform support from automatic location quality improvements. [Source][M6].
- **Included — `maui.author-themed-app-assets`.** Group Android monochrome adaptive-icon layer, dark-mode splash metadata, and intentional resize quality; split their platform-specific authored asset routes within the recipe as necessary. [Preview 4][M4], [RC1][M1].
- **Grouped — `maui.author-themed-app-assets`: Apple Icon Composer.** Author one platform-conditioned icon item per target; MAUI `.icon` bundle support arrives in RC2 while the underlying Apple workload support appeared earlier. Preserve SVG/PNG for other targets. [Source][M2RC], [underlying workload][M6].
- **Grouped — `maui.replace-color-inheritance` into `maui.reapply-mutated-style-state` (PIN-903), edition11.0.** Original .NET10 derived NamedSwatch comparison compiles; current wrapper contains sealed record Color. No binary-channel-format claim or earlier immutability grouping. [Source][M1].
- **Included — `maui.use-apple-intelligence-abstractions`.** Delivered as experimental edition 11.0 by PIN-922 with original HeronNotes release-specific Apple samples and exact package/source review. Both native builds fail NETSDK1147 (ios); compilation/model execution is not-tested under PIN-699. MAUIAI0001 retained. Audit the shipped Essentials.AI `AppleIntelligenceChatClient` and embedding abstractions as a .NET MAUI release-family surface. Label `MAUIAI0001` experimental and document OS/device/model availability and required package dependencies. This is distinct from cataloguing every native Apple Intelligence binding. [Source][M5].
- **Delivered — `maui.include-avalonia-template-output` (edition 11.0).** Cover the first-party MAUI template/configuration integration under the accepted release-family boundary. The Preview7 `--with-avalonia` path adds `Avalonia.Controls.Maui` and `Avalonia.Controls.Maui.Desktop` with a neutral `net11.0` desktop target; RC2's multi-project template has an optional separate Avalonia desktop project. Record those exact third-party package prerequisites, generated versions, platform setup, and option/sample-content restrictions in the finished recipe. Standalone Avalonia features/tutorials remain outside scope. The QuayIntegration check pins the actual template and Avalonia RC2 packages, exercises generated option combinations, and compiles the original neutral desktop consumer. Generated native heads and visual behavior are explicitly untested; see [delivery evidence](../../verification/quay-integration.md). [Preview 7][M7], [RC2][M2RC].
- **Excluded — `maui.automatic-size-and-ui-fixes`.** Lazy resources, trimmed unused CSS, faster TypedBinding, deferred handlers, stock Material 3 restyling, and automatic reliability/memory/layout fixes do not qualify solely because they improve upgraded applications. [Preview 2][M2], [Preview 3][M3], [RC2][M2RC].

## Android workload

These entries also apply to non-MAUI .NET Android applications where their workload APIs/configuration apply. Use actual `net11.0-android*` targets. Runtime-specific prerequisites must not be inferred from a MAUI package version.

- **Included — `android.migrate-runtime-and-platform-targets`.** Update minimum API to 24, remove x86 assumptions, and replace runtime-incompatible Mono AOT options when using CoreCLR/NativeAOT. Runtime defaults do not themselves justify performance recipes; explicit compatibility/configuration changes do. Verify supported runtime choices in final artifacts instead of perpetuating old `UseMonoRuntime` advice. [Preview 5][M5], [RC2][M2RC], [overview][MOverview].
- **Included — `android.control-native-library-preloading`.** Select per-library or global JNI preload exemptions where application initialization requires explicit native load order. [Source][M2].
- **Included — `android.migrate-java-peer-binding`.** Audit trimmable type-map restrictions, package-name compatibility, Export callbacks, custom IJavaObject requirements, removed reflection-based NativeAOT mapping, and obsolete JavaFinalize replacement. Binding authors need original minimal binding samples, not generic performance claims. [Preview 5][M5], [Preview 7][M7], [current JavaFinalize note][MOverview].
- **Authored — `android.handle-http-failures-and-cancellation` (edition11.0, Breakwater; PIN-925/PIN-926).** Replace legacy WebException-only handling with HttpRequestException/cancellation contracts where AndroidMessageHandler changed, retaining access to inner diagnostics. Automatic redirect/disposal crash repairs alone are not separate recipes. [Source][M6].
- **Included — `android.configure-predictive-back`.** Use the new ApplicationAttribute setting where opting into Android's predictive-back behavior changes application declarations; document required target/platform behavior instead of claiming a universal navigation replacement. [Source][M5].
- **Authored — `android.preserve-r8-sensitive-members` (edition11.0, Breakwater; PIN-925/PIN-926).** Review Java name lookup/consumer keep rules for the RC2 optimizing private-member-obfuscation default; use the explicit compatibility route when necessary. Managed-assembly rewriting is not part of RC2. Do not label all R8 use experimental based on that unshipped design. [Source][M2RC].
- **Included — `android.declare-target-sdk-intentionally`.** Repair partial uses-sdk manifest assumptions: generated target SDK now follows the project setting rather than accidentally falling back to minimum SDK. Keep minimum device support distinct from target-SDK-gated behavior. [Source][MOverview], [PR](https://github.com/dotnet/android/pull/12290).
- **Included — `android.configure-build-toolchain`.** Pin supported JDK/Gradle and platform targeting when adopting the new workload: RC2 includes API37.2 and supports JDK21/25, with Gradle9.1+ needed when invoking it through JDK25. Record exact compatibility instead of a generic instruction to upgrade everything. [Source][M2RC].
- **Grouped — `android.migrate-runtime-and-platform-targets`: trimming and R2R.** Explain XA0119 and the trimming requirement if retaining authored ReadyToRun settings. Full-R2R opt-in is not automatically a standalone recipe unless a meaningful deployment decision/sample demonstrates its tradeoff. [Source][M2RC].
- **Grouped — mobile diagnostics/development workflow.** Crash-writer opt-in, `dotnet run` target/device selection, and intentional Fast Deployment fallback belong in a concrete diagnosable-development goal, alongside corresponding Apple settings; automatic adb/build speedups alone are excluded. Candidate shared ID `mobile.capture-native-dotnet-diagnostics`. [Source][M7].
- **Excluded — `android.automatic-build-and-startup-optimizations`.** Faster resource processing, assembly-store compression, launch no-GC region, default fast deployment, stable typemaps, and automatic unused-method trimming do not need recipes without authored work. [RC1][M1], [RC2][M2RC].

## Apple workloads: iOS, tvOS, Mac Catalyst, macOS

These are base workloads, not a claim MAUI runs on every listed target. RC2's published Apple release requires Xcode27.0 on macOS26.6+ and lists package versions `27.0.12212-net11-rc.2`; exact OS deployment floors vary by workload and member. Test sample compilation against the actual selected workload set; do not copy old Xcode26.x preview prerequisites. App signing/entitlements remain separate from compiling ordinary managed source.

- **Included — `apple.adopt-scene-lifecycle`.** Existing iOS/Mac Catalyst apps built with Xcode27 SDKs need scene manifest/delegate registration and moved lifecycle/launch-option handlers. MAUI needs its scene delegate subclass; app-action handlers must acknowledge completion, including false for unhandled actions. A UIKit scene is not a request to force multi-window UI. [Source][M2RC], [published breaking requirement][AppleRelease].
- **Delivered with native exclusions (PIN-929) — `apple.pin-deployment-and-architecture`.** Update effective minimums (including MAUI Mac Catalyst17), and explicitly choose RIDs/universal builds where RC2's early deployment-target evaluation changes Apple desktop architecture defaults. Place required values early enough in MSBuild evaluation; a late project assignment may not select the expected artifacts. [Source][M2RC].
- **Delivered with native exclusions (PIN-930) — `apple.configure-native-http-proxies`.** Use NSUrlSessionHandler proxy controls and separate handlers where first-destination session proxy selection would be wrong. Document HTTPS CONNECT credential limitation; include digest support as a separate capability within handler-auth configuration where relevant. [RC2][M2RC], [digest addition][M4].
- **Delivered with native exclusions (PIN-931) — `apple.author-plist-from-msbuild`.** Add/replace/remove typed Info.plist values through AppManifestEntry items rather than editing a shared plist for every configuration. Verify allowed Boolean/string/string-array syntax against the package. [Source][M2RC], [PR](https://github.com/dotnet/macios/pull/26240).
- **Delivered with native exclusions (PIN-932) — `apple.bind-failable-initializers`.** Binding authors model nullable Objective-C construction with FactoryMethod and return NullAllowed; this changes generated C# contracts instead of merely adding native platform APIs. [Source][M1], [PR](https://github.com/dotnet/macios/pull/26196).
- **Delivered with native exclusions (PIN-931) — `apple.preserve-required-framework-metadata`.** Use the explicit stripping opt-out only when bundled framework headers/modules must remain; explain signing/package implications without recommending disabling stripping by default. [Source][M1], [PR](https://github.com/dotnet/macios/pull/26253).
- **Grouped (PIN-929) — `apple.migrate-runtime-dependent-publishing` into `apple.pin-deployment-and-architecture`.** Reconcile CoreCLR/NativeAOT configuration, registrar/assembly-preparation defaults, and DynamicCodeSupport with dependencies. The RC2 CoreCLR dynamic-code default differs from RC1; NativeAOT still does not support dynamic code. Plain automatic CoreCLR speed/size improvement remains excluded. [RC1][M1], [RC2][M2RC].
- **Delivered with native exclusions (PIN-933) — `mobile.capture-native-dotnet-diagnostics`, edition11.0.** Original Sandpiper Android/Apple configuration libraries distinguish EnableCrashReport startup collection from deliberate archive/dSYM/PDB and IPA symbol retention. Both .NET10 configurations compile on Windows; current native builds and runtime collection/symbolication are not-tested under PIN-699. Exact RC2 inputs remain in samples/SandpiperFieldwork/sources.json. Default symbols alone do not prove collection. [Preview 7][M7], [RC1][M1].
- **Grouped — `maui.author-themed-app-assets`.** Native `.icon` compilation and alternate-icon paths can be an Apple-specific edition/route alongside the later MAUI MauiIcon integration. Do not assume both surfaces introduced in the same milestone. [Source][M6].
- **Reconciled — `apple.remove-obsolete-native-bindings` (PIN-694, 25 October 2026).** The [source/reference audit](../audits/mobile-api-gaps.md) corrects the native-versus-managed removal implication: Apple removed HMError.QuotaExceeded from native headers, but the actual RC2 iOS/tvOS/Mac Catalyst references retain value 106. This is historical compatibility context, not a usable new native capability or a replacement API. AppKit result and CoreNFC callback nullability have a narrow approval-only proposal; failable factory generation remains PIN-932. No exhaustive native OS catalogue or native execution is claimed. [Preview 4][M4], [current platform notes][MOverview].
- **Excluded — `apple.build-without-full-xcode`.** The Preview5 headline points to dotnet/macios repository-contributor infrastructure, not an application-project capability. PR25429 introduces repository `configure --no-xcode`/`NO_XCODE`; it skips native/runtime/workload paths and permits managed portions on Linux/older Macs. It does not establish an Apple sample compilation route that replaces the supported workload/Xcode toolchain. Do not document it as a consumer MSBuild switch. [Preview5 wording][M5], [actual source change](https://github.com/dotnet/macios/pull/25429).
- **Excluded — `apple.independent-native-feature-catalogue`.** Exhaustive UIKit/AppKit/Metal/Android native-OS additions and separate product release tutorials exceed the agreed .NET11-family boundary. Native APIs required by a qualifying workload or MAUI recipe remain documented dependencies. Apple Intelligence **MAUI abstractions** above remain included; that does not automatically include every underlying native AI API. [RC2 workload refresh][M2RC], [release scope][AppleRelease].
- **Excluded — `apple.automatic-runtime-and-build-repairs`.** Automatic symbol stripping, registrar optimizations, smaller packages, linker fixes, asset rebuilding, and native binding quality corrections are not standalone goals without an authored migration. [Preview 3][M3], [Preview 6][M6], [RC2][M2RC].

## Review gaps and status conflicts to carry forward

- **Reconciled — `desktop.complete-public-api-audit` (PIN-693, 25 October 2026).** The [source/reference/diagnostic audit](../audits/windowsdesktop.md) resolves the exact missing Preview2/3 paths and compares 50 references plus six analyzer/code-fix assets across .NET 10 and every Preview1–7/RC1/RC2 pack. It separately inventories the Visual Studio Designer SDK, maps qualifying changes to PIN-890–896 and retains automatic exclusions. WPF has the concrete clipboard exception goal; no WPF/VB feature is invented from automatic fixes or WPF's internal C++/CLI migration. RC3 availability and cumulative/Preview6 version conflicts remain explicit. Source audit closure does not deliver samples, native behavior or GA proof; final artifact reconciliation stays PIN-966.
- **Reconciled — `maui.complete-api-and-breakage-audit` (PIN-694, 25 October 2026).** The [named-gap audit](../audits/mobile-api-gaps.md) pins all 142 RC2 shipped/unshipped PublicAPI lists and the implementation/prior-source evidence. Converter culture and removing an empty-to-null-only Entry workaround have one narrow approval-only proposal; IViewScreenshot and public backend contracts group into PIN-913. Microsoft.Maui.Graphics.Font's old setters were private, so readonly alone establishes no public setter-removal migration; Color composition remains PIN-903. The five assigned automatic/contributor/native-catalogue exclusions are retained with explicit eligible carve-outs. This closes source dispositions, not recipe delivery, native behavior, the independent PIN-695 composition/status audit or PIN-966 GA reconciliation. [Preview 2][M2], [Preview 7][M7], [breaking index][Breaking].
- **Delivered with native exclusions (PIN-934) — `mobile.run-tests-in-app-process`, edition11.0.** All five pinned RC2 Android/iOS/tvOS/macOS/MacCatalyst MTP templates generate on Windows. Original Sandpiper Android/iOS tests replace placeholder assertions; generated bootstrap remains intact. Android accepts --no-restore while the Apple templates reject it. SDK inventory `sdk.run-mobile-tests` is grouped here. Current native compilation and all app-process execution remain not-tested under PIN-699; no recurring native CI or Human gate is added. [Source][M1].
- **Reconciled — `mobile.resolve-artifact-and-status-baselines` (PIN-695, 25 October 2026).** The [artifact/status audit](../audits/mobile-baselines.md) downloads both published RC2 sets: CLI versions 11.0.100-rc.2.26509.2 and 11.0.100-rc.2.26513.2 have identical complete 18-entry maps, selecting Apple 27.0.12212-net11-rc.2, MAUI 11.0.0-rc.2.26505.5 and Android 37.2.0-rc.2.84. Six actual manifests and package provenance resolve the 12211 comment/source dependency conflict. SDK 11.0.100-rc.2.26504.105 and Xcode27/macOS26.6+ are the preparation baseline; “sixth preview” installation prose is stale. AI is a separate maui-labs preview package with net10 assets and an actual experimental assembly annotation. No workload install or native compilation is claimed. [MAUI RC2][M2RC], [Apple release][AppleRelease].
- **Reconciled — `mobile.resolve-overview-lag` (PIN-695, 25 October 2026).** The [pinned conflict/status table](../audits/mobile-baselines.md#stability-and-cumulative-overview-conflicts) retains preview expression/x:Code opt-ins, preview incremental XAML Hot Reload despite its Debug default, experimental platform NativeAOT and assembly-level MAUIAI0001 despite the older “stable surface” wording. The cumulative overview's Xcode26.4 and omitted RC2 migrations cannot override selected artifacts. This closes the source finding, not native content proof or GA reconciliation; PIN-698/content slices/PIN-966 retain those obligations. [Overview][MOverview], [RC2][M2RC], [Apple known issues][AppleKnown].
- **Unresolved — `mobile.verify-native-original-examples`.** Produce original previous/current projects with pinned native tooling and exact SDK/workload evidence. Determine feasible compilation lanes without adding broad Windows/macOS CI; record source/compile/execution separately. No compilation has yet established the proposed APIs' actual signatures, generated code, behavior, or down-level package routes.

## Native compilation evidence during authoring

Apple workload guidance requires the specific Xcode paired with the installed workload; a different version is unsupported even if a limited build happens to pass. Current Apple RC2 pairs with Xcode27.0 on macOS26.6+. The no-Xcode PR above only changes how contributors build portions of the dotnet/macios repository; it is not proof that complete app samples, asset compilation, native registration/linking, or publish behavior can be verified on Windows/Linux. [Supported toolchain requirement](https://learn.microsoft.com/en-us/dotnet/ios/troubleshooting/xcode-requirement), [RC2 requirements][AppleRelease].

During sample authoring, obtain compilation evidence on a Mac with the matching .NET SDK, workload set, and Xcode for Apple-specific app/build/asset/binding examples; a Windows-launched remote build still needs that Mac toolchain. Record command/target, configuration/RID, output type, and exact versions. A compile-only managed fragment against reference assemblies may establish type/member syntax, but does not establish native app/build integration. A simulator-target build can avoid claiming a device run; signing/device provisioning and hardware-dependent behavior are separate evidence requirements when actually exercised. NativeAOT/trimming/registrar or asset-pipeline recipes require the corresponding build/publish path, not merely a normal managed build.

This is an authoring-time evidence constraint within the accepted compilation requirement. It proposes no additional recurring native CI jobs, UI/device tests, or emulator matrix. Until suitable toolchain evidence exists, retain a precise verification gap rather than treating the portable tool smoke or contributor no-Xcode mode as substitute proof.

[WPF2]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/wpf.md
[WPF1]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/wpf.md
[WPFOverview]: https://learn.microsoft.com/en-us/dotnet/desktop/wpf/whats-new/net110
[WF2]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/winforms.md
[WF1]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/winforms.md
[WF7]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview7/winforms.md
[WFOverview]: https://learn.microsoft.com/en-us/dotnet/desktop/winforms/whats-new/net110
[M2]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview2/dotnetmaui.md
[M3]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview3/dotnetmaui.md
[M4]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview4/dotnetmaui.md
[M5]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview5/dotnetmaui.md
[M6]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview6/dotnetmaui.md
[M7]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/preview7/dotnetmaui.md
[M1]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc1/dotnetmaui.md
[M2RC]: https://github.com/dotnet/core/blob/main/release-notes/11.0/preview/rc2/dotnetmaui.md
[MOverview]: https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-11
[AppleRelease]: https://github.com/dotnet/macios/releases/tag/dotnet-11.0.1xx-rc2-12212
[AppleKnown]: https://github.com/dotnet/macios/wiki/Known-issues-in-.NET11
[Breaking]: https://learn.microsoft.com/en-us/dotnet/core/compatibility/11

## Quay Board delivery (PIN-899 / PIN-900)

The 11.0 edition of maui.choose-xaml-generation groups
maui.simplify-xaml-namespaces. The 11.0 edition of
maui.author-view-local-csharp-in-xaml groups maui.bind-xaml-csharp-expressions.
Stable goal IDs are retained; no additional .NET10 foundation is introduced.
Original paired QuayBoard pages retain custom mappings and compare ordinary
code-behind with preview x:Code, qualified expressions and attached-property reads.

The .NET10 Windows page is compiled on the existing local route. The .NET11
page is official-review only and explicitly not-tested: the local SDK11 root
lacks Microsoft.Maui.Sdk and fails NETSDK1147 before compilation. Source and
actual Controls/Build.Tasks artifact identities are retained in
samples/QuayBoard/sources.json. No generated-output, native execution, performance,
other-platform or GA verification is claimed; PIN-699 permits this native exclusion.

## Android and routed navigation delivery (PIN-905 / PIN-906)

- maui.navigate-with-path-parameters edition11.0 groups maui.show-tab-badges, maui.control-swipe-action-colors and maui.describe-navigation-for-accessibility: HarbourNavigation provides original previous/current sources, absolute templates, explicit tint, independent spoken labels and platform exceptions. Older Windows compilation and controlled forward/query/back execution are distinct from current source review.
- maui.migrate-navigation-handlers edition11.0 groups maui.remove-compatibility-package: HarbourAndroid removes an explicit legacy registration, preserves Shell styling and distinguishes the unshipped optional package from surviving Controls.Core ShellRenderer. Older Android compilation is retained. The same edition now also carries the Apple navigation route delivered by PIN-907 below.
- Current SDK11 MAUI compilation and native interaction scopes are explicitly not-tested under PIN-699. Existing Human gates elsewhere and the eight .NET10 foundations are unchanged. See canonical recipe requirements, sample evidence and samples/HarbourNavigation/sources.json for exact identities.

## Apple navigation and custom item delivery (PIN-907 / PIN-908)

- `maui.migrate-navigation-handlers` edition11.0 now also covers inventory03:029/030: JettyApple compares the previous Appearing hook with NavigatedTo, explains repush controller identity and secondary toolbar placement, lists NavigationPage/TabbedPage/FlyoutPage handler defaults and explicit renderer fallbacks, and keeps Apple Shell opt-in separate from Android defaults. Existing Android guidance and identity remain.
- `maui.migrate-custom-items-handlers` edition11.0 covers inventory03:031: paired JettyItems and JettyWindows custom registrations distinguish Apple Items2 obsoletion from supported Windows legacy handlers and the Windows CollectionView2 default/fallback. Windows CarouselView is not assigned Apple's Items2 implementation.
- Older Windows compilation is retained; Apple builds, current Windows native compilation and all new native interaction scopes are explicitly not-tested under PIN-699. Exact input identities and pinned official-source contracts are retained in samples/JettyHandlers/sources.json and canonical evidence records. No new Human obligation or native CI matrix; existing Human/release gates and eight foundations remain unchanged.

### Quay integration delivery

The four edition11 goals above use original .NET10/11 Quay examples and the existing packed stdio harness. [Evidence and limits](../../verification/quay-integration.md) distinguish real neutral contract execution, original Windows10 compilation/lifecycle, actual first-party template generation, Avalonia package compilation and unavailable native11 behavior. Backend coverage is the approved contract demonstration, excluding rendering and real-view capture; source-only APIs are not called verified native behavior. GA reconciliation stays separate.

## Media recovery and chrome delivery (PIN-917 / PIN-918)

`maui.recover-interrupted-media-picking`, `maui.save-captured-media-to-gallery`
and `maui.choose-status-bar-appearance` retain inventory03:045/046/047 and edition11.0.
Original paired ReefSurvey examples preserve station identity, copy before clearing,
expose explicit pending discard and separate gallery consent, icon contrast,
Android background opt-in and application theme. The older route deliberately
retakes interrupted captures and uses native Android icon control.
Exact source/package identities are in samples/ReefSurvey/sources.json.
Older Android compilation is separate from current source/artifact review;
current MAUI compilation and all new native interactions are not-tested under
PIN-699. No extra .NET10 foundation, older-target package route, provider resource,
Human obligation, native CI matrix or GA verification is introduced.

## Activation, permissions and themed assets (PIN-919 / PIN-920 / PIN-921)

Inventory03:048 retains `maui.route-single-instance-activations`, edition11.0:
original TernActivation pairs preserve valid Windows AppInstance ownership on10
and use the MAUI lifecycle hook on11. Separate .NET10 processes and a temporary
Windows URI scheme established redirection and callback delivery; no OAuth proof.

Inventory03:049/050/051 are grouped into `maui.inject-permission-checks`, edition11.0.
TernField retains a static-call/application-filter comparison and introduces borrowed
IPermissions plus native minimum distance. iOS authorization and Android altitude
reference/accuracy semantics remain distinct from notification scheduling.

Inventory03:052/053/077 remain grouped into `maui.author-themed-app-assets`, edition11.0.
TernAssets owns original silhouettes, monochrome and dark artwork, sampling metadata,
and mutually exclusive image/Apple bundle selection. Apple hosting and actool are
explicitly source-only. No older-target route or additional .NET10 foundation.

All three .NET10 apps compiled. SDK11 RC3 native builds failed NETSDK1147;
current native compilation/execution and unavailable Android/Apple interactions
remain sample-specific PIN-699 not-tested exclusions. Official source/package
identities are retained under samples/TernGuidance. No new Human gate or GA proof.
