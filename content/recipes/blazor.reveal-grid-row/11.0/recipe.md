# Keep a place in a large results table

## Goal

A harbour dispatcher returns to a long consignment manifest at the row they were reading. Reveal a known consignment without searching, and keep that row in view when earlier arrivals are inserted. The worked domain is fictional; the technique applies to virtualized QuickGrid results.

## Older approach

.NET 10 QuickGrid supports virtualization and stable row keys, but has no InitialItemIndex, ScrollToItemAsync, AnchorMode or ItemComparer forwarding parameters. A fixed-height table can manage its own scroll container through JavaScript. That is a valid option when the application owns the geometry and knows which insertions change an index. It is not the newer API under another name.

{{snippet:older}}

{{snippet:scroll}}

The older helper assumes a 32px header and 32px rows. It waits for the initial spacer, then changes the real scroll container, letting the virtualizer load the visible window. Before a refresh it captures the offset and edge state; five known prepended rows add 160px while an active start edge remains at zero. This is a bounded alternative, not a general identity-preserving solution for arbitrary sorting, deletions or remote windows. Re-resolve the chosen identity after filtering or sorting. Do not scroll a row DOM element that virtualization has not rendered.

## Newer approach

On .NET 11 use the grid's own forwarding surface. InitialItemIndex=400 selects the zero-based position of consignment 401 on the first interactive render. The Reveal button resolves consignment 701 against the current ordered result and awaits ScrollToItemAsync. After prepends its identity still resolves to the correct, changed index.

{{snippet:newer}}

{{snippet:identity}}

Start anchoring keeps new arrivals visible when already at the start. Away from that edge, overlapping refreshes preserve the reading position. Each refresh creates new Consignment instances deliberately. ItemComparer compares their stable IDs, avoiding false change detection from reference equality. ItemKey separately preserves DOM row identity; it does not replace ItemComparer. Neither comparer sorts data or finds a missing record.

## Migration

1. Use Microsoft.NET.Sdk.Web, net11.0, C# 15.0, nullable and implicit usings. Reference Microsoft.AspNetCore.Components.QuickGrid 11. The canonical pins are SDK 11.0.100-rc.2.26504.105, QuickGrid/Internal.Assets 11.0.0-rc.2.26504.105. The actual older comparison uses SDK 10.0.402, net10.0, C# 14.0 and QuickGrid/Internal.Assets 10.0.13. SDK/package pins reproduce RC2 proof; no optional workload or external service is needed. No newer-package/older-target route is claimed.
2. Put ManifestPage.razor and Consignment.cs in namespace Playbook.Samples. Set @namespace Playbook.Samples in _Imports.razor and import Microsoft.AspNetCore.Components, its Routing and Web namespaces, Web.Virtualization, QuickGrid, Playbook.Samples and static Microsoft.AspNetCore.Components.Web.RenderMode. The older component additionally imports JSInterop and loads wwwroot/manifest.js. Keep the shared host:

{{snippet:host}}

3. App.razor uses an HTML document, base href="/", a Router with AppAssembly="typeof(App).Assembly", a Found context named route and a RouteView using that route. Include the framework boot script with src="@Assets["_framework/blazor.web.js"]" inside the body. Keep this exact geometry in the document head:

{{snippet:geometry}}

4. Remove the older scroll helper when adopting the grid APIs. Enable Virtualize=true and keep a finite, focusable scrolling container. The sample's rows stay exactly 32px tall through fixed table layout, no wrapping and clipped overflowing cells; ItemSize=32 must match. QuickGrid defaults OverscanCount to 3, unlike the list Virtualize default of 15. ItemSize defaults to 50 and InitialItemIndex to 0. Do not transfer list defaults or variable-height claims to QuickGrid.
5. Await a rendered interactive instance before calling ScrollToItemAsync. The sample disables actions until OnAfterRender. InitialItemIndex applies once after the item count is known; changing it later is not a scroll command. Indices are zero-based and out-of-range values are clamped. Near the end, available content limits top alignment. Calls belong on the renderer context; a background callback must use InvokeAsync. A later scroll supersedes an earlier one; accept a CancellationToken when your caller needs cancellation.
6. Refresh data and re-evaluate the chosen row's index in the current result. Use a comparer whose Equals and GetHashCode agree on an immutable unique ID. For an ItemsProvider, preserve stable ordering and truthful total counts; the sample executes local IQueryable data with overlapping windows, not a remote database/provider.
7. Open the real application. Expect 401 at the viewport's top; Reveal shows 701. Reload returns fresh objects without moving 701. Add five earlier arrivals: the same row stays at the same pixel offset although scrollTop increases by 160px. Appending while reading must not pull the viewport away. Go to the start and prepend again: stay at the start and show the new arrivals.

## Constraints

- QuickGrid Virtualize=true, a rendered instance and constant row heights matching ItemSize are required. Static SSR/prerender markup alone cannot perform these operations. Interactive Server is executed; WebAssembly/Auto need their own client composition and are not tested here.
- The explicit Start mode is also the default. None disables edge pinning; End follows appended items only while at the end, disengaging when the reader moves away. Flags can combine Start and End. Those other modes are source-reviewed contracts, not the canonical sample's execution claim. This recipe is grid-specific; list anchoring and variable-height lists are separate goals.
- RC2 removes ASP0030 from QuickGrid AnchorMode and ItemComparer. Use them without an obsolete experimental suppression. Earlier preview examples calling these parameters experimental describe older metadata. The pinned RC2 package exposes stable contracts, while prerelease artifacts are not GA verification.
- Initial positioning is not persistence storage. Supply the initial index from application state only after resolving the desired identity in the current ordered/filter context. The sample starts at a known index and does not implement URL state or row-click callbacks.
- Anchoring needs usable overlap and stable identities. Arbitrary replacement of all results, removal of the anchor, changing sort/filter and remote provider races need an application policy; this sample does not promise to retain a vanished row.

## Evidence

Both actual pinned Web SDK consumers compile with warnings as errors, separately from browser execution. Real Chrome Interactive Server checks measure rendered row identity, bounding rectangles, uniform 32px heights, a bounded virtualized window, initial/programmatic positioning and anchor retention through fresh-object reload, prepend and append. The T3 preview's standalone IntersectionObserver did not fire and its snapshot timed out; its failed synthetic moves are not application execution proof. Chrome is the rendered proof. No mock, calculated offset alone or cross-compilation substitutes for it.

Saved compilation records and browser reports bind exact sources, package assets and built artifacts. The existing packed-tool harness retrieves the complete canonical comparison and applicability over real stdio with an isolated local feed/cache and unrelated directory. Other OS, assistive technology, remote providers, WebAssembly/Auto, additional anchor modes and GA execution are not claimed. No required Human-only check is outstanding for the stated sample behavior.
