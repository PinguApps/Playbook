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
namespace QuayAtlas;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder().UseMauiApp<QuayApp>().UseMauiMaps();
        return builder.Build();
    }
}
public sealed class QuayApp : Application
{
    protected override Window CreateWindow(IActivationState? activationState) => new Window(CreatePage());
    private static ContentPage CreatePage()
    {
        var map = new Map();
        map.MoveToRegion(new MapSpan(new Location(51.62, -3.94), 0.04, 0.06));
        var boundary = new Circle { Center = new Location(51.62, -3.94), Radius = Distance.FromMeters(350), StrokeColor = Colors.Teal, StrokeWidth = 3 };
        map.MapElements.Add(boundary);
        map.Pins.Add(new Pin { Label = "Quay gate", Location = new Location(51.621, -3.941) });
        // Hide an older overlay by removing it; add it again to show it.
        var toggle = new Button { Text = "Toggle survey boundary" };
        toggle.Clicked += (_, _) => { if (map.MapElements.Contains(boundary)) map.MapElements.Remove(boundary); else map.MapElements.Add(boundary); };
        var layout = new Grid { RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) } };
        layout.Add(toggle);
        layout.Add(map, 0, 1);
        return new ContentPage { Content = layout };
    }
}
// endsnippet:complete
