# Keep a custom event distinct from its browser trigger

## Goal

Keep a component's typed edit notification working when upgrading to ASP.NET Core 11.
CairnSignals records which survey station received an edit, the input operation
and the inserted text. A browser trigger describes the native action;
a custom alias identifies the application's payload contract. Give those two
roles different names.

## Older approach

The .NET 10 registry permits an unregistered alias named `beforeinput` to refer
to the browser's `beforeinput` event. This is a native browser event even though
Blazor does not pre-register it as a built-in event type in the inspected assets.
The original older sample below deliberately retains that collision for comparison.
In the measured browser, one trusted keypress delivered the payload twice.
Allowance by the old registry was never a promise of exactly one notification.

A distinct alias was already valid on .NET 10. Applications that already use one
can keep it; custom event metadata and `registerCustomEventType` are not new APIs.
Do not use `click` as the sole upgrade test: it is already registered and the
pre-existing duplicate-registration guard rejects it in both releases.

Create a fresh `net10.0/CairnSignals` directory and save each file at the path
shown below. These files are self-contained: the three empty Directory files
deliberately stop parent build/package inheritance. No `_Imports.razor`,
template, CSS, launchSettings or appsettings file is required. The project
defaults its assembly name to CairnSignals, matching the initializer filename.

### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### .NET 10: `CairnSignals.csproj`

{{snippet:n10-cairnsignals-csproj}}

### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

### .NET 10: `App.razor`

{{snippet:n10-app-razor}}

### .NET 10: `EventHandlers.cs`

{{snippet:n10-eventhandlers-cs}}

### .NET 10: `SignalNote.razor`

{{snippet:n10-signalnote-razor}}

### .NET 10: `wwwroot/CairnSignals.lib.module.js`

{{snippet:n10-wwwroot-cairnsignals-lib-module-js}}

## Newer approach

Browser event-name collision now throws. The ASP.NET Core 11 registration guard
rejects an alias equal to its nonempty `browserEventName` before adding it.
This differs from the older duplicate-registration check: an otherwise unused
`beforeinput` alias now fails with a same-name message.

The repaired application registers `cairnedit`, declares `oncairnedit` in
`EventHandlers`, and uses `@oncairnedit` on the input. The trigger remains
`beforeinput`. Its meaningful payload still contains station, operation and data.
No browser event itself is renamed.

Create a separate fresh `net11.0/CairnSignals` directory and save every file below.
The document supplies the namespace and render mode import explicitly. The
component imports the namespace containing the public EventHandlers class.
`afterWebStarted(blazor)` receives the Blazor API; `beforeWebStart` receives
options and is not a registration hook. The SDK discovers
`wwwroot/CairnSignals.lib.module.js` automatically; do not also import or call it
manually. Registration belongs to application startup, not each component render.

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### .NET 11: `CairnSignals.csproj`

{{snippet:n11-cairnsignals-csproj}}

### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

### .NET 11: `App.razor`

{{snippet:n11-app-razor}}

### .NET 11: `EventHandlers.cs`

{{snippet:n11-eventhandlers-cs}}

### .NET 11: `SignalNote.razor`

{{snippet:n11-signalnote-razor}}

### .NET 11: `wwwroot/CairnSignals.lib.module.js`

{{snippet:n11-wwwroot-cairnsignals-lib-module-js}}

## Migration

1. Inventory each JS alias, its browser trigger and every Razor EventHandler
   attribute/directive using it. Distinguish an already-registered-name error
   from the new alias-equals-trigger error. Choose one unused application name.
2. Change the JS alias and the Razor metadata/directive together, retaining
   `browserEventName`, the event-argument properties and the handler's meaning.
   Rebuild static assets and components together; refresh cached browser assets.
