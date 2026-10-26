# Keep a beacon watch running while its browser application lives

## Goal

Refresh a fictional Westmere beacon display periodically without tying the loop to one component. Register an IHostedService and let the .NET 11 WebAssembly host start it. Give orderly shutdown an explicit owner, cancel the loop, and await its completion before disposing the application.

WASM browser lifetime; not durable work after tab/process termination. A timer is cooperative browser work, not a dedicated worker thread or an operating-system background service. Hidden tabs may throttle timers, and suspended or terminated pages cannot keep this work running.

## Older approach

.NET 10 can use the same registered IHostedService contract, but its WebAssemblyHost does not start or stop those registrations. The original comparison explicitly owns that coordination. This remains a useful supported approach; a component-owned cancellation loop can also be suitable when the work belongs only to that component.

Create an empty BeaconWatch directory. Save these files as BeaconWatch.csproj and Directory.Packages.props. The complete project declares the settings needed outside this repository; no parent imports are required. Add global.json with sdk.version 10.0.402 and sdk.rollForward disable. Its canonical language is C# 14, without a newer compiler targeting an older runtime.

{{snippet:project-10}}

{{snippet:packages-10}}

Program.cs registers the service, creates the host and an explicit lifetime owner, then starts the registered services before rendering. The ?unowned=1 query is a negative verification control: registration alone on .NET 10 produces no started event or ticks, even though the counter renders and works. Omit that query for normal use.

{{snippet:program-10}}

OwnedBrowserLifetime.cs stops the services before disposing the renderer and service provider. The external document button remains available when the rendered root is disposed. This is deliberate in-app shutdown, not an unload handler.

{{snippet:lifetime-10}}

## Newer approach

Create a separate BeaconWatch directory for .NET 11. Use these complete project/package files and a global.json with sdk.version 11.0.100-rc.2.26504.105, sdk.rollForward disable and sdk.allowPrerelease true. The canonical mode is C# 15; this example needs no C# 15-only syntax. Exact RC2 packages and matching framework/browser assets are required for the recorded proof. Hosting.Abstractions is already available through the .NET 11 reference set; adding its redundant explicit reference raises NU1510 with warnings treated as errors.

{{snippet:project-11}}

{{snippet:packages-11}}

Program.cs uses AddHostedService. There is no hand-written StartAsync call, Task.Run startup task, service worker or server-side substitute. The host starts the service after culture loading and before rendering the root components. Keep StartAsync short: it launches a tracked asynchronous loop which yields at its first timer wait.

{{snippet:program-11}}

OwnedBrowserLifetime.cs invokes the real public WebAssemblyHost.DisposeAsync. The pinned .NET 11 implementation awaits registered StopAsync methods before disposing the renderer, scope and provider. Repeated calls share one shutdown task. This ends the sample's owned application resources, but does not make the never-ending RunAsync task complete or unload the .NET runtime.

{{snippet:lifetime-11}}

## Shared complete application files

Save the following identical files in both applications. There is no _Imports.razor requirement: Dashboard declares its imports and namespace, and the C# files declare theirs. No persistence provider, credentials, external endpoint or account is needed. The in-memory event journal intentionally resets on reload and is not business storage.

BeaconTicker.cs implements the registered service. PeriodicTimer serializes iterations: a slow asynchronous refresh cannot overlap another iteration. Pass the stopping token into real cancellable I/O as well. StartAsync's token covers startup; the service owns a separate cancellation source for its running loop. StopAsync cancels that source and awaits the actual task. The 250 ms interval is a visible demonstration setting, not a timing guarantee. Unexpected loop faults surface when StopAsync awaits the task; production work also needs an explicit error/retry/reporting policy.

{{snippet:ticker}}

Dashboard.razor supplies a working interactive counter and records real root disposal. The synchronous WebAssembly JS call is only a tiny disposal observation; it must not become blocking work.

{{snippet:dashboard}}

Create wwwroot/index.html. The lifetime controls are outside #app so they survive renderer disposal; load watch-lifetime.js before the matching blazor.webassembly.js.

{{snippet:document}}

