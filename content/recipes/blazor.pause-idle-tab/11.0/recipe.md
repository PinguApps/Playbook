# Release a hidden worksheet's live circuit

## Goal

Let a hidden estuary tide worksheet release its live server resources, then resume its readings when the reader returns.

This is Interactive Server guidance. .NET 11 adds opt-in automatic pause in Components.Server.AutoPause. .NET 10 already supports browser pause/resume and selected-state persistence; the comparison owns its coordination policy. Persisted drafts are not durable business records.

## Older approach — .NET 10 application coordination

.NET 10 has no AddAutoPause registration or Circuit.RequestCircuitPauseAsync. An application can still coordinate its supported Blazor.pauseCircuit/Blazor.resumeCircuit primitives from JavaScript. The original worksheet uses a small visibility timer and a server-to-browser bridge. This remains a valid application-owned policy; it does not reproduce all newer browser activity guards.

Create TideCharts.csproj below. Enable Nullable and ImplicitUsings in its PropertyGroup (the repository supplies both through samples/Directory.Build.props). Restore/build with SDK 10.0.402 for the recorded baseline. The Web SDK supplies its matching Internal.Assets dependency; packages.lock.json records 10.0.13.

{{snippet:project-10}}

All files use the Playbook.Samples namespace. In _Imports.razor add @using directives for Microsoft.AspNetCore.Components, Microsoft.AspNetCore.Components.Web, Microsoft.AspNetCore.Components.Routing, Microsoft.JSInterop and Playbook.Samples, plus @using static Microsoft.AspNetCore.Components.Web.RenderMode.

Program.cs registers the real Interactive Server app and host lifecycle service. JournalFile is optional local instrumentation. No shutdown endpoint is needed in a production app. Finish Program.cs with app.Run() after mapping the components.

{{snippet:host-10}}

App.razor supplies the complete document. Put tide.js in wwwroot and load it before the matching framework script. Prerendering is disabled so each displayed resource ID belongs to a live circuit.

{{snippet:document-10}}

Worksheet.razor persists the readings and sheet identity. Defaults are initialized after persistent state is supplied. The held-operation button is a fixture that creates a real pending JS interop call.

{{snippet:worksheet-10}}

The complete tide.js starts with fixture work and the old maintenance bridge. true acknowledges scheduling the custom browser action; it is not framework acceptance or completed persistence.

{{snippet:browser-10}}

Append the app-owned visibility policy. Its four-second delay is a demonstration choice. This simple worksheet has no media or streams; a production policy must handle its own critical work and promise failures.

{{snippet:idle-10}}

MaintenanceRegistry.cs closes circuit admission and tracks connection/closure separately:

{{snippet:registry-10}}

MaintenanceShutdown.cs runs in the real host lifecycle and bounds its closure wait. Its verification-only canceled request tests a request boundary, not cancellation of host shutdown.

{{snippet:shutdown-10}}

These complete helper files make the sample self-contained. PauseJournal.cs optionally writes local evidence; leave JournalFile unset outside verification. CircuitLease.cs represents a nonpersistent scoped resource.

{{snippet:journal-10}}

{{snippet:lease-10}}

## Newer approach — .NET 11 framework coordination

Reference Microsoft.AspNetCore.Components.Server.AutoPause at the RC2 pin below. Its static-web-asset initializer is required as well as the extension call. AddAutoPause opts in, Enabled defaults true there, and HiddenDelay defaults to two minutes. An enabled configuration requires a positive delay.

The inspected APIs have no experimental diagnostic. Their matching RC2 package/runtime assets are prerelease evidence; GA reconciliation remains outstanding.

{{snippet:project-11}}

Use the nullable/implicit-usings settings and _Imports.razor directives listed above. This complete combined host also enables automatic hidden-tab pause. The maintenance API itself comes from the shared Components.Server framework and does not need AutoPause. Its two-session maintenance test passes AutoPause=false; the idle test passes HiddenDelay=4000 while the normal default remains 120000 ms.

{{snippet:host-11}}

Finish this Program.cs with app.Run() after mapping. Current-release App.razor and Worksheet.razor:

{{snippet:document-11}}

{{snippet:worksheet-11}}

Current-release tide.js supplies fixture work and retains the old bridge for comparison. The newer registry never calls that bridge. Automatic visibility handling comes from the package initializer.

{{snippet:browser-11}}

The registry snapshots connected circuits under a lock and rejects new connections once maintenance starts. OnConnectionDown removes a transport; OnCircuitClosed completes that captured circuit's closure task.

{{snippet:registry-11}}

The lifecycle service awaits acceptance and actual closure separately. A false request or deferred/unresponsive client can exhaust the budget. An expired budget is recorded as incomplete drain and shutdown continues.

{{snippet:shutdown-11}}

