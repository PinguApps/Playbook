# Refresh user-specific content when authentication state changes

## Goal

A fictional archive opens a sealed atlas only for a curator. The older view reads authentication state when it starts and when its explicit Refresh action runs. That is a valid user-driven design, but an external identity update leaves its cached reading list stale until that action. The current view subscribes to state changes and reloads immediately.

The provider is a circuit-scoped local demonstration. Its grant/revoke buttons exercise real `NotifyAuthenticationStateChanged` notifications and a real Interactive Server renderer. They are **not authentication endpoints** or production permission controls. Production changes must come from a trusted authentication integration. Refreshing SignalR credentials and automatic framework propagation are context; they do not make a custom provider refresh itself.

## Migration steps

1. Keep the authentication provider scoped to the circuit and obtain an initial state. Do not keep a captured `HttpContext.User` as the circuit's permanent source.
2. Subscribe to `AuthenticationStateChanged` before loading initial content. RC2 BL0013 warns when a component reads state without subscribing; analyzer silence alone does not prove UI updates.
3. Await the supplied state task and reload user-specific content through `InvokeAsync`, which enters the renderer dispatcher. Request rendering after the external notification. The sample's in-memory lookup completes synchronously; production asynchronous loads need cancellation/latest-result and error handling appropriate to that data source.
4. Unsubscribe when the component is disposed. Do not force a full page reload to refresh a local view. The older explicit Refresh action remains an alternative for a deliberately manual UX, including on .NET 10.
5. Enforce authorization again when serving protected data and executing operations. Hiding an atlas title does not revoke a download, authorize a request or terminate work already running.

## Run and observe

Run the selected complete Web project with its matching SDK, `dotnet run -c Release -- --urls http://127.0.0.1:5199`, and open that local address. On .NET 10, Grant changes the notification count but the view stays public until Refresh. On .NET 11, Grant immediately displays `Curator: sealed atlas`; Revoke restores `Public: tide tables`. The displayed circuit identifier and browser navigation remain unchanged. The code's local provider is intentionally bounded to these sequential transitions; it is not a concurrent data-loader implementation.

## Constraints

Interactive Server with prerender disabled is the executed mode. The notification/subscription technique also exists on .NET 10; the new edition explains the .NET 11 analyzer and refreshed-state context, not a newly invented subscription API. SDK 10.0.402/framework 10.0.13 and SDK 11.0.100-rc.2.26504.105/framework RC2 are measured pins. Implicit Internal.Assets packages are locked. No remote identity provider, cookie/JWT refresh, automatic SignalR-to-provider propagation, cross-tab revocation, protected-data authorization, concurrent loader, WASM/Auto/Hybrid, AOT or GA behaviour is proved. No experimental opt-in is required by the inspected notification API.

## Older approach

The complete original older consumer preserves its explicit reconnect or refresh action.

### .NET 10: `AccessDesk.razor`

{{snippet:n10-accessdesk-razor}}

### .NET 10: `App.razor`

{{snippet:n10-app-razor}}

### .NET 10: `ArchiveAccess.csproj`

{{snippet:n10-archiveaccess-csproj}}

### .NET 10: `ArchiveIdentity.cs`

{{snippet:n10-archiveidentity-cs}}

### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

## Newer approach

The complete current consumer uses the new refresh or notification contract.

### .NET 11: `AccessDesk.razor`

{{snippet:n11-accessdesk-razor}}

### .NET 11: `App.razor`

{{snippet:n11-app-razor}}

### .NET 11: `ArchiveAccess.csproj`

{{snippet:n11-archiveaccess-csproj}}

### .NET 11: `ArchiveIdentity.cs`

{{snippet:n11-archiveidentity-cs}}

### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

## Evidence

The saved evidence names the exact commands, inputs, SDKs, resolved packages and actual execution. `eng/Record-LiveIdentity.ps1` captures the real .NET/TypeScript and browser checks. Installed stdio inspection verifies delivery separately; it does not prove production identity-provider support.
