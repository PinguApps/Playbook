// snippet:complete
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
namespace SignalDeck;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp() => MauiApp.CreateBuilder().UseMauiApp<SignalApp>().Build();
}
public sealed class SignalApp : Application
{
    protected override Window CreateWindow(IActivationState? activationState) => new(new SignalPage());
}
// endsnippet:complete
