# Navigate between nearby routes from the current page

## Goal and choice of reference point

A tide archive groups its overview, notes and checklist beneath each collection.
From /atlas/collections/kelp/overview, the notes destination is
/atlas/collections/kelp/notes. The hosting prefix belongs to the application;
the collection directory belongs to the current route. Keep those two reference
points separate when choosing where a short link should go.

NavigationManager.BaseUri is the application root, including a trailing slash.
NavigationManager.Uri is the current absolute page URI, including query and
fragment. An ordinary href or NavigateTo destination such as collections/kelp/overview
is application-base relative. It deliberately returns to the archive home.
The new RelativeToCurrentUri option instead resolves notes from the current
URI's directory. Without a trailing slash the last path segment is replaced;
with a trailing slash it appends inside that directory. Route templates do not
change these URI rules.

## Older approach: explicit URI resolution on .NET 10

The older implementation uses System.Uri with the current absolute URI. This
remains a valid way to express the same reference point. It requires an explicit
helper for both the rendered link and button handler.

{{snippet:old-links}}

{{snippet:old-navigate}}

## Newer approach: declare relative navigation on .NET 11

Enable RelativeToCurrentUri on each current-route NavLink and on the
NavigationOptions passed to NavigateTo. The archive-home link deliberately
retains the default application-base behavior.

{{snippet:links}}

{{snippet:navigate}}

RC2's resolver may render an absolute URI; do not depend on a root-relative
spelling described by earlier preview documentation. Follow the resolved
destination. RelativeToCurrentUri defaults to false. The option requires a
relative destination: RC2 rejects absolute URLs and leading-slash paths.
Use ordinary navigation for those destinations. A relative path can contain
parent-directory segments; this option is not an authorization or confinement
boundary. Choose destinations from trusted application routes.

## Complete application setup

Use Microsoft.NET.Sdk.Web with net11.0, LangVersion 15.0, Nullable enable and
ImplicitUsings enable. The comparison uses net10.0/C# 14.0. The independently
pinned SDKs are 11.0.100-rc.2.26504.105 and 10.0.402. No explicit NuGet reference
is needed for these framework APIs. Keep the SDK-provided browser assets aligned;
the measured SDKs restore Internal.Assets 11.0.0-rc.2.26504.105 and 10.0.13.
No provider, native workload or service account is required.

{{snippet:startup}}

Place these controls in CollectionPage.razor with
@page "/collections/{Collection}/{Panel}", @inject NavigationManager Nav, and
public string Collection/Panel properties marked [Parameter]. Import
Microsoft.AspNetCore.Components and Microsoft.AspNetCore.Components.Routing.
Use a Routes component containing Router with AppAssembly="typeof(App).Assembly"
and a Found template containing RouteView RouteData="route". Render Routes with
@rendermode="InteractiveServer" in App.razor and import the static
Microsoft.AspNetCore.Components.Web.RenderMode members. The document head needs
one BasePath component (Microsoft.AspNetCore.Components in RC2). At the end of
body, load the script with src='@Assets["_framework/blazor.web.js"]'.
The companion base-path recipe includes the complete document shell.

## Migration

1. Configure the external application path in the server before routing and
   component/static-asset endpoints. The local sample defaults to /atlas.
2. Identify each link's intended reference point. Use the new option only for
   current-directory destinations; leave application-wide links at the default.
3. Replace explicit current-URI assembly with NavLink and NavigationOptions.
   Test a nested route with a query/fragment and a non-root hosting path.
4. Keep imperative button handlers inside an interactive component. Static SSR
   can render navigable anchors but does not run .NET click handlers in the browser.

## Constraints

The canonical browser execution is Interactive Server with a static document
shell. WebAssembly/Auto require their usual client services/assets and are not
executed here. Relative navigation does not configure the proxy or deployment
path. A reverse proxy must correctly establish the external request PathBase.
There is no newer-package route claimed for net10.0. These contracts are
non-experimental; the measured .NET 11 assets are RC2, with GA reconciliation
still pending.

## Evidence

Both actual Web SDK projects compile at their declared target/language pins.
Real HTTP requests load overview, notes, checklist and browser assets below
/atlas/. Chromium follows the rendered sibling link, the imperative button and
the application-base home link on both apps. The same .NET 11 assembly also
passes below /museum/, proving the prefix is configuration-derived. Separate
saved execution reports bind these checks to exact inputs and binary hashes.
Compilation alone, mocks, cross-compilation, reverse-proxy deployment and other
render modes are not reported as this execution evidence.
