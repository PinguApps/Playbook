# Keep route matching after removing the inert router setting

## Goal

A tidal observatory needs `/stations/new` to open its creation page and `/stations/SA7` to open station SA7. Remove the obsolete Router parameter during a .NET 11 migration while keeping those outcomes. The setting never controlled precedence in the measured .NET 10 release.

## Older approach — .NET 10

The original `Routes.razor` still declares `false`:

{{snippet:older}}

The .NET 10 Router source explicitly says configuring `PreferExactMatches` has no effect. The historical sample suppresses CS0618 only to compile that migration fixture. Deleting the parameter is also a valid, supported .NET 10 approach; there is no replacement switch to enable. Keep the routes unambiguous rather than relying on markup order.

## Newer approach — .NET 11

Replace `Routes.razor` with the supported form:

{{snippet:newer}}

Use these two separate routable component files, `NewStation.razor` and `Station.razor`. The literal route wins over the parameter route:

{{snippet:exact}}

{{snippet:parameter}}

For a complete small Web App, use the following project, `_Imports.razor`, `Program.cs` and `App.razor`. The namespace in `_Imports.razor` applies to all components:

{{snippet:project}}

In `_Imports.razor`, declare `@namespace Playbook.Samples` and add `@using` directives for `Playbook.Samples`, `Microsoft.AspNetCore.Components`, `Microsoft.AspNetCore.Components.Forms`, `Microsoft.AspNetCore.Components.Routing`, and `Microsoft.AspNetCore.Components.Web`. Add `@using static Microsoft.AspNetCore.Components.Web.RenderMode` for render-mode names.

{{snippet:startup}}

{{snippet:shell}}

Without a query parameter, this shell renders static SSR. `?interactive=1` selects Interactive Server without prerendering, so observing the heading proves a live circuit selected the route. The links intentionally select that mode for navigation testing. This query switch is sample instrumentation, not a new routing API or required application design.

### Asset inventory correction

The official removal table's ResourceAsset entry must be qualified. Both measured releases expose `ResourceAsset(string url, IReadOnlyList<ResourceAssetProperty>? properties = null)`. Neither has a distinct one-argument CLR constructor. The C# one-argument call already uses the optional second argument and requires no migration:

{{snippet:assets}}

These three calls compile against both actual targets. They do not establish asset loading or browser caching behavior, and this correction adds no separate asset feature.

## Migration

1. Inspect the application's target, SDK, matching ASP.NET Core assets and Router declarations. Preserve the supported .NET 10 implementation if the application is staying on .NET 10.
2. Remove every `PreferExactMatches` markup parameter or code assignment, including assignments of `false`. Remove its obsolete-warning suppression once no historical fixture needs it.
3. Retain the literal and parameter route templates, `AppAssembly`, route view and existing application layout/authentication decisions. The new sample has neither an authorization provider nor a layout dependency.
4. Rebuild against the selected framework. Shared Components APIs provide the Router and ResourceAsset; no WebAssembly authentication package is needed.
5. Request `/stations/new` and `/stations/SA7` without the query to check static SSR endpoint routing. Expect `Create station` and `Station SA7` respectively.
6. Open `/stations/new?interactive=1` in a real browser and follow Open SA7, then Create station. Expect the same headings without a document reload. Static SSR request success alone does not verify interactive Router navigation.

## Constraints

The recipe edition targets .NET 11. Its paired comparison is a real .NET 10 application, not an additional .NET 10 foundation. Supported Router markup without the flag and the optional ResourceAsset constructor already work on .NET 10. No .NET 11 package-on-.NET 10 route is claimed.

Use the matching ASP.NET Core shared runtime, Web SDK/reference pack and SDK-aligned browser assets. The current canonical project declares C# 15; the previous project declares C# 14. No new language syntax, experimental opt-in, native workload, external provider or account is required. Interactive Server requires a working browser circuit connection. WebAssembly, Auto, authorization, reverse proxies, ambiguous identical routes and custom route tables were not exercised here.

The exact verification pins are SDK 10.0.402 / framework and browser assets 10.0.13, and SDK 11.0.100-rc.2.26504.105 / framework and browser assets 11.0.0-rc.2.26504.105. They are recorded test artifacts, not universal exact consumer prerequisites. The stable replacement API does not make RC2 execution into GA evidence.

## Evidence

Both original TidalStations projects compiled with their own SDK pin, language mode, matching framework references and locked browser asset dependency. Saved compilation records include input hashes, exact commands and dependencies. Separate freshness-checked execution records contain actual static HTTP responses and Chromium 153 navigation through both routes with prerender disabled and a same-document marker retained.

`eng/Record-RemovedBlazorBehavior.ps1` runs the real built applications through `eng/Test-RemovedBlazorConsumers.mjs`; its `behavior.evidence.json` records distinguish execution from compilation-only asset calls. The bounded API audit was rerun and matched both original reference-asset hashes/signatures. Installed-package checks retrieve the complete canonical body, discovery goal, applicability and evidence over real stdio outside the checkout, with explicit target 11.0.

The aggregate `blazor.replace-removed-api` / WEB-089 remains outstanding for its other migration families. Comprehensive coverage and GA reconciliation remain separate gates. Consult the embedded official sources for the inert Router contract and the unchanged optional asset constructor.
