// snippet:complete
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
namespace MarshEntry;
public sealed class SurveyEntry : Entry { }
public sealed class SurveyHandler : EntryHandler
{
    protected override MauiAppCompatEditText CreatePlatformView()
    {
        var view = base.CreatePlatformView();
        view.ContentDescription = "Marsh survey station name";
        return view;
    }
}
public static class MauiProgram
{
    public static MauiApp CreateMauiApp() => MauiApp.CreateBuilder().UseMauiApp<SurveyApp>()
        .ConfigureMauiHandlers(handlers => handlers.AddHandler<SurveyEntry, SurveyHandler>()).Build();
}
public sealed class SurveyApp : Application
{
    protected override Window CreateWindow(IActivationState? activationState) => new(new ContentPage
    { Content = new VerticalStackLayout { Children = { new SurveyEntry { Placeholder = "Station" }, new Entry { Placeholder = "Unchanged stock entry" } } } });
}
// endsnippet:complete
