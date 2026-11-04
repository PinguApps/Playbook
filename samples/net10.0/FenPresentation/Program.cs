// snippet:complete
using System.Drawing;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        Application.EnableVisualStyles();
        using Form window = new() { Text = "Fen field briefing", Bounds = new Rectangle(120, 120, 640, 420) };
        window.Shown += (_, _) =>
        {
            Rectangle bounds = window.Bounds;
            FormBorderStyle border = window.FormBorderStyle;
            FormWindowState state = window.WindowState;
            bool topMost = window.TopMost;
            try
            {
                window.WindowState = FormWindowState.Normal;
                window.FormBorderStyle = FormBorderStyle.None;
                window.Bounds = Screen.FromControl(window).Bounds;
                if (window.FormBorderStyle != FormBorderStyle.None) throw new InvalidOperationException("Entry failed.");
            }
            finally
            {
                window.WindowState = FormWindowState.Normal;
                window.FormBorderStyle = border;
                window.Bounds = bounds;
                window.TopMost = topMost;
                window.WindowState = state;
            }
            if (window.Bounds != bounds || window.WindowState != state || window.FormBorderStyle != border || window.TopMost != topMost)
                throw new InvalidOperationException("Manual restoration failed.");
            Console.WriteLine("PASS manual normal-window presentation and restoration. No idle request or global shortcut.");
            window.Close();
        };
        Application.Run(window);
    }
}
// endsnippet:complete
