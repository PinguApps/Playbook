# Set browser startup policy from C#

## Goal

Keep a harbour operations desk's retry policy beside its server configuration.
A dropped connection should display the chosen dialog, try a bounded number of
times and retain the current desk when the same circuit becomes reachable again.
Use endpoint defaults for the application and component options for a uniform
page policy. These are public browser settings, never a place for secrets.

## Older approach

.NET 10 supports this goal through startup JavaScript. It remains a valid choice,
particularly when the configuration includes functions. There is no .NET 10
BrowserOptions API to substitute.

Create sibling directories TideDesk and TideDesk.Client. In **both** directories,
create global.json with SDK version `10.0.402` and rollForward `disable`:
`{"sdk":{"version":"10.0.402","rollForward":"disable"}}`.
These version-specific examples pin C# 14 and the actual 10.0.13 packages.

### net10.0/TideDesk/TideDesk.csproj
{{snippet:older-project}}

### net10.0/TideDesk.Client/TideDesk.Client.csproj
{{snippet:older-client-project}}

Put the following Directory.Packages.props in **both** project directories.
The isolated boundary prevents inheritance of another application's central pins.

### net10.0/TideDesk/Directory.Packages.props
{{snippet:older-packages}}

### net10.0/TideDesk/Program.cs
{{snippet:older-host}}

### net10.0/TideDesk/App.razor
{{snippet:older-document}}

### net10.0/TideDesk/wwwroot/startup.js
{{snippet:startup}}

This starts Blazor once with three attempts, a 1000 ms scalar delay, the desk
dialog and the public client environment name. The script belongs after
blazor.web.js with autostart disabled. The shared Launch and Desk components and
client Program below are identical in both comparisons.

## Newer approach

.NET 11 moves these serializable settings into C#. The final RC1 names retained
in the tested RC2 are InteractiveServer, StaticServer and
InteractiveWebAssembly. WithBrowserOptions supplies endpoint defaults.
ConfigureBrowser replaces non-null scalar values during request rendering;
unspecified values retain the existing value. The desk's component changes
five attempts at 3000 ms to three at 1000 ms, while retaining the dialog ID and
two-minute inactivity value. PreserveDom=true emits disableDomPreservation=false.

Create the same two sibling directories. In **both**, use global.json
`{"sdk":{"version":"11.0.100-rc.2.26504.105","rollForward":"disable"}}`.
The project files declare the namespace, language, nullability, implicit usings,
package lock creation and browser asset composition. They do not require a
repository build file, optional workload, external service or database provider.
Informational versions omit the Git revision and these comparison projects
disable SDK Source Link/source-control queries so documentation-only commits do
not change their assemblies. Saved evidence records exact source and input hashes;
the production Playbook tool retains its own source-link configuration.

### net11.0/TideDesk/TideDesk.csproj
{{snippet:newer-project}}

### net11.0/TideDesk.Client/TideDesk.Client.csproj
{{snippet:newer-client-project}}

Put this Directory.Packages.props in **both** project directories.

### net11.0/TideDesk/Directory.Packages.props
{{snippet:newer-packages}}

### net11.0/TideDesk/Program.cs
{{snippet:newer-host}}

### net11.0/TideDesk/App.razor
{{snippet:newer-document}}

The read-only static lookup in App reads the current request's options at the
start of rendering. Child ConfigureBrowser processing occurs later; that
paragraph is not a promise that the final emitted configuration has already
been merged. Inspect the emitted configuration for the final values. Do not
replace this call with an invented HttpContext extension.

For **each** target, add these three unchanged files. No _Imports.razor,
generated template shell or omitted stylesheet is needed. The server App owns
the complete document, dialog CSS and boot script.

### shared/TideDesk/Launch.razor
{{snippet:launch}}

### shared/TideDesk.Client/Desk.razor
{{snippet:desk}}

### shared/TideDesk.Client/Program.cs
{{snippet:client-boot}}

## Migration

1. Assemble the target's two complete project directories above. Keep its SDK and
   package pins together. From TideDesk, run `dotnet restore TideDesk.csproj`,
   then `dotnet build TideDesk.csproj -c Release -p:RestoreLockedMode=true`.
   The restore creates both package lock files and downloads the matching
   framework/browser assets; subsequent locked builds preserve that dependency
   graph. Set DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=0 when using an isolated CLI home.
2. Run `dotnet run --project TideDesk.csproj -c Release --no-build -- --urls http://127.0.0.1:5388 --environment Development`.
   Development enables the build's static web assets; this is a local build/run
   example, not a deployment recipe. Open /server, /wasm and /auto. Click the
   harbour-check button and inspect the runtime label to distinguish Server
   execution from WebAssembly execution. Auto selects a runtime according to
   client availability; it does not migrate an already running instance.
