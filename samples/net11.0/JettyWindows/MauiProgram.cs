// snippet:complete
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
namespace JettyWindows;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp() => MauiApp.CreateBuilder().UseMauiApp<JettyApp>().UseJettyHandlers().Build();
}
public sealed class JettyApp : Application
{
    protected override Window CreateWindow(IActivationState? activationState) => new Window(new JettyPage());
}
// endsnippet:complete
