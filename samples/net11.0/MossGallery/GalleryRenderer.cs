// snippet:complete
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Logging;

namespace MossGallery;

// Advanced framework boundary: this bounded renderer consumes one figure.
#pragma warning disable BL0006
public sealed class GalleryRenderer(IServiceProvider services, ILoggerFactory logs) : Renderer(services, logs)
{
    private int root;
    public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();
    public string? Caption { get; private set; }
    public int Batches { get; private set; }

    public Task ShowAsync(string? caption) => Dispatcher.InvokeAsync(async () =>
    {
        if (Batches == 0) root = AssignRootComponentId(new GalleryCard());
        await RenderRootComponentAsync(root, ParameterView.FromDictionary(
            new Dictionary<string, object?> { [nameof(GalleryCard.Caption)] = caption }));
    });

    protected override Task UpdateDisplayAsync(in RenderBatch renderBatch)
    {
        // Read the submitted batch; never mutate it or its frames.
        if (renderBatch.UpdatedComponents.Count != 1) throw new InvalidOperationException("Unexpected render batch.");
        var frames = GetCurrentRenderTreeFrames(root);
        Caption = null;
        for (var i = 0; i < frames.Count; i++)
            if (frames.Array[i].FrameType == RenderTreeFrameType.Attribute && frames.Array[i].AttributeName == "data-caption")
                Caption = (string?)frames.Array[i].AttributeValue;
        Batches++;
        return Task.CompletedTask;
    }

    protected override void HandleException(Exception exception) => throw new InvalidOperationException("Gallery render failed.", exception);
}
#pragma warning restore BL0006
// endsnippet:complete
