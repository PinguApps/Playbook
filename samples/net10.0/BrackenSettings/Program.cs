// snippet:complete
using System.Drawing;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
#if NET11_0_OR_GREATER
        Application.SetDefaultVisualStylesMode(VisualStylesMode.Net11);
#endif
        using Form screen = new() { Text = "Bracken station preferences", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
        using FlowLayoutPanel layout = new() { FlowDirection = FlowDirection.TopDown, AutoSize = true, Padding = new Padding(16), Dock = DockStyle.Fill };
        using TextBox station = new() { Text = "Bracken north", Width = 280, Padding = new Padding(8), BorderStyle = BorderStyle.Fixed3D };
        using CheckBox reports = new() { Text = "Send a daily station report", AutoSize = true, Checked = true };
        using Button legacy = new() { Text = "Open retained editor", AutoSize = true };
        using TreeView locations = new() { Height = 90, Width = 280, ItemHeight = 24 };
        locations.Nodes.Add("Bracken north");
#if NET11_0_OR_GREATER
        reports.Appearance = Appearance.ToggleSwitch;
        legacy.VisualStylesMode = VisualStylesMode.Classic;
#endif
        layout.Controls.AddRange([station, reports, legacy, locations]);
        screen.Controls.Add(layout);
        screen.Shown += (_, _) =>
        {
#if NET11_0_OR_GREATER
            if (station.VisualStylesMode != VisualStylesMode.Inherit) throw new InvalidOperationException("Local mode must remain inherited.");
            layout.VisualStylesMode = VisualStylesMode.Classic;
            layout.VisualStylesMode = VisualStylesMode.Net11;
            if (legacy.VisualStylesMode != VisualStylesMode.Classic) throw new InvalidOperationException("Child override was lost.");
            using PropertyGrid grid = new();
            grid.VisualStylesMode = VisualStylesMode.Net11;
            if (grid.VisualStylesMode != VisualStylesMode.Classic) throw new InvalidOperationException("PropertyGrid fallback changed.");
            Console.WriteLine("PASS inherited request, retained classic child, PropertyGrid fallback");
#endif
            reports.Checked = false;
            if (reports.Checked || station.Text != "Bracken north") throw new InvalidOperationException("Settings changed unexpectedly.");
            Console.WriteLine($"PASS settings screen lifecycle; {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
            screen.Close();
        };
        Application.Run(screen);
    }
}
// endsnippet:complete
