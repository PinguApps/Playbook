# Render the application base from its hosting path

## Goal

The tide archive is mounted beneath /atlas/ on one host and /museum/ on another.
Its document base must describe the application root on either host. A base of
/ sends links and framework assets to the enclosing site's root; a base of the
current collection directory makes application-wide links drift into that
collection. Neither represents the application deployment location.

## Older approach: request-derived markup on .NET 10

Server rendering already provides HttpContext.Request.PathBase. The older app
uses it to emit a trailing-slash base, avoiding a separate hard-coded deployment
prefix in the document. This is a valid manual implementation, not a broken
approach. Keep HttpContext use in the static server document shell; do not rely
on it remaining available in an interactive circuit.

{{snippet:old-base}}

## Newer approach: BasePath on .NET 11

BasePath emits the base element using NavigationManager.BaseUri. Put it in the
Blazor Web App's document head before links or browser assets need resolution.
Keep only one base element. In RC2 the component namespace is
Microsoft.AspNetCore.Components, even though its assembly is
Microsoft.AspNetCore.Components.Endpoints.dll. Earlier preview Endpoints
namespace examples do not compile against the measured RC2 assets.

{{snippet:base}}

BasePath renders a trailing-slash application path such as /atlas/. It does not
discover an external reverse-proxy mount or change server routing. Establish
PathBase first. In the local sample the host accepts ApplicationPathBase as
configuration, defaulting to /atlas; routing and assets then see the remaining
request path.

{{snippet:startup}}

## Project and router setup

Use Microsoft.NET.Sdk.Web, net11.0, LangVersion 15.0, Nullable enable and
ImplicitUsings enable, with RootNamespace Playbook.Samples. The comparison uses
net10.0/C# 14.0. Each project has its own global.json: SDK 11.0.100-rc.2.26504.105
or 10.0.402. The measured SDK restores aligned
Microsoft.AspNetCore.App.Internal.Assets browser files (11.0.0-rc.2.26504.105 or
10.0.13); no explicit framework API package, provider, native workload or service
account is required. Preserve the SDK-aligned assets and lock file on restore.

Import Microsoft.AspNetCore.Components.Routing and
Microsoft.AspNetCore.Components.Web, plus static
Microsoft.AspNetCore.Components.Web.RenderMode in _Imports.razor. The Routes
component used above contains:

{{snippet:router}}

Give CollectionPage.razor the route /collections/{Collection}/{Panel}, with
matching string [Parameter] properties and ordinary content. From a PowerShell
terminal in the newer sample directory, run its pinned SDK with
dotnet run -c Release --no-launch-profile -- --urls http://localhost:5295
--ApplicationPathBase /museum --environment Development. Request
http://localhost:5295/museum/collections/kelp/overview. The application base is
/museum/, not /museum/collections/kelp/. The newer relative-navigation recipe
shows the full declarative/imperative controls for that page.

## Migration

1. Preserve the current server/proxy path-base setup and verify the external path
   reaches ASP.NET Core correctly. Set it before routing, static assets and endpoints.
2. Replace the manual base element in the static App document head with BasePath.
   Use the RC2 Components namespace and remove the duplicate base element.
3. Request a nested URL directly, inspect the generated HTML, then follow a link
   and load a framework asset. Repeat at a different configured prefix.

## Constraints

This is Blazor Web App server-document guidance. A standalone WebAssembly
index.html remains a deployment artifact with its own base configuration;
BasePath does not rewrite that file. The canonical routes use Interactive Server,
while the document shell is static server-rendered. No WebAssembly/Auto or actual
reverse-proxy execution is claimed. The .NET 10 sample is a comparison, not a
conditional package route for BasePath. Contracts are non-experimental; .NET 11
execution uses RC2, with final GA reconciliation pending.

## Evidence

The actual .NET 10 and .NET 11 apps compile independently. Real HTTP requests
return the generated base and all three nested routes plus blazor.web.js beneath
/atlas/. The same .NET 11 binary emits /museum/ when only host configuration
changes. Chromium checks exactly one base element and follows nested declarative,
imperative and section navigation on each app/prefix. Saved execution reports
include exact input/binary fingerprints. These are local requests and actual
browser execution, not deployment, cross-compilation, mocks or GA proof.
