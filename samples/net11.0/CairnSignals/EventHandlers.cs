// snippet:complete
using Microsoft.AspNetCore.Components;

namespace CairnSignals;

[EventHandler("oncairnedit", typeof(SignalEditEventArgs),
    enableStopPropagation: true, enablePreventDefault: true)]
public static class EventHandlers;

public sealed class SignalEditEventArgs : EventArgs
{
    public string Data { get; set; } = "";
    public string InputType { get; set; } = "";
    public string Station { get; set; } = "";
}
// endsnippet:complete
