// snippet:complete
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
namespace PuffinScenes;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder().UseMauiApp<App>();
        builder.ConfigureLifecycleEvents(events => events.AddiOS(ios =>
        {
            ios.OpenUrl((application, url, options) =>
            {
                Console.WriteLine(url);
                return false;
            });
            ios.PerformActionForShortcutItem((application, item, completed) =>
            {
                // This observer logs; another handler owns navigation.
                if (item.Type == "org.puffin.new-observation") Console.WriteLine("New puffin observation requested");
                completed(false);
            });
        }));
        return builder.Build();
    }
}
// endsnippet:complete
