# Expose an intended JavaScript callback with the right visibility

## Goal

Make a deliberately exposed browser-to-.NET instance callback public, call its declared JSInvokable identifier, and release the object reference when the component leaves. Willow Quay is an original fictional ferry desk, aquarium and water-level panel. The three controls share one runnable host so the checks can distinguish event renders, delayed closures and real interop.

## Older approach

The water-level instrument calls a .NET instance through a DotNetObjectReference. Before/InstrumentPanel.razor marks a private ReceiveLevel method with JSInvokable("AcceptLevel"); actual JavaScript rejects the call. The attribute did not make private methods callable on .NET 10. The corrected public .NET 10 implementation below already works and remains valid.

The root component files below are working .NET 10 equivalents. The Before files deliberately preserve all three old mistakes for a separate failure build; they are excluded by default. The project boundaries prevent inherited tool build/analyzer settings.

### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### .NET 10: `WillowQuay.csproj`

{{snippet:n10-willowquay-csproj}}

### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

### .NET 10: `PanelAudit.cs`

{{snippet:n10-panelaudit-cs}}

### .NET 10: `App.razor`

{{snippet:n10-app-razor}}

### .NET 10: `ActionDesk.razor`

{{snippet:n10-actiondesk-razor}}

### .NET 10: `TicketDisplay.razor`

{{snippet:n10-ticketdisplay-razor}}

### .NET 10: `FeedingControls.razor`

{{snippet:n10-feedingcontrols-razor}}

### .NET 10: `InstrumentPanel.razor`

{{snippet:n10-instrumentpanel-razor}}

### .NET 10: `wwwroot/panel.js`

{{snippet:n10-wwwroot-panel-js}}

### .NET 10: `Before/TicketDisplay.razor`

{{snippet:n10-before-ticketdisplay-razor}}

### .NET 10: `Before/FeedingControls.razor`

{{snippet:n10-before-feedingcontrols-razor}}

### .NET 10: `Before/InstrumentPanel.razor`

{{snippet:n10-before-instrumentpanel-razor}}

## Newer approach

BL0015 diagnoses the non-public callback before running the browser. Make only the intended callback public and keep its declared identifier AcceptLevel. panel.js invokes that identifier, not the C# name ReceiveLevel. The button event awaits JS and displays its returned value. It catches JSException for this bounded demonstration; general disconnect/error recovery belongs to separate guidance.

No suppression or experimental opt-in is required. The new warning does not introduce new runtime behavior. Retain the other working components and complete host setup when copying this self-contained example.

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### .NET 11: `WillowQuay.csproj`

{{snippet:n11-willowquay-csproj}}

### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

### .NET 11: `PanelAudit.cs`

{{snippet:n11-panelaudit-cs}}

### .NET 11: `App.razor`

{{snippet:n11-app-razor}}

### .NET 11: `ActionDesk.razor`

{{snippet:n11-actiondesk-razor}}

### .NET 11: `TicketDisplay.razor`

{{snippet:n11-ticketdisplay-razor}}

### .NET 11: `FeedingControls.razor`

{{snippet:n11-feedingcontrols-razor}}

### .NET 11: `InstrumentPanel.razor`

{{snippet:n11-instrumentpanel-razor}}

### .NET 11: `wwwroot/panel.js`

{{snippet:n11-wwwroot-panel-js}}

### .NET 11: `Before/TicketDisplay.razor`

{{snippet:n11-before-ticketdisplay-razor}}

### .NET 11: `Before/FeedingControls.razor`

{{snippet:n11-before-feedingcontrols-razor}}

### .NET 11: `Before/InstrumentPanel.razor`

{{snippet:n11-before-instrumentpanel-razor}}

## Migration

1. Choose the consumer target and matching SDK/runtime. For this primary diagnostic route use net11.0 and the pinned RC2 SDK when reproducing the recorded result. Keep net10.0 and its own SDK for the valid older equivalent. Do not install the analyzer into an unsupported target by assumption.
2. Confirm the method is intended to be callable by JavaScript. Make ReceiveLevel public and retain [JSInvokable("AcceptLevel")]. Remove the attribute instead if the method should not be exposed.
3. Retain input checks and enforce any real authorization separately on the trusted server. A public JS-invokable method and its identifier are interop discovery, not an authorization boundary or secret. This sample performs no privileged action.
4. Own one DotNetObjectReference per component, reuse it for reads, and dispose it when the component is removed. Do not invoke its stale JS proxy after disposal. The callback itself only returns a value; its awaiting component event handles display updates. An independently initiated callback that changes UI may require an explicit renderer dispatch/render.
5. Copy each target's files into its own directory, preserving Before/ and wwwroot/. Run the PowerShell commands below from that directory, using the absolute matching dotnet executable. Build with locked restore after the first restore generates packages.lock.json. Each command starts a local server and occupies the terminal until Ctrl+C. The /audit route is an unauthenticated local test probe only; omit it in a production app.

