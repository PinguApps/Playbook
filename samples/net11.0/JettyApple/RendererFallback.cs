// snippet:complete
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Handlers.Compatibility;
namespace JettyApple;
public static class RendererFallback
{
    // Call after UseMauiApp only when intentionally retaining these renderers.
    public static MauiAppBuilder UseLegacyApplePages(this MauiAppBuilder builder) => builder.ConfigureMauiHandlers(handlers =>
    {
        handlers.AddHandler<NavigationPage, NavigationRenderer>();
        handlers.AddHandler<TabbedPage, TabbedRenderer>();
        handlers.AddHandler<FlyoutPage, PhoneFlyoutPageRenderer>();
    });
}
// endsnippet:complete
