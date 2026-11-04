// snippet:complete
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
namespace BeaconBoard;

public sealed class BoardPage : ContentPage
{
    private readonly Label heading = new() { Text = "Beacon watch" };
    private readonly Label result = new() { Text = "Idle" };
    private readonly BoxView swatch = new() { HeightRequest = 60, WidthRequest = 180, Color = Colors.CadetBlue };
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
        var press = new Label { Text = "Hold to prepare beacon (Apple lifecycle only)" };
        var gesture = new LongPressGestureRecognizer { MinimumPressDuration = 650, AllowableMovement = 12 };
        gesture.LongPressing += OnLongPressing;
        press.GestureRecognizers.Add(gesture);
        Content = new VerticalStackLayout { Padding = 24, Spacing = 16, Children = { heading, swatch, update, press, result } };
    }

    private void RefreshPalette()
    {
        var accent = new NamedSwatch("Night watch", Colors.DarkCyan);
        // Mutate the existing shared style, then explicitly reapply it.
        heading.Style.Setters[0].Value = accent.Value;
        heading.InvalidateStyle();
        // Let inference follow the concrete VisualStateGroupList return type.
        var groups = VisualStateManager.GetVisualStateGroups(heading);
        groups[0].States[0].Setters[0].Value = 0.85;
        VisualStateManager.InvalidateVisualStates(heading);
        swatch.Fill = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0), EndPoint = new Point(1, 0),
            GradientStops = { new GradientStop(accent.Value, 0), new GradientStop(Colors.MidnightBlue, 1) }
        };
    }


    private CancellationTokenSource? hold;
    private async void OnLongPressing(object? sender, LongPressingEventArgs args)
    {
        if (args.Status is GestureStatus.Completed or GestureStatus.Canceled) { StopHold(); return; }
        if (args.Status != GestureStatus.Started || hold is not null) return;
        using var current = new CancellationTokenSource();
        hold = current;
        try
        {
            bool canceled = await swatch.ScaleToAsync(1.2, 3000, Easing.Linear, current.Token);
            if (ReferenceEquals(hold, current)) result.Text = canceled ? "Stopped" : "Ready";
        }
        finally
        {
            if (ReferenceEquals(hold, current)) { hold = null; swatch.Scale = 1; }
        }
    }
    private void StopHold() { hold?.Cancel(); result.Text = "Stopped"; }

    protected override void OnDisappearing() { StopHold(); base.OnDisappearing(); }
}

public sealed record NamedSwatch(string Name, Color Value);
// endsnippet:complete
