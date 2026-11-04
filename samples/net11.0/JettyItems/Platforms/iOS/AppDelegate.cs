// snippet:complete
using Foundation;
namespace JettyItems;
[Register("AppDelegate")]
public class AppDelegate : Microsoft.Maui.MauiUIApplicationDelegate
{
    protected override Microsoft.Maui.Hosting.MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
// endsnippet:complete
