# WindowsDesktop public changes and retained exclusions: PIN-693

Reviewed **25 October 2026**, against the inventory's **.NET 11 RC2 preparation
baseline**. This closes the assigned source/reference/diagnostic audit, not the
desktop recipe backlog. No sample, inspected analyzer, Designer, clipboard or
native UI was executed. No GA verification is claimed.

## Evidence boundary

The [source manifest](windowsdesktop.sources.json) pins **65 complete official
files** by immutable URL and SHA-256. IDs below refer to that manifest. It covers
all nine Preview 1–7/RC1/RC2 component indexes and WPF notes, the seven available
WinForms notes, every file in the available WindowsDesktop API-diff directories
(Preview 1–4/6/7), cumulative desktop pages, RC2 analyzer release tables and
descriptors, VB startup, painting/rendering/fullscreen implementation, shared
clipboard implementation and the separate Visual Studio Designer SDK boundary.
Preview 5's missing API-diff directory is bridged by the actual package comparison.

Canonical commits:

- dotnet/core **60871330fce2b3da383461be027ebdf3ce30f8fb**.
- RC2 implementation, dotnet/dotnet
  **820230c386db5b0106c5430b3e5aee92a7826bf5**, taken from the actual package nuspec.
- Cumulative desktop docs, dotnet/docs-desktop
  **1c487f4e64ba00769475c03aa8a65f05f137c13d**.
- Designer extensibility docs, microsoft/winforms-designer-extensibility
  **4959e7c5076faf3d8b095db36951fa3c6d77cb79**.

The [generated evidence](windowsdesktop.assets.json) comes from
[Inspect-WindowsDesktopApis.ps1](../../../eng/Inspect-WindowsDesktopApis.ps1).
It downloads the actual **Microsoft.WindowsDesktop.App.Ref** ZIPs and inspects
**50 reference DLLs and six analyzer/code-fix DLLs in every package**. It records
archive/asset hashes, repository commits and public/protected declarations,
including signatures, generic constraints, constants, parameter/default metadata,
custom attributes, properties, events and visible exported types. Internal
exported implementation types are excluded. Declaration metadata is retained
exactly; serialized assembly-version attributes and nullable-context relocation
can produce pairs without a new callable operation. These are declaration-line
counts, not API or recipe counts. This is not a native ABI or behavior test.

| Compared packages | Added lines | Removed lines | Source/reference disposition |
| --- | ---: | ---: | --- |
| 10.0.14 → 11.0.0-preview.1.26104.118 | 284 | 288 | Version-qualified metadata, ListControl converter and removed ButtonBase overrides; see register. The note's API index compares 10.0 GA; this actual older baseline is explicitly the serviced .NET 10 patch. |
| Preview 1 → 11.0.0-preview.2.26159.112 | 0 | 0 | No declaration delta; unchanged WFO IDs. |
| Preview 2 → 11.0.0-preview.3.26207.106 | 0 | 0 | No declaration delta; unchanged WFO IDs. |
| Preview 3 → 11.0.0-preview.4.26230.115 | 150 | 148 | EventLog nullable/flow/default metadata; existing Extensions disposition. |
| Preview 4 → 11.0.0-preview.5.26302.115 | 0 | 0 | Notes describe repairs; no declaration delta. |
| Preview 5 → 11.0.0-preview.6.26359.118 | 0 | 0 | Actual version corrects the note's unavailable package citation. |
| Preview 6 → 11.0.0-preview.7.26381.103 | 732 | 493 | Modern rendering, settings, reveal, painting scopes and genuine VB startup changes. |
| Preview 7 → 11.0.0-rc.1.26425.128 | 177 | 126 | Kiosk component, toggle/appearance refinements and preview TreeView replacement. |
| RC1 → 11.0.0-rc.2.26504.105 | 43 | 4 | Appearance refinements and Pkcs CMS additions. The separate analyzer delta adds fourteen rules without requiring new framework API declarations. |

The replay reads metadata using PEReader; it never loads the inspected DLLs for
execution. WFO strings prove an ID is in an asset, **not** that the analyzer
executes, emits by default or applies to every TFM/language. Defaults and scope
below come from the pinned descriptor/release sources. API equality does not
prove implementation equality or the absence of semantic breaking changes.

## Missing Preview 2/3 paths resolved

