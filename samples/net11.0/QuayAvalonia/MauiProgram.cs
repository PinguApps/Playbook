// snippet:complete
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.Logging;
namespace QuayAvalonia;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp(bool useSingleViewLifetime = false)
    {
        var builder = MauiApp.CreateBuilder().UseMauiApp<QuayApp>().UseAvaloniaApp(useSingleViewLifetime);
        builder.Logging.AddDebug();
        return builder.Build();
    }
}
public sealed class QuayApp : Application
{
    protected override Window CreateWindow(IActivationState? activationState) =>
        new(new ContentPage { Content = new Label { Text = "Quay Ledger: crates ready", FontSize = 24 } });
}
// endsnippet:complete
