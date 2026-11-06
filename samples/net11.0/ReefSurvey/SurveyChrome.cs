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
        window.StatusBarTheme = StatusBarTheme.Dark; // Dark background means light icons.
        return window;
    }
}
// endsnippet:complete
