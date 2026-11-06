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
    bool busy;
    public SurveyPage()
    {
        var capture = new Button { Text = "Photograph station" };
        capture.Clicked += async (_, _) =>
        {
            if (busy) return;
            busy = true;
            capture.IsEnabled = false;
            try
            {
                if (Preferences.Default.ContainsKey("reef.pending")) { status.Text = "Resolve the interrupted station first"; return; }
                if (!MediaPicker.IsCaptureSupported || string.IsNullOrWhiteSpace(station.Text)) return;
                var stationKey = station.Text;
                Preferences.Default.Set("reef.pending", stationKey);
                Preferences.Default.Set("reef.gallery", gallery.IsToggled);
                var result = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions { SaveToGallery = gallery.IsToggled });
                if (result is not null) await StoreAsync(result, stationKey);
                Preferences.Default.Remove("reef.pending");
                status.Text = result is null ? "Capture cancelled" : "Station photograph stored";
            }
            catch (Exception error) { status.Text = error.Message; }
            finally { capture.IsEnabled = true; busy = false; }
        };
        Content = new VerticalStackLayout { Children = { station, new Label { Text = "Also save new capture to gallery" }, gallery, capture, status } };
        station.Text = Preferences.Default.Get("reef.pending", "");
        gallery.IsToggled = Preferences.Default.Get("reef.gallery", false);
        var recover = new Button { Text = "Recover interrupted photograph" };
        recover.Clicked += async (_, _) =>
        {
            if (busy) return;
            busy = true;
            try
            {
                using var wait = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var results = await MediaPicker.GetRecoveredMediaPickerResultsAsync();
                if (results.Count == 0) results = await MediaPicker.WaitForRecoveredMediaPickerResultsAsync(wait.Token);
                var key = Preferences.Default.Get("reef.pending", "");
                if (results.Count != 1) { status.Text = "Resolve ambiguous recovered results before continuing"; return; }
                foreach (var result in results)
                {
                    // Do not guess ownership of an orphan or an unexpected picker operation.
                    if (key.Length == 0 || result.Kind != RecoveredMediaPickerResultKind.CapturePhoto) continue;
                    foreach (var file in result.Files) await StoreAsync(file, key);
                    await MediaPicker.ClearRecoveredMediaPickerResultAsync(result.Id);
                    Preferences.Default.Remove("reef.pending");
                }
                status.Text = "Recovery inspected";
            }
            catch (OperationCanceledException) { status.Text = "No replay yet; keep pending station or explicitly abandon"; }
            catch (Exception error) { status.Text = error.Message; }
            finally { busy = false; }
        };
        var abandon = new Button { Text = "Abandon pending recovery" };
        abandon.Clicked += async (_, _) =>
        {
            if (busy) return;
            busy = true;
            try
            {
                if (!await DisplayAlertAsync("Discard pending photograph?", "An unreplayed result may be lost.", "Discard", "Keep")) return;
                await MediaPicker.DiscardPendingMediaPickerOperationAsync();
                Preferences.Default.Remove("reef.pending");
            }
            catch (Exception error) { status.Text = error.Message; }
            finally { busy = false; }
        };
        ((VerticalStackLayout)Content).Children.Add(recover);
        ((VerticalStackLayout)Content).Children.Add(abandon);
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


