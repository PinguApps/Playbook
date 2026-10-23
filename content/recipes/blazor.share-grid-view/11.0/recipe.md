# Restore and share sorted grid pages

## Goal

Use this when a results page must survive refresh or be sent to someone else.
Keep sorting and pagination in the URL, rather than expecting the recipient to
recreate an in-memory view. A repair workshop has a repairs list and a loan-kit
list on one page. Each list needs its own sort and page without disturbing the
other list or an existing `scope=open` query value.

## Constraints

The newer implementation targets **net11.0** with ASP.NET Core 11 and
`Microsoft.AspNetCore.Components.QuickGrid` 11. Canonical verification uses SDK
**11.0.100-rc.2.26504.105**, QuickGrid and SDK browser assets
**11.0.0-rc.2.26504.105**, and C# **15.0**. These are RC2 measurements, not GA
execution evidence. URL navigation is not marked experimental in these artifacts
and requires no experimental opt-in. No workload, database/provider, native
toolchain or account is required. The data below is in-memory `IQueryable`.

Static SSR follows ordinary HTTP links without an interactive circuit. The same
grid also runs in Interactive Server; its location subscriptions restore state
when navigation occurs inside the circuit. WebAssembly/Auto and other OS hosts
are not execution claims here. An `ItemsProvider` is an alternative for remote
data, but must return an accurate total count for last-page resolution; provider
translation and concurrency are outside this example.

**Stable column identifiers in RC2:** there is no dedicated `ColumnId` property.
QuickGrid matches the URL sort value against `ColumnBase.Title`. Treat `Title`
as an immutable, unique key within that grid, such as `code` or `priority`.
Render human-facing wording separately in `HeaderTemplate`. Changing “Job code”
to “Ticket reference” then leaves old bookmarks intact. Do not rename the key
when localizing or editing a caption. A future key rename needs an explicit
bookmark migration. Two grids may reuse the same column keys because their
query names are distinct. A row's `ItemKey` serves a different purpose and does
not supply URL column identity.

## Complete shared application shell

Create a Web SDK project named `WorkshopViews`. For the newer comparison set
`TargetFramework` to `net11.0`, `LangVersion` to `15.0`, `RootNamespace` to
`Playbook.Samples`, and `RestorePackagesWithLockFile` to `true`. Add a
`PackageReference` with `Include="Microsoft.AspNetCore.Components.QuickGrid"`
and `Version="11.0.0-rc.2.26504.105"`. Pin the SDK above in `global.json` with
`rollForward: disable`. Keep this version-specific sample outside an automated
central package policy, or explicitly give it an isolated package boundary.
Restore to produce `packages.lock.json`, then use locked restore for verification.
The Web SDK supplies matching framework references and Internal.Assets browser
files; no additional component package is needed.

Both comparisons use the following `Program.cs`, imports and document. The
generated `WorkshopViews.styles.css` includes QuickGrid's scoped styles; retain
it alongside the application's link styles.

{{snippet:host}}

Put these directives in `_Imports.razor`:

{{snippet:imports}}

`App.razor`:

{{snippet:document}}

`WorkItem.cs` and `Workshop.razor` are shared by both targets:

{{snippet:item}}

{{snippet:workshop}}

Add `Static.razor` with `@page "/static"` followed by `<Workshop />`.
Add `Interactive.razor` with `@page "/interactive"` followed by
`<Workshop @rendermode="InteractiveServer" />`. The heading button is enabled
only after a real interactive render. Static requests can use `labels=alt` to
render the alternative headings; grid state is the same.

## Older approach: explicit URL restoration

The .NET 10 comparison is a valid way to support this goal. It sorts the data
before passing it to QuickGrid, restores a separate `PaginationState` from each
namespace and creates page links itself. No native URL-state API is claimed for
QuickGrid 10. The standard .NET 10 interactive controls remain a good choice
when a view does not need a shareable URL.

Use the same shell with `net10.0`, C# **14.0**, SDK **10.0.402**, and
QuickGrid **10.0.13**. Its matching Web SDK browser assets are **10.0.13**.
Do not put the QuickGrid 11 package on this older target to imitate the new route.
This older application is comparison evidence, not an additional catalogue
edition or an advertised older-target route for the newer API.

`GridPanel.razor` in the older application:

{{snippet:older}}

The in-memory count gives this custom pager a total before it renders. For
remote data, fetch a real total instead of counting a locally loaded window.
The location handler is detached when the component is disposed.

## Newer approach: let QuickGrid restore the view

Replace the older `GridPanel.razor` with this implementation:

