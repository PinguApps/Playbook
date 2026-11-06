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
namespace QuayWindows;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder().UseMauiApp<QuayApp>().UseMauiMaps();
        builder.ConfigureEssentials(options => options.UseMapServiceToken("PLAYBOOK_PLACEHOLDER_NOT_A_CREDENTIAL"));
        return builder.Build();
    }
}
public sealed class QuayApp : Application
{
    protected override Window CreateWindow(IActivationState? activationState) => new Window(CreatePage());
    private static ContentPage CreatePage()
    {
        var map = new Map { MapType = MapType.Street, IsTrafficEnabled = false, IsScrollEnabled = true, IsZoomEnabled = true };
        // This exact older implementation accepts MapSpan directly; service execution is not claimed.
        map.MoveToRegion(new MapSpan(new Location(51.62, -3.94), 0.04, 0.06));
        map.Pins.Add(new Pin { Label = "Quay gate", Location = new Location(51.621, -3.941) });
        return new ContentPage { Content = map };
    }
}
// endsnippet:complete
