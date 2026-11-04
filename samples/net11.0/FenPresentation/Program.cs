// snippet:complete
using System.Drawing;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        Application.EnableVisualStyles();
        using Form window = new() { Text = "Fen field briefing", Bounds = new Rectangle(120, 120, 640, 420) };
        using UserControl surface = new() { Dock = DockStyle.Fill };
        using Label instructions = new() { Dock = DockStyle.Fill, Text = "F11: presentation. Escape: restore. Close: release idle request." };
        surface.Controls.Add(instructions);
        window.Controls.Add(surface);
        using KioskModeManager presentation = new() { ContainerControl = surface };
        // This request is independent of FullScreen; release it explicitly or dispose the owner.
        window.Shown += (_, _) =>
        {
            Rectangle bounds = window.Bounds;
            FormBorderStyle border = window.FormBorderStyle;
            bool topMost = window.TopMost;
            if (args.Contains("--interactive"))
            {
                presentation.AlwaysOn = true;
                Console.WriteLine("Owner check: F11, Escape, then close. Display/system idle requested until close.");
                return;
            }
            presentation.FullScreen = true;
            if (!presentation.FullScreen || window.FormBorderStyle != FormBorderStyle.None || window.Bounds != Screen.FromControl(window).Bounds)
                throw new InvalidOperationException("Presentation entry failed.");
            presentation.FullScreen = false;
            if (window.Bounds != bounds || window.FormBorderStyle != border || window.TopMost != topMost || window.WindowState != FormWindowState.Normal)
                throw new InvalidOperationException("Normal window restoration failed.");
            window.WindowState = FormWindowState.Maximized;
            presentation.FullScreen = true;
            presentation.Dispose();
            if (window.WindowState != FormWindowState.Maximized || window.FormBorderStyle != border || window.TopMost != topMost)
                throw new InvalidOperationException("Disposal did not restore maximized state.");
            Console.WriteLine("PASS child-container resolution, normal restoration, maximized restoration on disposal. Idle and owner-visible behaviour not executed.");
            window.Close();
        };
        Application.Run(window);
    }
}
// endsnippet:complete
