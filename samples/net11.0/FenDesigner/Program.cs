// snippet:complete
using System.Drawing;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        using BriefingForm window = new();
        window.Shown += (_, _) => { window.CheckClick(); window.Close(); };
        Application.Run(window);
    }
}
// endsnippet:complete
