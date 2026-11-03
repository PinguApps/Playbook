// snippet:complete
using System.Runtime.InteropServices;
using System.Windows;

internal static class TransferReader
{
    // Call on the WPF UI thread after selecting the process-wide retrieval policy.
    // Only apply text when this returns true; display error separately on failure.
    public static bool TryReadStationLabel(out string text, out string? error)
    {
        try
        {
            text = Clipboard.GetText(TextDataFormat.UnicodeText);
            error = null;
            return true;
        }
        catch (ExternalException)
        {
            text = string.Empty;
            error = "Clipboard unavailable; try again.";
            return false;
        }
    }
}
// endsnippet:complete
