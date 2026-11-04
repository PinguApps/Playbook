// snippet:complete
using Android.App;
using Android.Runtime;
namespace SignalDeck;
[Application]
public class MainApplication : Microsoft.Maui.MauiApplication
{
    public MainApplication(nint handle, JniHandleOwnership ownership) : base(handle, ownership) { }
    protected override Microsoft.Maui.Hosting.MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
// endsnippet:complete
