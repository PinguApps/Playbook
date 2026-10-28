// snippet:complete
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ErrorEventArgs = Microsoft.AspNetCore.Components.Web.ErrorEventArgs;

namespace GreenhouseValves;

// Both callback shapes for all ten removed event-argument overload pairs.
internal static class CallbackForms
{
    internal static void Check(object receiver)
    {
        _ = EventCallback.Factory.Create<ClipboardEventArgs>(receiver, (Action<ClipboardEventArgs>)(_ => { }));
        _ = EventCallback.Factory.Create<ClipboardEventArgs>(receiver, (Func<ClipboardEventArgs, Task>)(_ => Task.CompletedTask));
        _ = EventCallback.Factory.Create<DragEventArgs>(receiver, (Action<DragEventArgs>)(_ => { }));
        _ = EventCallback.Factory.Create<DragEventArgs>(receiver, (Func<DragEventArgs, Task>)(_ => Task.CompletedTask));
        _ = EventCallback.Factory.Create<ErrorEventArgs>(receiver, (Action<ErrorEventArgs>)(_ => { }));
        _ = EventCallback.Factory.Create<ErrorEventArgs>(receiver, (Func<ErrorEventArgs, Task>)(_ => Task.CompletedTask));
        _ = EventCallback.Factory.Create<FocusEventArgs>(receiver, (Action<FocusEventArgs>)(_ => { }));
        _ = EventCallback.Factory.Create<FocusEventArgs>(receiver, (Func<FocusEventArgs, Task>)(_ => Task.CompletedTask));
        _ = EventCallback.Factory.Create<KeyboardEventArgs>(receiver, (Action<KeyboardEventArgs>)(_ => { }));
        _ = EventCallback.Factory.Create<KeyboardEventArgs>(receiver, (Func<KeyboardEventArgs, Task>)(_ => Task.CompletedTask));
        _ = EventCallback.Factory.Create<MouseEventArgs>(receiver, (Action<MouseEventArgs>)(_ => { }));
        _ = EventCallback.Factory.Create<MouseEventArgs>(receiver, (Func<MouseEventArgs, Task>)(_ => Task.CompletedTask));
        _ = EventCallback.Factory.Create<PointerEventArgs>(receiver, (Action<PointerEventArgs>)(_ => { }));
        _ = EventCallback.Factory.Create<PointerEventArgs>(receiver, (Func<PointerEventArgs, Task>)(_ => Task.CompletedTask));
        _ = EventCallback.Factory.Create<ProgressEventArgs>(receiver, (Action<ProgressEventArgs>)(_ => { }));
        _ = EventCallback.Factory.Create<ProgressEventArgs>(receiver, (Func<ProgressEventArgs, Task>)(_ => Task.CompletedTask));
        _ = EventCallback.Factory.Create<TouchEventArgs>(receiver, (Action<TouchEventArgs>)(_ => { }));
        _ = EventCallback.Factory.Create<TouchEventArgs>(receiver, (Func<TouchEventArgs, Task>)(_ => Task.CompletedTask));
        _ = EventCallback.Factory.Create<WheelEventArgs>(receiver, (Action<WheelEventArgs>)(_ => { }));
        _ = EventCallback.Factory.Create<WheelEventArgs>(receiver, (Func<WheelEventArgs, Task>)(_ => Task.CompletedTask));
    }
}
// endsnippet:complete
