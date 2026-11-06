// snippet:complete
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
namespace TernActivation;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder().UseMauiApp<DispatchApp>();
        ActivationRoute.Configure(builder);
        return builder.Build();
    }
}
public sealed class DispatchApp : Application
{
    protected override Window CreateWindow(IActivationState? activationState) => new(new ContentPage
    {
        Content = new Label { Text = "Tern Dispatch: awaiting activation" }
    });
}

// endsnippet:complete
