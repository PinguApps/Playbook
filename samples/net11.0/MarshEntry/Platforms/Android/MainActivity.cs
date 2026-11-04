// snippet:complete
using Android.App;
using Android.Content.PM;
namespace MarshEntry;
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public sealed class MainActivity : Microsoft.Maui.MauiAppCompatActivity { }
// endsnippet:complete
