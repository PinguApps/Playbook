// snippet:complete
namespace TernActivation.WinUI;
public partial class App : Microsoft.Maui.MauiWinUIApplication
{
    public App()
    {
        ProtocolRegistration.RunRequestedAction();
        if (ActivationRoute.Redirect())
        {
            Environment.Exit(0);
            return;
        }
        InitializeComponent();
    }
    protected override Microsoft.Maui.Hosting.MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}

// endsnippet:complete
