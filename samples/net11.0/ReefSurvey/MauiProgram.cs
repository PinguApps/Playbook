// snippet:complete
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
namespace ReefSurvey;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp() => MauiApp.CreateBuilder().UseMauiApp<SurveyApp>().Build();
}
public sealed class SurveyApp : Application
{
    protected override Window CreateWindow(IActivationState? activationState) => SurveyChrome.Create();
}
// endsnippet:complete

