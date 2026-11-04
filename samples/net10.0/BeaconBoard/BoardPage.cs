// snippet:complete
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
namespace BeaconBoard;

public sealed class BoardPage : ContentPage
{
    private readonly Label heading = new() { Text = "Beacon watch" };
    private readonly Label result = new() { Text = "Idle" };
    private readonly Microsoft.Maui.Controls.Shapes.Rectangle swatch = new() { HeightRequest = 60, WidthRequest = 180, Fill = new SolidColorBrush(Colors.CadetBlue) };
    public BoardPage()
    {
        var initial = new Style(typeof(Label));
        initial.Setters.Add(new Setter { Property = Label.TextColorProperty, Value = Colors.CadetBlue });
        heading.Style = initial;
        var groups = new VisualStateGroupList();
        var group = new VisualStateGroup { Name = "Watch" };
        var state = new VisualState { Name = "Active" };
        state.Setters.Add(new Setter { Property = VisualElement.OpacityProperty, Value = 1.0 });
        group.States.Add(state); groups.Add(group);
        VisualStateManager.SetVisualStateGroups(heading, groups);
        VisualStateManager.GoToState(heading, "Active");
        var update = new Button { Text = "Night palette" };
        update.Clicked += (_, _) => RefreshPalette();
        var press = new Button { Text = "Hold to prepare beacon" };
        press.Pressed += StartHold;
        press.Released += (_, _) => StopHold();
        Content = new VerticalStackLayout { Padding = 24, Spacing = 16, Children = { heading, swatch, update, press, result } };
    }

    private void RefreshPalette()
    {
        var accent = new NamedSwatch("Night watch", Colors.DarkCyan);
        var style = new Style(typeof(Label));
        style.Setters.Add(new Setter { Property = Label.TextColorProperty, Value = accent.Value });
        heading.Style = style;
        var groups = new VisualStateGroupList();
        var group = new VisualStateGroup { Name = "Watch" };
        var state = new VisualState { Name = "Active" };
        state.Setters.Add(new Setter { Property = VisualElement.OpacityProperty, Value = 0.85 });
        group.States.Add(state); groups.Add(group);
        VisualStateManager.SetVisualStateGroups(heading, groups);
        VisualStateManager.GoToState(heading, "Active");
        swatch.Fill = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0), EndPoint = new Point(1, 0),
            GradientStops = { new GradientStop(accent.Value, 0), new GradientStop(Colors.MidnightBlue, 1) }
        };
    }


    private CancellationTokenSource? hold;
    private async void StartHold(object? sender, EventArgs args)
    {
        if (hold is not null) return;
        using var current = new CancellationTokenSource();
        hold = current;
        try
        {
            await Task.Delay(650, current.Token);
            bool canceled = await swatch.ScaleToAsync(1.2, 3000, Easing.Linear);
            if (!current.IsCancellationRequested) result.Text = canceled ? "Stopped" : "Ready";
        }
        catch (OperationCanceledException) when (current.IsCancellationRequested) { }
        finally
        {
            if (ReferenceEquals(hold, current)) { hold = null; swatch.Scale = 1; }
        }
    }
    private void StopHold()
    {
        hold?.Cancel();
        swatch.AbortAnimation("ScaleToAsync");
        result.Text = "Stopped";
    }

    protected override void OnDisappearing() { StopHold(); base.OnDisappearing(); }
}

public sealed class NamedSwatch(string name, Color value) : Color(value.Red, value.Green, value.Blue, value.Alpha)
{
    public string Name { get; } = name;
    public Color Value => this;
}
// endsnippet:complete
