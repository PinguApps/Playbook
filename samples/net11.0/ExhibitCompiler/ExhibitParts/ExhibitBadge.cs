// snippet:complete
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
namespace ExhibitParts;
public sealed class ExhibitBadge : ComponentBase
{
    [Parameter] public string Label { get; set; } = "";
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "strong");
        builder.AddContent(1, Label);
        builder.CloseElement();
    }
}
// endsnippet:complete
