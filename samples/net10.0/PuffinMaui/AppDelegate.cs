// snippet:complete
using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
namespace PuffinScenes;
[Register("AppDelegate")]
public sealed class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
// endsnippet:complete
