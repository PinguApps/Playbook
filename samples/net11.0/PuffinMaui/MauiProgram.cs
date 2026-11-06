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
            ios.SceneWillConnect((scene, session, options) =>
            {
                foreach (var context in options.UrlContexts) Console.WriteLine(context.Url);
            });
            ios.SceneOpenUrl((scene, contexts) =>
            {
                foreach (var context in contexts) Console.WriteLine(context.Url);
                return false; // Logging observes a URL; it does not handle it.
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
