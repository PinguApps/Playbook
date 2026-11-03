// snippet:complete
using Microsoft.Maui.Controls;
namespace SignalDeck;
public partial class SignalPage : ContentPage
{
    public SignalPage()
    {
        InitializeComponent();
        Echo.SetBinding(Label.TextProperty, Binding.Create(static (Entry entry) => entry.Text, source: Callsign));
        Deck.SetBinding(Label.TextProperty, Binding.Create(static (SignalPage page) => page.Title, source: this));
    }
    private void OnInspect(object? sender, System.EventArgs e) => Status.Text = "Signal inspected";
}
// endsnippet:complete
