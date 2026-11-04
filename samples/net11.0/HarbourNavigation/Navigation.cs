// snippet:complete
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
namespace HarbourNavigation;

public sealed class HarbourShell : Shell
{
    public HarbourShell()
    {
        FlyoutBackgroundColor = Colors.LightBlue;
        Routing.RegisterRoute("pier/{pierId}", typeof(PierPage));
        var open = new Button { Text = "Open North pier" };
        open.Clicked += async (_, _) => await GoToAsync("//home/pier/NORTH-7");
        var tabs = new Button { Text = "Open departure tabs" };
        tabs.Clicked += (_, _) => Application.Current!.OpenWindow(new Window(new DepartureTabs()));
        Items.Add(new ShellContent { Route = "home", Title = "Harbour", Content = new ContentPage {
            Title = "Harbour departures", Content = new VerticalStackLayout { Children = { open, tabs } }
        } });
    }
}
public sealed class PierPage : ContentPage, IQueryAttributable
{
    private readonly Label destination = new();
    public PierPage()
    {
        Title = "Pier details";
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior { TextOverride = "Back", AccessibilityLabel = "Return to harbour departures" });
        var action = new SwipeItem { Text = "Mark inspected", BackgroundColor = Colors.LightBlue,
            IconImageSource = new FontImageSource { Glyph = "✓", Color = Colors.DarkBlue } };
        action.IconColor = Colors.DarkBlue;
        action.TextColor = Colors.DarkBlue;
        action.Invoked += (_, _) => destination.Text += " — inspected";
        var swipe = new SwipeView { Content = destination, RightItems = new SwipeItems { action } };
        Content = swipe;
    }
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("pierId", out var value) || value is not string id || id != "NORTH-7")
            throw new ArgumentException("Unknown pier.");
        destination.Text = "North pier: inspection due";
        SemanticProperties.SetDescription(destination, "North pier. Inspection due. Swipe for inspection action.");
    }
}
public sealed class DepartureTabs : TabbedPage
{
    public DepartureTabs()
    {
        var departures = new ContentPage { Title = "Departures", Content = new Label { Text = "3 departures awaiting clearance" } };
        TabbedPage.SetBadgeText(departures, "3");
        TabbedPage.SetBadgeColor(departures, Colors.DarkBlue);
        TabbedPage.SetBadgeTextColor(departures, Colors.White);
        Children.Add(departures);
        Children.Add(new ContentPage { Title = "Arrivals", Content = new Label { Text = "No arrivals awaiting clearance" } });
    }
}
// endsnippet:complete
