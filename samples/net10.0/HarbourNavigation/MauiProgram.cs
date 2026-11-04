// snippet:complete
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
namespace HarbourNavigation;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp() => MauiApp.CreateBuilder().UseMauiApp<HarbourApp>().Build();
}
public sealed class HarbourApp : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = new HarbourShell();
        NavigationCheck.Attach(shell);
        return new Window(shell);
    }
}
// endsnippet:complete
