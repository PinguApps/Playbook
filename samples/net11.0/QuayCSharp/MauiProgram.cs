// snippet:complete
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
namespace QuayLedger;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp() => MauiApp.CreateBuilder().UseMauiApp<LedgerApp>().Build();
}
public sealed class LedgerApp : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var count = new Label { Text = "Crates checked: 0", FontSize = 24 };
        var button = new Button { Text = "Check crate" };
        var checkedCrates = 0;
        button.Clicked += (_, _) => count.Text = $"Crates checked: {++checkedCrates}";
        var page = new ContentPage { Content = new VerticalStackLayout { Padding = 24, Spacing = 12, Children = { count, button } } };
        var window = new Window(page);
        if (Environment.GetEnvironmentVariable("PLAYBOOK_QUAY_PROBE") is { Length: > 0 } receipt)
        {
            count.Loaded += (_, _) => page.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(300), () =>
            {
                ((IButtonController)button).SendClicked();
                if (count.Text != "Crates checked: 1") throw new InvalidOperationException("Crate count did not update.");
                File.WriteAllText(receipt, $"PASS: Label.Loaded and managed button event; {count.Text}; runtime={Environment.Version}");
                Current!.CloseWindow(window);
            });
        }
        return window;
    }
}
// endsnippet:complete
