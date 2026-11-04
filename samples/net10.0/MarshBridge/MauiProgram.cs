// snippet:complete
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using System.Text.Json.Serialization;
namespace MarshBridge;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp() => MauiApp.CreateBuilder().UseMauiApp<SurveyApp>().Build();
}
public sealed class SurveyApp : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var web = new HybridWebView();
        web.SetInvokeJavaScriptTarget(new SurveyActions());
        return new Window(new ContentPage { Content = web });
    }
}
public sealed class SurveyActions
{
    public SurveyReceipt Record(SurveyRequest request) => new(request.Station, request.Birds + 1);
}
public sealed record SurveyRequest(string Station, int Birds);
public sealed record SurveyReceipt(string Station, int Birds);
[JsonSerializable(typeof(SurveyRequest))]
[JsonSerializable(typeof(SurveyReceipt))]
internal partial class SurveyJsonContext : JsonSerializerContext { }
// endsnippet:complete
