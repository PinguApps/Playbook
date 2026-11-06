// snippet:complete
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
                bool handled = item.Type == "org.puffin.new-observation";
                if (handled) Console.WriteLine("New puffin observation requested");
                completed(handled);
            });
        }));
        return builder.Build();
    }
}
// endsnippet:complete