In the copied .NET 11 directory, use the matching absolute SDK executable as $dotnet. Set `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` in the child environment. Run `& $dotnet restore`, then `& $dotnet build -c Release -p:RestoreLockedMode=true`, then `& $dotnet ./bin/Release/net11.0/WillowQuay.dll --urls http://127.0.0.1:5987`.

Visit http://127.0.0.1:5987. Click Read instrument: expect Level 137 cm. Direct real-JS AcceptLevel(138) returns Level 138 cm. Negative input and calling ReceiveLevel instead of AcceptLevel both reject. Toggle the panel off: the component's Dispose counter becomes 1 and the saved JS proxy rejects further invocations. The counter measures component disposal; proxy rejection separately demonstrates reference release. The private before control reports Callback unavailable. RC2 reports BL0015 before and none after.

To reproduce the original diagnostics, stop the host and rebuild with `-t:Rebuild -p:BeforeMigration=true -p:TreatWarningsAsErrors=false`. This intentional negative build selects all three Before files and reports BL0012, BL0014 and BL0015 on RC2; it does not repair anything automatically. Run its output to observe the old failures. Restore the default with `-t:Rebuild` and no control properties. For the necessary-render negative control, use `-t:Rebuild -p:OmitTimerRender=true` instead; /audit confirms the timer fired while the UI waits for a later click. These controls are test-only. On .NET 10 use its matching executable, net10.0 output path and SDK pin; the failures and fixes still execute, but its analyzer has none of these three warnings.

## Constraints

- Click Read instrument: expect Level 137 cm. Direct real-JS AcceptLevel(138) returns Level 138 cm. Negative input and calling ReceiveLevel instead of AcceptLevel both reject. Toggle the panel off: the component's Dispose counter becomes 1 and the saved JS proxy rejects further invocations. The counter measures component disposal; proxy rejection separately demonstrates reference release. The private before control reports Callback unavailable. RC2 reports BL0015 before and none after.
- The fixes are valid on .NET 10; only the diagnostic is new in the measured .NET 11 analyzer. No SDK-only older-target analyzer route, package backport or additional .NET 10 foundation is advertised.
- Interactive Server with prerender disabled is the executed route. No WASM, static SSR callback, Hybrid, AOT/trimming, arbitrary concurrency, performance or GA execution is established.
- Matching SDK/compiler/analyzer, reference/runtime and implicit asset package are measured separately in saved evidence. No manually referenced compiler DLL or native asset is required. Historical RC2 compilation/execution is not GA support verification.
- No authentication, database/provider, production resource or device is involved. Input validation is demonstrated; a real application must independently decide and enforce authorization.

## Evidence

Both actual dependency sets compile with fresh sample evidence. The authoring check builds before, after and missing-timer-render variants on both matching runtimes and uses installed Chrome through Playwright. It checks callback visibility, declared identifier, invalid-input rejection, one component disposal and stale-proxy rejection; the shared harness also exercises render and loop controls. RC2 analyzer and Razor compiler hashes are recorded alongside runtime identities. The saved records retain their capture commit and dirty state; freshness binds their exact input hashes, rather than requiring the capture commit to equal the current Git HEAD. The ordinary package is separately installed from a local-only feed/cache; real offline stdio list/get must preserve ID, title, goal, edition, full canonical files, applicability and current evidence. Browser and package checks prove different claims.

## References

- [Pinned BL0015 analyzer implementation](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Analyzers/src/JSInvokableAnalyzer.cs)
- [Pinned analyzer diagnostic descriptors](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Analyzers/src/DiagnosticDescriptors.cs)
- [Component event and rendering guidance](https://learn.microsoft.com/aspnet/core/blazor/components/rendering?view=aspnetcore-11.0)
- [Browser-to-.NET interop and object references](https://learn.microsoft.com/aspnet/core/blazor/javascript-interoperability/call-dotnet-from-javascript?view=aspnetcore-11.0)
