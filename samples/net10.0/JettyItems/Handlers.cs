// snippet:complete
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Handlers.Items;
namespace JettyItems;
public static class JettyHandlers
{
    public static MauiAppBuilder UseJettyHandlers(this MauiAppBuilder builder) => builder.ConfigureMauiHandlers(handlers =>
    {
        handlers.AddHandler<JettyList, JettyListHandler>();
        handlers.AddHandler<JettyCarousel, JettyCarouselHandler>();
    });
}
public sealed class JettyListHandler : CollectionViewHandler { }
public sealed class JettyCarouselHandler : CarouselViewHandler { }
// endsnippet:complete
