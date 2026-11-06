// snippet:complete
using Microsoft.Maui;
using Microsoft.Maui.Controls;
namespace PuffinScenes;
public sealed class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
        => new(new ContentPage { Content = new Label { Text = "Puffin notebook" } });
}
// endsnippet:complete
