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
#pragma warning disable CS0618
        _ = WebEventCallbackFactoryEventArgsExtensions.Create(EventCallback.Factory, receiver, (Action<ClipboardEventArgs>)(_ => { }));
        _ = WebEventCallbackFactoryEventArgsExtensions.Create(EventCallback.Factory, receiver, (Func<ClipboardEventArgs, Task>)(_ => Task.CompletedTask));
        _ = WebEventCallbackFactoryEventArgsExtensions.Create(EventCallback.Factory, receiver, (Action<DragEventArgs>)(_ => { }));
        _ = WebEventCallbackFactoryEventArgsExtensions.Create(EventCallback.Factory, receiver, (Func<DragEventArgs, Task>)(_ => Task.CompletedTask));
        _ = WebEventCallbackFactoryEventArgsExtensions.Create(EventCallback.Factory, receiver, (Action<ErrorEventArgs>)(_ => { }));
        _ = WebEventCallbackFactoryEventArgsExtensions.Create(EventCallback.Factory, receiver, (Func<ErrorEventArgs, Task>)(_ => Task.CompletedTask));
        _ = WebEventCallbackFactoryEventArgsExtensions.Create(EventCallback.Factory, receiver, (Action<FocusEventArgs>)(_ => { }));
        _ = WebEventCallbackFactoryEventArgsExtensions.Create(EventCallback.Factory, receiver, (Func<FocusEventArgs, Task>)(_ => Task.CompletedTask));
        _ = WebEventCallbackFactoryEventArgsExtensions.Create(EventCallback.Factory, receiver, (Action<KeyboardEventArgs>)(_ => { }));
        _ = WebEventCallbackFactoryEventArgsExtensions.Create(EventCallback.Factory, receiver, (Func<KeyboardEventArgs, Task>)(_ => Task.CompletedTask));
        _ = WebEventCallbackFactoryEventArgsExtensions.Create(EventCallback.Factory, receiver, (Action<MouseEventArgs>)(_ => { }));
        _ = WebEventCallbackFactoryEventArgsExtensions.Create(EventCallback.Factory, receiver, (Func<MouseEventArgs, Task>)(_ => Task.CompletedTask));
        _ = WebEventCallbackFactoryEventArgsExtensions.Create(EventCallback.Factory, receiver, (Action<PointerEventArgs>)(_ => { }));
        _ = WebEventCallbackFactoryEventArgsExtensions.Create(EventCallback.Factory, receiver, (Func<PointerEventArgs, Task>)(_ => Task.CompletedTask));
        _ = WebEventCallbackFactoryEventArgsExtensions.Create(EventCallback.Factory, receiver, (Action<ProgressEventArgs>)(_ => { }));
        _ = WebEventCallbackFactoryEventArgsExtensions.Create(EventCallback.Factory, receiver, (Func<ProgressEventArgs, Task>)(_ => Task.CompletedTask));
        _ = WebEventCallbackFactoryEventArgsExtensions.Create(EventCallback.Factory, receiver, (Action<TouchEventArgs>)(_ => { }));
        _ = WebEventCallbackFactoryEventArgsExtensions.Create(EventCallback.Factory, receiver, (Func<TouchEventArgs, Task>)(_ => Task.CompletedTask));
        _ = WebEventCallbackFactoryEventArgsExtensions.Create(EventCallback.Factory, receiver, (Action<WheelEventArgs>)(_ => { }));
        _ = WebEventCallbackFactoryEventArgsExtensions.Create(EventCallback.Factory, receiver, (Func<WheelEventArgs, Task>)(_ => Task.CompletedTask));
#pragma warning restore CS0618
    }
}
// endsnippet:complete
