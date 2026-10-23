# Keep a changing list in place

## Goal

Use this when entries arrive at either end of a feed, or existing entries expand,
and the reader should keep following an edge or keep reading the same entry.
The orchard journal below contains picking and packing notes with different
heights. New notes must not steal a reader's place halfway through the journal.

## Choose the viewport policy

Set `AnchorMode` explicitly. `Start` follows inserted head entries when already
near the start. `End` follows arriving tail entries when already near the end;
scrolling away disengages tail following. `None` disables edge following while
retaining the reading position. Flags can combine `Start | End`. These policies
are about the visible item and its offset, not a promise that `scrollTop` stays
numerically unchanged: inserting or expanding content above the reader can
require a different scroll offset to show the same text.

`ItemComparer` identifies provider items across fetches. Compare by a unique,
immutable identity, never the row index or mutable display text. `@key` separately
preserves a rendered row's component and local input state when data instances
change. Neither keeps local component state after virtualization removes the row;
store durable edits outside the row when that is required.

## Older approach: bounded variable-height page

.NET 10 `Virtualize` assumes uniform item heights and has no `AnchorMode` or
`ItemComparer` equivalent. A fixed-height virtualized list remains a sound choice
when its content can fit that contract. For this small arbitrary-height journal,
the original comparison renders a bounded page and explicitly captures/restores
the first visible ID and offset after each controlled mutation. A paged production
feed should bound its data and DOM; this is not an unbounded feed implementation.

{{snippet:older}}

The module disables competing native anchoring on this container and restores
the selected edge or retained row after Blazor commits the new DOM. It handles
the example's controlled changes, not arbitrary late image/font resizing or
deleted anchor rows. Such content needs additional application policy.

{{snippet:compensation}}

Keep the `@implements IAsyncDisposable` and `@inject IJSRuntime JS` directives
at the top of the older Journal component. Both complete applications, including
their document, interactive route, imports, stylesheet, SDK pins and package
locks, are canonical repository samples; the newer host below also applies to
the older target. The document loads `@Assets["_framework/blazor.web.js"]` and
the route renders `<Journal @rendermode="InteractiveServer" />`.

## Newer approach: measured virtualized rows

The .NET 11 version removes that custom compensation and lets `Virtualize`
measure row sizes and maintain the viewport. `ItemSize` is an initial estimate,
not a fixed row height. Use one full-width block per entry inside a finite-height
vertical scroll container; here `#journal` has `height:420px; overflow-y:auto`.
Avoid transforms and competing scroll code. Browser-native observers must run;
rendering HTML or assigning an offset alone does not prove the list loaded.

{{snippet:newer}}

The provider intentionally returns new **class instances** on every request.
The comparer keeps unchanged IDs equal even after expansion changes their data.
Call `RefreshDataAsync` after mutating provider data, then render the parent
(event callbacks already do so). Keep ordering and total counts coherent and
honour cancellation in an asynchronous provider. This example covers inserts
at the head/tail; arbitrary reordering/deletion needs its own viewport policy.

{{snippet:identity}}

The keyed component includes a local reader note, making mistaken identity
visible. Expanding a row above the visible window increases its real height.

{{snippet:entry}}

Wire the Interactive Server host and load its matching framework browser assets:

{{snippet:host}}

`OverscanCount="15"` deliberately retains extra rows around the viewport. This is
also the .NET 11 list default; .NET 10's list default and QuickGrid's default are
3. Larger values improve average-height estimates and reduce fetch churn, at the
cost of more DOM, component work and provider results. Do not transfer the list
default to a grid. Measure your content before overriding it.

## Migration

1. Preserve stable IDs and add an explicit provider comparer and matching `@key`.
2. Choose edge following, retain a focusable scroll container and remove custom
   compensation only after testing the new measured contract on the real layout.
3. Verify prepend at the start, prepend while reading, append at the end, append
   after scrolling away, and expansion of an already rendered row above the
   reader. Check the actual visible ID/offset and input state, not only scroll
   offsets or calculated keys. Confirm extra overscanned rows and bounded DOM.
4. Recheck responsive widths, images/fonts and the browsers/layouts you support.
   The pinned implementation uses native anchoring where available and manual
   compensation for other paths, including tables/Safari; this sample proves
   only the tested Chrome block layout. Static SSR/prerender has no live viewport.

## Constraints

The primary route is net11.0/ASP.NET Core 11, SDK 11 and the sample's explicit
C# 15 mode; the anchoring API itself does not require new language syntax.
Actual pins are SDK `11.0.100-rc.2.26504.105` and matching ASP.NET Core reference,
runtime and SDK `Microsoft.AspNetCore.App.Internal.Assets` browser assets. No
separate Virtualize NuGet package, optional workload, database or service account
is required. The previous comparison is net10.0/C# 14, SDK `10.0.402` and framework/
Internal.Assets `10.0.13`. It is not a compatible older-target route for new APIs.

RC2's actual list parameters carry no experimental attribute or required opt-in;
no obsolete experimental suppression is taught. The cumulative release page's
BL0011 wording differs from the pinned analyzer/Virtualize source, which still
warns on `ItemsProvider` without explicit `ItemComparer`. This sample assigns a
sound comparer and needs no diagnostic suppression. RC2 also fixes initial
provider-load End anchoring. Release-note reconciliation does not prove browser
execution, and stable API metadata does not mean GA has been tested.

Official sources are included in the recipe metadata: the versioned virtualization
contract, immutable RC2 implementation/browser sources and RC2 release notes.
The source-disposition register maps WEB-015/016 to this single stable goal.

## Evidence

Both actual pinned applications compile; separate saved real Chrome/Windows
Interactive Server measurements exercise variable-height prepend/append/expand,
identity and overscan. The browser script marks user-scroll intent before moving
the viewport, uses real framework observers and reads rendered geometry. No
mocked browser, cross-compilation, WebAssembly/Auto, Safari/table-layout, other
platform or GA execution is claimed. Exact inputs, artifacts and limitations are
in `docs/verification/PIN-784.md` and its browser/package reports. Installed offline
stdio checks independently reconstruct the full Markdown and compare all authored
applicability fields; they prove delivery, not the consumer's layout or environment.