3. To reproduce the complete examples offline after restoring SDK/package assets,
   open PowerShell in the chosen example directory. Set
   `$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH='0'`. Build with the matching absolute
   .NET executable: `& 'C:\Program Files\dotnet\dotnet.exe' build -c Release`
   for .NET 10 or
   `& 'C:\Users\pingu\.dotnet-playbook-11\dotnet.exe' build -c Release`
   for .NET 11 (substitute your installed executable locations).
   The pinned SDK resolves Microsoft.AspNetCore.App.Internal.Assets 10.0.13 or
   11.0.0-rc.2.26504.105; it is implicit in Microsoft.NET.Sdk.Web.
4. Set process-only `$env:ASPNETCORE_ENVIRONMENT='Development'` and
   `$env:DOTNET_ENVIRONMENT='Development'` so build-output static assets load.
   Run the matching executable with
   `./bin/Release/net10.0/CairnSignals.dll --urls http://127.0.0.1:5971`
   or `./bin/Release/net11.0/CairnSignals.dll --urls http://127.0.0.1:5972`.
   Each host occupies its terminal; use a second window for the other release.
   This is local build-output execution, not a publish/deployment recipe.
5. Browse to the loopback root, wait for `Interactive: True`, focus the labelled
   input and press `a` once. The old comparison displays `Deliveries: 2` in the
   measured Chrome; the new pair displays `Deliveries: 1` and
   `North cairn | insertText | a`. The exact old duplication is observed evidence,
   not a cross-browser guarantee. Close the page and use Ctrl+C to stop each host.
6. As a new-framework failure control, temporarily change only the new JS alias
   back to `beforeinput`, rebuild and reload. Registration must throw a message
   containing `cannot have the same name as its browserEventName`. Restore
   `cairnedit`, rebuild and reload before testing payload delivery again.
   Our real browser check calls the same registration API directly after startup
   to capture this rejection without altering the repaired sample.

## Constraints

The demonstrated rendering mode is a Blazor Web App using Interactive Server
with prerendering. Static SSR renders markup but has no interactive .NET event
handler. Server-side handlers need a connected circuit; the input remains a
normal browser input. There is no persistence, deduplication or delivery guarantee.

Runtime/SDK family 11 and matching browser assets are required for this changed
guard. Samples pin SDK 10.0.402/C# 14 and SDK 11.0.100-rc.2.26504.105/C# 15,
ASP.NET Core/runtime 10.0.13 and 11.0.0-rc.2.26504.105 respectively. There are no
external packages, providers or required workloads. The Web SDK implicitly resolves
the matching Internal.Assets package; preserve the generated lock file after the
first restore for a repeatable local build. An empty workload requirement is not a
claim that the author's installed SDK has no workloads.

These event contracts are stable APIs, not experimental APIs. Current execution
is RC2 evidence, not GA validation. WebAssembly/Hybrid startup hooks, other OS or
browsers, AOT, deployment, and using new assets on an older target are unverified.
Only target 11.0 selects this migration edition; the .NET 10 comparison remains
complete offline, without advertising an untested older-target route.

## Evidence

Both original projects compile with their own pinned SDK. Separate real Chrome
checks exercise a trusted native `beforeinput`, actual Interactive Server handlers,
payload values and counts. They compare the served framework asset bytes with
the expected guard and distinguish the existing `click` duplicate error from
the new `beforeinput` same-name error. Source review, compilation, HTTP artifact
inspection and browser execution are distinct records; none is a DOM mock.

The installed packed tool returns this complete expanded body and every authored
applicability/evidence field over offline stdio. Temporary consumers are reconstructed
solely from that returned Markdown, then built and exercised separately.
See the repository's `docs/verification/PIN-799.md` for exact artifacts, source
commit/dirty provenance, commands and measured limits. No package publication or
external resource is required.

Official references: [previous registry](https://github.com/dotnet/aspnetcore/blob/v10.0.13/src/Components/Web.JS/src/Rendering/Events/EventTypes.ts),
[pinned new guard](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Web.JS/src/Rendering/Events/EventTypes.ts), and [initializer lifecycle](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/startup?view=aspnetcore-11.0).
The compatibility-guide URL in the inventory could not be read during this
authoring check; the version-pinned actual source and served assets establish the
guard. No content from that inaccessible guide is claimed as reviewed.
