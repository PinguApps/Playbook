# Reuse startup options when changing Blazor hosts

## Goal

A parcel dispatch desk needs the same connection setup and browser resource
loader whether it runs on a server circuit or inside the browser. Keep one
configuration factory when moving the desk between legacy hosting and a
Blazor Web App; choose the framework script to match the host.

## Older approach

.NET 10 already accepts nested circuit and webAssembly groups in blazor.web.js.
Its legacy blazor.server.js and blazor.webassembly.js instead read their options
at the top level. The previous-release sample projects the relevant group for
each legacy script. That is a valid adapter, not a broken configuration.

{{snippet:old-startup}}

## Newer approach

.NET 11's legacy scripts accept the same nested circuit and webAssembly groups
as blazor.web.js. The unused hosting group does not start another renderer.
The legacy scripts still support the older flat format. They select the nested
group when present rather than merging flat and nested values. Keep one shape
per call so that a stale flat setting cannot look like an override.

The Web App script continues to use nested groups; flat legacy compatibility
does not make a flat Web App configuration equivalent. Its additional ssr
options remain specific to server-rendered document behavior. JavaScript uses
circuit, not the server key in a serialized server-supplied browser policy.
This recipe does not configure that separate C# policy.

{{snippet:new-startup}}

The configureSignalR callback runs on the framework's actual connection builder.
The sample observes the built connection's 45000 ms server timeout to make
selection checkable. That diagnostic wrapper is optional in a consumer; choose
a timeout appropriate to its connection. The loader records real boot requests
and returns undefined so framework fetching and integrity checks continue.
applicationCulture is observed as fr-FR inside the running browser component,
rather than inferred from a printed options object. Never put secrets in these
browser-visible options.

## Migration

1. Upgrade matching server/runtime/browser assets together. Keep the .NET 10
   adapter for a host still using .NET 10 legacy scripts.
2. Put circuit settings under circuit and WASM settings under webAssembly.
   Load exactly one framework script with autostart="false", followed by the
   startup file. Do not call Blazor.start again after automatic startup.
3. Keep JavaScript callbacks in browser code. Serialization cannot transmit a
   function. Confirm each callback is invoked by the real selected renderer.
4. Exercise the interactive dispatch button after startup. A resolved
   Blazor.start promise, especially for blazor.web.js, does not prove that the
   deferred circuit or WASM renderer is ready.

## Complete offline setup

Create three sibling folders named Host, Client and LegacyClient, with the
files below at their stated paths. Each release is a separate copy, never a
newer SDK pretending to be the previous release. No stylesheet is required.
The inherited sample build policy only adds deterministic compilation and
warnings as errors; the projects below declare their own runtime/language,
nullable/import behavior and executable SDKs.

Place global.json in **each** project directory. Set sdk.version to
10.0.402 for the previous release or 11.0.100-rc.2.26504.105 for the current
release, and set `rollForward` to `disable`. For example, the complete
current file is `{"sdk":{"version":"11.0.100-rc.2.26504.105","rollForward":"disable"}}`.
Use C# 14/.NET 10.0.13 assets versus C# 15/.NET 11.0.0-rc.2.26504.105 assets.
The exact pins are verification baselines; RC evidence is not GA certification.

Place the shown Directory.Packages.props in each of the three project directories. It isolates package
versions from any consuming repository's central policy. If embedding these
projects in a repository with shared build imports, isolate Directory.Build.props
and Directory.Build.targets at that boundary too. A standalone directory needs
no shared imports. Restore produces packages.lock.json; retain it and use
RestoreLockedMode=true for repeatable follow-up builds.

### Previous-release projects

Directory.Packages.props:

{{snippet:old-packages}}

Host/Host.csproj, Client/Client.csproj and LegacyClient/LegacyClient.csproj:

{{snippet:old-host-project}}

{{snippet:old-client-project}}

{{snippet:old-legacy-project}}

### Current-release projects

Directory.Packages.props:

{{snippet:new-packages}}

Host/Host.csproj, Client/Client.csproj and LegacyClient/LegacyClient.csproj:

{{snippet:new-host-project}}

{{snippet:new-client-project}}

{{snippet:new-legacy-project}}

WasmBuildNative=false selects the prebuilt browser runtime, avoiding an optional
native workload even on a machine where wasm-tools is installed. There is no
DevServer, Gateway, AOT, provider or external service dependency for these checks.

### Imports and namespaces

In each release, Host/_Imports.razor contains these exact directives:

@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Web
@using static Microsoft.AspNetCore.Components.Web.RenderMode
@using ParcelStartup.Client
@namespace ParcelStartup.Host

Client/_Imports.razor uses the same first four lines and
`@namespace ParcelStartup.Client`. LegacyClient/_Imports.razor uses the
same first three lines, then `@using ParcelStartup.LegacyClient` and
`@namespace ParcelStartup.LegacyClient`. These are component imports, not
HTML markup. The legacy Razor Page includes its own MVC imports below.

### Server and document files

The following canonical files are identical between the two release copies.
They compile separately against each copy's actual dependencies.

Host/Program.cs:

{{snippet:new-host}}

Host/App.razor and Host/Routes.razor:

{{snippet:new-app}}

{{snippet:new-routes}}

Host/Server.razor and Host/Wasm.razor:

{{snippet:new-server-page}}

{{snippet:new-wasm-page}}