Save wwwroot/watch-lifetime.js. Its event journal and external controls make the service lifecycle observable without relying on a component still being alive after shutdown. The awaited .NET call records failure visibly. It does not install beforeunload/pagehide cleanup or pretend those events can await arbitrary asynchronous operations.

{{snippet:browser}}

The document includes the complete demonstration CSS.

From each directory, restore and run with its matching absolute dotnet executable. SDK 10.0.402 uses Microsoft.NETCore.App 10.0.13; the .NET 11 SDK uses runtime 11.0.0-rc.2.26504.105. Set DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0 before invoking either SDK. Run dotnet restore, then dotnet run --configuration Release --no-restore --no-launch-profile --urls http://localhost:5811 for the older app or http://localhost:5812 for the newer app. DevServer is a local development host, not a deployment recommendation. The RC2 DevServer emits a deprecation warning recommending Components.Gateway for future migration; this deliberately pinned comparison still runs it and does not broaden into that separate hosting migration. Initial restore downloads the matching WebAssembly SDK/runtime packs; interpreted WASM needs no installed wasm-tools workload, native compiler or AOT. Repository packages.lock.json files retain the exact resolved dependency closure; a fresh standalone directory generates its own lock on restore.

## Migration

1. Move application-wide recurring work into a registered IHostedService; retain component-owned loops when their shorter lifetime is intended.
2. On .NET 11 remove the manual service StartAsync/StopAsync enumeration, use the host registration, and preserve an explicit owner for orderly live-document disposal.
3. Supply all document/JS/import/package files above and run the browser checks before removing the old coordination. Keep durable scheduling and persistence on a backend.

## Why this helps

Registration places startup and stop beside the host's lifecycle instead of a particular screen. The explicit stop path lets an application finish cooperative cleanup while the document is alive. Awaiting the loop proves it has stopped; a disabled button or an unchanged DOM alone does not.

## Constraints

- This edition selects .NET 11 only. The compiled .NET 10 manual-owner sample is a previous-release comparison, not a compatible route to native WASM hosted-service integration. No new .NET 10 foundation is added.
- The route is browser-side WebAssembly/CSR. Static SSR cannot run it, and Interactive Server has a different server lifetime. Hosted or Interactive Auto apps need separate client startup ownership and must avoid duplicate server/client work; those compositions are not executed here.
- IHostedService and these host APIs have no Experimental diagnostic in the reviewed RC2 artifacts. The package baseline remains prerelease RC2 evidence, not GA verification.
- RunAsync() has no public cancellation-token overload. Its task normally never completes. Calling the public DisposeAsync while the document is alive is the demonstrated resource-shutdown path; it does not imply a general imperative runtime-exit API.
- Browser close, refresh, navigation, crashes and process termination do not guarantee StopAsync or asynchronous disposal. Do not promise final uploads or durable work after the tab/process ends. Move durable scheduling and authoritative persistence to an appropriate backend with its own delivery guarantees.
- Background-tab throttling, sleep and suspension change timing. Keep each iteration short and asynchronous so the UI remains usable. IHostedService does not create a worker thread or make CPU-heavy work nonblocking.
- The pinned host passes CancellationToken.None to its stop executor and catches stop exceptions during disposal. Bound real shutdown work in your own service and report failures; this sample's already cancellable timer has no external I/O. A successful DisposeAsync return alone is not proof every service succeeded.

## Evidence

Both complete canonical projects are built with their actual SDKs and locked dependencies. Those captures are compilation only. Separate real Chrome WebAssembly execution checks compare .NET 10 registration-only inactivity and manual startup with .NET 11 automatic startup, advancing ticks and a working counter, then stop-requested, cancelled, stopped and root-disposed events, awaited disposal and no further ticks in the still-loaded document. Exact inputs and executed browser artifact hashes are recorded in docs/verification/browser-service-lifetime.md. No desktop .NET host mocks or cross-compilation substitute for browser execution.

The existing installed-tool harness retrieves this entire body, every canonical snippet, complete nested applicability, edition and current compilation evidence through the actual locally packed tool with an isolated feed/cache, unrelated working directory and no network. Consumer browser policies, mobile suspension, AOT, hosted/Auto composition and GA remain outside the executed evidence. No Human-only proof or resource provisioning is required for this local route.
