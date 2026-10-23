# Reveal a virtual list item

## Goal

Use this when someone chooses a result that is outside a long list's current
window. A lantern workshop has 200 collection tickets. Opening a link should
show ticket 70; choosing ticket 150 should bring that actual ticket into view
even when the provider takes time to supply it. Index means position in the
current ordered result, not the ticket's persistent identity.

## Constraints

The newer implementation uses **net11.0**, the .NET 11 Web SDK and ASP.NET Core
11's shared-framework `Microsoft.AspNetCore.Components.Web` asset. There is no
separate virtualization package, QuickGrid dependency, workload, database,
external service or account. Matching SDK browser assets and `blazor.web.js`
must be served. The actual baselines are SDK **11.0.100-rc.2.26504.105**, framework
and Internal.Assets **11.0.0-rc.2.26504.105**, C# **15.0**. These are reproducible
RC2 measurements, not GA execution. The APIs have no experimental annotation or
required opt-in in these artifacts. C# 15-specific syntax is not required.

**Interactive rendered Virtualize; index bounds and provider timing** are the
important boundaries. Static SSR and prerender alone cannot perform the jump.
The samples use Interactive Server with prerender disabled so one circuit owns
each demonstration. WebAssembly/Auto, other OS hosts, screen readers and GA
execution have not been tested. The viewport is 240px high; all real rows and
placeholders are 40px tall, with overscan explicitly set to 3 on both releases.
This comparison does not establish a variable-height or list-mutation policy.

`InitialItemIndex` defaults to zero and is applied once when the count becomes
known. Changing the parameter later does not request another reveal. Both new
APIs clamp out-of-range indices; this application separately rejects invalid
programmatic input. An empty list has no item to reveal. Near the end, the
browser's maximum scroll extent can prevent top alignment; the target should
still be visible. Do not promise a precise pixel offset at every boundary.

Call `ScrollToItemAsync` on an interactively initialized component reference.
Calling it before initialization throws. A UI event runs on the renderer's
synchronization context; background notifications must enter it with
`InvokeAsync`. Await the task, but remember that a later call supersedes an
earlier one and user scrolling can interrupt positioning. Completion is not a
durable selection or permission check. This sample serializes button requests.
It enables the button only after interactive rendering and the first provider
result, so a click cannot race an unknown initial item count.

The provider must return the requested ordered slice and accurate total count,
honor cancellation, and keep the index-to-ticket mapping consistent. Resolve
an identity into an index in that same ordering before calling the API. A
requested provider index or a nonzero `scrollTop` proves neither that data
arrived nor that the intended row is visible.

## Older approach — .NET 10 fixed-height scrolling

.NET 10's Virtualize has no `InitialItemIndex` or `ScrollToItemAsync`. A valid
approach for uniform rows is application-owned JavaScript: wait until the
provider establishes the full scroll extent, set the offset, then wait for the
requested row and check its geometry. The bounded wait exposes a failed fetch
instead of reporting success from the scroll offset alone. This is useful
existing code when its fixed-height assumptions hold.

The older consumer uses **net10.0**, SDK **10.0.402**, ASP.NET Core/Internal.Assets
**10.0.13** and C# **14.0**. It uses the same host registration, imports, layout
and delayed provider shown below. Its module is `wwwroot/reveal.js`. It must run
after interactive rendering, and its 60-second deadline is a sample policy,
not a universal network timeout. Disposing its JS module releases the reference;
a disconnected server circuit is an expected disposal condition.

{{snippet:older}}

{{snippet:scroll}}

## Newer approach — let Virtualize position its own window

Declare the initial index and keep a reference for later requests. Virtualize
coordinates its provider window, committed render and browser alignment, so the
application can remove its fixed-height offset calculation and polling module.
Keep truthful sizing and provider semantics; the method does not fetch an
arbitrary database identity or fix an incorrect total count.

{{snippet:newer}}

The original provider deliberately waits 120ms and exposes hold/release controls
for deterministic local tests. Its journal distinguishes requests, completion
and cancellation. Those controls and retained journals are demonstration-only;
they are not an authenticated production data service. Both comparison apps use
the same provider implementation.

{{snippet:provider}}

Host setup and the document are complete below. Use `Microsoft.NET.Sdk.Web`,
`TargetFramework=net11.0`, `LangVersion=15.0`, nullable and implicit usings, and
`RootNamespace=Playbook.Samples`. No explicit NuGet package reference is needed.
In `_Imports.razor`, import `Microsoft.AspNetCore.Components`, its `Routing`,
`Web` and `Web.Virtualization` namespaces, plus `Microsoft.JSInterop`, and set
`@namespace Playbook.Samples`. The older project uses net10.0/C# 14 instead.

{{snippet:host}}

{{snippet:document}}

## Migration

1. Check the application's target, SDK, rendering mode and browser assets. Use
   the .NET 11 APIs only in the .NET 11 consumer; the .NET 10 comparison is not
   a newer-package/older-target compatibility route.
2. Establish the chosen item's zero-based position in a consistent result order.
   Return that ordering and its accurate total from the provider. Decide whether
   to reject invalid input or accept framework clamping.
3. Supply `InitialItemIndex` for first positioning. For later requests, retain
   `@ref`, wait for interactive initialization, then await `ScrollToItemAsync`
   from a UI handler or the renderer context. Do not change the initial parameter
   as a substitute for a later call.
4. Keep a sized scroll container and suitable row/placeholder layout. Remove
   the old offset/polling module only after the new path passes the same actual
   rendered-ticket assertions. Keep keyboard access to the jump control.
5. Start each app locally and open `/?initial=70&run=manual`. Expect ticket 70
   visibly in the viewport. Reveal 150, then 0 and 199. Negative or 200 input
   must display the bounds message. For a provider timing check, POST to
   `/control/manual/hold`, reveal a distant ticket, confirm it is absent and the
   status is still Revealing, then POST `/control/manual/release` and verify the
   correct row text and viewport geometry. Journals are at `/journal/manual`.

## Evidence

Both actual Web SDK projects compiled with zero warnings/errors at the exact
baselines above. Saved compilation evidence is separate from real Windows Chrome
155 Interactive Server execution. The canonical browser fixture held the real
provider before initial loading and a later distant reveal. It observed the
target absent and the operation incomplete while held, then checked the actual
row text and full viewport visibility after release. It also checked initial
clamping, both valid endpoints, invalid programmatic input without new provider
requests, and fewer than 200 rendered rows throughout. No mock, source-only
review or cross-compilation stands in for those checks.

The existing packed-tool harness retrieves the complete canonical Markdown,
every authored applicability route property, current evidence and edition 11.0
over real installed stdio with isolated feed/cache and unrelated cwd, offline.
Target 10.0 has no new edition. The repository's PIN-785 verification record
contains exact input/artifact hashes, commands and browser/provider journals;
the essential instructions and examples are embedded here.

Official references: [virtualization and provider/layout requirements](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/virtualization?view=aspnetcore-11.0),
[pinned RC2 implementation](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Web/src/Virtualization/Virtualize.cs),
and [the .NET 10 comparison surface](https://github.com/dotnet/aspnetcore/blob/0ef4bbfa3291b306a21e5001cb0491277bdd35bc/src/Components/Web/src/Virtualization/Virtualize.cs).
