# Choose how a live server session connects and expires

## Goal

Set the allowed connection methods, sign-in expiry behaviour and buffering thresholds for an Interactive Server circuit. The fictional Westmere lock desk accepts WebSockets, closes an expired cookie-authenticated connection and relays a cargo note larger than its pipe thresholds. These are application policy choices, not universal safe values.

`ConfigureConnection` belongs to the server endpoint. Browser reconnect options control retry behaviour after a lost connection; they cannot enable a transport that the endpoint forbids, renew an expired sign-in or change server buffers. This recipe does not implement sign-in refresh, pause or maintenance drain.

## Older approach — .NET 10 endpoint convention

ASP.NET Core 10 has `HttpConnectionDispatcherOptions`, but no `ServerComponentsEndpointOptions.ConfigureConnection`. For a Blazor Web App using `AddInteractiveServerRenderMode`, the original comparison applies a convention to the generated endpoints and modifies their existing dispatcher-options metadata. This working approach couples configuration to endpoint metadata; retain the negotiation and real circuit checks when upgrading it. It is not an older-target route for the new callback.

For a traditional Blazor Server host, `MapBlazorHub` already has a dispatcher-options overload. Do not add a second hub mapping beside the Web App's generated hub: duplicate routes can make endpoint selection ambiguous. That separate hosting shape is not the executed comparison here.

Create an empty `LockhouseSessions` directory. Save the following as `LockhouseSessions.csproj`; it contains all project properties needed outside this repository. Pin `global.json` to `{"sdk":{"version":"10.0.402","rollForward":"disable"}}`. The Web SDK implicitly supplies Microsoft.AspNetCore.App and resolves Microsoft.AspNetCore.App.Internal.Assets **10.0.13**; the recorded restore lock file pins this SDK-resolved asset. No explicit NuGet package, provider or workload is required.

{{snippet:project-10}}

Save this complete `Program.cs`. The unauthenticated `/sign-in` fixture issues a 25-second fictional ticket. It runs only in Development and rejects non-loopback peers; it is not a production authentication implementation. Keys are ephemeral, and no account or database is created. `/policy` exposes actual mapped options for the local check. The default expiry choice is true; `--CloseOnExpiry false` exists solely as a negative control.

{{snippet:host-10}}

## Newer approach — .NET 11 connection callback

Create the same named project in a separate directory, using this project file and `global.json` set to `{"sdk":{"version":"11.0.100-rc.2.26504.105","rollForward":"disable"}}`. The recorded browser assets are **11.0.0-rc.2.26504.105**. Match the Web SDK, reference pack, ASP.NET Core runtime and `blazor.web.js`; do not copy the newer script into an older renderer. C# 15 is the canonical language mode, although the configuration needs no new language syntax. This is pinned RC2 evidence; GA and other browser/platform behaviour are not verified.

{{snippet:project-11}}

Replace the older host with the following complete `Program.cs`. `ConfigureConnection` receives the actual dispatcher options before the endpoint serves connections, so the new code no longer searches endpoint metadata. Other application setup is deliberately equivalent.

{{snippet:host-11}}

## Complete shared UI and browser setup

For **both** projects, save `_Imports.razor` exactly as follows. There is no external component library or CSS requirement.

Use one directive per line: `@namespace Playbook.Samples`, `@using Microsoft.AspNetCore.Components`, `@using Microsoft.AspNetCore.Components.Routing`, `@using Microsoft.AspNetCore.Components.Web` and `@using static Microsoft.AspNetCore.Components.Web.RenderMode`.

Save `App.razor`. The script paths are the SDK's mapped static assets in both recorded samples. Run from the project directory so static-asset discovery has the correct content root.

{{snippet:document}}

Save `Session.razor`. Disabled prerendering makes the displayed identity and receipts belong to a real circuit. The cargo note is transient; it is not persistent business data, and it is lost on a full page refresh. No persistence store or serialization setup is needed for this example.

{{snippet:session}}

Create `wwwroot/session.js`. The observer shows connection state and suppresses automatic retry only to make the fixture's expiry observable. Its nested `circuit` configuration is a browser policy, separate from the server's `ConfigureConnection` callback. A production app needs its own reconnect/sign-in experience; this observer is not a retry strategy.

