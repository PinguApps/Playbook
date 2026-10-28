# Custom renderer and WASM navigation observations

PIN-1150 / PIN-1151, measured 28 October 2026 on Windows x64 in the T3 integrated
Chromium 152.0.7977.167 browser. These focused real-browser observations establish
claims that compilation and mocked interop cannot. Canonical sample records retain
the exact build commands, dependency closure and input hashes separately.

## Actual artifacts

| Target | SDK / language | ASP.NET packages / browser runtime |
| --- | --- | --- |
| net10.0 | 10.0.402 / C# 14 | 10.0.13 |
| net11.0 | 11.0.100-rc.2.26504.105 / C# 15 | 11.0.0-rc.2.26504.105 |

Both targets use Microsoft.NET.Sdk.BlazorWebAssembly with matching explicitly
pinned WebAssembly and DevServer packages. Default interpreted build/serve needs
no optional workload, native SDK, external provider or production resource.
`Inspect-BlazorRemovedApis.ps1 -CompareWith` passed against the original
[exact artifact/signature inventory](../planning/audits/blazor-removed-apis.assets.json).
No API availability was inferred from a documentation table alone.

Three isolated .NET 11 consumer probes also failed as expected: reintroducing the
old init assignment produced CS0200, reintroducing the two-argument notification
produced CS7036, and removing the renderer's local diagnostic suppression produced
BL0006. These used real current builds in disposable directories, preserving the
working samples. All four supported canonical builds passed with no warnings/errors.

## Renderer construction and event dispatch

`eng/Planetarium.browser.js` was evaluated at the fresh root of both real WASM
hosts. The JS adapter forwards the actual framework attachment to Blazor's
registry, retains its real DotNetObjectReference and invokes its DispatchEventAsync
callback. Actual Surface component render frames supply each button's event ID
and text. There is no mocked IJSRuntime or substituted callback implementation.

| Observation | .NET 10 | .NET 11 RC2 |
| --- | --- | --- |
| Supported control identities, attached before StartAsync | 201, 202 | 201, 202 |
| First click, on Dome | Dome 1, Orrery 0; dispatch 201 | Dome 1, Orrery 0; dispatch 201 |
| Second click, on Orrery | Dome 1, Orrery 1; dispatch 202 | Dome 1, Orrery 1; dispatch 202 |
| Historical init request 900 | Actual hook ID 203 | Setter absent; probe omitted |

The allocator runs during base construction and uses only static state. It does
not depend on the derived constructor's later IJSRuntime initialization or
Dispatcher property. The four-argument base constructor has no identity parameter.
BL0006 remains a real compatibility warning; suppressions cover only deliberate
adapter classes. The historical CS0618 suppression covers only the inert assignment.

Initial browser execution failed with missing JSON converter metadata. Forwarding
through the actual runtime's public IJSRuntime interface fixed attachment without
mocking it. A second run exposed the default Dispatcher's missing synchronization
context on the browser callback. The bounded BrowserThreadDispatcher now accepts
only its owning thread, and both actual event sequences passed after that change.
This fixture deliberately has no worker/threading support. Compilation alone
would have missed both failures.

## Committed navigation observations

`eng/MineralArchive.browser.js` was evaluated at each fresh MineralArchive root.
It checks actual browser URI and `history.state.userState`, the managed current
state, routed heading and the complete ordered LocationChanged journal after each
action. The old direct-notification fixture compiles only; it is not loaded or
called by the supported app. There is no JSInteropMethods call in the supported
current path.

Both actual targets produced this sequence:

| Action / observer | Path | History entry state | IsNavigationIntercepted |
| --- | --- | --- | --- |
| Open specimen / 1 | /specimens/fluorite | return-selection=drawer-7 | False |
| Intercepted galena link / 2 | /specimens/galena | null | True |
| Back / 3 | /specimens/fluorite | return-selection=drawer-7 | True |
| Back / 4 | / | null | True |
| Forward / 5 | /specimens/fluorite | return-selection=drawer-7 | True |
| Forward / 6 | /specimens/galena | null | True |

Initial setup added no LocationChanged entry. Popstate notifications are browser
initiated and report intercepted=True in these baselines; that flag does not mean
an anchor was necessarily clicked. The fresh galena entry has no inherited
application state, while Back/Forward restores fluorite's own state. The obsolete
two-argument notification cannot convey state and does not perform navigation;
the retained three-argument protocol is framework-only.

