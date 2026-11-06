// snippet:complete
using UIKit;
namespace PuffinScenes;
public static class ObservationView
{
    public static UIViewController Create(string message)
    {
        UIViewController view = new();
        view.View = new UILabel { Text = message, BackgroundColor = UIColor.SystemBackground, TextAlignment = UITextAlignment.Center };
        return view;
    }
}
// endsnippet:complete
