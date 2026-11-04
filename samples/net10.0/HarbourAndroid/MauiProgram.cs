// snippet:complete
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Graphics;
namespace HarbourAndroid;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder().UseMauiApp<HarbourApp>();
        builder.ConfigureMauiHandlers(handlers => handlers.AddHandler<HarbourShell, Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer>());
        return builder.Build();
    }
}
public sealed class HarbourApp : Application
{
    protected override Window CreateWindow(IActivationState? activationState) => new(new HarbourShell());
}
public sealed class HarbourShell : Shell
{
    public HarbourShell()
    {
        FlyoutBehavior = FlyoutBehavior.Flyout;
        FlyoutBackgroundColor = Colors.LightBlue;
        Items.Add(new ShellContent { Title = "Departures", Content = new ContentPage { Content = new Label { Text = "Harbour departures" } } });
        Items.Add(new ShellContent { Title = "Arrivals", Content = new ContentPage { Content = new Label { Text = "Harbour arrivals" } } });
    }
}
// endsnippet:complete
