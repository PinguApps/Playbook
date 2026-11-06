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
        var status = new Label { Text = "Select a survey marker" };
        var points = Enumerable.Range(0, 24).Select(i => new Location(51.62 + i * 0.0002, -3.94 + i % 4 * 0.0003)).ToArray();
        var map = new Map { Region = MapSpan.FromLocations(points), IsClusteringEnabled = true };
        var boundary = new Circle { Center = points[0], Radius = Distance.FromMeters(350), StrokeColor = Colors.Teal, StrokeWidth = 3, IsVisible = true, ZIndex = 2 };
        boundary.CircleClicked += (_, _) => status.Text = "Survey boundary selected";
        map.MapElements.Add(boundary);
        for (int i = 0; i < points.Length; i++)
        {
            var pin = new Pin { Label = $"Survey {i + 1}", Location = points[i], ClusteringIdentifier = "quay-surveys", ImageSource = ImageSource.FromFile("survey.png") };
            pin.MarkerClicked += (_, _) => status.Text = pin.Label;
            pin.InfoWindowClicked += (_, _) => pin.HideInfoWindow();
            map.Pins.Add(pin);
        }
        map.ClusterClicked += (_, e) => { status.Text = $"{e.Pins.Count} nearby surveys"; }; // Keep default zoom.
        map.MapLongClicked += (_, e) => status.Text = $"Coordinate: {e.Location.Latitude:F4}, {e.Location.Longitude:F4}";
        // User location requires a separately granted location permission before IsShowingUser=true.
        map.UserLocationChanged += (_, e) => status.Text = "Device location updated";
#if ANDROID
        map.MapStyle = "[{\"featureType\":\"poi\",\"stylers\":[{\"visibility\":\"off\"}]}]";
#endif
        var toggle = new Button { Text = "Toggle survey boundary" };
        toggle.Clicked += (_, _) => boundary.IsVisible = !boundary.IsVisible;
        var fit = new Button { Text = "Fit surveys" };
        fit.Clicked += (_, _) => map.MoveToRegion(MapSpan.FromLocations(points), animated: true);
        var layout = new Grid { RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) } };
        layout.Add(new VerticalStackLayout { Children = { status, toggle, fit } });
        layout.Add(map, 0, 1);
        return new ContentPage { Content = layout };
    }
}
// endsnippet:complete
