# Grid url state evidence

Historical authoring observations from October 2026. Compilation stays in the
sample records; these observations do not establish a new run, broader platform
support or GA verification. Reuse only while the claim and relevant inputs remain
valid; see [capture and refresh](../contributing.md#capture-and-refresh-sample-evidence).
[Original capture and detailed provenance](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-781.md).

## RC2 source reconciliation

The ticket, source SPEC and approved proposal require **stable column
identifiers**, not a specific API spelling. Actual RC2 QuickGrid has no
`ColumnId` property: metadata inspection returns false and an actual compile
assigning it to PropertyColumn rejects CS1061. QueryParameterNameOptions and
Title exist. [API evidence](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-781.api.evidence.json) records the package identity
and bounded diagnostic commands.

The reviewed pinned source uses `column.Title` for URL sort values and matches
that value during restoration. The supported implementation freezes Title as
`code`/`priority` and places separately editable visible captions in
HeaderTemplate. Actual static requests with alternative headings and live
interactive heading changes retain the same keys and rows. No dedicated API is
invented and no capability criterion is removed. A changed key still needs a
bookmark migration. Source commit 4a70870818bf5662f21b0c28f9a8c267dccb77f0 is
review evidence; the NuGet assembly's informational-version source commit could
not be fetched through the raw-source URL, so that exact-source retrieval is
not claimed. Real artifact compilation and execution govern compatibility.

The default .NET 10 interactive controls remain valid for in-memory views.
Its comparison application explicitly implements URL sorting/paging and anchor
navigation instead. The .NET 11 application delegates restoration and pagination
to QuickGrid/QueryParameterNameOptions/Paginator. Custom headers on both targets
subscribe to LocationChanged: real browser testing caught stale header URLs
after another grid navigated; the fixed helper regenerates links and detaches
its subscription at disposal.

## Actual evidence

- Both pinned Web SDK/QuickGrid consumers compile with zero warnings/errors;
  verification-evidence.json records **compile**, exact inputs, SDKs and locks.
  QuickGrid pins are 10.0.13 and 11.0.0-rc.2.26504.105. The matching SDK browser
  assets are locked too. Samples have isolated author-maintained package policy;
  no root package, net10 host, toolchain, CI or Renovate permission changes.
- [HTTP 10](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-781.http10.evidence.json) and [HTTP 11](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-781.http11.evidence.json)
  each pass 15 real requests/observations: initial sorted pages, repeated bookmark,
  different headings, rendered next/previous/sort links, query preservation,
  disabled anchors, defaults and malformed/negative/excessive pages.
- [Browser 10](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-781.browser10.evidence.json) and
  [browser 11](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-781.browser11.evidence.json) run real T3 Chromium 152
  Interactive Server circuits. Both restore both bookmarked grids, change live
  captions without changing keys/links, navigate each grid independently,
  sort both ways, retain scope=open and execute back/forward. Disabled links
  have aria-disabled, tabindex=-1 and computed pointer-events:none.
- [Execution fingerprints](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-781.execution.evidence.json) bind those reports
  to inputs and actual assemblies/deps/runtime configuration/static asset
  manifests. Process module inspection establishes CoreCLR 10.0.13 and
  11.0.0-rc.2.26504.105. Captures explicitly retain sourceDirty=true from the
  implementation worktree; they are not claimed as clean reviewed-head builds.
- Chrome extension visual inspection showed aligned, legible tables and disabled
  link styling without clipping. T3 snapshot calls timed out while evaluate and
  navigation worked; the integrated-browser fallback was unavailable, so the
  existing Chrome extension supplied that visual inspection. Its temporary tab
  was closed. No accessibility assistive-technology or broad rendering/OS matrix
  is claimed.

No mocks or cross-compilation substitute for execution. Provider translation,
WebAssembly/Auto, GA, other OS execution and full unrelated sample verification
remain outside the evidence. No required Human-only proof is outstanding.

Main 23231e82efa32460bf3f8f2c5c4d4544534b8872 is integrated, retaining
MuseumRows, QuarryLedger, SeedLabels, NavigationAtlas, ArchiveCards and WorkshopViews plus
their shared test/documentation registrations. The corrected existing package
wrapper expects 36 full scenarios and one smoke scenario. No duplicate count
repair or scenario/byte-budget removal is part of this change. A pre-integration
full packed run found 170095/170000 bytes for max50; only this recipe's discovery
wording was shortened. Complete requirements, canonical body and selection facts
remain present. Final combined artifact results follow below.

The actual combined `eng/Test-Offline.ps1 -PackageVersion 0.0.0-workshop-views-r1
-RuntimeMajor 10` wrapper passed **36/36**, zero failures/errors/skips, using
Docker `--network none`, .NET 10.0.12 and no checkout mount. Both focused Windows
budget scenarios also passed against the same packages: fixture default20
70856/80000 bytes, max50 **169940/170000**; ordinary target-11 default20
70852/80000 and largest summary 1982/3000. Max50 has only 60 bytes of headroom.
[Package evidence](https://github.com/PinguApps/Playbook/blob/e60419660a082da40dfc1bd8f0667401677c53f3/docs/verification/PIN-781.package.evidence.json) binds the suite and both artifacts:

- Ordinary SHA-256: `6d68b0bc904fd58771adc57bcb2c4c209f4a107af5a636e9ad9a5d744a47ebf7`.
- Contract fixture SHA-256: `e344f66d67691a101afec063c98a24d888492f5bea711a5c7a6fe8e44e50ae3e`.

Both packages were rebuilt at 882f2af after MuseumRows integration, with the
scoped installed-test change uncommitted. The prior artifacts were not reused.
The rebuilt test assembly independently compares every authored applicability
route property against embedded recipe.json, including complete nested version
requirements, prerequisites, constraints, package assets, stability and
source/evidence references. Its real installed full-body/applicability scenario
passes 1/1 and the network-isolated full wrapper passes 36/36 with this stronger
assertion. Scenario count and all byte guards are unchanged.

Historical sample captures retain their original dirty-source provenance.
Every current source-input hash and executed artifact hash matches both saved
real HTTP/browser reports; unchanged sample behavior proof is reused. The exact
Inspector block was rerun successfully from C:\Windows after integration.
No clean reviewed-head sample rebuild is claimed.

The canonical browser fixture retains its original source-era PIN-781 comment so its saved input hash stays valid. That historical capture is linked above; current check instructions are in the contribution and Inspector guides.
