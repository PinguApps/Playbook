# Restore completed conversation turns (experimental draft)

Historical planning draft, superseded by the
[served recipe and compiled canonical examples](../../../content/recipes/blazor.restore-agent-conversation/11.0/recipe.md).
The unexecuted checks below remain deferred; use the recipe's current Evidence section.

Use this when a view is recreated but its completed conversation should remain.
Intended identity: `blazor.restore-agent-conversation/11.0` (PIN-1170 / B16).
No storage or renderer path has run for this draft.

## Applicability

Intended consumer: C#, `net11.0`, Blazor Interactive Server, SDK
`11.0.100-rc.2.26504.105` / runtime `11.0.0-rc.2.26504.105`.
These are planned qualification inputs. Experimental Components.AI
`0.1.0-preview.1.26466.103` documents `IConversationThread`,
`UIAgentOptions.Thread` and `UIAgent.RestoreAsync`. Its dependency minima are
Components.Web `11.0.0-rc.1.26425.128` and AI.Abstractions `10.6.0` for `net11.0`;
the actual restored closure remains unqualified. The older audited
`11.0.0-preview.7.26425.128` lacks the thread/restore contracts.

Local completed-turn persistence is the bounded goal. Remote model/provider
operations are outside it. AG-UI thread/run identity claims require an actual
loopback. No older-target package route, other rendering mode, crash durability,
storage encryption or GA support is established.

## Previous approach: save an application transcript

A fictional observatory records each finished visitor question and answer as an
application-owned turn. Its .NET 10 view loads that transcript on creation and
renders it before accepting another question. In-progress text stays separate;
cancellation does not turn a partial answer into a finished one. This remains
valid when the application needs direct control of storage and rendering.

Keep the displayed transcript separate from provider request history. Showing
old text does not prove a remote agent retains its context. Both the old sample
and its persistence behaviour remain unverified.

## Current approach: persist behind the thread contract

The thread abstraction delegates persistence to an application implementation;
it does not automatically configure browser storage.

1. Choose local storage with a stable observatory conversation key and schema
   version. Define visitor ownership, retention and failure handling.
2. Implement the real pinned `IConversationThread` contract. `AppendUserMessage`
   starts a turn, `AppendUpdate` stages updates and `CompleteTurn` commits it.
   Keep incomplete data separate so `GetUpdates` returns committed updates in
   chronological order.
3. Configure the recreated agent's `UIAgentOptions.Thread` with the loaded store.
   Restore before accepting another question. On load failure, show the error;
   do not replace saved history with an empty conversation.
4. Call `RestoreAsync` explicitly. Do not store replayed turns as newly received
   turns. Give one lifecycle owner responsibility for restoring this view once.
5. Continue through that conversation owner. Prevent a disposed view from writing
   after its replacement starts; serialize writes or reject stale owners.

`ThreadId`, `IsStateful` and `ConversationId` are separate properties. Do not
equate a local key with a provider conversation ID or fabricate protocol run IDs.
Exact signatures, real `ChatResponseUpdate` serialization and atomic commit still
need implementation and testing. No fabricated interface is presented as code.

## Deferred checks

Compile both original consumers against their real dependencies. Exercise the
actual renderer and local storage: complete a turn, destroy/recreate the view,
restore, then continue. Check incomplete-turn exclusion, ordering, duplicate
replay prevention, separate keys and storage failure preserving completed turns.
Remote provider operations remain excluded.

Capture fresh compilation/execution evidence. Inspect the ordinary installed
tool's full offline body, canonical snippets, applicability and edition. These
checks and served metadata remain outstanding; PIN-959/PIN-964 stay open.

## References

- [Pinned AI package and XML contract](https://www.nuget.org/packages/Microsoft.AspNetCore.Components.AI/0.1.0-preview.1.26466.103)
- [Historical B16 audit](../audits/blazor.md)
- [Existing streaming recipe](../../../content/recipes/blazor.stream-agent-conversation/11.0)
