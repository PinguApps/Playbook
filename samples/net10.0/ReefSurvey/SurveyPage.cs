// snippet:complete
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
namespace ReefSurvey;
public sealed class SurveyPage : ContentPage
{
    readonly Entry station = new() { Placeholder = "Reef station" };
    readonly Switch gallery = new();
    readonly Label status = new();
    public SurveyPage()
    {
        var capture = new Button { Text = "Photograph station" };
        capture.Clicked += async (_, _) =>
        {
            capture.IsEnabled = false;
            try
            {
                if (!MediaPicker.IsCaptureSupported || string.IsNullOrWhiteSpace(station.Text)) return;
                var stationKey = station.Text;
                Preferences.Default.Set("reef.pending", stationKey);
                var result = await MediaPicker.CapturePhotoAsync();
                if (result is not null) await StoreAsync(result, stationKey);
                Preferences.Default.Remove("reef.pending");
                status.Text = result is null ? "Capture cancelled" : "Station photograph stored";
            }
            catch (Exception error) { status.Text = error.Message; }
            finally { capture.IsEnabled = true; }
        };
        Content = new VerticalStackLayout { Children = { station, new Label { Text = "Also save new capture to gallery" }, gallery, capture, status } };
        station.Text = Preferences.Default.Get("reef.pending", "");
        gallery.IsEnabled = false; // .NET 10 has no built-in SaveToGallery option.
        status.Text = Preferences.Default.ContainsKey("reef.pending") ? "Interrupted capture: retake the station photograph" : "Ready";
    }
    static async Task StoreAsync(FileResult file, string stationKey)
    {
        // One photo per station. Retry replaces the same file rather than duplicating it.
        var name = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(stationKey)));
        var destination = Path.Combine(FileSystem.AppDataDirectory, name + ".jpg");
        await using var input = await file.OpenReadAsync();
        await using (var output = File.Create(destination + ".pending")) await input.CopyToAsync(output);
        File.Move(destination + ".pending", destination, overwrite: true);
    }
}
// endsnippet:complete