3. On .NET 11, keep the endpoint policy and render ConfigureBrowser while an
   HttpContext exists. Remove this sample's manual startup.js and autostart=false;
   the automatic blazor.web.js startup reads the emitted configuration. Do not
   start Blazor twice. Keep any required function-based initialization in an
   appropriate JavaScript initializer instead of trying to serialize delegates.
4. Inspect the base64 JSON in the Blazor-Configuration DOM comment of an actual
   HTTP response. Expect server.reconnectionMaxRetries=3,
   reconnectionRetryIntervalMilliseconds=1000 and reconnectionDialogId=tide-reconnect;
   ssr.disableDomPreservation=false and circuitInactivityTimeoutMs=120000;
   webAssembly.environmentName=TideDesk. C# group names and wire names differ.
5. Interrupt the actual Server connection while keeping the process and circuit
   alive, then restore reachability before retries finish. Expect the custom
   dialog, recovery of the existing counter and a working next click. Keep the
   connection unreachable to observe exactly three attempts and the failed
   class. Timers are scheduling delays, not a real-time guarantee. Reconnecting
   cannot revive a circuit the server has discarded; this sample does not promise
   recovery after process restart. Reload after an exhausted test.
6. Use one uniform, public policy on the tested RC2. The separate-request probe
   described below establishes sharing for endpoint/component composition:
   endpoint metadata must not be treated as isolated per-request storage.
   If the consumer needs user/tenant-specific policy, do not copy this shared
   composition and assume isolation; require independent proof of a safe design.

## Constraints

Server options control Interactive Server reconnection, including an Auto
instance currently executing on the server. WebAssembly runs locally and has
no server circuit to reconnect. WebAssembly and Auto need the actual client
project and matching packages/assets shown above. Standalone WebAssembly
without server-rendered component endpoints cannot receive this DOM-comment
configuration. StaticServer options concern the Blazor Web App's SSR browser
behavior; plain static markup without the Blazor startup path is not an
interactive session.

Only serializable values travel. Custom reconnection handlers, callback retry
functions, configureSignalR, loadBootResource and configureRuntime stay in
JavaScript initialization. This recipe does not set connection transport,
authentication expiry, circuit pause, client hosted services or culture policy.

**Measured RC2 sharing:** in a separate deliberately request-varying diagnostic
consumer, a first HTTP request emitted seven endpoint retries, a second request
used ConfigureBrowser to set one, and a third independent request with no
override still emitted one. Static lookup returned the same endpoint-metadata
instance. This narrows the earlier unverified concern to an actual local
reproduction on the pinned artifacts. It is not evidence of framework-wide
security leakage or final GA behavior. The canonical sample applies the same
non-secret values on every request; it does not use mutable tenant/user data.
An absent HttpContext during interactive rendering is not a supported way to
update startup policy after the runtime has started.

The API is not marked experimental in the inspected RC2 reference surface.
RC2 is prerelease evidence, with final GA reconciliation still required.
Native AOT, publishing, other browsers/operating systems and external deployment
have not been established by these local checks.

## Evidence

The two actual hosts and two actual WebAssembly clients compile against their
declared SDKs, language modes, frameworks and locked package assets. Saved
records are **compile** evidence; real HTTP/browser execution is recorded
separately. No mocked SignalR connection, source-only substitute, cross-compile
claim or new OS/client matrix is used.

The real generated response verifies endpoint/component precedence, final API
names, static lookup and wire conversions. A loopback fault proxy closes the
real WebSocket and refuses real subsequent negotiations: the selected dialog
appears, temporary loss recovers the same counter, and sustained loss yields
three actual failed negotiations at the configured scalar cadence. The browser
also executes the WebAssembly client and Auto routes. Separate-request sharing
is established only for the explicitly described RC2 diagnostic composition.

Installed-tool checks use the actual package, a fresh local feed/cache and an
unrelated working directory. They retrieve this complete expanded Markdown,
target/edition, every authored nested applicability route field and current
compilation records offline. A disposable consumer assembled from the returned
body checks the setup independently of repository build inheritance.

Official attribution: RC1 release notes define the final names; pinned RC2
BrowserOptions, ConfigureBrowser, endpoint convention, Boot.Web and retry sources
explain lookup, merging and consumption. The actual RC2 artifact's VMR commit
820230c386db5b0106c5430b3e5aee92a7826bf5 maps through its source manifest to
ASP.NET Core 4a70870818bf5662f21b0c28f9a8c267dccb77f0.
The .NET 10 startup documentation supports the valid JavaScript comparison.
The source list supplies the complete official URLs independently of this prose.
