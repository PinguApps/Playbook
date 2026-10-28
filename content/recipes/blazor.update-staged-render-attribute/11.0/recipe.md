# Replace a staged renderer attribute deliberately

## Goal

Replace a known pending attribute after constructing an owned render tree when building an advanced renderer integration. Use the experimental indexed operation before submission; ordinary components should render their final values directly.

## Older approach

The actual .NET 10 builder has no SetAttributeValue. The original rebuild path clears and rebuilds its own pending figure after the curator chooses a caption. It never changes submitted frames. Rebuilding is a supported alternative for this bounded tree and remains valid.

### .NET 10: `GalleryCard.cs`

{{snippet:n10-gallerycard-cs}}

### .NET 10: `GalleryRenderer.cs`

{{snippet:n10-galleryrenderer-cs}}

### .NET 10: `global.json`

{{snippet:n10-global-json}}

### .NET 10: `MossGallery.csproj`

{{snippet:n10-mossgallery-csproj}}

### .NET 10: `Program.cs`

{{snippet:n10-program-cs}}

## Newer approach

The experimental .NET 11 operation updates frame 1 after Stage finishes. The assertion checks frame count, sequence, name and value before submission. GalleryRenderer executes real first/update batches; a separate HtmlRenderer verifies encoded text and null output. The harness also executes the documented failures.

### .NET 11: `GalleryCard.cs`

{{snippet:n11-gallerycard-cs}}

### .NET 11: `GalleryRenderer.cs`

{{snippet:n11-galleryrenderer-cs}}

### .NET 11: `global.json`

{{snippet:n11-global-json}}

### .NET 11: `MossGallery.csproj`

{{snippet:n11-mossgallery-csproj}}

### .NET 11: `Program.cs`

{{snippet:n11-program-cs}}

## Migration

1. Keep a copy of the valid rebuild path; select the newer call only for an intentional framework integration on ASP.NET Core 11.
2. Create the complete MossGallery project below, preserving its runtime/SDK/language pins when reproducing this evidence. From its directory run `dotnet run --configuration Release` with the matching .NET installation.
3. Build the fixed tree and record the intended attribute index before duplicate processing or structural edits can invalidate it. Replace its value before returning from BuildRenderTree.
4. Localize ASP0032 and BL0006 acknowledgements to this boundary. Keep ordinary application rendering free of this integration.
5. Run both consumers. Expect Fern portrait, encoded Lichen detail, a figure without data-caption for null, and Rehung. Run `eng/Test-StagedAttribute.ps1` with the actual .NET executables to verify unavailable older API and unsuppressed diagnostics.

## Constraints

Moss Gallery stages a figure caption, then selects its final value before BuildRenderTree returns. The .NET 10 path rebuilds this tiny owned tree; this remains valid on .NET 11.

The argument is a zero-based frame index, not a source sequence number. Index 1 is established by this fixed tree; production adapters must track their own attribute frame and reject other layouts. Duplicate-attribute processing can move frames.

Only replace a staged attribute of the same intended meaning. This sample has a plain string data attribute; changing event callbacks/handler IDs or parameter contracts is outside scope.

Setting null leaves an attribute frame with a null value. It does not replay AddAttribute omission rules or compact the tree. The real HTML renderer omits this null-valued attribute; that output does not imply the frame disappeared.

Wrong frame type throws InvalidOperationException. Negative or count-or-higher indices throw ArgumentOutOfRangeException. Do not edit GetFrames().Array, retained renderer frames or an already submitted RenderBatch.

Advanced in-process custom Renderer and HtmlRenderer output only; no browser DOM adapter, live batch mutation or stable rendering abstraction. Local #pragma warning disable/restore ASP0032 around SetAttributeValue, with BL0006 confined to intentional frame/Renderer boundaries. Never suppress these project-wide.

## Evidence

Both real consumers compile and execute through Record-Evidence.ps1. Four custom-renderer batches and four actual HTML outputs cover replacement, encoding, null and subsequent replacement. The RC2 consumer executes wrong-frame and both index-boundary failures; Test-StagedAttribute.ps1 compiles independent negative consumers for CS1061, ASP0032 and BL0006. No DOM, event-handler mutation, performance or cross-version renderer compatibility is claimed.

The exact test sets are .NET 10/C# 14, SDK 10.0.402, framework 10.0.13, and .NET 11/C# 15, SDK 11.0.100-rc.2.26504.105, framework 11.0.0-rc.2.26504.105. These independently pinned consumers are separate from the supported prerequisites above. No native assets or workloads are required. RC execution does not establish GA verification. The .NET 10 comparison adds no catalogue foundation or edition. PIN-959 and PIN-964 remain open.

Official source support:

- [Previous builder contract](https://github.com/dotnet/aspnetcore/blob/0ef4bbfa3291b306a21e5001cb0491277bdd35bc/src/Components/Components/src/Rendering/RenderTreeBuilder.cs)
- [Experimental staged attribute replacement](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Components/src/Rendering/RenderTreeBuilder.cs)
- [Framework renderer contract](https://github.com/dotnet/aspnetcore/blob/4a70870818bf5662f21b0c28f9a8c267dccb77f0/src/Components/Components/src/RenderTree/Renderer.cs)
