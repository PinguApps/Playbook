// snippet:complete
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Planetarium;

// This entire class is a deliberate framework integration, not application guidance.
// BL0006 warns these RenderTree contracts may change; never suppress it project-wide.
#pragma warning disable BL0006
public class PlanetariumRenderer : WebRenderer
{
    // Host-owned range, disjoint from built-in renderer IDs. Never reuse an ID.
    private static int nextIdentity = 200;
    private readonly IJSRuntime js;
    private string selector = "";
    private int rootId;

    public PlanetariumRenderer(IServiceProvider services, ILoggerFactory logs)
        : base(new RendererServices(services), logs, new JsonSerializerOptions(JsonSerializerDefaults.Web),
            new JSComponentInterop(new JSComponentConfigurationStore()))
    {
        js = services.GetRequiredService<IJSRuntime>();
    }

    public int Identity => RendererId;
    public ValueTask ShutdownAsync() => DisposeAsync();
    public override Dispatcher Dispatcher { get; } = new BrowserThreadDispatcher();

    // Called from WebRenderer's constructor, before this constructor's body.
    // Static state only: no dependency on js, selector, Dispatcher or parameters.
    protected override int GetWebRendererId() => Interlocked.Increment(ref nextIdentity);

    public Task StartAsync(string element, string label) => Dispatcher.InvokeAsync(async () =>
    {
        rootId = AddRootComponent(typeof(Surface), element);
        await RenderRootComponentAsync(rootId,
            ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(Surface.Label)] = label }));
    });

    protected override void AttachRootComponentToBrowser(int componentId, string domElementSelector)
        => selector = domElementSelector;

    protected override Task UpdateDisplayAsync(in RenderBatch renderBatch)
    {
        // Bounded adapter: this host renders exactly one button, not arbitrary Blazor trees.
        var frames = GetCurrentRenderTreeFrames(rootId);
        ulong handler = 0;
        var text = new System.Text.StringBuilder();
        for (var index = 0; index < frames.Count; index++)
        {
            var frame = frames.Array[index];
            if (frame.FrameType == RenderTreeFrameType.Attribute && frame.AttributeName == "onclick")
                handler = frame.AttributeEventHandlerId;
            if (frame.FrameType == RenderTreeFrameType.Text) text.Append(frame.TextContent);
        }
        if (handler == 0) throw new InvalidOperationException("Expected a wired Surface button.");
        return SendAsync(handler, text.ToString());
    }

    private async Task SendAsync(ulong handler, string text)
    {
        try { await js.InvokeVoidAsync("planetarium.render", Identity, selector, handler, text); }
        catch (JSException error) { throw new InvalidOperationException("Browser surface update failed.", error); }
    }

    protected override void HandleException(Exception exception) => throw new InvalidOperationException("Planetarium renderer failed.", exception);
}
#pragma warning restore BL0006
// endsnippet:complete
