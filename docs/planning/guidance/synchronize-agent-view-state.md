# Synchronize agent and view state (experimental draft)

Historical planning draft, superseded by the
[served recipe and compiled canonical examples](../../../content/recipes/blazor.synchronize-agent-view-state/11.0/recipe.md).
The unexecuted checks below remain deferred; use the recipe's current Evidence section.

Use this when a person and an assistant edit the same data, and the interface
must distinguish a saved edit from a proposal. Intended identity:
`blazor.synchronize-agent-view-state/11.0` (PIN-807 / WEB-035).
Compilation and behaviour checks are deferred; this is not a runnable sample.

## Applicability

- Intended current target: C#, `net11.0`, Blazor Interactive Server, SDK
  `11.0.100-rc.2.26504.105` / runtime `11.0.0-rc.2.26504.105`.
  These are planned qualification inputs, not measured execution.
- Pin Components.AI `0.1.0-preview.1.26466.103`. Its nuspec selects `net11.0`,
  Components.Web at least `11.0.0-rc.1.26425.128` and AI.Abstractions at least
  `10.6.0`. Resolve and record the full closure before claiming compatibility.
- Same experimental package as conversation components; AG-UI is required for
  the remote state protocol. AG-UI/client/server/provider pins remain unqualified.
- No older-target package route, WASM/Hybrid, trimming, NativeAOT or GA guarantee
  is established. Experimental status applies even without a compiler diagnostic;
  use only diagnostic opt-ins actually required by the selected artifacts.

## Previous approach: application-owned state

A fictional tidal museum maintains an exhibit caption and revision. On .NET 10,
the component can own the saved caption, proposed caption and decision buttons.
A service adapter translates assistant messages into application edits. This
remains valid without the new Components.AI state API.

Keep the baseline revision beside each proposal. If a curator edits the caption
while a proposal arrives, flag a conflict instead of overwriting the edit.
The older canonical sample and its execution checks remain to be authored.

## Current approach: typed state and explicit decisions

The published package documents `UIAgent<TState>`, `AgentState<TState>` and
`UIAgentOptions.StateMapper`. Keep a typed museum model and an application
revision: typed state alone does not supply a distributed merge policy.

1. Define caption state with exhibit identity, caption and revision. Begin with
   committed data; make ordinary user edits replace that model.
2. Before each remote run, serialize current committed data into the AG-UI
   request. Scope its thread to the intended workspace. Validate incoming data;
   do not interpret chat text as trusted edits.
3. Map snapshots through `SetState`. Apply deltas only against the expected
   baseline, with an explicit policy for invalid paths and stale revisions.
4. Use `SetPredictiveState` for a proposed caption. Display its baseline and
   proposed text together. Connect the pending frontend action to the curator's
   decision instead of saving merely because a tool call arrived.
5. Before acceptance, recheck the application revision. Use
   `AcceptPredictiveState` or `RejectPredictiveState` for the decision. Define
   what happens to intervening user edits; rejection must not silently discard
   one. This interaction still requires testing against the selected package.
6. Subscribe to state changes, schedule rendering through the dispatcher and
   dispose the subscription with its owner. Keep persistence separate from
   display state and report remote failures visibly.

The package documents a pending-prediction flag and explicit accept/reject
methods. Cancellation, failure and unresolved completion require qualification;
this draft supplies no concurrency guarantee.

## Deferred checks

Compile original .NET 10/11 consumers. Through real AG-UI loopback, check user
edits in the next request, snapshots/deltas in the rendered editor, accepted and
rejected proposals, invalid updates and edits during a proposal. Exercise
cancellation/failure, subscription cleanup and workspace isolation. A scripted
endpoint must be identified as such; it proves no model provider behaviour.

Record exact packages, toolchains and inputs with fresh sample evidence. Then
inspect installed-tool discovery, full offline Markdown, canonical snippets,
applicability and experimental labels. None of those checks ran for this draft.

## References

- [Microsoft agentic UI overview](https://devblogs.microsoft.com/dotnet/build-agentic-ui-blazor/)
- [Pinned AI package](https://www.nuget.org/packages/Microsoft.AspNetCore.Components.AI/0.1.0-preview.1.26466.103)
- [AG-UI documentation](https://docs.ag-ui.com/)
- [Historical reconciliation](../audits/blazor.md)
