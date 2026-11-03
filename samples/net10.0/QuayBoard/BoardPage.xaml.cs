// snippet:complete
using Microsoft.Maui.Controls;
namespace QuayBoard;
public partial class BoardPage : ContentPage
{
    public BoardPage()
    {
        InitializeComponent();
        Status.Text = BerthText.Ready();
        Position.Text = $"Inspection row {Grid.GetRow(Inspect)}";
    }
    private void OnInspect(object? sender, System.EventArgs e) => Status.Text = "Inspection recorded";
}

// endsnippet:complete