{{snippet:newer}}

`QueryParameterNameOptions` defaults to `sort`, `direction`, and `page`.
Its prefix constructor includes exactly the string supplied, including any
separator. These two instances produce `repairs_sort`, `repairs_direction`,
`repairs_page` and `loans_sort`, `loans_direction`, `loans_page`.
For an existing public URL contract, set the `Sort`, `Direction`, and `Page`
properties explicitly instead. Use all three unique names per grid and never
share a `PaginationState`: QuickGrid assigns its page query name to that object
and the linked `Paginator` uses it automatically.

The stable `Title` values above are intentionally independent of the visible
headings. Because a custom `HeaderTemplate` replaces QuickGrid's default header,
it must supply its own navigation link. Both comparisons use this complete
`SortHeading.razor`:

{{snippet:sort}}

Generate URLs with `NavigationManager` so unrelated query entries and the other
grid's latest state are retained. The `LocationChanged` subscription matters:
without it a cached custom header can keep an old page or the other grid's old
state in its next link. Dispose the subscription with the component. The heading
helper toggles the selected key between `asc` and `desc`; it preserves the page.
If a product wants sorting to return to page one, remove only that grid's page
key in the same query update.

## Migrate controls and styles together

QuickGrid 11's ordinary sortable headers and `Paginator` render anchors with
`href`, so links support copying and opening another tab. Update button-specific
selectors and event assumptions. A disabled paginator anchor has
`aria-disabled="true"` and `tabindex="-1"`; it does not have the HTML button
`disabled` attribute. Preserve the supplied framework styles and style the
anchor's disabled state, rather than relying on `button:disabled`.

Save this as `Styles.razor` in either application:

{{snippet:styles}}

Do not use the AppContext switch
`Microsoft.AspNetCore.Components.QuickGrid.EnableUrlBasedQuickGridNavigationAndSorting`
as a way to restore purely in-memory behavior. In the measured RC2 implementation
it changes default anchors back to interactive buttons, while the navigation
methods still read/write URLs. This recipe leaves the switch at its default.
Custom templates remain responsible for their own markup.

## Migration

1. Give every grid a stable namespace and a distinct `PaginationState`. Migrate
   existing query names deliberately so saved URLs keep their meaning.
2. Freeze unique `Title` keys. Put display text in custom headers and ensure
   those headers regenerate links when the location changes.
3. Replace custom sort/page restoration with QuickGrid 11 and its `Paginator`.
   Keep custom headers, link styling and any unrelated application filters.
4. Run either application in Development using its declared SDK. Open
   `/static?scope=open&repairs_sort=priority&repairs_direction=desc&repairs_page=2&loans_sort=code&loans_direction=desc&loans_page=3`.
   Repairs must be `R-E, R-F`; loans must be `L-B, L-A`. Reloading or requesting
   that address through HTTP must give the same rows without a circuit.
5. Follow repairs next: expect `R-D, R-B`, with loans unchanged. Follow loans
   previous: expect `L-D, L-C`, with repairs unchanged. Change either sort and
   confirm the other namespace and `scope=open` remain in the address.
6. Repeat at `/interactive`. Rename visible headings and confirm the existing
   URL and sort keys remain unchanged. Exercise both sort directions, paging,
   and browser back/forward. Check disabled links' attributes and computed CSS.

Page values in the URL are one-based; `PaginationState.CurrentPageIndex` is
zero-based. The first page omits its query key. Malformed, zero or negative pages
resolve to the first page; excessive values resolve to the last available page.
Removing sort parameters restores the configured default column/direction.
Never interpret query text as an arbitrary property or SQL expression; the newer
grid matches declared column keys, and the older sample selects a fixed switch.

## Evidence

Both actual projects compile with their pinned SDKs, languages, package locks
and framework/browser assets. Saved evidence is **compile**, separately from
real loopback HTTP and Chromium execution. HTTP checks request bookmarks,
follow the rendered links and change visible captions. Interactive checks
change both grids, change headings in the live circuit, retain unrelated query
state and exercise browser history. They also check actual anchor disabled
attributes, tab order and pointer-event CSS. No mocks, cross-compilation,
provider execution, new OS/client matrix, GA build or assistive-technology
verification is substituted for that bounded evidence.

The installed packed-tool harness retrieves the full canonical comparison and
applicability over real stdio using an isolated feed/cache and unrelated working
directory. It verifies target 11.0 edition selection and rejects target 10.0 for
this edition. Source/API inspection established the RC2 `Title` identity and
absence of `ColumnId`; it does not claim an API that the compiler rejected.
