// snippet:complete
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;

namespace KittiwakeExchange;

public static class TripShare
{
    // Call on the native UI thread. The caller owns an existing durable image.
    public static async Task<string> SendAsync(IShare share, string imagePath,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return "Cancelled before dispatch.";
        try
        {
            var request = new ShareTextRequest
            {
                Title = "Kittiwake: North ridge walk",
                Text = "Meet at the old signal hut at ten.",
                Uri = "https://kittiwake.example/walks/north-ridge"
            };
#if NET11_0_OR_GREATER
            if (!File.Exists(imagePath))
                throw new FileNotFoundException("The walk preview is unavailable.", imagePath);
            request.PreviewImage = new ShareFile(imagePath, "image/png");
#endif
            await share.RequestAsync(request);
            return "Share UI dispatched or closed; delivery and cancellation are unknown.";
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException
            or ArgumentException or NotSupportedException)
        {
            return $"Unable to open sharing: {error.Message}";
        }
    }
}
// endsnippet:complete

