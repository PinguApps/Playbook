# Let component events schedule their own rerender

## Goal

Remove redundant StateHasChanged calls from framework-managed component events and initialization while retaining explicit rendering for external callbacks. Willow Quay is an original fictional ferry desk, aquarium and water-level panel. The three controls share one runnable host so the checks can distinguish event renders, delayed closures and real interop.

## Older approach

Willow Quay shows ferry ticket 40, then 41 after a click. Before/TicketDisplay.razor adds StateHasChanged in OnInitialized and NextTicket. Those calls are redundant on .NET 10 too; the old framework already schedules these renders. The corrected .NET 10 file below remains valid. The old analyzer does not report BL0012; absence of that warning does not make the calls necessary.

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

Keep only the necessary external-callback render. The RC2 analyzer reports BL0012 for the original component when that source is built as a real net11.0 Razor consumer. The corrected component removes the redundant calls and has no BL0012. InvokeAsync moves the timer work onto the renderer dispatcher; StateHasChanged requests the otherwise missing render. The component owns and disposes its timer and checks a disposed flag inside dispatched work. Disposal with an already-queued timer callback was not exercised.

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
2. Inspect each call and its execution context before deleting it. In this example OnInitialized and the synchronous EventCallback method NextTicket are framework-managed. Remove their extra calls.
3. Retain InvokeAsync and StateHasChanged in ReceiveExternalUpdate. The timer is outside the framework event/lifecycle flow. Dispatcher access and requesting a render serve different purposes.
4. Keep timer ownership and disposal. Do not infer that OnAfterRender, async void, every intermediate await, custom IHandleEvent implementations or arbitrary notifications automatically rerender. BL0012 is a bounded analyzer, not a proof about all rendering flows.
5. Copy each target's files into its own directory, preserving Before/ and wwwroot/. Run the PowerShell commands below from that directory, using the absolute matching dotnet executable. Build with locked restore after the first restore generates packages.lock.json. Each command starts a local server and occupies the terminal until Ctrl+C. The /audit route is an unauthenticated local test probe only; omit it in a production app.

In the copied .NET 11 directory, use the matching absolute SDK executable as $dotnet. Set `DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0` in the child environment. Run `& $dotnet restore`, then `& $dotnet build -c Release -p:RestoreLockedMode=true`, then `& $dotnet ./bin/Release/net11.0/WillowQuay.dll --urls http://127.0.0.1:5987`.

Visit http://127.0.0.1:5987. Click Next ticket: 40 becomes 41 with the redundant calls removed. Click Receive external update: timer output becomes 1 without another click. The OmitTimerRender control proves the timer ran via /audit but output stays 0 until a subsequent framework event. No exact render-count or performance saving is claimed.

To reproduce the original diagnostics, stop the host and rebuild with `-t:Rebuild -p:BeforeMigration=true -p:TreatWarningsAsErrors=false`. This intentional negative build selects all three Before files and reports BL0012, BL0014 and BL0015 on RC2; it does not repair anything automatically. Run its output to observe the old failures. Restore the default with `-t:Rebuild` and no control properties. For the necessary-render negative control, use `-t:Rebuild -p:OmitTimerRender=true` instead; /audit confirms the timer fired while the UI waits for a later click. These controls are test-only. On .NET 10 use its matching executable, net10.0 output path and SDK pin; the failures and fixes still execute, but its analyzer has none of these three warnings.

## Constraints

- Click Next ticket: 40 becomes 41 with the redundant calls removed. Click Receive external update: timer output becomes 1 without another click. The OmitTimerRender control proves the timer ran via /audit but output stays 0 until a subsequent framework event. No exact render-count or performance saving is claimed.
- The fixes are valid on .NET 10; only the diagnostic is new in the measured .NET 11 analyzer. No SDK-only older-target analyzer route, package backport or additional .NET 10 foundation is advertised.
- Interactive Server with prerender disabled is the executed route. No WASM, static SSR callback, Hybrid, AOT/trimming, arbitrary concurrency, performance or GA execution is established.
- Matching SDK/compiler/analyzer, reference/runtime and implicit asset package are measured separately in saved evidence. No manually referenced compiler DLL or native asset is required. Historical RC2 compilation/execution is not GA support verification.
- No authentication, database/provider, production resource or device is involved. Input validation is demonstrated; a real application must independently decide and enforce authorization.

## Evidence

Both actual dependency sets compile with fresh sample evidence. The authoring check builds before, after and missing-timer-render variants on both matching runtimes and uses installed Chrome through Playwright. It checks framework-event updates and the necessary external-timer render; the shared harness also exercises the loop and interop controls. RC2 analyzer and Razor compiler hashes are recorded alongside runtime identities. The saved records retain their capture commit and dirty state; freshness binds their exact input hashes, rather than requiring the capture commit to equal the current Git HEAD. The ordinary package is separately installed from a local-only feed/cache; real offline stdio list/get must preserve ID, title, goal, edition, full canonical files, applicability and current evidence. Browser and package checks prove different claims.

## References

- [Pinned BL0012 analyzer implementation](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Analyzers/src/StateHasChangedAnalyzer.cs)
- [Pinned analyzer diagnostic descriptors](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Analyzers/src/DiagnosticDescriptors.cs)
- [Component event and rendering guidance](https://learn.microsoft.com/aspnet/core/blazor/components/rendering?view=aspnetcore-11.0)
- [Browser-to-.NET interop and object references](https://learn.microsoft.com/aspnet/core/blazor/javascript-interoperability/call-dotnet-from-javascript?view=aspnetcore-11.0)
