// snippet:complete
namespace QuayLedger.WinUI;
public partial class App : Microsoft.Maui.MauiWinUIApplication
{
    public App() => InitializeComponent();
    protected override Microsoft.Maui.Hosting.MauiApp CreateMauiApp() => QuayLedger.MauiProgram.CreateMauiApp();
}
// endsnippet:complete
