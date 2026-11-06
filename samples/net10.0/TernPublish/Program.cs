// snippet:complete
using Foundation;
using UIKit;
namespace TernPublish;
public static class Program
{
    public static void Main(string[] args) => UIApplication.Main(args, null, typeof(TernApplication));
}
[Register("TernApplication")]
public sealed class TernApplication : UIApplicationDelegate { }
[Register("TernScene")]
public sealed class TernScene : UIWindowSceneDelegate
{
    public override void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
    {
        if (scene is not UIWindowScene windowScene) return;
        Window = new UIWindow(windowScene);
        Window.RootViewController = new UIViewController();
        Window.RootViewController.View!.AddSubview(new UILabel { Text = RuntimePolicy.Describe(), Frame = new CoreGraphics.CGRect(16, 32, 700, 80), Lines = 0 });
        Window.MakeKeyAndVisible();
    }
}
// endsnippet:complete
