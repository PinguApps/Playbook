// snippet:complete
using Microsoft.Maui.Controls;
namespace JettyApple;
public sealed class JettyPage : ContentPage
{
    private readonly Label arrival = new() { Text = "Waiting for the harbour page" };
    public JettyPage() { Title = "Jetty arrivals"; Content = arrival; }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        arrival.Text = "Jetty arrivals visible";
        arrival.Focus();
    }
}
// endsnippet:complete