{{snippet:browser}}

## Migration

1. Decide which transports the deployed path supports. This example deliberately selects WebSockets only, so a proxy or browser that cannot upgrade cannot fall back. Keep other transports when the application requires them; restricting transports is not inherently a security improvement.
2. Set `CloseOnAuthenticationExpiration` from the application's authentication contract. It uses the authenticated ticket's expiry; it does not validate revocation continuously or renew credentials. Closure is observed by the connection manager's periodic checks, not an exact millisecond deadline. Require authorization on the endpoints so an expired client cannot create a fresh authenticated circuit.
3. Choose finite pipe thresholds from measured message sizes, slow readers and concurrency. `ApplicationMaxBufferSize` bounds incoming buffered application data before backpressure; `TransportMaxBufferSize` applies to outgoing buffered transport data. They are not maximum message sizes or total per-circuit memory quotas. Zero disables the corresponding threshold. Separately configure SignalR `MaximumReceiveMessageSize` if required; here it is 64 KiB so the 27,648-byte ASCII input can be relayed. Account for protocol overhead and other circuit state when measuring memory.
4. Replace the .NET 10 metadata convention with the .NET 11 callback. Do not retain two independent writers for the same options. Keep the rest of the host and its matching assets together, then rerun negotiation, expiry and real interactive payload checks.
5. Restore/build with the pinned SDK in each project directory: `dotnet restore --use-lock-file`, then `dotnet build -c Release --no-restore`. Run `dotnet run -c Release --no-build --no-launch-profile -- --urls http://localhost:5791 --environment Development` for the newer app, or port **5790** for the older app. Use each project's own SDK executable. Restore needs the SDK-resolved asset available locally or online; the delivered instructions and source are fully offline-readable, not a promise of an empty NuGet cache building without dependencies.

## Evidence

Open `/sign-in` on the selected loopback port. Immediately relay a note: the receipt must advance and the returned text must match. In browser network tools, negotiation must advertise only WebSockets, the circuit must upgrade with HTTP **101**, and a Long Polling attempt must return **404**, `LongPolling transport not supported by this end point type`. `/policy` reports the configured 8,192/16,384-byte thresholds on the mapped dispatcher metadata.

The original real Chrome check relays `Westmere-` repeated 3,072 times: **27,648 characters** arrive through a component change event and return in an actual render. Success above both thresholds demonstrates that they are not message-size caps. It does **not** measure peak memory, throughput, slow-reader backpressure or capacity under load. Those need workload-specific tests.

With the default policy, expect `Disconnected` after the ticket expires, and **401** for a new `/_blazor/negotiate` request. Restart with `--CloseOnExpiry false`, sign in again and repeat: the existing circuit continues to relay after expiry, while a new connection still receives **401**. This negative control separates ticket expiry from closing an already established transport. A retained page or displayed identity alone does not prove continued authorization.

## Constraints

Both .NET 10 and RC2 hosts were compiled with their actual pinned Web SDKs and executed with matching ASP.NET Core runtimes on Windows/Chrome. Saved compilation is distinct from the real browser execution records; there are no mocks or cross-compilation claims. Static SSR has no live circuit, and WebAssembly uses a different execution model. Interactive Auto, other authentication schemes/providers, reverse proxies, distributed hosting, load behaviour and GA are outside the demonstrated scope.

## Sources

- [Official ConfigureConnection API and release scope](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.components.server.servercomponentsendpointoptions.configureconnection?view=aspnetcore-11.0).
- [SignalR configuration: transports, expiry and separate buffers/message size](https://learn.microsoft.com/en-us/aspnet/core/signalr/configuration?view=aspnetcore-11.0).
- [Pinned RC2 endpoint options](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Server/src/Builder/ServerComponentsEndpointOptions.cs) and [previous-release options](https://github.com/dotnet/aspnetcore/blob/0ef4bbfa3291b306a21e5001cb0491277bdd35bc/src/Components/Server/src/Builder/ServerComponentsEndpointOptions.cs).
- [Pinned dispatcher pipe options](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/SignalR/common/Http.Connections/src/HttpConnectionDispatcherOptions.cs) and [connection-manager expiry scan](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/SignalR/common/Http.Connections/src/Internal/HttpConnectionManager.cs).
