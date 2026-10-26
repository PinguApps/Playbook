# Stream replies while retaining conversation history (Experimental)

## Goal

A harbour survey desk shows each question and the agent's reply as it arrives. Keep the first exchange visible while a second reply is still being written. This is **experimental throughout .NET 11**, including after GA: selecting this recipe is an explicit adoption decision.

## Older approach

The original .NET 10 QuayChat consumer uses the real Microsoft.Extensions.AI.Abstractions 10.6.0 IChatClient contract. It owns a list of messages, snapshots the input before adding the in-progress assistant message, appends fragments and requests rendering. It disables overlapping sends and cancels on disposal. This remains a valid way to build a deliberately small text interface; Components.AI is not available as a net10.0 asset in the checked package.

Save all listed files in a separate QuayChat directory. No template, hidden imports, remote model or account is needed. The source is intentionally scripted: one fragment arrives immediately, then Release reply allows the second fragment. The UI and stream consumption are real; no language model is exercised.

#### .NET 10: `global.json`

{{snippet:n10-global-json}}

#### .NET 10: `Directory.Build.props`

{{snippet:n10-directory-build-props}}

#### .NET 10: `Directory.Build.targets`

{{snippet:n10-directory-build-targets}}

#### .NET 10: `Directory.Packages.props`

{{snippet:n10-directory-packages-props}}

#### .NET 10: `QuayChat.csproj`

{{snippet:n10-quaychat-csproj}}

#### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

#### .NET 10: `App.razor`

{{snippet:n10-app-razor}}

#### .NET 10: `SurveyChatClient.cs`

{{snippet:n10-surveychatclient-cs}}

#### .NET 10: `SurveyPanel.razor`

{{snippet:n10-surveypanel-razor}}

## Newer approach

The original .NET 11 consumer delegates conversation state and subscriptions to UIAgent and AgentBoundary. MessageList owns the rendered turns; MessageInput sends through the cascaded conversation. Its registered BlockRenderer<RichContentBlock> displays the accumulated RawText, role, identifier and lifecycle. A constant MessageId across fragments lets the actual package update the same block. Earlier turns remain in the boundary while a new turn streams.

The exact experimental package is 0.1.0-preview.1.26466.103, built from dotnet/dotnet b2f96bd7ac75c07fb8067c05c54767d3d5e12e8d. It declares only a net11.0 asset. The SDK prunes its framework-provided Components.Web dependency; do not add an unnecessary explicit framework package reference. Both consumers resolve AI.Abstractions 10.6.0's net10.0 asset; that shared IChatClient contract is not an older-target Components.AI route.

#### .NET 11: `global.json`

{{snippet:n11-global-json}}

#### .NET 11: `Directory.Build.props`

{{snippet:n11-directory-build-props}}

#### .NET 11: `Directory.Build.targets`

{{snippet:n11-directory-build-targets}}

#### .NET 11: `Directory.Packages.props`

{{snippet:n11-directory-packages-props}}

#### .NET 11: `QuayChat.csproj`

{{snippet:n11-quaychat-csproj}}

#### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

#### .NET 11: `App.razor`

{{snippet:n11-app-razor}}

#### .NET 11: `SurveyChatClient.cs`

{{snippet:n11-surveychatclient-cs}}

#### .NET 11: `SurveyPanel.razor`

{{snippet:n11-surveypanel-razor}}

## Migration

1. Preserve the application's real IChatClient registration and provider-specific authentication, cancellation and retention policies. This demonstration injects SurveyChatClient directly so the scripted dependency is unmistakable.
2. On net11.0, replace the manual message collection/update loop with one UIAgent per conversation and an AgentBoundary. Place MessageList and MessageInput inside it. Place typed BlockRenderer registrations inside MessageList.
3. Include ai-chat.css through the static asset map and the matching Blazor script. Keep the UIAgent alive across sends and dispose it with its component. Recreating it for every message loses history.
4. Restore in each sample directory with its pinned SDK, then run: dotnet run --project QuayChat.csproj -c Release --no-restore --urls http://127.0.0.1:6011. The older app can use port 6010. Initial restore needs NuGet access; the installed Playbook body itself is offline.
5. Send First survey. Before pressing Release reply, observe Survey 1: and Streaming. Release, send Second survey, and observe the first completed exchange alongside Survey 2:. After release, the second response reports Input messages=3. Those are the two user messages and the completed first reply.

## Constraints

RawText is rendered as ordinary Razor text, including the deliberate script-like payload. Do not cast provider output to MarkupString or insert it as raw HTML. This sample does not render Markdown/rich nodes, execute tools, persist conversations, restore across circuits or connect to AG-UI. Content-block mapping beyond conversational text needs separate actual package/provider evidence.

The in-view history belongs to one Interactive Server circuit with prerendering disabled. No browser/renderer matrix, durable storage, provider networking, model quality, retries or shared application state is claimed. The package and all associated UI contracts remain experimental throughout .NET 11; RC2 proof requires GA reconciliation.

## Evidence

Both canonical consumers compile under their actual .NET 10/11 SDKs with zero warnings/errors. The real Windows Chrome renderer was observed at first-held, first-complete, second-held and second-complete states. The new assistant block changes Active to Inactive without changing survey-1's identity; the second stream retains the first turn and receives three input messages. Encoded text creates no script descendant.

Compilation evidence is separate from the real browser receipts in docs/verification/streaming-and-form-protection.md. The existing packed-tool harness independently expands these canonical snippets and compares the entire installed Markdown, discovery goal, experimental stability, exact edition and nested applicability while the server runs from an unrelated directory without network access. No mock renderer, live model or GA test is represented as execution.
