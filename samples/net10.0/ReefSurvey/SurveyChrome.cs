// snippet:complete
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
namespace ReefSurvey;
public static class SurveyChrome
{
    public static Window Create()
    {
        var page = new NavigationPage(new SurveyPage()) { BarBackgroundColor = Colors.MidnightBlue, BarTextColor = Colors.White };
        var window = new Window(page);
        window.Created += (_, _) =>
        {
            var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            if (activity?.Window?.DecorView is not { } view) return;
            var controller = AndroidX.Core.View.WindowCompat.GetInsetsController(activity.Window, view);
            controller!.AppearanceLightStatusBars = false; // Light status icons; app theme stays unchanged.
        };
        return window;
    }
}
// endsnippet:complete

