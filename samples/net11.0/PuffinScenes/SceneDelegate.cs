// snippet:complete
using Foundation;
using UIKit;
namespace PuffinScenes;
[Register("PuffinSceneDelegate")]
public sealed class SceneDelegate : UIResponder, IUIWindowSceneDelegate
{
    [Export("window")]
    public UIWindow? Window { get; set; }
    [Export("scene:willConnectToSession:options:")]
    public void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions options)
    {
        if (scene is not UIWindowScene windowScene) return;
        Window = new UIWindow(windowScene) { RootViewController = ObservationView.Create("Puffin notebook") };
        Window.MakeKeyAndVisible();
        if (options.ShortcutItem is { } item) Show(item);
    }
    private bool Show(UIApplicationShortcutItem item)
    {
        if (item.Type != "org.puffin.new-observation" || Window is null) return false;
        Window.RootViewController = ObservationView.Create("New puffin observation");
        return true;
    }
    [Export("windowScene:performActionForShortcutItem:completionHandler:")]
    public void PerformAction(UIWindowScene scene, UIApplicationShortcutItem item, Action<bool> completionHandler)
        => completionHandler(Show(item));
}
// endsnippet:complete