Host/Pages/Legacy.cshtml starts with `@page "/legacy-server"` as its first
line, followed by this complete page body:

{{snippet:new-legacy-page}}

The Web App endpoint registration supplies the circuit endpoint for both pages.
Do not also map a second default Blazor hub: duplicate initializer endpoints
cause an ambiguous match. Prerender is disabled on the Web App pages to make
browser-versus-server execution visible. The legacy page prerenders, so wait
for a responding button before calling its initial HTML interactive.
The counter keeps its button disabled during prerender and enables it after the
first interactive render. The verifier then clicks exactly once and awaits the
result, including with real server event traffic delayed beyond 50 ms.

Client/Program.cs and Client/ParcelCounter.razor:

{{snippet:new-client}}

{{snippet:new-counter}}

LegacyClient/Program.cs, LegacyClient/ParcelCounter.razor and
LegacyClient/wwwroot/index.html:

{{snippet:new-legacy}}

{{snippet:new-legacy-counter}}

{{snippet:new-index}}

Copy the release's canonical startup.js shown above to both
Host/wwwroot/startup.js and LegacyClient/wwwroot/startup.js. The documents'
data-startup values choose the older adapter; the current default passes the
nested object unchanged. The flat query switch is only a compatibility
diagnostic for current legacy scripts.

### Build and run without repository files

From each project directory use that release's pinned dotnet executable.
Run `dotnet restore`, then `dotnet build -c Release -p:RestoreLockedMode=true`.
The Host project also builds its Client reference. From Host run
`dotnet run -c Release --no-build --no-launch-profile -- --urls http://127.0.0.1:5391 --environment Development`.
For the older host use port 5390. Development enables the build-time static
asset manifest, including the referenced Client browser runtime.

From LegacyClient run `dotnet publish -c Release -p:RestoreLockedMode=true`.
Serve **only** bin/Release/net11.0/publish/wwwroot (net10.0 for the older copy)
from a local static HTTP server on port 5393 (5392 older). With Node.js 22+
save this dependency-free server as serve.mjs, then run
`node serve.mjs <absolute-publish-wwwroot-path> 5393` (5392 older):

{{snippet:static-server}}

The server must return
application/wasm for .wasm and text/javascript for .js. Do not open index.html
as file:// or substitute another client's _framework directory. No deployment
or account is needed. A normal production publish/hosting setup is a separate
consumer validation.

## Evidence

Open /legacy-server, /web-server and /web-wasm on each host, and / on the
standalone client. Click Dispatch parcel: count must change from 0 to 1.
Server pages invoke configureSignalR exactly once and observe timeout 45000.
WASM pages invoke loadBootResource, display execution browser and culture fr-FR.
On .NET 11 repeat each legacy script with ?shape=flat: those same observations
prove flat compatibility. Static SSR alone does not exercise these paths.

All six real .NET 10/11 projects compile. Both standalone clients were
published with prebuilt browser runtimes. Real Chromium 152 on Windows
executed the eight primary paths and both current flat legacy comparisons.
The source commit, normalized inputs, exact binaries/browser assets and runtime
reports are recorded separately in PIN-789's verification record. Compilation
does not prove startup behavior; neither source review nor a promise-resolution
test substitutes for the callback and responding-control checks.

This stable API was checked using .NET 11 RC2 assets; GA reconciliation remains
pending. Auto selection, Hybrid, other browsers/platforms and production
deployment are outside this evidence. No newer-package/older-target route is
claimed. .NET 10 examples are comparisons rather than a served older edition
of this new cross-script contract.

## Constraints

Keep existing flat legacy configuration when no shared factory is needed.
For mixed .NET 10/11 estates, keep the small projection adapter until the
legacy assets are upgraded. A common object shape does not make circuit
settings meaningful in WASM, remove rendering restrictions, share client
secrets safely, or guarantee transport/reconnection policy.

SSR settings apply to blazor.web.js. Interactive Server requires a reachable
ASP.NET Core circuit endpoint. Interactive WebAssembly requires the matching
client assembly, boot/runtime files and a WASM-capable browser; this recipe
does not infer those from a server build alone.

## References

- [Blazor startup configuration](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/startup?view=aspnetcore-11.0)
- [Previous legacy Server](https://github.com/dotnet/aspnetcore/blob/0ef4bbfa3291b306a21e5001cb0491277bdd35bc/src/Components/Web.JS/src/Boot.Server.ts)
- [Previous legacy WebAssembly](https://github.com/dotnet/aspnetcore/blob/0ef4bbfa3291b306a21e5001cb0491277bdd35bc/src/Components/Web.JS/src/Boot.WebAssembly.ts)
- [Previous Web App groups](https://github.com/dotnet/aspnetcore/blob/0ef4bbfa3291b306a21e5001cb0491277bdd35bc/src/Components/Web.JS/src/Platform/WebStartOptions.ts)
- [RC2 legacy Server normalization](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Web.JS/src/Boot.Server.ts)
- [RC2 legacy WebAssembly normalization](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Web.JS/src/Boot.WebAssembly.ts)
- [RC2 Web App options](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Web.JS/src/Platform/WebStartOptions.ts)
- [RC2 circuit callback contract](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Web.JS/src/Platform/Circuits/CircuitStartOptions.ts)
- [RC2 WebAssembly callback and culture](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Web.JS/src/Platform/WebAssemblyStartOptions.ts)
