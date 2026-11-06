// snippet:complete
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Maps;
using Map = Microsoft.Maui.Controls.Maps.Map;
namespace QuayWindows;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder().UseMauiApp<QuayApp>();
        return builder.Build();
    }
}
public sealed class QuayApp : Application
{
    protected override Window CreateWindow(IActivationState? activationState) => new Window(CreatePage());
    private static ContentPage CreatePage()
    {
        return new ContentPage { Content = new Label { Text = "MAUI 10 has no first-party Windows map handler. Choose an explicitly assessed alternative map integration." } };
    }
}
// endsnippet:complete
