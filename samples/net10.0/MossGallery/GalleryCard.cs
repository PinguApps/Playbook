// snippet:complete
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace MossGallery;

public sealed class GalleryCard : ComponentBase
{
    [Parameter] public string? Caption { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        Stage(builder, "awaiting curator");
        ReplacePendingCaption(builder, Caption);
        // The renderer receives this tree only after BuildRenderTree returns.
    }

    private static void Stage(RenderTreeBuilder builder, string? caption)
    {
        builder.OpenElement(0, "figure");
        builder.AddAttribute(10, "data-caption", caption);
        builder.AddContent(20, "Moss study");
        builder.CloseElement();
    }

    // BL0006 is confined to inspecting this intentionally owned pending tree.
#pragma warning disable BL0006
    private static void ReplacePendingCaption(RenderTreeBuilder builder, string? caption)
    {
        // Rebuild this small owned tree after the final caption is known.
        // Do not write into GetFrames().Array or a submitted RenderBatch.
        builder.Clear();
        Stage(builder, caption);
    }
#pragma warning restore BL0006
}
// endsnippet:complete
