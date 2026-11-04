// snippet:complete
using Microsoft.Maui.Controls;
namespace JettyItems;
public sealed class JettyPage : ContentPage
{
    public JettyPage()
    {
        Title = "Jetty inspections";
        Content = new JettyList { ItemsSource = new[] { "North jetty", "Outer light" } };
    }
}
public sealed class JettyList : CollectionView { }
public sealed class JettyCarousel : CarouselView { }
// endsnippet:complete
