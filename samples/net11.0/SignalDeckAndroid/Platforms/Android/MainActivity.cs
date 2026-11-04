// snippet:complete
using Android.App;
using Android.Content.PM;
namespace SignalDeck;
[Activity(Theme = "@style/Maui.MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : Microsoft.Maui.MauiAppCompatActivity { }
// endsnippet:complete
