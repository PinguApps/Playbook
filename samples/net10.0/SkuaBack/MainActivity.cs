// snippet:complete
using Android.App;
using Android.OS;
using Android.Widget;



namespace SkuaBack;

[Activity(Label = "Skua field notes", MainLauncher = true, Exported = true)]
public sealed class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(new TextView(this) { Text = "Skua observation saved. Back returns to the launcher." });
    }
}
// endsnippet:complete
