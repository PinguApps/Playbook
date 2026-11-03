// snippet:complete
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        Application.EnableVisualStyles();
        // Deliberate selection, independent of automatic renderer/dark-mode policy.
        Application.SetDefaultFormRevealMode(FormRevealMode.Deferred);
        using Form window = new() { Text = "Fen daily observations" };
        if (window.FormRevealMode != FormRevealMode.Deferred || !Application.IsFormRevealDeferred)
            throw new InvalidOperationException("Startup reveal policy was not applied.");
        using Form immediate = new() { FormRevealMode = FormRevealMode.Classic };
        if (immediate.FormRevealMode != FormRevealMode.Classic) throw new InvalidOperationException("Form override was lost.");
        window.Controls.Add(new Label { Text = "Observation queue prepared before first presentation", AutoSize = true });
        window.Shown += (_, _) => { Console.WriteLine("PASS C# deferred default, inherited form, classic override and Shown"); window.Close(); };
        Application.Run(window);
    }
}
// endsnippet:complete