Current-release helper files:

{{snippet:journal-11}}

{{snippet:lease-11}}

## Migration

1. Identify the draft values to restore. Use [PersistentState] for JSON-serializable properties, initialize defaults in OnInitialized, and retain stable component identity. For persisted scoped-service properties, also use RegisterPersistentService for Interactive Server. Do not persist scoped leases, component trees, file inputs, streams or native handles.
2. Compose the complete Web SDK host, imports, document and matching blazor.web.js above. For automatic pause, restore AutoPause and expose its initializer through static assets, then call AddAutoPause with a positive delay. Remove the manual visibility policy after verifying its replacement.
3. Plan persistence capacity, retention and key ownership. This example uses the default local in-memory provider: 1,000 circuits/two hours. Resume in the same loaded document without refreshing. Restart recovery additionally needs the browser's protected state, compatible app/component identities and the same Data Protection keys. Distributed/multi-instance storage and shared keys need separate evidence.
4. Register the CircuitHandler, registry and IHostedLifecycleService for maintenance. Start orderly stopping while the transport still serves requests. The verified local route awaits WebApplication.StopAsync. Signaling StopApplication first raced the connection in a real negative run; plan proactive draining before a supervisor or signal tears down transports.
5. Record request acceptance, then observe down/closed within a shutdown budget. Cancellation before acceptance can reject a request; cancellation afterwards is not rollback or completed shutdown. Keep a durable-save/fallback policy for rejected, deferred, expired and abruptly terminated sessions.
6. Test native hidden tabs, early return, critical-work deferral, scoped disposal and restored values. Test multiple maintenance sessions across a real process restart without reloading. Compare circuit/resource identities and saved values; stale DOM alone proves nothing.

## Constraints

Interactive Server; Components.Server.AutoPause; persistence/resumption prerequisites.

Only selected application state is saved. Components and scoped services are reconstructed, not hibernated. The sheet ID/readings survive while the scoped lease ID changes. File input values cannot be restored; persist suitable metadata separately. Use durable storage for committed business operations.

Native Page Visibility differs from a window losing focus. Returning before HiddenDelay cancels the timer. The package guards pending circuit work/JS interop and documented browser activity. Edited bound input, media, Web Locks, Picture-in-Picture, uploads/downloads and custom browser state need their documented guard/deferral policy. This focused proof executes pending JS interop; other guards are source-reviewed rather than individually executed.

Desktop timers can be throttled. Mobile backgrounding can suspend JavaScript before the delay expires, so graceful mobile auto-pause is not a supported guarantee. Persistence can fail, expire or be evicted. A full refresh loses the paused document's continuation. Static SSR, WebAssembly, Interactive Auto, cross-node failover and mobile lifecycle are outside the executed routes.

For server-directed pause, cancellation of request is not guaranteed completed shutdown. Acceptance is not completion. OnConnectionDown is a transport transition; OnCircuitClosed plus the fixture's disposed lease establish circuit release. Request cancellation does not guarantee that a client paused or a host stopped. The negative StopApplication run returned true but lost its connection before the expected persistence/closure sequence and expired its drain budget. Do not generalize the successful StopAsync route into a guarantee for termination signals or abrupt kills.

## Evidence

Both original TideCharts projects compile with zero warnings/errors under SDK 10.0.402/C# 14 and SDK 11.0.100-rc.2.26504.105/C# 15. Saved compilation records contain exact inputs, dependencies and commands. Compilation is separate from browser/server execution.

Actual Chrome extension tests switch native tabs with focus emulation disabled. No document.hidden/visibilityState override or synthetic visibility event is used. RC2 idle execution observes down/closed, old-resource disposal, automatic return into a new circuit/resource and the original readings/sheet. A brief return before the delay prevents pause. A real held JS call keeps the resource alive beyond the delay; release allows pause. Enabled=false preserves a hidden circuit.

Actual StoppingAsync execution records pre-canceled false requests, accepted true requests, subsequent down/closed/disposal, completed drain and host exit. Two RC2 sessions retain independent one/two-reading drafts across process restart into new circuit/resource identities. The .NET 10 manual timer/bridge also executes real pause/resume and restart recovery; it is not an older-target route for newer APIs.

[PIN-787 proof](https://github.com/PinguApps/Playbook/blob/main/docs/verification/PIN-787.md) records exact input/artifact hashes, native browser snapshots, lifecycle events, the negative ordering trace and packed offline MCP retrieval. See [official persistence guidance](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management/server?view=aspnetcore-11.0) and [pinned RC2 API source](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Server.AutoPause/src/AutoPauseBrowserOptionsExtensions.cs). No mock, cross-compilation or DOM-only observation replaces execution. GA, mobile, distributed storage and unrelated OS/client/native matrices are not verified.
