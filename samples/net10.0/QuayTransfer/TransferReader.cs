// snippet:complete
using System.Runtime.InteropServices;
using System.Windows;

internal static class TransferReader
{
    // Call on the WPF UI thread after selecting the process-wide retrieval policy.
    public static string ReadStationLabel()
    {
        try { return Clipboard.GetText(TextDataFormat.UnicodeText); }
        catch (ExternalException) { return "Clipboard unavailable; try again."; }
    }
}
// endsnippet:complete
