# Synchronize agent and view state (Experimental)

## Goal

Keep a curator’s caption and an assistant’s proposed caption coordinated when both can change the same museum record; use typed state and an explicit accept/reject boundary with the experimental AG-UI integration.

This is usable documentation-based guidance, authorized before full testing. Read the exact availability and evidence limits below; catalogue target 11 is limited to the recorded checks.

## Older approach

The .NET 10 tidal-museum editor owns its committed caption and proposal separately. Its proposal button is deliberately local and fixed: substitute your application-owned adapter for real assistant output. This remains a valid small editing model. It does not pretend a net10 Components.AI package exists.

### .NET 10: `App.razor`

{{snippet:n10-app-razor}}

### .NET 10: `CaptionPanel.razor`

{{snippet:n10-captionpanel-razor}}

### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### .NET 10: `MuseumCaptions.csproj`

{{snippet:n10-museumcaptions-csproj}}

### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `packages.lock.json`

{{snippet:n10-packages-lock-json}}

## Newer approach

The current editor uses the actual typed state and prediction APIs. CaptionAgentEndpoint must identify your compatible AG-UI HTTP endpoint (default http://127.0.0.1:6161/agent is a placeholder; no endpoint or model is started by this sample). RunAgentInput.State carries Pascal-case Caption/Revision on every request. The endpoint must return a full STATE_SNAPSHOT whose snapshot contains those same fields and Revision=current+1, or propose_caption with a JSON string caption. RegisterUIAction suspends that frontend action for the local decision buttons; accepted is supplied by the UI. This source-reviewed protocol integration has not been executed.

The view disables edits while a run/proposal is active, serializes local decisions on the dispatcher, and rejects stale or overlapping snapshots. It intentionally rejects STATE_DELTA with an actionable fresh-snapshot error: implementing arbitrary RFC6902 patches is outside this bounded editor. A different conflict policy requires application design; typed state supplies no merge or authorization guarantee.

### .NET 11: `App.razor`

{{snippet:n11-app-razor}}

### .NET 11: `CaptionPanel.razor`

{{snippet:n11-captionpanel-razor}}

### .NET 11: `CaptionSession.cs`

{{snippet:n11-captionsession-cs}}

### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### .NET 11: `MuseumCaptions.csproj`

{{snippet:n11-museumcaptions-csproj}}

### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `packages.lock.json`

{{snippet:n11-packages-lock-json}}

## Migration

1. Save the older or newer file set in an independent MuseumCaptions directory. Restore with its pinned SDK, then build Release; keep the lock file and all Directory files.
2. Start the older Web App to inspect its application-owned proposal flow. For the current app, provide --CaptionAgentEndpoint with your actual compatible AG-UI URL when running the app; never embed service credentials in examples. Neither endpoint nor provider support is assumed.
3. Configure that endpoint to read the current Caption/Revision state, emit full snapshots with the next revision or the declared propose_caption tool, and honor cancellation. Preserve AG-UI thread/run correlation in the protocol implementation; this sample does not prove it.
4. Keep pending state visibly separate from its saved baseline. Accept/reject explicitly; do not let an assistant tool result itself persist the caption. The sample blocks user edits until a run ends rather than silently merging concurrent edits.
5. Handle malformed/stale events as failures and request a fresh snapshot when a delta arrives. Cancellation/failure rejects unresolved predictive state; this does not roll back an already accepted or authoritative snapshot.
6. Dispose state subscriptions and cancel the run when removing the view. Add a separately designed persistence/authorization boundary before using caption changes in production.

## Constraints

Same experimental Components.AI package as the conversation components. AG-UI is required for the remote state protocol; ordinary IChatClient text is not shared-state proof.

The only compiled rendering mode here is Interactive Server with prerender disabled. Rendering, event flow, provider authentication, network failure/retry and concurrent-agent behaviour are untested. Do not infer WASM/Hybrid, trimming/AOT or GA support.

This sample accepts complete snapshots and rejects deltas. Its disabled-editor policy avoids concurrent local edits; unrelated clients require server-side revision/authorization enforcement. The pending baseline is displayed only, not durable saved data.

No runtime or performance claims. After building, running the current app still requires a separately configured AG-UI endpoint; the compilation record does not qualify that integration.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Pingu authorized documentation-based recipe delivery on 3 November 2026. Final GA, browser/provider/AG-UI, renderer/storage and Linux native TLS behaviour checks remain pending and are not claimed. These deferred checks do not block this ticket; stable release readiness remains gated.

Five available consumers compile on SDK10.0.403 or SDK11.0.100, with their actual locked dependencies and fresh input hashes. Current HarbourTls instead records the real CS0246 failure for missing DirectTlsEndpoint as official-review, never compilation success. The exact baselines relevant to this recipe are the two evidence records. No sample was executed.

The ordinary packed/installed tool is inspected separately over real stdio list/get, including full canonical Markdown and applicability. That verifies delivery, not the sample application. PIN-959 and PIN-964 retain broader coverage/readiness and actual-GA reconciliation.

Expected future checks: local edit in outgoing request, snapshot revision validation, explicit delta rejection, pending proposal acceptance/rejection, invalid/stale updates, cancellation/failure, dispatcher lifetime and cleanup through an actual AG-UI endpoint.

## Official references

- [Experimental Blazor AI documentation](https://devblogs.microsoft.com/dotnet/build-agentic-ui-blazor/)
- [Exact Components.AI package](https://www.nuget.org/packages/Microsoft.AspNetCore.Components.AI/0.1.0-preview.1.26466.103)
- [AG-UI client package](https://www.nuget.org/packages/AGUI.Client/0.0.6)
