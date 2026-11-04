// snippet:complete
using Microsoft.Maui.Controls;
namespace HarbourNavigation;

// Optional local integration probe. The normal app remains interactive.
internal static class NavigationCheck
{
    internal static void Attach(HarbourShell shell)
    {
        var receipt = Environment.GetEnvironmentVariable("HARBOUR_NAVIGATION_RECEIPT");
        if (string.IsNullOrEmpty(receipt)) return;
        var started = false;
        shell.Loaded += async (_, _) =>
        {
            if (started) return;
            started = true;
            try
            {
                await shell.GoToAsync("pier?pierId=NORTH-7", false);
                if (shell.CurrentPage is not PierPage page || page.Content is not SwipeView swipe ||
                    swipe.Content is not Label { Text: "North pier: inspection due" })
                    throw new InvalidOperationException("Shell did not deliver the pier identifier.");
                await shell.GoToAsync("..", false);
                if (shell.CurrentPage is PierPage) throw new InvalidOperationException("Back navigation failed.");
                File.WriteAllText(receipt, "PASS: actual Windows Shell forward/query delivery/back navigation.");
            }
            catch (Exception error) { File.WriteAllText(receipt, "FAIL: " + error); }
            finally { Application.Current!.Quit(); }
        };
    }
}
// endsnippet:complete