Built consumer DLL SHA-256 values used in the final browser checks:

| Consumer | SHA-256 |
| --- | --- |
| net10.0/Planetarium | c259ee36a5755aa3946535457af951d2be327ecacbe6b14811b6a222f7d58f37 |
| net11.0/Planetarium | 9a43718d9bdddd82ed3c93f60976239f8c9b9f5749552bae5fd5c0427b29f602 |
| net10.0/MineralArchive | 763efa1e9c7bcb3cea2f9b26217a5fa08031a97e2fd01c789ff122b46b232d29 |
| net11.0/MineralArchive | bba1d03a0e510897ec0dcfe4ec77cea91b430a939e34063f2c6fbdee7296534b |

## Reproduce and inspect the delivered guidance

Requires PowerShell 7, both pinned SDKs in the selected .NET installation, and a
real browser. These commands select this worktree explicitly from any starting
directory. `-KeepRunning` returns with four owned local servers; their process IDs
are recorded under ignored `artifacts/renderer-navigation/hosts.json`.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-0e75f3cb'
& ./eng/Test-RendererNavigationSamples.ps1 `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' -KeepRunning
```

Open Planetarium on ports 6230 and 6231 and repeat the two clicks above. Open
MineralArchive on ports 6240 and 6241 and repeat the six navigation actions. Reload
the root before independent repetitions. The browser scripts supply the same
explicit assertions when evaluated in a real browser on those origins. HTTP boot
page checks alone are not browser behavior proof. To clean up, stop only the
owned process trees recorded in hosts.json, using `taskkill.exe /PID <pid> /T /F`.

The ordinary-package check requires Node.js 22.19+ and npm. Setup may download
pinned Inspector 2.11.0 and restore uncached dependencies; served guidance is
embedded and does not fetch content. Both list/get use explicit target 11.0.

```powershell
Set-Location -LiteralPath 'V:\_worktrees\Playbook\t3-0e75f3cb'
& ./eng/Inspect-BlazorMigrationGuidance.ps1 -BuildAndPack `
  -PackageVersion '0.0.0-renderer-navigation' `
  -RecipeId 'blazor.allocate-web-renderer-identity','blazor.preserve-wasm-navigation-state' `
  -OutputDirectory './artifacts/renderer-navigation/inspection' `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe' `
  -Node 'C:\Program Files\nodejs\node.exe'
& ./eng/Test-RendererNavigationConsumer.ps1 `
  -Dotnet 'C:\Users\pingu\AppData\Local\dotnetup\dotnet\dotnet.exe'
```

Expected: both ID/title/goal discovery entries, complete expanded Markdown,
edition 11.0, target-11 applicability (including BL0006 for the renderer), current
compilation evidence and equal JSON-text/structured responses. The inspector
installs the exact ordinary artifact from an isolated local-only feed/cache and
calls real stdio outside the checkout. The consumer helper reconstructs all four
complete projects solely from returned Markdown, then builds their actual SDK and
dependency sets. An unusable proxy proves proxy-independent retrieval, not network
isolation; a separate network-disabled container check establishes that boundary.

The same package, SHA-256
`fe3fee5856d2fdda163d5d8c5b8c82e5dff554e4d7c427d1aa16bbb61fc44f21`,
passed real list/get in the existing digest-pinned .NET 10 SDK container
`mcr.microsoft.com/dotnet/sdk@sha256:e70cdb7f80b0348f5cb85f19a8f670fca061f033d57eed12fa003d58b0e06317`
with `docker run --rm --network none`. Only its local feed, expected returned
responses and disposable stdio driver were mounted; no checkout or network was
available. Discovery and both complete recipe objects matched Windows exactly,
including text/structured equality, and the server shut down cleanly. The existing
installed HostSmoke scenario also passed (1/1). No recurring matrix was added.

## Retained limits

RC2 is not GA proof. Other browsers/OS, workers, AOT/published linking, deployment,
reconnection, reusable renderer teardown/ID recycling, arbitrary component trees,
server/Hybrid navigation and external providers are not established. The small
renderer bridge intentionally uses internal framework contracts and is not a
general rendering framework. The legacy notification is compile-only comparison.
Existing recipe identities, the eight .NET 10 foundations, WEB-089's remaining
families, PIN-959 and actual-GA PIN-964 remain unchanged/open.
