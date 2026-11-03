// snippet:complete
using Microsoft.Maui.Controls;
namespace SignalDeck;
public partial class SignalPage : ContentPage
{
    public SignalPage()
    {
        InitializeComponent();
    }
    private void OnInspect(object? sender, System.EventArgs e) => Status.Text = "Signal inspected";
}
// endsnippet:complete
