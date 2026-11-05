# Monitor network changes with a supported fallback

## Goal

Invalidate the fictional Reed Station's cached network observations when an address changes. Keep a manual Refresh action usable when notifications are unavailable. An address event is a hint to recheck the application operation; it does not establish Internet access, DNS health or service reachability.

## Older approach

.NET 10 already has address-change events. For this bounded desktop/browser comparison, allow Windows, Linux and macOS before subscribing and choose manual refresh elsewhere. This conservative list remains valid; it is not an exhaustive statement of platform support. There is no NetworkChange.IsSupported property in the audited .NET 10 references.

{{snippet:older}}

Copy the complete StationNetwork class, its imports and Subscription helper into a project with implicit usings and nullable enabled. Supply a short, thread-safe invalidation callback and an application callback that enables the manual Refresh action. Keep the IDisposable until the owner stops.

## Newer approach

.NET 11 adds NetworkChange.IsSupported. Check it before subscribing on the supported desktop route. Actual RC2 browser execution exposed an important limitation: the unsupported assembly stub throws even from this getter. Check browser/WASI before accessing the property; do not promise a universal nonthrowing capability probe. The canonical example retains that explicit guard and the same manual-refresh fallback.

{{snippet:newer}}

For example, a station service can mark its cached observations stale in invalidate and expose a Refresh button in requestManualRefresh. The callbacks do not perform a network operation themselves. Dispose the returned subscription when that service stops; do not repeatedly subscribe on every render or refresh.

## Migration

1. Target net11.0 with a .NET 11 SDK/runtime. The sample declares C#15; SDK11.0.100-rc.3.26522.148/runtime11.0.0-rc.2.26504.105 identify tested artifacts, not final-GA proof.
2. Retain the older route for net10.0. Replace only the desktop allowlist with the capability property, keeping explicit browser/WASI exclusions before accessing it.
3. Supply short, thread-safe callbacks. Marshal UI updates to the application's dispatcher. Do not throw from an event callback or treat notification as successful service connectivity.
4. Retain and dispose the returned IDisposable with its owner. Unsubscription prevents future delivery registration, but does not drain a callback already queued or running. Coordinate owner teardown separately when needed.
5. On fallback, enable an explicit manual Refresh action that retries the actual application operation. Do not poll another unsupported NetworkInformation API. Propagate actual subscription failures instead of misreporting a working watcher.
6. Verify one real interface change while subscribed and a separate change after disposal, plus the actual unsupported runtime. Compilation alone does not establish notification delivery.

## Constraints

The framework console route needs no packages, provider, rendering integration, workload or experimental opt-in. The browser proof uses the exact locked Microsoft.AspNetCore.Components.WebAssembly host and interpreted Mono assets. It needs no wasm-tools workload, AOT or native relinking. macOS, WASI, mobile and other browsers are unmeasured; a conservative older exclusion does not claim those platforms are universally incapable.

IsSupported is not permission, adapter availability or reachability. Subscription may still fail for environmental reasons. Events may be coalesced and unrelated to a specific operation. The helper is an invalidation mechanism, not a connectivity authority or a callback-draining synchronization primitive.

## Evidence

eng/Record-SupportPaths.ps1 -Record captures paired pinned builds and real Windows console, Linux Docker and single-thread Mono Chromium checks. The isolated Linux test attaches an internal network after subscribing, observes a real address event, disposes and detaches that network, then verifies no callback for the second change. It changes no host adapter and removes its container/networks. Browser consumers select manual refresh; RC2's initially observed throwing getter required the explicit exclusion above.

Exact inputs, locked dependencies, commands and behavior records accompany the samples. The source manifest pins paired official contracts and implementations. Installed package inspection must verify offline discovery, Markdown, target11.0/edition11.0, applicability and canonical snippets. The eight .NET10 foundations are unchanged. PIN-959 and PIN-962 remain open; no publication, performance or GA verification is implied.