Both pinned `preview2/winforms.md` and `preview3/winforms.md` return **404**. Their
canonical component indexes nevertheless link `./winforms.md`. The GitHub
commits endpoint, bounded to the exact core commit above and each exact path,
returns **zero commits** for each; no deleted note or renamed replacement is
established in that reachable history. This is bounded historical evidence, not
a claim about unreachable branches or all possible spellings.

The existing `preview2-api-11.0-preview2` and
`preview3-api-11.0-preview3` files contain the standard diff preamble and no
assembly entries. Independently, the downloaded Preview 1/2/3 packs each contain
50 references with **zero added/removed declaration lines** at both transitions.
Their six analyzer assets retain the same WFO inventory. The missing notes can
therefore be dispositioned without inventing feature text. Their WPF notes
positively identify the DWM and WeakEventTable repairs below. No empty-index
inference is used as proof of native behavior.

## Accountable disposition register

**Grouped** means existing work owns delivery, not that this audit implements it.
Existing stable goal IDs remain unchanged. No additional narrow recipe proposal
is justified by the inspected desktop changes; PIN-986 need not create a duplicate
of the seven existing desktop delivery slices.

| Source/API family | Authored decision and disposition | Existing owner |
| --- | --- | --- |
| `wpf.restore-clipboard-error-handling` | **Included, delivery outstanding.** RC2 WinForms notes document `Windows.Clipboard.ThrowExceptionsForGetAPIs`; `rc2-clipboard-2` defines/caches it and `rc2-native-adapter` changes exception/HRESULT handling. WPF uses the shared clipboard pipeline. Choose empty data versus propagated retrieval errors before reading; never recommend enabling it universally or call it a new WinForms Clipboard member. WPF RC2's separate `autoConvert` repair is not this opt-in. | [PIN-890](https://linear.app/pinguapps/issue/PIN-890) |
| `winforms.adopt-modern-rendering`, toggle options and small appearance details | **Grouped.** `preview7-api-*`, RC1/RC2 notes and actual declarations cover application/control ambient modes, padding, toggle constraints, grid sort glyph color and PrintPreviewControl foreground. Retain High Contrast, Classic/Disabled/Latest/inheritance and composite-control fallbacks; the two composite opt-ins do not make every control modern. ForeColor affects empty preview presentation, not printed document colors. | [PIN-891](https://linear.app/pinguapps/issue/PIN-891) |
| `winforms.author-adaptive-control-rendering`, `winforms.follow-system-visual-settings` | **Grouped.** Protected effective-mode/change-impact/base notification hooks and settings snapshot/category events change custom-control code. Framework-added overrides for paint, colors, DPI, focus, disposal and padding support that implementation; they are not independent application goals. Preview 1 removes ButtonBase's declared OnBackColorChanged/OnForeColorChanged overrides, but the overridable Control methods remain inherited. Do not prescribe renaming an application override merely because its intermediate implementation moved. | [PIN-892](https://linear.app/pinguapps/issue/PIN-892); settings judgment remains [PIN-897](https://linear.app/pinguapps/issue/PIN-897) |
| `winforms.control-initial-form-reveal`, `winforms.configure-vb-application-defaults` | **Grouped.** Default/resolved reveal, form overrides and splash/top-level exceptions share one goal. Actual Microsoft.VisualBasic.Forms additions expose ApplyApplicationDefaultsEventArgs values and protected WindowsFormsApplicationBase properties; pinned VB source retains Classic defaults. These require real VB ApplicationEvents authoring, not translated C# startup. No new VB language version is inferred. | [PIN-893](https://linear.app/pinguapps/issue/PIN-893) |
| `winforms.batch-control-mutations` | **Grouped.** ControlMutationExtensions, LayoutSuspendTraversal, ISupportSuspendPainting and protected begin/end core hooks own exception-safe painting/layout scopes. Acquire the right subtree scope before changing an ambient value. Internal control interface/override plumbing is infrastructure; low-level begin/end calls are not the ordinary recipe. | [PIN-894](https://linear.app/pinguapps/issue/PIN-894) |
| `winforms.manage-fullscreen-experience` | **Grouped.** KioskModeManager ownership/disposal, container/form resolution, state restoration, focus-local shortcuts, pointer policy and idle requests remain one presentation goal. No Windows Assigned Access/security lockdown is provided. | [PIN-895](https://linear.app/pinguapps/issue/PIN-895); owner-session proof [PIN-898](https://linear.app/pinguapps/issue/PIN-898) |
| Designer structure, VB events and getter identity | **Grouped.** WFO2000 and WFO3000–3012 are present only in the inspected RC2 assets, with the source defaults/language restrictions below. Keep generated partials explicit; use VB WithEvents/Handles; choose stable identity versus a deliberately creating method for allocating getters. | [PIN-896](https://linear.app/pinguapps/issue/PIN-896) |
| Preview-only TreeView `NodeLeading` | **Grouped historical migration.** RC1 removes/replaces the preview member. Current samples use supported sizing members; no .NET 10-to-11 application ever needs to remove a member absent from .NET 10. | PIN-891 |
| ListControl.DataSource converter and framework Designer metadata | **Excluded automatic residual.** Preview 1 adds DataSourceConverter metadata; ComponentTray's paired metadata and version-qualified editor/converter attributes do not establish an authored application migration. Framework `System.Windows.Forms.Design`/`.Editors` assets are inspected separately from the external extension SDK. A concrete failing custom-designer workflow would reopen the decision. | Retained `desktop.automatic-reliability-fixes` |
| WPF references and supporting facade/DirectoryServices attributes | **Excluded metadata residual.** Paired WPF/Ribbon/XAML/DirectoryServices changes serialize framework type names at assembly version 11 instead of 10; no new callable WPF member is established. Do not invent a WPF or VB feature from these records. | Retained exclusion |
| EventLog reference annotations | **Grouped existing exclusion.** Preview 4's canonical EventLog diff and actual desktop reference changes concern nullable/flow/default metadata, not a new logging operation. Respect nullable results. The [Extensions audit](extensions.md) already owns this family and its exact package evidence; desktop pack membership does not create a second recipe. | [PIN-706](https://linear.app/pinguapps/issue/PIN-706) |
| Pkcs key-encapsulation/CMS additions | **Grouped existing coverage.** Actual RC2 desktop references include CmsRecipient factories, KemRecipientInfo/RecipientInfoType and composite Decrypt overloads with their experimental attributes. The [runtime/security audit](runtime-security.md) and [actual cryptography evidence](../../verification/PIN-750-751.md) retain supported versus unsupported operations. Desktop reference presence adds no native success or blanket platform proof. | [PIN-751](https://linear.app/pinguapps/issue/PIN-751) |

## Diagnostics and Designer SDK composition

All ten framework packs ship the same six analyzer/code-fix asset paths; hashes
and per-asset WFO strings are in the report. The inspected .NET 10/Preview 1–7/RC1
union is **WFO0001–0003, WFO1000–1001 and WFO2001**. These are historical baseline
IDs, not new .NET 11 recipes. WFO5001–5003 are also allocated in DiagnosticIDs.cs;
an allocation is not proof of an emitted analyzer diagnostic or stable feature.
Keep the actual API's experimental attributes/requirements when authoring.

RC2 adds **fourteen** IDs. Shared `AnalyzerReleases.Unshipped.md` names all of
them, and SharedDiagnosticDescriptors plus the language-specific descriptors
establish defaults. “Unshipped” is the source tracking filename; the actual
RC2 DLLs contain these IDs, so the filename is not evidence of absence.

| Rule | Default | Languages / concrete repair |
| --- | --- | --- |
| WFO2000 | Warning | C#, VB: getter allocates a fresh reference. Cache only if identity is intended, otherwise consider a creating method and compatibility. Not limited to Designer files. |
| WFO3000/3001/3003 | Error | C#, VB: explicit unrolled InitializeComponent; move runtime logic, custom members and event/delegate declarations into the user partial. |
| WFO3002 | Warning | C#, VB: keep Designer-owned members in their supported position; do not move unrelated/inherited fields. |
| WFO3004 | Error | C# only: replace Designer collection expressions with supported explicit initialization. |
| WFO3005/3009 | Warning | C#, VB: serialize the deterministic NameOf/interpolated result. |
| WFO3006–3008/3010 | Error | C#, VB: move conditional/coalescing/conditional-access/lambda logic out of generated initialization. |
| WFO3011/3012 | Error | VB only: Designer members use WithEvents and named Handles methods. Supported local-component AddHandler wiring remains valid. |

RC2 notes restrict the new Designer defaults to .NET 11+, not NetFx. This audit
does not invoke a compiler or validate a diagnostic trigger/code fix. PIN-896
retains original C#/VB compilation, emission and Designer round-tripping proof.

The **out-of-process Visual Studio extension SDK is a different asset family**.
Pinned `rc2-designer-readme` explicitly ties Designer releases to Visual Studio,
and points to Microsoft's separate extensibility repository/package. Actual
**Microsoft.WinForms.Designer.SDK 1.13.0-preview.2.24575.3** has net472 client/protocol
references, net6.0-windows7.0 server/protocol references, netstandard2.0 analyzer
references, analyzer/generator copies and build props/targets. Its nuspec declares
System.CodeDom 5.0.0 and System.Composition 6.0.0 for both dependency groups; it is
not a net11 framework analyzer pack. The report hashes all **11** DLL/props/targets
assets and retains its own repository commit. No .NET 11-specific SDK release or
required extension-package migration is established by these sources. Exclude a
standalone SDK tutorial under the agreed independently-versioned-product boundary;
do not claim complete Visual Studio/extension-SDK API or behavior coverage. A
future qualifying desktop recipe must declare this dependency if it actually
uses the out-of-process extension model. No vendor examples were copied.

## Retained automatic exclusions and exact source conflicts

- **WPF Preview 1/2/3:** Fluent backdrop/background, DWM crash guarding and
  WeakEventTable deferred updates repair existing code. No concrete removable
  workaround was established. **Preview 4/5/7:** positive no-feature/no-change
  notes. **Preview 6:** automatic self-contained `ijwhost.dll` inclusion, not a
  new app property. **RC1:** native hardening and WPF's own C++/CLI project
  migration, not application migration advice. **RC2:** restored clipboard
  auto-conversion. All remain excluded residuals; the exception opt-in stays
  included separately.
- **WinForms Preview 4/5/6 and RC2:** clipboard RTF/bitmap retrieval, painting,
  input/modal tracking, layout/DPI, dark-mode colors, Designer paste/captions,
  PropertyGrid and automatic border/RTL repairs benefit correctly authored apps.
  No demonstrated previous workaround removal justifies another slice. This
  exclusion never absorbs the explicit rendering/scoping/diagnostic choices.
- **Broken component links:** Preview 2/3 indexes link nonexistent WinForms files;
  exact 404/history and real package evidence above resolve the gap. Preview 5
  has no WindowsDesktop API-diff directory at this pin; actual P4→P5 fills it.
- **Preview 6 version conflict:** the WinForms note comments cite
  `11.0.0-preview.6.26328.106`; NuGet's current framework-pack index does not list
  it. Inspected **11.0.0-preview.6.26359.118** exists and has zero P5→P6 changes.
  Do not silently copy the comment into a package pin.
- **Cumulative lag/usage drift:** both desktop overviews still identify RC1.
  They omit RC2's Designer/default/fallback details. The WinForms overview also
  says to call Application.IsFormRevealDeferred; the actual asset exposes a
  **property**. RC2 notes and actual declarations take precedence. Protected
  custom-control hooks are not public application queries.
- **Newer availability:** the live framework-pack index also lists
  **11.0.0-rc.3.26522.148**; at the pinned core tree RC3 has only release.json and
  the release announcement, without desktop component notes. This audit retains
  the assigned RC2 preparation baseline rather than calling it latest or GA.
  [PIN-966](https://linear.app/pinguapps/issue/PIN-966) owns final-family actual
  artifact/source/stability reconciliation against the prepared catalogue;
  later RC/GA changes cannot be cleared by replaying this frozen evidence.

## Replay and remaining delivery

From PowerShell 7 with network access; no SDK installation or workload is needed:

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-f53726dc'
& ./eng/Inspect-WindowsDesktopApis.ps1 `
    -OutputPath ./artifacts/windowsdesktop.replayed.json `
    -CompareWith ./docs/planning/audits/windowsdesktop.assets.json
```

Expect exact source/archive/asset hashes and matching declaration/diagnostic
inventories, including zero P1→P2/P2→P3 changes. A hash/evidence mismatch fails;
inspect it before updating a baseline. This frozen replay does not discover new
releases. [The verification ledger](../../verification/PIN-693.md) records actual
executed checks, negative control and environment results.

The inventory's stable audit ID is now reconciled. Its historical counts stay
unchanged; there is no new recipe count claim. The seven existing delivery
tickets still owe original older/newer samples, exact native prerequisites,
compilation, claim-specific behavior and installed offline list/get evidence.
PIN-897/898 retain their separately owned Human checks. PIN-986 owns proposal
approval/reconciliation and PIN-959 the comprehensive publication gate. No
related issue or estimate is changed by this audit, and none is created.
