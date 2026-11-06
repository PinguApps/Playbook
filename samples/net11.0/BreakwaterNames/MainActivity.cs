// snippet:complete
namespace CoastalProbe;

[Android.App.Activity(Label = "Coastal probe", MainLauncher = true, Exported = true)]
public sealed class MainActivity : Android.App.Activity
{
    protected override void OnCreate(Android.OS.Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(new Android.Widget.TextView(this) { Text = BreakwaterNames.NameReader.Read() });
    }
}
// endsnippet:complete
