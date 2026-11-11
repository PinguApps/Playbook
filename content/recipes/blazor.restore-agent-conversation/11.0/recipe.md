# Restore completed conversation turns (Experimental)

## Goal

Recreate an observatory conversation view from completed local text turns, explicitly restore the experimental agent history and continue without replaying stored turns as new writes.

This is usable documentation-based guidance, authorized before full testing. Read the exact availability and evidence limits below; catalogue target 11 is limited to the recorded checks.

## Older approach

The .NET 10 observatory app stores completed question/answer pairs in ProtectedSessionStorage, loads them after the interactive view connects and renders the transcript directly. A fixed delayed reply stands in for application output; there is no model. Direct transcript ownership remains valid.

### .NET 10: `App.razor`

{{snippet:n10-app-razor}}

### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

### .NET 10: `LogPanel.razor`

{{snippet:n10-logpanel-razor}}

### .NET 10: `ObservatoryLog.csproj`

{{snippet:n10-observatorylog-csproj}}

### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

### .NET 10: `SavedTurn.cs`

{{snippet:n10-savedturn-cs}}

### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `packages.lock.json`

{{snippet:n10-packages-lock-json}}

## Newer approach

The current app supplies the real IConversationThread interface. LocalThread stages the user message and text updates, commits only on CompleteTurn, and exposes completed updates with stable per-message IDs. RestoreAsync returns Task<IReadOnlyList<ContentBlock>>: await it once after loading storage, before accepting new input. The view renders restored RichContentBlock text and then adds live blocks without duplicating object instances. This flow has been exercised through the actual Interactive Server renderer and protected session storage; see Evidence.

The durable format is deliberately plain completed text, not arbitrary polymorphic AIContent. Save occurs after the agent turn finishes; a save failure leaves the in-memory result visible and blocks another send until Retry save succeeds. Browser reload/view recreation loads the last saved snapshot. The ephemeral data-protection key makes host restart a distinct unsupported durability boundary.

### .NET 11: `App.razor`

{{snippet:n11-app-razor}}

### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

### .NET 11: `LocalThread.cs`

{{snippet:n11-localthread-cs}}

### .NET 11: `LogPanel.razor`

{{snippet:n11-logpanel-razor}}

### .NET 11: `ObservatoryLog.csproj`

{{snippet:n11-observatorylog-csproj}}

### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

### .NET 11: `SavedTurn.cs`

{{snippet:n11-savedturn-cs}}

### .NET 11: `VisitorClient.cs`

{{snippet:n11-visitorclient-cs}}

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `packages.lock.json`

{{snippet:n11-packages-lock-json}}

### .NET 11: Recreate the interactive view

{{snippet:n11-workspace-razor}}

## Migration

1. Save either complete ObservatoryLog file set in a separate directory, restore the lock file with its pinned SDK and build Release. Run the host only when ready for a local browser/storage check. No provider/account is required; VisitorClient is scripted.
2. In the current app, load the versioned storage key only after the Interactive Server connection exists. Treat a load error as a visible failure and leave the stored value intact.
3. Attach the loaded LocalThread to UIAgentOptions.Thread and await RestoreAsync exactly once for this view instance. Do not append restored blocks back into the store or call restoration for every render.
4. For a new question, collect updates in the real thread contract and save its completed snapshot after enumeration finishes. A canceled/failed turn stays staged, so it is absent from GetUpdates and the next browser snapshot.
5. On storage failure, retry the same completed snapshot rather than sending another question and producing duplicate turns. This sample serializes sends within one component; a multi-writer store needs its own concurrency/atomicity design.
6. Cancel pending work on disposal and recreate a new owner for a replacement view. Qualify actual renderer/storage behaviour before claiming durability or cross-view correctness.

## Constraints

Experimental Components.AI has only a net11.0 asset. The newer 0.1.0-preview.1.26466.103 exposes thread/restore APIs absent from the historical preview-7 audit; actual compilation confirms signatures, not runtime behaviour.

Local browser-tab session storage, text-only completed turns and one active component owner are the bounded design. Host-process restart, key persistence, quota/tampering recovery, arbitrary tool/media/state serialization and multiple concurrent writers are not implemented guarantees.

ThreadId identifies this application thread; IsStateful=false and ConversationId=null do not invent remote provider state. No remote model, provider conversation, AG-UI run/thread ID or resume behaviour is exercised.

All five available sample builds are compilation only; no browser storage or framework restoration was executed. The scripted client is explicitly not a model.

## Evidence

SDK11.0.100/ASP.NET11.0.0 and Components.AI 0.1.0-preview.1.26466.103: real Interactive Server rendering and ProtectedSessionStorage in Chromium. Completed-turn persistence, reload/recreate/restore/continue, ordering, replay duplicate prevention, canceled/disposed turn exclusion and storage failure/retry exercised. Scripted text client only; no remote conversation/run durability, host restart durability, WASM/Hybrid or model proof. PIN-967 remains open.

Run eng/Test-AgentViews.cjs with an absolute playwright-core module path and dotnet executable after building both hosts. Compilation receipts and rendered execution receipts have separate scopes. Historical RC evidence remains in Git at fc07b364cbf83d8a4b6285bbc39f23240adcdb0d. The .NET 10 comparison is compilation evidence only.

## Official references

- [Exact Components.AI package](https://www.nuget.org/packages/Microsoft.AspNetCore.Components.AI/0.1.0-preview.1.26466.103)
- [Protected browser storage](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management/protected-browser-storage)
