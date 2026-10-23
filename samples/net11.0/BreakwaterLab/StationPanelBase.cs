// snippet:complete
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BreakwaterLab;

public abstract class StationPanelBase : ComponentBase
{
    [Inject]
    public StationName Station { get; set; } = default!;

    [Inject(Key = "depth")]
    public Scale Scale { get; set; } = default!;

    [Inject]
    public SessionStamp Session { get; set; } = default!;

    [Inject]
    public IJSRuntime JS { get; set; } = default!;
}
// endsnippet:complete
