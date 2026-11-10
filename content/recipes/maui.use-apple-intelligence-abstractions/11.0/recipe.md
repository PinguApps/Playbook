# Summarise local notes with experimental Apple Intelligence

## Goal

Heron Notes offers an optional on-device summary of fictional harbour maintenance notes. The caller displays the result as a draft for a person to check, without executing instructions from generated text.

## Older approach

The .NET 10 comparison uses the already-published experimental Essentials.AI 10.0.80 preview with its actual iOS 26.0 asset. It already exposes AppleIntelligenceChatClient through IChatClient. The same application-owned summary boundary remains useful; upgrading the runtime does not itself introduce the abstraction or guarantee a better answer. Neither sample uses a remote model as a substitute. The .NET 10 sample is comparison material, not another foundation or a supported older-target recipe route.

## Newer approach

{{snippet:samples-net11-0-heronnotes-notes-cs}}

The current sample uses Essentials.AI 11.0.0-preview.6.26360.8 and its actual net11.0-ios26.5 asset. The local pragma acknowledges MAUIAI0001 and restores the warning afterward. The assembly remains experimental even though the API is shipped. The method owns/disposes its client; an app using a shared injected IChatClient should instead let that service's owner dispose it. Propagate cancellation from page/workflow lifetime and prevent overlapping submissions in the surrounding UI.

An OS version alone does not establish Apple Intelligence availability. Require eligible hardware, Apple Intelligence enabled, a downloaded model and a supported language/region. This pinned client's public surface does not expose the newer GetModelStatusAsync helper: do not copy that call from newer package documentation into this sample. The host must offer an unavailable/error state when native generation fails and retain manual editing. Do not silently upload notes to a cloud model. Never log prompt/response content by default.

Embeddings are a separate capability. NLEmbeddingGenerator wraps Apple's Natural Language framework, not Foundation Models/Apple Intelligence; model/language availability and bridge deployment requirements differ. This summary sample does not claim embedding behavior, image input, tools or coverage of every Apple binding. Only the inspected text-chat surface is demonstrated.

## Migration

1. Use an existing iOS native host and copy the appropriate release's canonical files. Preserve the isolated package/build boundary and exact SDK/package pins. Restore the matching native workload and assets on a suitable Apple authoring host; this Windows checkout has no such route.
2. For the current sample select the package's actual net11.0-ios26.5 asset, not a neutral assembly. Supply a compatible Apple SDK/Xcode pairing and retain the deployment floor 26.0. The target binding level is distinct from that floor; upgrading to the repository's RC2 Apple 27 baseline requires its reviewed Xcode 27/macOS 26.6 pairing and fresh evidence.
3. Retain the narrow MAUIAI0001 suppression. Call Notes.SummariseAsync only after the user opts in, pass the page's cancellation token, and display the returned text for review. The caller must catch cancellation and generation failures and expose retry/manual editing.
4. Verify actual model availability and output on eligible hardware before claiming runtime behavior. Neither the ordinary Playbook package smoke nor a portable abstraction build proves the native bridge or model.

## Constraints

Experimental package contracts can change independently of the .NET runtime. Exact .NET 10 and .NET 11 package assets are different; do not assume latest documentation describes either pin. Apple native build and hardware/model execution are not available on this Windows machine or its Docker engine. No model download, account, external service, cloud fallback or new hardware is provisioned. No Android/Windows model implementation is inferred from package naming or newer releases.

## Evidence

The .NET 11 SDK baseline is now 11.0.100 GA with preview selection disabled. The .NET 10 comparisons retain their declared SDKs. Earlier RC audit details below are historical; use the current sample pins, dependency locks and GA verification records for current coverage. Existing native exclusions and pending Human gates still apply.

Both real native build attempts stop at NETSDK1147 (ios workload unavailable). Immutable package-provenance source and actual native assembly documentation support the constructor and IChatClient method contract. These are official-review records, not compilation or successful generation. Both exact sample paths are not-tested for native compilation/execution under PIN-699. PIN-937 is canceled and has not executed; it creates no future hardware obligation. No GA or Apple model execution claim is made.

## Canonical inputs

### samples/net10.0/HeronNotes/HeronNotes.csproj

{{snippet:samples-net10-0-heronnotes-heronnotes-csproj}}

### samples/net10.0/HeronNotes/Notes.cs

{{snippet:samples-net10-0-heronnotes-notes-cs}}

### samples/net11.0/HeronNotes/HeronNotes.csproj

{{snippet:samples-net11-0-heronnotes-heronnotes-csproj}}

### samples/net11.0/HeronNotes/Notes.cs

{{snippet:samples-net11-0-heronnotes-notes-cs}}

### samples/net10.0/HeronNotes/global.json

{{snippet:samples-net10-0-heronnotes-global-json}}

### samples/net10.0/HeronNotes/Directory.Build.props

{{snippet:samples-net10-0-heronnotes-directory-build-props}}

### samples/net10.0/HeronNotes/Directory.Build.targets

{{snippet:samples-net10-0-heronnotes-directory-build-targets}}

### samples/net10.0/HeronNotes/Directory.Packages.props

{{snippet:samples-net10-0-heronnotes-directory-packages-props}}

### samples/net11.0/HeronNotes/global.json

{{snippet:samples-net11-0-heronnotes-global-json}}

### samples/net11.0/HeronNotes/Directory.Build.props

{{snippet:samples-net11-0-heronnotes-directory-build-props}}

### samples/net11.0/HeronNotes/Directory.Build.targets

{{snippet:samples-net11-0-heronnotes-directory-build-targets}}

### samples/net11.0/HeronNotes/Directory.Packages.props

{{snippet:samples-net11-0-heronnotes-directory-packages-props}}
