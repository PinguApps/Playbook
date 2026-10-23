# Build section links for the current page

## Goal

An archive reader opens /atlas/collections/kelp/overview?sort=date#previous and
wants the packing instructions on that same page. The destination must retain
the application prefix, collection, page and query. Appending another # would
leave the old fragment in the wrong place; resolving a bare #packing against
the document base can point at the application root instead of the page.

## Older approach: UriBuilder on .NET 10

The older implementation sets UriBuilder.Fragment on the current absolute URI.
It replaces any existing fragment while preserving the page and query. This is
a supported and reasonable implementation; its tiny helper keeps the rules in
one place.

{{snippet:old-fragment}}

{{snippet:old-sections}}

## Newer approach: GetUriWithFragment on .NET 11

Use the NavigationManager extension in Microsoft.AspNetCore.Components.
It starts from Nav.Uri, not Nav.BaseUri. It adds or replaces the fragment,
accepts an optional leading #, and removes the fragment for null or empty input.
The current API spelling is GetUriWithFragment; the historical GetUriWithHash
name is not the RC2 contract.

{{snippet:fragment}}

{{snippet:sections}}

The result for packing in the opening example is
/atlas/collections/kelp/overview?sort=date#packing at the current origin.
An existing #previous is replaced. Null produces the same page and query with
no fragment. The helper constructs a URI string; the anchor or NavigateTo must
still follow it. It does not scroll without navigation, create the target DOM
element or escape arbitrary fragment input. Use known valid IDs as shown; encode
or otherwise validate dynamic fragment values before calling it. The UriBuilder
comparison can normalize/escape other URI text differently, so do not assume
byte-for-byte equivalence for arbitrary inputs.

## Complete setup

Use Microsoft.NET.Sdk.Web, net11.0 and C# 15.0; the comparison uses net10.0 and
C# 14.0. Enable nullable and implicit usings. Pin SDK 11.0.100-rc.2.26504.105 or
10.0.402 independently for reproducing the samples. These APIs come from the
ASP.NET Core shared/reference framework. The measured Web SDK supplies aligned
Internal.Assets browser files 11.0.0-rc.2.26504.105 or 10.0.13; no additional API
package, provider, native workload or account is required.

{{snippet:startup}}

Place the helper in CollectionPage.razor with @inject NavigationManager Nav,
@page "/collections/{Collection}/{Panel}" and matching string [Parameter]
properties. Import Microsoft.AspNetCore.Components. Add a section element with
id="packing" around the packing content. Render the page through Router and
RouteView inside Routes, with InteractiveServer on Routes in App.razor. Import
Microsoft.AspNetCore.Components.Routing and the static
Microsoft.AspNetCore.Components.Web.RenderMode members. In the static document
head use one BasePath component (Components namespace in RC2); load
@Assets["_framework/blazor.web.js"] at the end of body. The application-base
recipe supplies the complete document shell. Ordinary anchors can also work
from static SSR; interactive button navigation needs an interactive component.

## Migration

1. Choose the current page as the reference point. Do not substitute BaseUri
   unless the intended target is actually the application root.
2. Replace manual fragment assembly with GetUriWithFragment using known target IDs.
   Keep the target element present when navigation completes.
3. Test from a nested page below a non-root base, including an existing query
   and fragment. Follow the generated link and confirm both URL and scroll.

## Constraints

Blazor navigation retains its usual routing and rendering constraints. Fragment
targets in content that arrives later may need deliberate post-render scrolling;
this recipe does not claim automatic discovery of delayed elements. No
WebAssembly/Auto, accessibility focus behavior or reverse-proxy execution is
established by the Interactive Server checks. No newer-package route on net10.0
is claimed. The API is non-experimental; the exact .NET 11 evidence is RC2 and
requires GA reconciliation later.

## Evidence

Both actual apps compile at their declared SDK/language pins. Chromium follows
packing links on both apps below /atlas/ and on the same newer binary below
/museum/. It starts with ?sort=date#previous, verifies replacement/preservation,
compares the optional # prefix, follows the link to the real section and measures
its viewport position, then follows the fragment-removal link. Separate HTTP
checks verify direct nested loading and generated base markup. Exact input and
assembly fingerprints accompany those execution reports; compilation, mocks
and source review are not presented as browser evidence.
