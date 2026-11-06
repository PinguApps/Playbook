// snippet:complete
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
namespace TernAssets;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp() => MauiApp.CreateBuilder().UseMauiApp<AssetApp>().Build();
}
public sealed class AssetApp : Application
{
    protected override Window CreateWindow(IActivationState? activationState) => new(new ContentPage
    {
        Content = new Image { Source = "marker.png" }
    });
}

// endsnippet:complete
