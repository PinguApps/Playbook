// snippet:complete
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Handlers.Items;
namespace JettyWindows;
public static class JettyHandlers
{
    public static MauiAppBuilder UseJettyHandlers(this MauiAppBuilder builder) => builder.ConfigureMauiHandlers(handlers =>
    {
        handlers.AddHandler<JettyList, JettyListHandler>();
        
    });
}
public sealed class JettyListHandler : CollectionViewHandler { }

// endsnippet:complete
