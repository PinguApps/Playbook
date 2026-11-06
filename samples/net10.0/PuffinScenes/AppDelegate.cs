// snippet:complete
using Foundation;
using UIKit;
namespace PuffinScenes;
[Register("AppDelegate")]
public sealed class AppDelegate : UIApplicationDelegate
{
    public override UIWindow? Window { get; set; }
    public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
    {
        // Historical pre-scene startup; iOS 26 marks this constructor obsolete.
#pragma warning disable CA1422
        // Historical pre-scene constructor is obsolete in iOS26.
#pragma warning disable CA1422
        Window = new UIWindow(UIScreen.MainScreen.Bounds) { RootViewController = ObservationView.Create("Puffin notebook") };
        #pragma warning restore CA1422
        #pragma warning restore CA1422
        Window.MakeKeyAndVisible();
        if (launchOptions?[UIApplication.LaunchOptionsShortcutItemKey] is UIApplicationShortcutItem item)
        {
            Show(item);
            return false;
        }
        return true;
    }
    private bool Show(UIApplicationShortcutItem item)
    {
        if (item.Type != "org.puffin.new-observation") return false;
        Window!.RootViewController = ObservationView.Create("New puffin observation");
        return true;
    }
    public override void PerformActionForShortcutItem(UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler)
        => completionHandler(Show(shortcutItem));
}
// endsnippet:complete
