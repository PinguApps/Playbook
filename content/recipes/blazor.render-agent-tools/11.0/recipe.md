# Show frontend tool work in a conversation (Experimental)

## Goal

A harbour desk selects a berth and decides whether crate H-17 may be dispatched. The conversation shows the requested frontend action, its result and an approval decision. **Components.AI is experimental throughout .NET 11**, including after GA. A button must deliver a correlated decision to the provider; the provider checks it before changing its ledger.

## Older approach

The .NET 10 consumer uses AI.Abstractions 10.6.0 and owns the message list, declaration-only tool options, result correlation and decision continuation. It invokes its local function only after Select berth and sends approval responses as user-role content. This is a valid small application design. The pinned Components.AI package has only net11.0 assets, so it is not presented as a compatible older-target route.

Save files under the exact samples/net10.0/HarbourDispatch and samples/net11.0/HarbourDispatch paths shown below, with an identical copy of the listed DispatchClient.cs in each directory. Empty Directory files isolate inherited package/build settings. No hidden template files or global imports are needed.

### Scripted provider: DispatchClient.cs in each consumer

{{snippet:shared-client}}

### .NET 10: samples/net10.0/HarbourDispatch/global.json

{{snippet:n10-global-json}}

### .NET 10: samples/net10.0/HarbourDispatch/Directory.Build.props

{{snippet:n10-directory-build-props}}

### .NET 10: samples/net10.0/HarbourDispatch/Directory.Build.targets

{{snippet:n10-directory-build-targets}}

### .NET 10: samples/net10.0/HarbourDispatch/Directory.Packages.props

{{snippet:n10-directory-packages-props}}

### .NET 10: samples/net10.0/HarbourDispatch/HarbourDispatch.csproj

{{snippet:n10-harbourdispatch-csproj}}

### .NET 10: samples/net10.0/HarbourDispatch/Program.cs

{{snippet:n10-program-cs}}

### .NET 10: samples/net10.0/HarbourDispatch/App.razor

{{snippet:n10-app-razor}}

### .NET 10: samples/net10.0/HarbourDispatch/DispatchPanel.razor

{{snippet:n10-dispatchpanel-razor}}

## Newer approach

UIAgent advertises registered functions as declarations and creates UIActionBlock objects. The custom renderer explicitly invokes the selected action. BerthActivityHandler maps two app-defined progress snapshots into one mutable activity block. FunctionApprovalBlock sends Approve/Reject responses through AgentContext; DispatchClient checks the request identity and Approved value before incrementing Dispatches. UI approval is a decision transport, not server authorization.


### .NET 11: samples/net11.0/HarbourDispatch/global.json

{{snippet:n11-global-json}}

### .NET 11: samples/net11.0/HarbourDispatch/Directory.Build.props

{{snippet:n11-directory-build-props}}

### .NET 11: samples/net11.0/HarbourDispatch/Directory.Build.targets

{{snippet:n11-directory-build-targets}}

### .NET 11: samples/net11.0/HarbourDispatch/Directory.Packages.props

{{snippet:n11-directory-packages-props}}

### .NET 11: samples/net11.0/HarbourDispatch/HarbourDispatch.csproj

{{snippet:n11-harbourdispatch-csproj}}

### .NET 11: samples/net11.0/HarbourDispatch/Program.cs

{{snippet:n11-program-cs}}

### .NET 11: samples/net11.0/HarbourDispatch/App.razor

{{snippet:n11-app-razor}}

### .NET 11: samples/net11.0/HarbourDispatch/DispatchPanel.razor

{{snippet:n11-dispatchpanel-razor}}

### .NET 11: samples/net11.0/HarbourDispatch/BerthActivity.cs

{{snippet:n11-berthactivity-cs}}

### .NET 11: samples/net11.0/HarbourDispatch/BehaviorChecks.cs

{{snippet:n11-behaviorchecks-cs}}

### .NET 11: samples/net11.0/HarbourDispatch/LoopbackChecks.cs

{{snippet:n11-loopbackchecks-cs}}

## Migration steps

1. Restore each pinned consumer with its matching SDK from its own sample directory. Keep the locked dependency graph. Components.AI requires .NET 11; do not suppress an incompatible-framework restore failure to use it on .NET 10.
2. Run `dotnet run -c Release -- --urls http://127.0.0.1:6070` in the older directory, and use port 6072 in the newer directory. The sample enables static web assets when run from build output and uses ephemeral data-protection keys. Production deployment and persistent state need their own design.
3. In the older UI choose Request berth; in the newer message input send berth. Select berth, then observe West-4 and the correlated frontend result. The new UI renders the app-defined activity and action state inside the conversation. The two immediate local activity snapshots may appear together; no timed progress animation is promised.
4. Request dispatch, deny it and observe the unchanged ledger. Request it again and approve it: only the approved continuation increments the ledger. Pending decision buttons become disabled after a decision. The scripted source is scoped to the circuit, not a production authorization service.
5. In the newer directory run `dotnet run -c Release -- --check`. This exercises real Components.AI and a real loopback HTTP/SSE AGUI.Client flow with frontend declarations/results and separate approval/denial interrupt/resume conversations. Its endpoint is deliberately scripted and local. Keep production authorization, identity, replay protection, persistence and error recovery on the actual agent server.

## Constraints

The browser UI uses a local scripted IChatClient; it does not claim to be an AG-UI browser session or a live model. Remote frontend declarations, backend events and approval interrupts require AG-UI packages and a compatible agent endpoint. The local transport check uses AGUI.Client 0.0.6 (net10.0 asset), its transitive Abstractions/Formatting 0.0.6 and AI 10.6.0 dependencies. It checks the real wire boundary with a scripted HTTP/SSE endpoint, not a simulated client/parser. It does not qualify another provider.

The pinned AG-UI client can append a synthetic rejected-tool result after a subsequent user message. The fixture's denial and approval cases deliberately use fresh conversations; later remote turns after rejection are not verified. The local UI checks do cover a denial followed by approval in one circuit. No SDK 11/net10 Components.AI route, persistent workflow, distributed authorization, concurrent requests, WebAssembly, broad browser/platform or GA behavior is established. Actual RC2 compilation and execution remain distinct from GA readiness.

## Evidence

The exact SDKs, inputs and resolved dependencies are captured by each sample's compilation record. Focused execution uses the actual package AgentContext and the actual AG-UI client over loopback Kestrel/SSE. The scripted provider checks declarations before returning a tool call, checks request identity before accepting a decision, and only mutates its ledger after approval. Real Chrome interaction separately checks waiting/completed tool UI and denial/approval continuation. Source review and compilation alone do not establish any browser or remote-provider claim.

## References

- [Official experimental Blazor AI overview](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-11?view=aspnetcore-11.0#experimental-blazor-ai-components-for-agentic-user-interfaces).
- [Exact Components.AI artifact](https://www.nuget.org/packages/Microsoft.AspNetCore.Components.AI/0.1.0-preview.1.26466.103).
- [Exact package source](https://github.com/dotnet/dotnet/tree/b2f96bd7ac75c07fb8067c05c54767d3d5e12e8d/src/aspnetcore/src/Components/AI/src).
- [Exact AG-UI source](https://github.com/ag-ui-protocol/ag-ui/tree/9f393021ecbd6bcc62dc4b1ad5a6ac4b797565f4/sdks/dotnet).
