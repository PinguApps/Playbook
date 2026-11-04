// snippet:complete
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
namespace QuayAvalonia;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp(bool useSingleViewLifetime = false) =>
        MauiApp.CreateBuilder().UseMauiApp<QuayApp>().UseAvaloniaApp(useSingleViewLifetime).Build();
}
public sealed class QuayApp : Application
{
    protected override Window CreateWindow(IActivationState? activationState) =>
        new(new ContentPage { Content = new Label { Text = "Quay Ledger: crates ready", FontSize = 24 } });
}
// endsnippet:complete
