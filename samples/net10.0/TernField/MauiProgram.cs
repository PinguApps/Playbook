// snippet:complete
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
namespace TernField;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp() => MauiApp.CreateBuilder().UseMauiApp<FieldApp>().Build();
}
public sealed class FieldApp : Application
{
    protected override Window CreateWindow(IActivationState? activationState) => new(new FieldPage());
}
public sealed class FieldPage : ContentPage
{
    private readonly FieldSession session = new();
    private readonly Label result = new() { Text = "Start a foreground field session" };
    public FieldPage()
    {
        Button start = new() { Text = "Start" };
        start.Clicked += async (_, _) =>
        {
            try { result.Text = await session.StartAsync(); }
            catch (Exception error) { result.Text = error.Message; }
        };
        Button stop = new() { Text = "Stop" };
        stop.Clicked += (_, _) => session.Stop();
        Content = new VerticalStackLayout { Children = { start, stop, result } };
    }
    protected override void OnDisappearing() { session.Stop(); base.OnDisappearing(); }
}

// endsnippet:complete
